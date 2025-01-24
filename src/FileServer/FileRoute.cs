using Microsoft.AspNetCore.Mvc;
using static FileServer.FileServerDirectory;

namespace FileServer;

internal static class FileRoute
{
    public static void Setup(WebApplication app, ILoggerFactory loggerFactory, Settings settings)
    {
        var logger = loggerFactory.CreateLogger(nameof(FileRoute));

        var userLookup = settings.FileServerUsers.ToDictionary(x => x.Username, x => x)
            ?? throw new InvalidOperationException("Could not convert file server users to a directory path lookup.");

        // Special route to get the favicon.
        app.MapGet("/favicon.ico", () =>
        {
            return Results.File(File.ReadAllBytes("Site/favicon.ico"), "image/x-icon");
        });

        app.MapGet(
            "{*route}",
            (HttpContext context, [FromRoute] string route = "") =>
            {
                ArgumentNullException.ThrowIfNull(
                    context.User?.Identity?.Name,
                    "The user identity was not set, something is wrong with the authentication middleware.");

                logger.LogInformation("{User} {Requested}.", context.User.Identity.Name, route);

                if (!userLookup.TryGetValue(context.User.Identity.Name, out FileServerUser? fileServerUser))
                {
                    throw new InvalidOperationException($"Could not find the user on username: {context.User.Identity.Name}");
                }

                var fileSystemEntryPath = Path.Combine(fileServerUser.FolderPath, route);

                if (!File.Exists(fileSystemEntryPath) && !Directory.Exists(fileSystemEntryPath))
                {
                    return Results.NotFound();
                }

                if (IsDirectory(fileSystemEntryPath))
                {
                    var isJsonResponse = context.Request.Query.ContainsKey("json");
                    if (isJsonResponse)
                    {
                        var directoryEntries = Directory.GetFileSystemEntries(fileSystemEntryPath)
                            .Select(x => new FileInfo(x))
                            .Select(x =>
                            {
                                var isDirectory = IsDirectory(x.FullName);
                                return new FileSystemEntry
                                {
                                    Name = x.Name,
                                    FileSizeBytes = isDirectory ? null : x.Length,
                                    LastWriteTimeUtc = x.LastWriteTime.ToUniversalTime(),
                                    IsDirectory = isDirectory,
                                    FileSize = isDirectory ? null : FileSizeFormat.SizeSuffix(x.Length)
                                };
                            })
                            .OrderByDescending(x => x.IsDirectory)
                            .ThenBy(x => x.Name);

                        return Results.Ok(directoryEntries);
                    }
                    else
                    {
                        var fileLinks = HtmlDirectoryBuilder
                            .CreateDirectoryLinkStructure(fileSystemEntryPath, route);

                        var indexFile = File.ReadAllText("Site/index.html")
                            .Replace("{{ Files }}", fileLinks, StringComparison.InvariantCulture);

                        return Results.Content(
                            indexFile,
                            "text/html");
                    }
                }
                else
                {
                    return Results.File(fileSystemEntryPath);
                }
            }
        );

        app.MapPost(
            "{*route}",
            (HttpContext context,
             HttpRequest request,
             string route = "") =>
            {
                ArgumentNullException.ThrowIfNull(
                    context.User?.Identity?.Name,
                    "The user identity was not set, something is wrong with the authentication middleware.");

                if (!userLookup.TryGetValue(context.User.Identity.Name, out FileServerUser? fileServerUser))
                {
                    throw new InvalidOperationException($"Could not find the user on username: {context.User.Identity.Name}");
                }

                logger.LogInformation("{User} {Requested} to upload one or more files.", context.User.Identity.Name, route);

                if (!fileServerUser.WriteAccess)
                {
                    logger.LogWarning("{User} tried to upload a file, but not have permissions to do it.", context.User.Identity.Name);
                    return Results.BadRequest("User does not have access to creating or writing to existing files.");
                }

                // Requesting to upload files, but no files were included.
                if (request.HasFormContentType && request.Form.Files.Count == 0)
                {
                    logger.LogWarning("{User} uploaded no files, requested failed.", context.User.Identity.Name);
                    return Results.BadRequest("No file uploaded");
                }

                // Upload files.
                if (request.HasFormContentType && request.Form.Files.Count > 0)
                {
                    foreach (var formFile in request.Form.Files)
                    {
                        // This is done to avoid issues where only half of the file has been uploaded and another user downloads it.
                        // We write it to temp storage and move it to the correct path after.
                        var tempFileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                        var filePath = Path.Combine(fileServerUser.FolderPath, route, formFile.FileName);
                        logger.LogInformation("{User} {Uploaded} in {Route}. Will be written to {FilePath}.", context.User.Identity.Name, formFile.FileName, route, filePath);

                        using (var uploadFileStream = formFile.OpenReadStream())
                        {
                            using (FileStream outStream = new FileStream(tempFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                uploadFileStream.Position = 0;
                                uploadFileStream.CopyTo(outStream);
                                outStream.Flush(); // Flush to OS
                                outStream.Flush(true); // Flush to disk
                            }
                        }

                        // Move the file to the destination folder.
                        File.Move(tempFileName, filePath);
                    }
                }
                // Requesting to create a directory.
                else
                {
                    var directoryPath = Path.Combine(fileServerUser.FolderPath, route);
                    Directory.CreateDirectory(directoryPath);
                }

                var shouldRedirect = context.Request.Query.ContainsKey("redirect");
                return shouldRedirect ? Results.Redirect($"/{route}") : Results.Ok();
            }
        ).DisableAntiforgery();

        app.MapDelete(
            "{*route}",
            (HttpContext context, string route = "") =>
            {
                ArgumentNullException.ThrowIfNull(
                   context.User?.Identity?.Name,
                   "The user identity was not set, something is wrong with the authentication middleware.");

                if (!userLookup.TryGetValue(context.User.Identity.Name, out FileServerUser? fileServerUser))
                {
                    throw new InvalidOperationException($"Could not find the user on username: {context.User.Identity.Name}");
                }

                logger.LogInformation("{User} {Requested} to be deleted.", context.User.Identity.Name, route);

                if (!fileServerUser.DeleteAccess)
                {
                    logger.LogWarning("{User} tried to delete a file, but not have permissions to do it.", context.User.Identity.Name);
                    return Results.BadRequest("User does not have access to delete files.");
                }

                if (string.IsNullOrWhiteSpace(route) || route.Trim() == "/")
                {
                    return Results.BadRequest("Cannot delete the root directory.");
                }

                var fileSystemEntryPath = Path.Combine(fileServerUser.FolderPath, route);

                if (!File.Exists(fileSystemEntryPath) && !Directory.Exists(fileSystemEntryPath))
                {
                    return Results.NotFound();
                }

                var isDirectory = IsDirectory(fileSystemEntryPath);
                if (isDirectory)
                {
                    Directory.Delete(fileSystemEntryPath, true);
                }
                else
                {
                    File.Delete(fileSystemEntryPath);
                }

                var shouldRedirect = context.Request.Query.ContainsKey("redirect");
                if (shouldRedirect)
                {
                    return isDirectory
                        ? Results.Redirect($"/{route.Replace($"{new DirectoryInfo(fileSystemEntryPath).Name}", "", StringComparison.InvariantCulture)}")
                        : Results.Redirect($"/{route.Replace($"{Path.GetFileName(fileSystemEntryPath)}", "", StringComparison.InvariantCulture)}");
                }
                else
                {
                    return Results.Ok();
                }
            }
        ).DisableAntiforgery();

        app.MapPut(
            "/move",
            (HttpContext context, [FromQuery] string sourceFilePath, [FromQuery] string destFilePath) =>
            {
                ArgumentNullException.ThrowIfNull(
                    context.User?.Identity?.Name,
                    "The user identity was not set, something is wrong with the authentication middleware.");

                if (!userLookup.TryGetValue(context.User.Identity.Name, out FileServerUser? fileServerUser))
                {
                    throw new InvalidOperationException($"Could not find the user on username: {context.User.Identity.Name}");
                }

                logger.LogInformation("{User} the file in path {FileRoute} to be moved to {MovedToRoute}.", context.User.Identity.Name, sourceFilePath, destFilePath);

                if (!fileServerUser.DeleteAccess || !fileServerUser.WriteAccess)
                {
                    logger.LogWarning("{User} tried to move a file, but not have permissions to do it.", context.User.Identity.Name);
                    return Results.BadRequest("User does not have access to move files.");
                }

                var fileSystemEntryPath = Path.Combine(fileServerUser.FolderPath, sourceFilePath);

                if (!File.Exists(fileSystemEntryPath))
                {
                    logger.LogWarning("{User} tried to move {File}, but it does not exist.", context.User.Identity.Name, fileSystemEntryPath);
                    return Results.BadRequest("File does not exist. Make sure you're sending a valid file path.");
                }

                var newFileSystemEntryPath = Path.Combine(fileServerUser.FolderPath, destFilePath);
                if (File.Exists(newFileSystemEntryPath))
                {
                    logger.LogWarning("{User} tried to move {File}, there is already a file with that name.", context.User.Identity.Name, newFileSystemEntryPath);
                    return Results.BadRequest("A file with the same name already exists in the new file path.");
                }

                File.Move(fileSystemEntryPath, newFileSystemEntryPath, false);

                return Results.Ok();
            }
        ).DisableAntiforgery();
    }
}
