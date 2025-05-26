using LearningAI.Api.Persistence;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Text;

namespace LearningAI.Api.RequestHandlers;

public class CreateDocumentRequestHandler(
    IKnowledgebaseDocumentRepository repository,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    ILogger<CreateDocumentRequestHandler> logger) : ICreateDocumentRequestHandler
{
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    public async Task<string> CreateDocumentAsync(CreateDocumentRequest request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid().ToString();
        var markdownParas = ChunkContents(request.Contents, request.Title);
        var generateEmbeddingsTasks = markdownParas.Select(p => GenerateEmbeddingAsync(p, cancellationToken));

        await repository.SaveDocumentAsync(
            new(id, request.Title, request.Contents, await Task.WhenAll(generateEmbeddingsTasks)),
            cancellationToken);

        logger.LogInformation("Created document with Id={Id} Title={Title}", id, request.Title);

        return id;
    }

    private static List<string> ChunkContents(string contents, string chunkHeader)
    {
        // Chunk content and save multiple vectors
        // See https://techcommunity.microsoft.com/blog/azure-ai-services-blog/azure-ai-search-outperforming-vector-search-with-hybrid-retrieval-and-reranking/3929167
        // Embedding each chunk into its own vector keeps the input within the embedding model’s token limit and enables the entire document to be searchable in an ANN
        // search index without truncation. Most deep embedding models have a limit of 512 tokens. Ada - 002 has a limit of 8,192 tokens.
        //
        // Also, check the limits on the chat LLM and if we exceed or get close to the token limit with the current documents. Instead of whole documents, we could
        // send the most relevant chunks only. Also see part 4 of the link above for some interesting ideas on chunking (overlapping, generating chunks so all of them
        // contain context/most relevant keywords/title).
        const int maxTokens = 512;
        const int tokensOverlap = maxTokens / 4; // use 25% token overlapping

        var markdownLines = TextChunker.SplitMarkDownLines(contents, maxTokens);

        return TextChunker.SplitMarkdownParagraphs(markdownLines, maxTokens, tokensOverlap, $"# {chunkHeader}\n\n");
    }

    private async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken)
    {
        var embeddingAsMemory = await embeddingGenerator.GenerateVectorAsync(text, cancellationToken: cancellationToken);

        return embeddingAsMemory.ToArray();
    }

#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
}