using System.Text.Json.Serialization;

namespace FileServer;

internal sealed record FileServerUser
{
    [JsonPropertyName("username")]
    public required string Username { get; init; }

    [JsonPropertyName("password")]
    public required string Password { get; init; }

    [JsonPropertyName("folderPath")]
    public required string FolderPath { get; init; }

    [JsonPropertyName("writeAccess")]
    public required bool WriteAccess { get; init; }

    [JsonPropertyName("deleteAccess")]
    public required bool DeleteAccess { get; init; }
}

internal sealed record Settings
{
    [JsonPropertyName("fileServerUsers")]
    public required IReadOnlyCollection<FileServerUser> FileServerUsers { get; init; }
}
