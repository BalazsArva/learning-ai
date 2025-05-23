using System.Web;

namespace LearningAI.Api.Utilities;

public class UriProvider(IHttpContextAccessor httpContextAccessor) : IUriProvider
{
    public string GetUriForKnowledgebaseDocumentByTitle(string title)
    {
        var request = httpContextAccessor.HttpContext!.Request;
        var encodedTitle = HttpUtility.UrlEncode(title);

        return $"{request.Scheme}://{request.Host}/api/knowledgebase/documents/{encodedTitle}";
    }
}