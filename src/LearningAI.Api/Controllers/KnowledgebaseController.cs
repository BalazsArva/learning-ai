using LearningAI.Api.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LearningAI.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class KnowledgebaseController(ILogger<KnowledgebaseController> logger) : ControllerBase
{
    [HttpPost("directory/{parentFolderId:guid}")]
    public async Task<IActionResult> CreateFolder(
        Guid? parentFolderId,
        CreateKnowledgebaseFolderRequest request,
        CancellationToken cancellationToken = default)
    {
        return NoContent();
    }

    [HttpPost("directory/{parentFolderId:guid}/documents")]
    public async Task<IActionResult> CreateDocument(
        Guid? parentFolderId,
        CreateKnowledgebaseDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        return NoContent();
    }

    [HttpGet("directory/{folderId:guid}/list")]
    public async Task<IActionResult> ListDirectoryContents(
        Guid? folderId,
        CancellationToken cancellationToken = default)
    {
        return NoContent();
    }

    [HttpGet("documents/{documentId:guid}")]
    public async Task<IActionResult> GetDocument(
        Guid? documentId,
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