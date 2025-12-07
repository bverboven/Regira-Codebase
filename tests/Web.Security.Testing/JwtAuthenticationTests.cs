using Duende.IdentityModel.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using Web.Security.Testing.Infrastructure;
using Web.Security.Testing.Infrastructure.Jwt;
using Xunit;

namespace Web.Security.Testing;

public class JwtAuthenticationTests : IClassFixture<TestingWebApplicationFactory<JwtStartup>>
{
    private readonly WebApplicationFactory<JwtStartup> _factory;
    public JwtAuthenticationTests(TestingWebApplicationFactory<JwtStartup> factory)
    {
        _factory = factory
            // otherwise throws DirectoryNotFound Exception
            .WithWebHostBuilder(builder => builder.UseSolutionRelativeContentRoot("tests"));
    }

    [Fact]
    public async Task Test_AllowAnonymous_Welcome()
    {
        var httpClient = _factory.CreateClient();

        var response = await httpClient.GetAsync("");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    [Fact]
    public async Task Test_Protected_Without_Token_Returns_Unauthorized()
    {
        var httpClient = _factory.CreateClient();

        var response = await httpClient.GetAsync("protected");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
    [Fact]
    public async Task Test_Protected_With_Valid_Token_Returns_Ok()
    {
        var httpClient = _factory.CreateClient();
        var token = await CreateToken();
        httpClient.SetBearerToken(token);

        var response = await httpClient.GetAsync("protected");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    [Fact]
    public async Task Test_Protected_With_Invalid_Token_Returns_Unauthorized()
    {
        var httpClient = _factory.CreateClient();
        var token = await CreateToken();
        httpClient.SetBearerToken(token);

        // wait 2.5 sec to invalidate the token
        await Task.Delay(2500);

        var response = await httpClient.GetAsync("protected");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    async Task<string> CreateToken()
    {
        var httpClient = _factory.CreateClient();
        var user = JwtUsers.Value.First();
        var response = await httpClient.PostAsJsonAsync("auth", new { username = user.Name });
        var tokenResult = await response.Content.ReadFromJsonAsync<TokenResult>();
        return tokenResult!.Token!;
    }
}