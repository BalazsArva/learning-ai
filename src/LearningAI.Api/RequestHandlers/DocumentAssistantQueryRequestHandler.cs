using LearningAI.Api.AIFunctions;
using Microsoft.Extensions.AI;

namespace LearningAI.Api.RequestHandlers;

public class DocumentAssistantQueryRequestHandler(
    IChatClient chatClient,
    Func<IKnowledgebaseTools> knowledgebaseToolsFactory,
    ILogger<DocumentAssistantQueryRequestHandler> logger) : IDocumentAssistantQueryRequestHandler
{
    public async Task<QueryAssistantResult> QueryAssistantAsync(QueryAssistantRequest request, CancellationToken cancellationToken)
    {
        var messages = new List<ChatMessage>
        {
            new(
                ChatRole.System,
                """
                You are an assistant who answers questions by finding and summarizing the requested information from a company's
                internal knowledgebase. Use the tools available to you to fulfil the request. Don't make up information,
                politely reply that you don't know the answer if you cannot find any information on what the user is asking.
                Your answer should be in markdown format. If you are able to find the requested information, then include the
                relevant links in your answer as well, at the end.
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
        var getDocumentUrlTool = AIFunctionFactory.Create(kbTools.GetUriForDocumentAsync);

        return new ChatOptions
        {
            AllowMultipleToolCalls = true,
            Tools = [searchKbTool, getDocumentUrlTool],
        };
    }
}