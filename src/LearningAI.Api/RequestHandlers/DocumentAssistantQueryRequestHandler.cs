using LearningAI.Api.Persistence;
using Microsoft.Extensions.AI;

namespace LearningAI.Api.RequestHandlers;

public class DocumentAssistantQueryRequestHandler(
    IKnowledgebaseDocumentRepository repository,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IChatClient chatClient,
    ILogger<DocumentAssistantQueryRequestHandler> logger) : IDocumentAssistantQueryRequestHandler
{
    private static QueryAssistantResult ResponseForNoMatchingDocuments = new("I'm sorry I could not find any information on what you are looking for.");

    public async Task<QueryAssistantResult> QueryAssistantAsync(QueryAssistantRequest request, CancellationToken cancellationToken)
    {
        var queryEmbedding = await embeddingGenerator.GenerateEmbeddingVectorAsync(request.Query, cancellationToken: cancellationToken);
        var documents = await repository.SearchDocumentsByContentEmbeddingAsync(queryEmbedding, cancellationToken);

        if (documents.Count == 0)
        {
            return ResponseForNoMatchingDocuments;
        }

        // TODO: Try to make it output where it found information (document name/title/whatever)
        var messages = new List<ChatMessage>
        {
            new(
                ChatRole.System,
                """
                You are an assistant who answers inquiries by finding and summarizing the requested information from a company's
                internal knowledgebase. Consider the following documents, and answer the user's query. Don't make up information,
                politely answer that you don't know the answer if you cannot find any information on what the user is asking.
                Your answer should be in markdown format.
                """),
        };

        foreach (var document in documents)
        {
            messages.Add(new ChatMessage(ChatRole.Tool, document.Contents));
        }

        messages.Add(new ChatMessage(ChatRole.User, request.Query));

        var assistantResponse = await chatClient.CompleteAsync(messages, new ChatOptions(), cancellationToken);

        return new(assistantResponse.Message.Text ?? "[Warning] No response received from assistant.");
    }
}