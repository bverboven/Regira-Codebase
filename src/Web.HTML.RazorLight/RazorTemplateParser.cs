using RazorLight;
using Regira.Web.HTML.Abstractions;

namespace Regira.Web.HTML.RazorLight;

public class RazorTemplateParser : IHtmlParser
{
    public class Options
    {
        public string? TemplateKey { get; set; }
    }

    private readonly Options? _options;
    public RazorTemplateParser(Options? options = null)
    {
        _options = options;
    }

    public async Task<string> Parse<T>(string html, T model)
    {
        var engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(GetType())
            .UseMemoryCachingProvider()
            .Build();
        var templateKey = _options?.TemplateKey ?? Guid.NewGuid().ToString();

        return await engine.CompileRenderStringAsync(templateKey, html, model);
    }
}