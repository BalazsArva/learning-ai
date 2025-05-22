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
                You are an assistant who answers questions by finding and compiling the requested information from a company's internal knowledgebase.

                Use the tools available to you to search for documents where you may find relevant information or to get any additional required data.

                Don't make up information, politely reply that you don't know the answer if you cannot find any information on what the user is asking.

                Your answer should:
                - be in markdown format
                - contain URLs which you created that point to the documents you used in the answer.
                """),
            new(ChatRole.User, request.Query)
        };

        // TODO: Play around with what to do if the references are too large. Ideas:
        // - Detect when there are too many tokens and only do the search. Return a simple response saying that the documents are too large but
        //   here are the relevant documents to read more: <link>
        // - In a separate LLM conversation for each search result, ask if it contains info on what's being asked. If so, ask to summarize
        //   it/create an extract if the doc contains other unrelated topics and send the shortened form to the main conversation
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
        var getFutureDatesWithDayNamesTool = AIFunctionFactory.Create(kbTools.GetCalendarForNextNDaysAsync);
        var searchKbByKeywordsTool = AIFunctionFactory.Create(kbTools.SearchDocumentsByQueryKeywordsAsync);
        var combinedKbSearchTool = AIFunctionFactory.Create(kbTools.SearchDocumentsAsync);

        return new ChatOptions
        {
            Temperature = 0,

            AllowMultipleToolCalls = true,
            //Tools = [searchKbTool, getDocumentUrlTool, getFutureDatesWithDayNamesTool, searchKbByKeywordsTool],
            Tools = [combinedKbSearchTool, getDocumentUrlTool, getFutureDatesWithDayNamesTool],

            ToolMode = ChatToolMode.RequireSpecific(combinedKbSearchTool.Name),
        };
    }
}