using System.Text.Json;
using System.Text.Json.Serialization;

namespace Klinkby.CleanApi.Serialization;

/// <summary>
///     Provide AOT-friendly source generation for JSON serialization of <see cref="HealthReportResponse" />
/// </summary>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(HealthReportResponse))]
internal partial class HealthReportResponseSerializerContext : JsonSerializerContext
{
}