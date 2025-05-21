using System.ComponentModel;

namespace LearningAI.Api.AIFunctions;

public interface IKnowledgebaseTools
{
    [Description("Searches for knowledgebase documents relevant to the specified query, by the documents' and the query's semantics.")]
    Task<IReadOnlyCollection<string>> SearchDocumentsByContentSemanticsAsync(
        [Description("The query to find relevant documents to.")] string query,
        CancellationToken cancellationToken);

    [Description("Gets the URI for the document with the specified title.")]
    Task<string> GetUriForDocumentAsync(
        [Description("The title of the document.")] string documentTitle,
        CancellationToken cancellationToken);

    [Description("Gets the calendar - dates and day names - for the specified amount of days.")]
    Task<IReadOnlyCollection<CalendarEntry>> GetCalendarForNextNDaysAsync(
        [Description("The number of days for which to return the calendar. Today is included as the first element, whose offset is 0.")] int daysOffset,
        CancellationToken cancellationToken);
}