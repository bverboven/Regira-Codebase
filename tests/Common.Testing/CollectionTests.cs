﻿using Common.Testing.Models;
using NUnit.Framework.Legacy;
using Regira.Utilities;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace Common.Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class CollectionTests
{
    private readonly List<Person> _persons;
    public CollectionTests()
    {
        _persons = new List<Person>
        {
            new Person
            {
                Id = 1,
                Name = "Lutgard Wittocx",
                PersonType = PersonType.Parent,
                Weight = 65,
                BirthDate = new DateTime(1956, 3, 7)
            },
            new Person
            {
                Id = 2,
                Name = "Jan Verboven",
                PersonType = PersonType.Parent,
                Weight = 60,
                BirthDate = new DateTime(1954, 5, 8)
            },
            new Person
            {
                Id = 3,
                Name = "Bram Verboven",
                PersonType = PersonType.Unknown,
                Weight = 75,
                BirthDate = new DateTime(1979, 5, 6),
                Spouse = new Person {Id = 6, Name = "Ursule Muleka"}
            },
            new Person
            {
                Id = 4,
                Name = "Dries Verboven",
                PersonType = PersonType.Brother,
                Weight = 80,
                BirthDate = new DateTime(1981, 10, 30)
            },
            new Person
            {
                Id = 5,
                Name = "Simon Verboven",
                PersonType = PersonType.Brother,
                Weight = 70,
                BirthDate = new DateTime(1983, 3, 25)
            },
            new Person
            {
                Id = 6,
                Name = "Ursule Muleka",
                PersonType = PersonType.Spouse,
                Weight = 55,
                BirthDate = new DateTime(1974, 8, 11),
                Spouse = new Person {Id = 3, Name = "Bram Verboven"}
            }
        };
    }

    [Test]
    public void Filter_Non_Existing_Prop()
    {
        var actual = _persons.FilterItems(new { nonExisting = 123 });
        CollectionAssert.IsEmpty(actual);
    }
    [Test]
    public void Filter_On_Enum()
    {
        var expected = _persons.Where(x => x.PersonType == PersonType.Brother);
        var actual = _persons.FilterItems(new { personType = PersonType.Brother });
        CollectionAssert.AreEquivalent(expected, actual);
    }
    [Test]
    public void Filter_On_EnumInt()
    {
        var expected = _persons.Where(x => x.PersonType == PersonType.Brother);
        var actual = _persons.FilterItems(new { personType = (int)PersonType.Brother });
        CollectionAssert.AreEquivalent(expected, actual);
    }
    [Test]
    public void Filter_On_EnumString()
    {
        var expected = _persons.Where(x => x.PersonType == PersonType.Brother);
        var actual = _persons.FilterItems(new { personType = "brother" });
        CollectionAssert.AreEquivalent(expected, actual);
    }
    [Test]
    public void Filter_On_Weight()
    {
        var expected = _persons.Where(x => x.Weight == 55);
        var actual = _persons.FilterItems(new { weight = 55 });
        CollectionAssert.AreEquivalent(expected, actual);
    }
    [Test]
    public void Filter_On_MinWeight()
    {
        var expected = _persons.Where(x => x.Weight >= 65);
        var actual = _persons.FilterItems(new { MinWeight = 65 });
        CollectionAssert.AreEquivalent(expected, actual);
    }
    [Test]
    public void Filter_On_MaxWeight()
    {
        var expected = _persons.Where(x => x.Weight <= 70);
        var actual = _persons.FilterItems(new { MaxWeight = 70 });
        CollectionAssert.AreEquivalent(expected, actual);
    }
    [Test]
    public void Filter_On_StringContains()
    {
        var expected = _persons.Where(x => x.Name?.Contains("ver", StringComparison.InvariantCultureIgnoreCase) == true);
        var actual = _persons.FilterItems(new { Name = "*ver*" });
        CollectionAssert.AreEquivalent(expected, actual);
    }
    [Test]
    public void Filter_On_StringStartsWith()
    {
        var expected = _persons.Where(x => x.Name?.StartsWith("jan", StringComparison.InvariantCultureIgnoreCase) == true);
        var actual = _persons.FilterItems(new { Name = "jan*" });
        CollectionAssert.AreEquivalent(expected, actual);
    }
    [Test]
    public void Filter_On_StringEndsWith()
    {
        var expected = _persons.Where(x => x.Name?.EndsWith("boven", StringComparison.InvariantCultureIgnoreCase) == true);
        var actual = _persons.FilterItems(new { Name = "*boven" });
        CollectionAssert.AreEquivalent(expected, actual);
    }
    [Test]
    public void Filter_On_Nested_Prop()
    {
        var expected = _persons.Where(x => x.Spouse?.Id == 3);
        var so = new { spouse = new { id = 3 } };
        var actual = _persons.FilterItems(so);
        CollectionAssert.AreEquivalent(expected, actual);
    }
    [Test]
    public void Filter_Combined()
    {
        var expected = _persons
            .Where(x => x.Name?.Contains("ver", StringComparison.InvariantCultureIgnoreCase) == true)
            .Where(x => x.Weight >= 65)
            .Where(x => x.Spouse?.Id == 6);
        var so = new
        {
            Name = "*ver*",
            MinWeight = 65,
            Spouse = new { id = 6 }
        };
        var actual = _persons.FilterItems(so);
        CollectionAssert.AreEquivalent(expected, actual);
    }

    [Test]
    public async Task Execute_List_In_Chunks()
    {
        var testItems = Enumerable.Range(0, 1000).Select(_ => Guid.NewGuid().ToString()).ToList();
        var tempFile = Path.GetTempFileName();
        await File.WriteAllLinesAsync(tempFile, testItems.Shuffle());
        var indexDic = await testItems.ChunkActionsAsync(async x =>
        {
            var lines = await File.ReadAllLinesAsync(tempFile);
            return new
            {
                value = x,
                index = Array.IndexOf(lines, x.ToString())
            };
        });

        ClassicAssert.AreEqual(testItems.Count, indexDic.Count);

        var fileLines = await File.ReadAllLinesAsync(tempFile);
        foreach (var kvp in indexDic)
        {
            var expected = Array.IndexOf(fileLines, kvp.value);
            ClassicAssert.AreEqual(expected, kvp.index);
        }
        File.Delete(tempFile);
    }
}