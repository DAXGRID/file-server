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
                var fileExists = File.Exists(fileSystemEntryPath);

                if (!fileExists)
                {
                    Results.NotFound();
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
                        var filePath = Path.Combine(fileServerUser.FolderPath, route, formFile.FileName);
                        logger.LogInformation("{User} {Uploaded} in {Route}. Will be written to {FilePath}.", context.User.Identity.Name, formFile.FileName, route, filePath);

                        using var uploadFileStream = formFile.OpenReadStream();
                        using Stream outStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

                        uploadFileStream.Position = 0;
                        uploadFileStream.CopyTo(outStream);
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
                var fileExists = File.Exists(fileSystemEntryPath);

                if (!fileExists)
                {
                    Results.NotFound();
                }

                if (IsDirectory(fileSystemEntryPath))
                {
                    Directory.Delete(fileSystemEntryPath, true);
                }
                else
                {
                    File.Delete(fileSystemEntryPath);
                }

                var shouldRedirect = context.Request.Query.ContainsKey("redirect");
                return shouldRedirect ? Results.Redirect($"/{route}") : Results.Ok();
            }
        ).DisableAntiforgery();
    }
}
