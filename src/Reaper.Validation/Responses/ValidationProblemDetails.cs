using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Reaper.Validation.Responses;

public class ValidationProblemDetails : ProblemDetails
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? ValidationFailures { get; set; }
}