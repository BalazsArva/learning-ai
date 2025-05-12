using LearningAI.Api.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LearningAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KnowledgebaseController(ILogger<KnowledgebaseController> logger) : ControllerBase
{
    [HttpPost("documents")]
    public async Task<IActionResult> CreateDocument(
        CreateKnowledgebaseDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        return Ok(new CreateKnowledgebaseDocumentResponse());
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
        KnowledgebaseAssistantQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        return NoContent();
    }
}