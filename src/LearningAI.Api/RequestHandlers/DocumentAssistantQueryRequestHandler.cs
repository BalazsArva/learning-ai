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
    private record SearchInputTransformation(string[] InputVariants);

    private enum InputKind
    {
        CannotDecide,
        TermsQuery,
        Prompt,
    }

    // TODO: Rename
    public async Task<QueryAssistantResult> QueryAssistantAsync(QueryAssistantRequest request, CancellationToken cancellationToken)
    {
        string? assistantResponse = null;
        var inputKind = await DetermineInputKindAsync(request, cancellationToken);

        if (inputKind == InputKind.Prompt)
        {
            assistantResponse = await GetAssistantResponseAsync(request, assistantResponse, cancellationToken);
        }

        var searchTerms = await GetSearchTermsAsync(request, inputKind, cancellationToken);
        var searchResults = await SmartKeywordsSearchAsync(searchTerms, cancellationToken);

        return new(
            assistantResponse,
            new(
                searchTerms,
                searchResults
                    .Select(x => new KnowledgebaseDocumentSearchResultItem(x.Id, x.Title, uriProvider.GetUriForKnowledgebaseDocumentByTitle(x.Title), "TODO:QUOTE"))
                    .ToList()));
    }

    private async Task<string> GetAssistantResponseAsync(QueryAssistantRequest request, string? assistantResponse, CancellationToken cancellationToken)
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
            new(ChatRole.User, request.Query),
        };

        // TODO: Play around with what to do if the references are too large. Ideas:
        // - Detect when there are too many tokens and only do the search. Return a simple response saying that the documents are too large but
        //   here are the relevant documents to read more: <link>
        // - In a separate LLM conversation for each search result, ask if it contains info on what's being asked. If so, ask to summarize
        //   it/create an extract if the doc contains other unrelated topics and send the shortened form to the main conversation
        var aiResponse = await chatClient.GetResponseAsync(messages, CreateChatOptions(), cancellationToken);

        return string.IsNullOrEmpty(aiResponse.Text)
            ? "[Warning] No response received from assistant."
            : aiResponse.Text;
    }

    private async Task<IReadOnlyCollection<string>> GetSearchTermsAsync(
        QueryAssistantRequest request,
        InputKind inputKind,
        CancellationToken cancellationToken)
    {
        const string instructionsForPrompt =
            """
            Consider the following prompt entered by a user on a company knowledgebase page.
            Create up to 10 terms-based queries derived from the prompt which have similar meaning to the prompt
            but only contains the key terms and phrases or their synonyms from the input.
            """;
        const string instructionsForTermsQuery =
            """
            Consider the following terms-based query entered by a user on a company knowledgebase page.
            Create up to 10 alternative terms-based queries derived from the input which have similar meaning to the original
            but only contains the key terms and phrases or their synonyms from the input.
            """;

        if (inputKind == InputKind.CannotDecide)
        {
            return [request.Query];
        }

        var regexTimeout = TimeSpan.FromSeconds(1);
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, inputKind == InputKind.TermsQuery ? instructionsForTermsQuery : instructionsForPrompt),
            new(ChatRole.User, request.Query),
        };

        var assistantResponse = await chatClient
            .GetResponseAsync<SearchInputTransformation>(messages, new ChatOptions { Temperature = 0 }, cancellationToken: cancellationToken);

        return assistantResponse.Result.InputVariants
            .Concat(inputKind == InputKind.TermsQuery ? [request.Query] : [])
            .Select(x => Regex.Split(x, "\\s+", RegexOptions.IgnoreCase, regexTimeout))
            .SelectMany(x => x)
            .ToHashSet();
    }

    private async Task<InputKind> DetermineInputKindAsync(
        QueryAssistantRequest request,
        CancellationToken cancellationToken)
    {
        var messages = new List<ChatMessage>
        {
            new(
                ChatRole.System,
                $"""
                The following is a search input entered by a user on a company knowledgebase.
                Decide the type of the input ({string.Join(" / ", Enum.GetNames<InputKind>())})
                """),
            new(ChatRole.User, request.Query),
        };

        var assistantResponse = await chatClient
            .GetResponseAsync<InputKind>(messages, new ChatOptions { Temperature = 0 }, cancellationToken: cancellationToken);

        logger.LogDebug("Query {Query} input kind classified as {InputKind}", request.Query, assistantResponse.Result);

        return assistantResponse.Result;
    }

    private async Task<IReadOnlyCollection<KnowledgebaseDocument>> SmartKeywordsSearchAsync(
        IReadOnlyCollection<string> searchTerms,
        CancellationToken cancellationToken)
    {
        var combinedQuery = string.Join(" ", searchTerms.Select(term => $"{term}*"));

        return await repository.SearchDocumentsByKeywordsAsync(combinedQuery, 25, cancellationToken);
    }

    private ChatOptions CreateChatOptions()
    {
        var kbTools = knowledgebaseToolsFactory();
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