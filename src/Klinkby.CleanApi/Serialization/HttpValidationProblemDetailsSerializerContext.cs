using System.Text.Json;
using System.Text.Json.Serialization;

namespace Klinkby.CleanApi.Serialization;

/// <summary>
///     Provide AOT-friendly source generation for JSON serialization of <see cref="HttpValidationProblemDetails" />
/// </summary>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(HttpValidationProblemDetails))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSerializable(typeof(Dictionary<string, object[]>))]
internal partial class HttpValidationProblemDetailsSerializerContext : JsonSerializerContext
{
}