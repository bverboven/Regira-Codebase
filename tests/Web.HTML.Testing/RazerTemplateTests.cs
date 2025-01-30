using NUnit.Framework.Legacy;
using Regira.Utilities;
using Regira.Web.HTML.Abstractions;
using Regira.Web.HTML.RazorLight;
using Regira.Web.Utilities;
using Web.HTML.Testing.Models;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace Web.HTML.Testing;

/* !Important!
To prevent errors, add the following lines to the cproj file:

  <PropertyGroup>
    <PreserveCompilationContext>true</PreserveCompilationContext>
  </PropertyGroup>
*/
[TestFixture]
[Parallelizable(ParallelScope.All)]
public class RazerTemplateTests
{
    private readonly string _assetsDir;
    private readonly IHtmlParser _parser;
    public RazerTemplateTests()
    {
        var assemblyDir = AssemblyUtility.GetAssemblyDirectory()!;
        _assetsDir = Path.Combine(assemblyDir, "../../../", "Assets");
        _parser = new RazorTemplateParser();
        Directory.CreateDirectory(Path.Combine(_assetsDir, "Output"));
    }

    [Test]
    public void Simple_Razor_Template()
    {
        var inputHtml = File.ReadAllText(Path.Combine(_assetsDir, "Input", "simple-razor.cshtml"));
        var model = "test";
        var parsedHtml = _parser.Parse(inputHtml, model).Result;

        ClassicAssert.IsNotNull(parsedHtml);
        ClassicAssert.AreNotEqual(inputHtml, parsedHtml);
        Assert.That(parsedHtml.Contains($"<p>Hello {model}!</p>"), Is.True);

        File.WriteAllText(Path.Combine(_assetsDir, "Output", "simple-razor.html"), parsedHtml);
    }

    [Test]
    public void Razor_Order_Model()
    {
        var logoBytes = File.ReadAllBytes(Path.Combine(_assetsDir, "Input", "regira-logo.png"));
        var logoBase64 = UriUtility.ToBase64ImageUrl(logoBytes);
        var inputHtml = File.ReadAllText(Path.Combine(_assetsDir, "Input", "razor-order.cshtml"));
        var model = new Order
        {
            Title = "Order #1",
            Created = DateTime.Now,
            ImgBase64 = logoBase64,
            OrderLines =
            [
                new OrderLine{ Title = "Item #1", Amount = 2, Price = 10 },
                new OrderLine{ Title = "Item #2", Amount = 1, Price = 25 },
                new OrderLine{ Title = "Item #3", Amount = 3, Price = 7.5m }
            ]
        };
        var parsedHtml = _parser.Parse(inputHtml, model).Result;

        ClassicAssert.IsNotNull(parsedHtml);
        ClassicAssert.AreNotEqual(inputHtml, parsedHtml);
        Assert.That(parsedHtml.Contains($"<p>Order: {model.Title}</p>"), Is.True);
        foreach (var orderline in model.OrderLines)
        {
            Assert.That(parsedHtml.Contains($"<li>{orderline.Title}: {orderline.Amount} x {orderline.Price}</li>"), Is.True);
        }

        File.WriteAllText(Path.Combine(_assetsDir, "Output", "razor-order.html"), parsedHtml);
    }
}