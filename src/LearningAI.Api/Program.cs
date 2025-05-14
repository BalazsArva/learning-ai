using System.ClientModel;
using Azure.AI.OpenAI;
using LearningAI.Api.Configuration;
using LearningAI.Api.Persistence;
using LearningAI.Api.Persistence.RavenDb;
using LearningAI.Api.RequestHandlers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Formatting.Compact;

try
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo
        .Console(formatter: new RenderedCompactJsonFormatter())
        .CreateLogger();

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders().AddSerilog(Log.Logger);

    builder.Services.AddOptionsWithValidateOnStart<RavenDbOptions>().BindConfiguration(RavenDbOptions.SectionName);
    builder.Services.AddOptionsWithValidateOnStart<AzureOpenAIOptions>().BindConfiguration(AzureOpenAIOptions.SectionName);

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    builder
        .Services
        .AddChatClient(services =>
        {
            var opts = services.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;

            return
                new ChatClientBuilder(
                    new AzureOpenAIClient(new Uri(opts.Endpoint), new ApiKeyCredential(opts.ApiKey))
                        .GetChatClient(opts.ModelId)
                        .AsIChatClient())
                    .UseFunctionInvocation()
                    .Build();
        });

    builder.Services.AddTransient<KnowledgebaseTools>();
    builder.Services.AddSingleton<Func<KnowledgebaseTools>>(services => () => services.GetRequiredService<KnowledgebaseTools>());
    builder.Services.AddSingleton<ICreateDocumentRequestHandler, CreateDocumentRequestHandler>();
    builder.Services.AddSingleton<IDocumentAssistantQueryRequestHandler, DocumentAssistantQueryRequestHandler>();
    builder.Services.AddSingleton<IKnowledgebaseDocumentRepository, KnowledgebaseDocumentRepository>();
    builder.Services.AddSingleton(serviceProvider =>
    {
        var opts = serviceProvider.GetRequiredService<IOptions<RavenDbOptions>>();

        return
            new DocumentStore
            {
                Database = opts.Value.Database,
                Urls = opts.Value.Urls,
            }
            .Initialize();
    });
    builder.AddOllamaApiClient("ollama", opts => opts.SelectedModel = "all-minilm").AddEmbeddingGenerator();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(scalar => scalar.WithDarkMode(false));
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    await IndexCreation.CreateIndexesAsync(
        typeof(KnowledgebaseDocumentRepository).Assembly,
        app.Services.GetRequiredService<IDocumentStore>());

    await app.RunAsync();
}
catch (Exception e)
{
    Console.WriteLine($"Host failed to start: {e.Message}");
    throw;
}