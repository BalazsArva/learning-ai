using System.ComponentModel;
using LearningAI.Api.Persistence;
using Microsoft.Extensions.AI;

namespace LearningAI.Api.RequestHandlers;

public class DocumentAssistantQueryRequestHandler(
    IChatClient chatClient,
    Func<IKnowledgebaseTools> knowledgebaseToolsFactory,
    ILogger<DocumentAssistantQueryRequestHandler> logger) : IDocumentAssistantQueryRequestHandler
{
    public async Task<QueryAssistantResult> QueryAssistantAsync(QueryAssistantRequest request, CancellationToken cancellationToken)
    {
        // TODO: Try to make it output where it found information (document name/title/whatever)
        var messages = new List<ChatMessage>
        {
            new(
                ChatRole.System,
                """
                You are an assistant who answers questions by finding and summarizing the requested information from a company's
                internal knowledgebase. Use the tools available to you to search for relevant documents. Don't make up information,
                politely answer that you don't know the answer if you cannot find any information on what the user is asking.
                Your answer should be in markdown format.
                """),
            new(ChatRole.User, request.Query)
        };

        var assistantResponse = await chatClient.GetResponseAsync(messages, CreateChatOptions(), cancellationToken);

        return new(
            string.IsNullOrEmpty(assistantResponse.Text)
                ? "[Warning] No response received from assistant."
                : assistantResponse.Text);
    }

    private ChatOptions CreateChatOptions()
    {
        var kbTools = knowledgebaseToolsFactory();

        var searchKbTool = AIFunctionFactory.Create(kbTools.SearchDocumentsByContentSemanticsAsync);

        return new ChatOptions
        {
            AllowMultipleToolCalls = true,
            Tools = [searchKbTool],
        };
    }
}

public interface IKnowledgebaseTools
{
    [Description("Searches for knowledgebase documents relevant to the specified query, by the documents' and the query's semantics.")]
    Task<IReadOnlyCollection<string>> SearchDocumentsByContentSemanticsAsync(
        [Description("The query to find relevant documents to.")] string query,
        CancellationToken cancellationToken);
}

// TODO: Move elsewhere
// TODO: Try with interface
public class KnowledgebaseTools(
    IKnowledgebaseDocumentRepository repository,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    ILogger<KnowledgebaseTools> logger) : IKnowledgebaseTools
{
    [Description("Searches for knowledgebase documents relevant to the specified query, by the documents' and the query's semantics.")]
    public async Task<IReadOnlyCollection<string>> SearchDocumentsByContentSemanticsAsync(
        [Description("The query to find relevant documents to.")] string query,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Searching for documents by query '{Query}'", query);

        var queryEmbedding = await embeddingGenerator.GenerateVectorAsync(query, cancellationToken: cancellationToken);
        var documents = await repository.SearchDocumentsByContentEmbeddingAsync(queryEmbedding, cancellationToken);

        logger.LogInformation("Search by query {Query} yielded {MatchCount} results.", query, documents.Count);

        return [.. documents.Select(d => d.Contents)];
    }
}