using RazorLight;
using Regira.Web.HTML.Abstractions;

namespace Regira.Web.HTML.RazorLight;

public class RazorTemplateParser(RazorTemplateParser.Options? options = null) : IHtmlParser
{
    public class Options
    {
        public string? TemplateKey { get; set; }
    }

    public async Task<string> Parse<T>(string html, T model)
    {
        var engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(GetType())
            .UseMemoryCachingProvider()
            .Build();
        var templateKey = options?.TemplateKey ?? Guid.NewGuid().ToString();

        return await engine.CompileRenderStringAsync(templateKey, html, model);
    }
}