namespace FileServer;

internal static class FileServerDirectory
{
    public static bool IsDirectory(string filePath)
    {
        return File.GetAttributes(filePath).HasFlag(FileAttributes.Directory);
    }
}
