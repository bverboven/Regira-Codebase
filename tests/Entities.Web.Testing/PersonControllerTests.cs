using Entities.TestApi;
using Entities.TestApi.Infrastructure;
using Entities.TestApi.Infrastructure.Courses;
using Entities.TestApi.Infrastructure.Departments;
using Entities.TestApi.Infrastructure.Persons;
using Entities.Web.Testing.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Web.Models;
using Regira.Utilities;
using System.Net;
using System.Net.Http.Json;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace Entities.Web.Testing;

[Collection(nameof(NonParallelCollectionDefinition))]
public class PersonControllerTests : IDisposable
{
    private readonly ContosoContext _dbContext;
    public PersonControllerTests()
    {
        _dbContext = new ContosoContext(new DbContextOptionsBuilder<ContosoContext>().UseSqlite(ApiConfiguration.ConnectionString).Options);
        _dbContext.Database.EnsureCreated();
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
        Assert.Empty(result.Items);
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

        await CreateTestItems(_dbContext);

        var personInput = new PersonInputDto
        {
            GivenName = "Homer",
            LastName = "Simpson"
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
        Assert.Equal(personInput.GivenName, detailsResult.Item.GivenName);
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
    public async Task Insert_And_Force_404()
    {
        var app = new WebApplicationFactory<Program>();
        using var client = app.CreateClient();

        var personInput = new Person
        {
            GivenName = "John",
            LastName = "Doe"
        };
        var inputResponse = await client.PostAsJsonAsync("/persons", personInput);
        Assert.Equal(HttpStatusCode.OK, inputResponse.StatusCode);

        var detailsResponse99 = await client.GetAsync("/persons/99");
        Assert.False(detailsResponse99.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, detailsResponse99.StatusCode);

        var detailsResponse0 = await client.GetAsync("/persons/0");
        Assert.False(detailsResponse0.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, detailsResponse0.StatusCode);
    }
    [Fact]
    public async Task Update()
    {
        var app = new WebApplicationFactory<Program>();
        using var client = app.CreateClient();

        var persons = await CreateTestItems(_dbContext);
        var personToUpdate = persons[1];

        var originalName = personToUpdate.GivenName;
        personToUpdate.GivenName = "Jeanne";
        personToUpdate.Description = "Testing an update";
        var updateResponse = await client.PutAsJsonAsync($"/persons/{personToUpdate.Id}", personToUpdate);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updateResult = await updateResponse.Content.ReadFromJsonAsync<SaveResult<PersonDto>>();

        Assert.NotEqual(originalName, updateResult!.Item.GivenName);
        Assert.Equal(personToUpdate.GivenName, updateResult.Item.GivenName);
        Assert.Equal(personToUpdate.Description, updateResult.Item.Description);
    }
    [Fact]
    public async Task Delete()
    {
        var app = new WebApplicationFactory<Program>();
        using var client = app.CreateClient();

        var persons = await CreateTestItems(_dbContext);
        var personToDelete = persons[2];

        var deleteResponse = await client.DeleteAsync($"/persons/{personToDelete.Id}");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        var deleteResult = await deleteResponse.Content.ReadFromJsonAsync<DeleteResult<PersonDto>>();

        Assert.Equal(personToDelete.Id, deleteResult!.Item.Id);

        var detailsResponse = await client.GetAsync($"/persons/{personToDelete.Id}");
        Assert.Equal(HttpStatusCode.NotFound, detailsResponse.StatusCode);
    }

    [Fact]
    public async Task Filter_Partial_SearchObject()
    {
        var app = new WebApplicationFactory<Program>();
        using var client = app.CreateClient();

        await client.PostAsync("/test-data", new StringContent(""));

        var coursesResponse = await client.GetAsync("/courses");
        var coursesResult = await coursesResponse.Content.ReadFromJsonAsync<ListResult<CourseDto>>();

        var response = await coursesResponse.Content.ReadAsStringAsync();

        var course1 = coursesResult!.Items[0];
        var course2 = coursesResult.Items[1];
        var course3 = coursesResult.Items[2];
        var course4 = coursesResult.Items[3];

        var persons = new[]
        {
            new Student
            {
                GivenName = "Student1",
                LastName = "One",
                Enrollments =
                [
                    new Enrollment { CourseId = course1.Id },
                    new Enrollment { CourseId = course2.Id }
                ],
            },
            new Student
            {
                GivenName = "Student2",
                LastName = "Two",
                Enrollments =
                [
                    new Enrollment { CourseId = course2.Id },
                    new Enrollment { CourseId = course4.Id }
                ],
            },
            new Person
            {
                GivenName = "Person1",
                LastName = "One",
            },
            new Student
            {
                GivenName = "Student3",
                LastName = "Three",
                Enrollments =
                [
                    new Enrollment { CourseId = course1.Id },
                    new Enrollment { CourseId = course3.Id }
                ],
            }
        };
        _dbContext.Persons.AddRange(persons);
        await _dbContext.SaveChangesAsync();

        var so = new PersonSearchObject
        {
            StudentCourseIds = [1, 3]
        };

        var query = string.Join('&', so.StudentCourseIds.Select(id => $"StudentCourseIds={id}"));
        var listResponse = await client.GetAsync($"/persons/?{query}");
        var listResult = await listResponse.Content.ReadFromJsonAsync<ListResult<PersonDto>>();
        Assert.Equal(2, listResult!.Items.Count);
        Assert.Contains(listResult.Items, p => new[] { "Student1", "Student3" }.Contains(p.GivenName));
    }

    [Fact]
    public async Task Filter_NormalizedContent()
    {
        var app = new WebApplicationFactory<Program>();
        using var client = app.CreateClient();

        await client.PostAsync("/test-data", new StringContent(""));

        var coursesResponse = await client.GetAsync("/courses");
        var coursesResult = await coursesResponse.Content.ReadFromJsonAsync<ListResult<CourseDto>>();

        var course1 = coursesResult!.Items[0];
        var course2 = coursesResult.Items[1];
        var course3 = coursesResult.Items[2];
        var course4 = coursesResult.Items[3];

        var persons = new[]
        {
            new Student
            {
                GivenName = "Student1",
                LastName = "One",
                NormalizedContent = "A fool thinks himself to be wise, but a wise man knows himself to be a fool".ToUpper(),
                Enrollments =
                [
                    new Enrollment { CourseId = course1.Id },
                    new Enrollment { CourseId = course2.Id }
                ],
            },
            new Student
            {
                GivenName = "Student2",
                LastName = "Two",
                NormalizedContent = null,
                Enrollments =
                [
                    new Enrollment { CourseId = course2.Id },
                    new Enrollment { CourseId = course4.Id }
                ],
            },
            new Person
            {
                GivenName = "Person1",
                LastName = "One",
                NormalizedContent = "A smart man makes a mistake, learns from it, and never makes that mistake again. But a wise man finds a smart man and learns from him how to avoid the mistake altogether.".ToUpper()
            },
            new Student
            {
                GivenName = "Student3",
                LastName = "Three",
                NormalizedContent = "A wise man can learn more from a foolish question than a fool can learn from a wise answer".ToUpper(),
                Enrollments =
                [
                    new Enrollment { CourseId = course1.Id },
                    new Enrollment { CourseId = course3.Id }
                ],
            }
        };
        _dbContext.Persons.AddRange(persons);
        await _dbContext.SaveChangesAsync();

        var listResponse = await client.GetAsync("/persons/?q=wise fool");
        var listResult = await listResponse.Content.ReadFromJsonAsync<ListResult<PersonDto>>();
        Assert.Equal(2, listResult!.Items.Count);
        Assert.Contains(listResult.Items, p => new[] { "Student1", "Student3" }.Contains(p.GivenName));
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

    [Fact]
    public async Task OrderBy_IdDesc()
    {
        var app = new WebApplicationFactory<Program>();
        using var client = app.CreateClient();

        var inputItems = await CreateTestItems(_dbContext);

        var listResponse = await client.GetAsync("/persons?sortBy=IdDesc");
        var listResult = await listResponse.Content.ReadFromJsonAsync<ListResult<PersonDto>>();
        var resultCount = listResult!.Items.Count;
        Assert.Equal(inputItems.Count, resultCount);
        for (var i = 0; i < inputItems.Count; i++)
        {
            Assert.Equal(inputItems[i].Id, listResult.Items[resultCount - i - 1].Id);
        }
    }

    static async Task<IList<Person>> CreateTestItems(ContosoContext dbContext)
    {
        var items = new Person[]
        {
            new () { GivenName = "Jane", LastName = "Doe" },
            new () { GivenName = "John", LastName = "Doe" },
            new () { GivenName = "Bart", LastName = "Simpson" }
        };
        dbContext.Persons.AddRange(items);
        await dbContext.SaveChangesAsync();

        return items;
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
    }
}