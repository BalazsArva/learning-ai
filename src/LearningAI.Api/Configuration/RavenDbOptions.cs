using System.ComponentModel.DataAnnotations;

namespace LearningAI.Api.Configuration;

public class RavenDbOptions
{
    public const string SectionName = "RavenDb";

    [Required]
    public string Database { get; set; } = default!;

    [Required]
    public string[] Urls { get; set; } = [];
}