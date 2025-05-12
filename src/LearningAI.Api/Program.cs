using LearningAI.Api.RequestHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<ICreateDocumentRequestHandler, CreateDocumentRequestHandler>();
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

app.Run();