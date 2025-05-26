using System.Text.RegularExpressions;
using LearningAI.Api.AIFunctions;
using LearningAI.Api.Persistence;
using LearningAI.Api.Utilities;
using Microsoft.Extensions.AI;

namespace LearningAI.Api.RequestHandlers;

public class DocumentAssistantQueryRequestHandler(
    IChatClient chatClient,
    Func<IKnowledgebaseTools> knowledgebaseToolsFactory,
    IKnowledgebaseDocumentRepository repository,
    IUriProvider uriProvider,
    ILogger<DocumentAssistantQueryRequestHandler> logger) : IDocumentAssistantQueryRequestHandler
{
    // TODO: Rename
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
        var aiResponse = await chatClient.GetResponseAsync(messages, CreateChatOptions(), cancellationToken);

        var searchTerms = await GetSearchTermsAsync(request, cancellationToken);
        var searchResults = await SmartKeywordsSearchAsync(searchTerms, cancellationToken);
        var assistantResponse = string.IsNullOrEmpty(aiResponse.Text)
            ? "[Warning] No response received from assistant."
            : aiResponse.Text;

        return new(
            assistantResponse,
            new(
                searchTerms,
                searchResults
                    .Select(x => new KnowledgebaseDocumentSearchResultItem(x.Id, x.Title, uriProvider.GetUriForKnowledgebaseDocumentByTitle(x.Title), "TODO:QUOTE"))
                    .ToList()));
    }

    private async Task<IReadOnlyCollection<KnowledgebaseDocument>> SmartKeywordsSearchAsync(IReadOnlyCollection<string> searchTerms, CancellationToken cancellationToken)
    {
        var combinedQuery = string.Join(" ", searchTerms.Select(term => $"{term}*"));

        return await repository.SearchDocumentsByKeywordsAsync(combinedQuery, 25, cancellationToken);
    }

    private async Task<IReadOnlyCollection<string>> GetSearchTermsAsync(QueryAssistantRequest request, CancellationToken cancellationToken)
    {
        var messages = new List<ChatMessage>
        {
            new(
                ChatRole.System,
                """
                Consider the following search queries entered by users.
                If the input is a question, transform it to a query that has the same semantics as the question.
                """),
            new(ChatRole.User, request.Query)
        };

        var chatOptions = new ChatOptions { Temperature = 0 };
        var assistantResponse = await chatClient.GetResponseAsync(messages, chatOptions, cancellationToken);
        var terms = Regex.Split(
            assistantResponse.Text,
            "\\s+",
            RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(1));

        return terms.ToHashSet();
    }

    private ChatOptions CreateChatOptions()
    {
        var kbTools = knowledgebaseToolsFactory();

        /*
        var kbSemanticSearchTool = AIFunctionFactory.Create(kbTools.SearchDocumentsByContentSemanticsAsync);
        var kbKeywordsSearchTool = AIFunctionFactory.Create(kbTools.SearchDocumentsByQueryKeywordsAsync);
        */

        var getFutureDatesWithDayNamesTool = AIFunctionFactory.Create(kbTools.GetCalendarForNextNDaysAsync);
        var combinedKbSearchTool = AIFunctionFactory.Create(kbTools.SearchDocumentsAsync);

        return new ChatOptions
        {
            Temperature = 0,
            AllowMultipleToolCalls = true,

            Tools = [combinedKbSearchTool, getFutureDatesWithDayNamesTool],
            ToolMode = ChatToolMode.RequireSpecific(combinedKbSearchTool.Name),
        };
    }
}