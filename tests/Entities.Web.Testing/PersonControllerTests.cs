using Entities.TestApi.Infrastructure;
using Entities.TestApi.Models;
using Entities.Web.Testing.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Web.Models;
using Regira.Utilities;
using System.Net;
using System.Net.Http.Json;
using Entities.TestApi;
using Testing.Library.Data;

namespace Entities.Web.Testing;

[Collection("Non-Parallel Collection")]
public class PersonControllerTests : IDisposable
{
    private readonly ContosoContext _dbContext;
    public PersonControllerTests()
    {
        _dbContext = new ContosoContext(new DbContextOptionsBuilder<ContosoContext>().UseSqlite(ApiConfiguration.ConnectionString).Options);
        _dbContext.Database.EnsureCreated();

        _dbContext.SaveChanges();
    }


    [Fact]
    public async Task Empty_Get()
    {
        var app = new WebApplicationFactory<Program>();
        using var client = app.CreateClient();
        var response = await client.GetAsync("/persons");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ListResult<PersonDto>>();
        Assert.NotNull(result!.Items);
        Assert.Equal(0, result.Items.Count);
    }
    [Fact]
    public async Task Get_404()
    {
        var app = new WebApplicationFactory<Program>();
        using var client = app.CreateClient();
        var personInput = new PersonInputDto
        {
            GivenName = "John",
            LastName = "Doe"
        };
        var inputResponse = await client.PostAsJsonAsync("/persons", personInput);
        inputResponse.EnsureSuccessStatusCode();

        var response1 = await client.GetAsync("/persons/1");
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        var response2 = await client.GetAsync("/persons/2");
        Assert.Equal(HttpStatusCode.NotFound, response2.StatusCode);
        var response3 = await client.GetAsync("/persons/new");
        Assert.Equal(HttpStatusCode.BadRequest, response3.StatusCode);
    }
    [Fact]
    public async Task Insert_And_Get_Details()
    {
        var app = new WebApplicationFactory<Program>();
        using var client = app.CreateClient();
        var personInput = new PersonInputDto
        {
            GivenName = "John",
            LastName = "Doe"
        };
        var inputResponse = await client.PostAsJsonAsync("/persons", personInput);

        Assert.Equal(HttpStatusCode.OK, inputResponse.StatusCode);

        var saveResult = await inputResponse.Content.ReadFromJsonAsync<SaveResult<PersonDto>>();

        Assert.NotNull(saveResult);
        Assert.NotNull(saveResult.Item);
        Assert.True(saveResult.IsNew);
        Assert.Equal(personInput.GivenName, saveResult.Item.GivenName);
        Assert.Equal(personInput.LastName, saveResult.Item.LastName);

        var detailsResponse = await client.GetAsync($"/persons/{saveResult.Item.Id}");
        var detailsResult = await detailsResponse.Content.ReadFromJsonAsync<DetailsResult<PersonDto>>();
        Assert.NotNull(detailsResult!.Item);
        Assert.NotEqual(0, detailsResult.Item.Id);
        Assert.Equal(saveResult.Item.Id, detailsResult.Item.Id);
    }
    [Fact]
    public async Task Insert_And_Get_List()
    {
        var app = new WebApplicationFactory<Program>();
        using var client = app.CreateClient();

        var inputPersons = PersonNames.EN
            .Select(name => new PersonInputDto
            {
                GivenName = name.GivenName,
                LastName = name.FamilyName
            })
            .ToArray();
        foreach (var personInput in inputPersons)
        {
            var inputResponse = await client.PostAsJsonAsync("/persons", personInput);
            Assert.Equal(HttpStatusCode.OK, inputResponse.StatusCode);
            var saveResult = await inputResponse.Content.ReadFromJsonAsync<SaveResult<PersonDto>>();
            personInput.Id = saveResult!.Item.Id;
        }

        var listResponse = await client.GetAsync("/persons");
        var listResult = await listResponse.Content.ReadFromJsonAsync<ListResult<PersonDto>>();
        Assert.Equal(inputPersons.Length, listResult!.Items.Count);
        foreach (var personInput in inputPersons)
        {
            var person = listResult.Items.First(x => x.Id == personInput.Id);
            Assert.Equal(personInput.GivenName, person.GivenName);
            Assert.Equal(personInput.LastName, person.LastName);
        }
    }
    [Fact]
    public async Task Update()
    {
        var app = new WebApplicationFactory<Program>();
        using var client = app.CreateClient();
        var personInput = new PersonInputDto
        {
            GivenName = "John",
            LastName = "Doe"
        };
        var insertResponse = await client.PostAsJsonAsync("/persons", personInput);
        Assert.Equal(HttpStatusCode.OK, insertResponse.StatusCode);

        var insertResult = await insertResponse.Content.ReadFromJsonAsync<SaveResult<PersonDto>>();
        personInput.Id = insertResult!.Item.Id;

        personInput.GivenName = "Jane";
        personInput.Description = "Testing an update";
        var updateResponse = await client.PutAsJsonAsync($"/persons/{personInput.Id}", personInput);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updateResult = await updateResponse.Content.ReadFromJsonAsync<SaveResult<PersonDto>>();

        Assert.NotEqual(insertResult.Item.GivenName, updateResult!.Item.GivenName);
        Assert.Equal(personInput.Description, updateResult.Item.Description);
    }
    [Fact]
    public async Task Delete()
    {
        var app = new WebApplicationFactory<Program>();
        using var client = app.CreateClient();
        var personInput = new PersonInputDto
        {
            GivenName = "John",
            LastName = "Doe"
        };
        var insertResponse = await client.PostAsJsonAsync("/persons", personInput);
        Assert.Equal(HttpStatusCode.OK, insertResponse.StatusCode);

        var insertResult = await insertResponse.Content.ReadFromJsonAsync<SaveResult<PersonDto>>();
        personInput.Id = insertResult!.Item.Id;

        var deleteResponse = await client.DeleteAsync($"/persons/{personInput.Id}");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        var deleteResult = await deleteResponse.Content.ReadFromJsonAsync<DeleteResult<PersonDto>>();

        Assert.Equal(personInput.Id, deleteResult!.Item.Id);

        var detailsResponse = await client.GetAsync($"/persons/{personInput.Id}");
        Assert.Equal(HttpStatusCode.NotFound, detailsResponse.StatusCode);
    }

    [Fact]
    public async Task ModifyDepartmentCollection()
    {
        var app = new WebApplicationFactory<Program>();
        using var client = app.CreateClient();
        var personInput = new PersonInputDto
        {
            GivenName = "John",
            LastName = "Doe",
            Departments = new List<DepartmentInputDto>
            {
                new() { Title = "Department #1" },
                new() { Title = "Department #2" }
            }
        };
        var insertResponse = await client.PostAsJsonAsync("/persons", personInput);
        Assert.Equal(HttpStatusCode.OK, insertResponse.StatusCode);

        var insertResult = await insertResponse.Content.ReadFromJsonAsync<SaveResult<PersonDto>>();
        personInput.Id = insertResult!.Item.Id;

        personInput.Description = "Updated";
        personInput.Departments.Remove(personInput.Departments.Last());
        personInput.Departments.Add(new DepartmentInputDto { Title = "Department #3" });

        var updateResponse = await client.PutAsJsonAsync($"/persons/{personInput.Id}", personInput);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updateResult = await updateResponse.Content.ReadFromJsonAsync<SaveResult<PersonDto>>();

        Assert.Equal(updateResult!.Item.Description, personInput.Description);
        Assert.DoesNotContain(updateResult.Item.Departments!, x => x.Title == "Department #2");
        Assert.Contains(updateResult.Item.Departments!, x => x.Title == "Department #3");
    }

    [Fact]
    public async Task Search()
    {
        var app = new WebApplicationFactory<Program>();
        using var client = app.CreateClient();

        // create dummy data
        var inputPersons = PersonNames.EN
            .Shuffle()
            .Select((name, i) => new PersonInputDto
            {
                GivenName = name.GivenName,
                LastName = name.FamilyName,
                Phone = (600_000_000 + i * 10_000).ToString(),
            })
            .ToArray();
        // set Id for dummy data
        foreach (var personInput in inputPersons)
        {
            var inputResponse = await client.PostAsJsonAsync("/persons", personInput);
            var saveResult = await inputResponse.Content.ReadFromJsonAsync<SaveResult<PersonDto>>();
            personInput.Id = saveResult!.Item.Id;
        }

        // searchParams
        var lastNameShouldStartWith = "no";
        var pageSize = 5;

        // calculate expected
        var expectedList = inputPersons
            .Where(x => x.LastName?.StartsWith(lastNameShouldStartWith, StringComparison.InvariantCultureIgnoreCase) == true)
            .OrderBy(x => x.GivenName?.ToUpperInvariant())
            .ToArray();

        // search
        var searchResponse = await client.GetAsync($"/persons/search?lastname={lastNameShouldStartWith}*&pagesize={pageSize}&sortby=givenname");
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<SearchResult<PersonDto>>();
        var resultItems = searchResult!.Items.ToArray();

        // test resultCount to expectedCount
        Assert.Equal(expectedList.Length, searchResult.Count);
        // test resultCount to pageSize
        Assert.Equal(Math.Min(expectedList.Length, pageSize), resultItems.Length);
        // all resultItems are present in expectedList
        Assert.True(resultItems.All(r => expectedList.Any(x => x.Id == r.Id)));
        // check sort order
        for (var i = 0; i < resultItems.Length; i++)
        {
            Assert.Equal(expectedList[i].Id, resultItems[i].Id);
            Assert.Equal(expectedList[i].GivenName, resultItems[i].GivenName);
        }
    }


    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
    }
}