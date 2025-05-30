using LearningAI.Api.Contracts.Requests;
using LearningAI.Api.RequestHandlers;
using Microsoft.AspNetCore.Mvc;

namespace LearningAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KnowledgebaseController : ControllerBase
{
    [HttpPost("documents")]
    public async Task<IActionResult> CreateDocument(
        [FromServices] ICreateDocumentRequestHandler requestHandler,
        CreateKnowledgebaseDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        var id = await requestHandler.CreateDocumentAsync(new CreateDocumentRequest(request.Title, request.Content), cancellationToken);

        return Ok(new CreateKnowledgebaseDocumentResponse(id));
    }

    [HttpGet("documents/{title}")]
    public async Task<IActionResult> GetDocument(
        string title,
        CancellationToken cancellationToken = default)
    {
        return NoContent();
    }

    [HttpPost("assistant/query")]
    public async Task<IActionResult> PerformAssistantQuery(
        [FromServices] IDocumentAssistantQueryRequestHandler requestHandler,
        KnowledgebaseAssistantQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await requestHandler.QueryAssistantAsync(new QueryAssistantRequest(request.Query), cancellationToken);

        return Ok(result);
    }
}