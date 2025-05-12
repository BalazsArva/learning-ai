using System.Text;
using LearningAI.TestDataGeneration.Apis.Knowledgebase;
using Microsoft.Extensions.Logging;

internal class KnowledgebaseTestDataGenerator(
    ILogger<KnowledgebaseTestDataGenerator> logger,
    IKnowledgebaseApi apiClient)
{
    public async Task GenerateKnowledgebaseData()
    {
        var rootDirectoryInfo = new DirectoryInfo("TestData/Knowledgebase");

        foreach (var file in rootDirectoryInfo.GetFiles())
        {
            var sb = new StringBuilder();
            var title = "Untitled";
            var titleSet = false;

            using var reader = new StreamReader(file.FullName);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine() ?? string.Empty;

                if (!titleSet)
                {
                    // Assumption: Markdown syntax and first line is used as document title
                    title = line.Trim('#').Trim();
                    titleSet = true;
                }

                sb.AppendLine(line);
            }

            logger.LogInformation("Creating test data file {File}, using title {Title}", file.FullName, title);

            await CreateDocumentAsync(title, sb.ToString());
        }
    }

    private async Task CreateDocumentAsync(string title, string documentContent)
    {
        using var response = await apiClient.CreateDocumentAsync(
            new CreateKnowledgebaseDocumentRequest(title, documentContent),
            CancellationToken.None);
    }
}