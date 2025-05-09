using Microsoft.AspNetCore.Mvc;

namespace LearningAI.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class DemoController(ILogger<DemoController> logger) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return NoContent();
    }
}