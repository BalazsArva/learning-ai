using System.ComponentModel.DataAnnotations;

namespace LearningAI.Api.Configuration;

public class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";

    [Required]
    public string Endpoint { get; set; } = default!;

    [Required]
    public string ApiKey { get; set; } = default!;

    [Required]
    public string ModelId { get; set; } = default!;
}