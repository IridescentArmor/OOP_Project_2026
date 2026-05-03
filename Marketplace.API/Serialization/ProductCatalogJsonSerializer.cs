using System.Text.Json;
using Marketplace.API.DTOs;

namespace Marketplace.API.Serialization;

public static class ProductCatalogJsonSerializer
{
    public static JsonSerializerOptions ExportOptions { get; } = new() { WriteIndented = true };

    public static JsonSerializerOptions ImportOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static string SerializeRecords(IReadOnlyList<ProductJsonRecord> rows)
        => JsonSerializer.Serialize(rows, ExportOptions);

    public static async Task<List<ProductJsonRecord>?> DeserializeRecordsAsync(
        Stream utf8Json,
        CancellationToken cancellationToken = default)
        => await JsonSerializer
            .DeserializeAsync<List<ProductJsonRecord>>(utf8Json, ImportOptions, cancellationToken)
            .ConfigureAwait(false);
}
