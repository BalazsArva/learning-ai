namespace LearningAI.Api.RequestHandlers;

public record KnowledgebaseDocumentSearchResult(
    [property: System.Text.Json.Serialization.JsonPropertyName("__debug_queryTerms")] IReadOnlyCollection<string> QueryTerms,
    IReadOnlyCollection<KnowledgebaseDocumentSearchResultItem> Items);