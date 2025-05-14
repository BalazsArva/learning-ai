namespace LearningAI.Api.Configuration;

public class RavenDbOptions
{
    public const string SectionName = "RavenDb";

    public string Database { get; set; } = default!;

    public string[] Urls { get; set; } = [];
}