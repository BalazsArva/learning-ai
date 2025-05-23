using System.ComponentModel;

namespace LearningAI.Api.AIFunctions;

public interface IKnowledgebaseTools
{
    [Description("Searches for knowledgebase documents relevant to the specified query, by the documents' and the query's semantics.")]
    Task<IReadOnlyCollection<string>> SearchDocumentsByContentSemanticsAsync(
        [Description("The query to find relevant documents to.")] string query,
        CancellationToken cancellationToken);

    [Description("Searches for knowledgebase documents based on keywords from the query. The included keywords may be synonyms of other included keywords.")]
    Task<IReadOnlyCollection<string>> SearchDocumentsByQueryKeywordsAsync(
        [Description("The keywords by which to search for relevant documents.")] string queryKeywords,
        CancellationToken cancellationToken);

    [Description("Searches for knowledgebase documents based on the query's semantics and by keywords extracted from the query.")]
    Task<IReadOnlyCollection<SearchDocumentToolResult>> SearchDocumentsAsync(
        [Description("The query by which to find semantically similar documents.")] string query,
        [Description("The keywords extracted from the query by which to search for relevant documents.")] string[] queryKeywords,
        CancellationToken cancellationToken);

    [Description("Gets the calendar - dates and day names - for the specified amount of days.")]
    Task<IReadOnlyCollection<CalendarEntry>> GetCalendarForNextNDaysAsync(
        [Description("The number of days for which to return the calendar. Today is included as the first element, whose offset is 0.")] int daysOffset,
        CancellationToken cancellationToken);
}