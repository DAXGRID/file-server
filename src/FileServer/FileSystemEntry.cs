using System.Text.Json.Serialization;

namespace FileServer;

internal sealed record FileSystemEntry
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("lastWriteTimeUtc")]
    public required DateTime LastWriteTimeUtc { get; init; }

    [JsonPropertyName("lastWriteTimeUtcUnixtimeStamp")]
    public long LastWriteTimeUtcUnixtimeStamp => ((DateTimeOffset)LastWriteTimeUtc).ToUnixTimeSeconds();

    [JsonPropertyName("fileSizeBytes")]
    public required long? FileSizeBytes { get; init; }

    [JsonPropertyName("fileSize")]
    public required string? FileSize { get; init; }

    [JsonPropertyName("isDirectory")]
    public required bool IsDirectory { get; init; }
}
