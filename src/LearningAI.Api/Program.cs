using System.ClientModel;
using Azure.AI.OpenAI;
using LearningAI.Api.Persistence;
using LearningAI.Api.Persistence.RavenDb;
using LearningAI.Api.RequestHandlers;
using Microsoft.Extensions.AI;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder
    .Services
    .AddChatClient(new ChatClientBuilder(services =>
    {
        const string ModelId = "gpt-4o";

        var url = builder.Configuration["AzureOpenAI:Endpoint"]!;
        var apikey = builder.Configuration["AzureOpenAI:ApiKey"]!;

        return new AzureOpenAIClient(new Uri(url), new ApiKeyCredential(apikey)).GetChatClient(ModelId).AsIChatClient();
    })
    .Build());

builder.Services.AddSingleton<ICreateDocumentRequestHandler, CreateDocumentRequestHandler>();
builder.Services.AddSingleton<IDocumentAssistantQueryRequestHandler, DocumentAssistantQueryRequestHandler>();
builder.Services.AddSingleton<IKnowledgebaseDocumentRepository, KnowledgebaseDocumentRepository>();
builder.Services.AddSingleton((serviceProvider) =>
{
    return
        new DocumentStore()
        {
            // TODO: Config
            Database = "learning-ai",
            Urls = ["http://localhost:8080"],
        }
        .Initialize();
});
builder.AddOllamaSharpEmbeddingGenerator("ollama", opts => opts.SelectedModel = "all-minilm");

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.Run();