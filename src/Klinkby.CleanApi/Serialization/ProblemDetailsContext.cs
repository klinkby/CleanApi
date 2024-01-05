using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Klinkby.CleanApi.Serialization;

/// <summary>
///     Provide AOT-friendly source generation for JSON serialization of <see cref="ProblemDetails" />
/// </summary>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class ProblemDetailsSerializerContext : JsonSerializerContext
{
}