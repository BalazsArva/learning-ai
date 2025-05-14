using System.ComponentModel;

namespace LearningAI.Api.AIFunctions;

public interface IKnowledgebaseTools
{
    [Description("Searches for knowledgebase documents relevant to the specified query, by the documents' and the query's semantics.")]
    Task<IReadOnlyCollection<string>> SearchDocumentsByContentSemanticsAsync(
        [Description("The query to find relevant documents to.")] string query,
        CancellationToken cancellationToken);

    [Description("Gets the document at which the document with the specified title can be accessed.")]
    Task<string> GetUriForDocumentAsync(
        [Description("The title of the document.")] string documentTitle,
        CancellationToken cancellationToken);
}