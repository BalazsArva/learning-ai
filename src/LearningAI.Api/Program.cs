using LearningAI.Api.Persistence;
using LearningAI.Api.Persistence.RavenDb;
using LearningAI.Api.RequestHandlers;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<ICreateDocumentRequestHandler, CreateDocumentRequestHandler>();
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
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await IndexCreation.CreateIndexesAsync(
    typeof(KnowledgebaseDocumentRepository).Assembly,
    app.Services.GetRequiredService<IDocumentStore>());

app.Run();