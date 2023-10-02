using Regira.Utilities;
using System.Text.Json;

namespace Common.Testing;

struct Country
{
    public string Code { get; set; }
    public string Title { get; set; }
    public DateTime? Created { get; set; }
    public float LifeExpectancy { get; set; }
    public int Population { get; set; }
    public string CallingCode { get; set; }
}
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class DictionaryUtilityTests
{
    private object? _plainObj;
    private object? _objWithArray;
    private object? _nestedObj;
    private readonly IList<Country> _countries;
    public DictionaryUtilityTests()
    {
        var assetsDir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "../../../assets")).FullName;
        var countriesJson = File.ReadAllText(Path.Combine(assetsDir, "countries.json"));
        _countries = JsonSerializer.Deserialize<IList<Country>>(countriesJson)!;
    }

    [SetUp]
    public void Setup()
    {
        _plainObj = new
        {
            Id = 1,
            Title = "Test object",
            Value = (string?)null,
            Created = new DateTime(2012, 10, 21)
        };
        _objWithArray = new
        {
            Values = new[] { 1, 2, 3 }
        };
        _nestedObj = new
        {
            Id = 3,
            Title = "Test object",
            Value = (string?)null,
            Created = new DateTime(2012, 10, 21),
            Parent = new
            {
                Id = 1,
                Title = "Parent object",
                Parent = new
                {
                    Id = 1,
                    Title = "Root object"
                },
                Collection = new[] {
                    new {
                        Id = "1.1",
                        Title = "Nested collection item #1"
                    }
                }
            }
        };
    }

    #region ToDictonary
    [Test]
    public void Args_To_Dictionary()
    {
        var args = new[]
        {
            "-input", "input directory/file.sql",
            "-p",
            "-o", "output",
            "ignore"
        };
        var dic = args.ToDictionary();
        Assert.That(dic["input"], Is.EqualTo("input directory/file.sql"));
        Assert.That(dic["p"], Is.Null);
        Assert.That(dic["o"], Is.EqualTo("output"));
        Assert.That(dic.ContainsKey("ignore"), Is.False);
        Assert.That(dic.Values.Contains("ignore"), Is.False);
    }
    [Test]
    public void Plain_Object_To_Dictionary()
    {
        dynamic obj = _plainObj;
        var dic = DictionaryUtility.ToDictionary(obj);
        Assert.IsNotEmpty(dic);
        Assert.AreEqual(obj.Id, dic["Id"]);
        Assert.AreEqual(obj.Title, dic["Title"]);
        Assert.AreEqual(obj.Created, dic["Created"]);
    }
    [Test]
    public void Plain_Object_To_Dictionary_With_CaseInsensitive()
    {
        dynamic obj = _plainObj;
        var dic = DictionaryUtility.ToDictionary(obj);
        var keys = ((ICollection<string>)dic.Keys).ToArray(); // use ToArray to remove case insensitivity on key collection
        Assert.IsTrue(keys.Contains("Id"));
        Assert.IsFalse(keys.Contains("id"));
        Assert.AreEqual(obj.Id, dic["id"]);
        Assert.AreEqual(obj.Title, dic["title"]);
        Assert.AreEqual(obj.Created, dic["created"]);
    }
    [Test]
    public void Object_With_Array_To_Dictionary()
    {
        dynamic obj = _objWithArray;
        var dic = DictionaryUtility.ToDictionary(obj);
        Assert.IsNotEmpty(dic["Values"]);
        var src = (int[])obj.Values;
        var result = (IList<object>)dic["Values"];
        CollectionAssert.AreEqual(src, result);
    }
    [Test]
    public void Nested_Object()
    {
        dynamic obj = _nestedObj;
        IDictionary<string, object?> dic = DictionaryUtility.ToDictionary(obj);
        IDictionary<string, object?> parent = (IDictionary<string, object?>)dic["Parent"];
        Assert.IsInstanceOf<IDictionary<string, object?>>(parent);
        Assert.AreEqual(obj.Parent.Id, parent["Id"]);
        Assert.AreEqual(obj.Parent.Title, parent["Title"]);
        IDictionary<string, object?> root = (IDictionary<string, object?>)parent["Parent"];
        Assert.AreEqual(obj.Parent.Parent.Id, root["Id"]);
    }
    #endregion

    #region TableArray tests
    [Test]
    public void Dictionaries_To_TableArray()
    {
        var countries = _countries
            .Select(c => DictionaryUtility.ToDictionary(c))
            .ToList();
        var table = DictionaryUtility.ToTableArray(countries);
        // test headers
        Assert.AreEqual("Code", table[0, 0]);
        Assert.AreEqual("Title", table[0, 1]);
        Assert.AreEqual("Created", table[0, 2]);

        // test values
        var keys = countries[0].Keys.ToArray();
        for (var i = 0; i < countries.Count(); i++)
        {
            for (var j = 0; j < keys.Length; j++)
            {
                var key = keys[j];
                Assert.AreEqual(countries[i][key], table[i + 1, j]);
            }
        }
    }
    [Test]
    public void TableArray_To_Dictionaries()
    {
        var countries = _countries
            .Select(c => DictionaryUtility.ToDictionary(c))
            .ToList();
        var table = DictionaryUtility.ToTableArray(countries);
        var dicList = DictionaryUtility
            .FromTableArray(table)
            .ToArray();

        // test values
        var keys = countries[0].Keys.ToArray();
        Assert.AreEqual("Code", keys[0]);
        Assert.AreEqual("Title", keys[1]);
        Assert.AreEqual("Created", keys[2]);
        for (var i = 0; i < countries.Count(); i++)
        {
            for (var j = 0; j < keys.Length; j++)
            {
                var key = keys[j];
                Assert.AreEqual(table[i + 1, j], dicList[i][key]);
            }
        }
    }
    #endregion

    #region Flatten tests
    [Test]
    public void Flatten_Returns_Dictionary()
    {
        var source = new Dictionary<string, object?>();
        var target = DictionaryUtility.Flatten(source);
        Assert.IsNotNull(target);
        Assert.IsInstanceOf<IDictionary<string, object?>>(target);
    }
    [Test]
    public void Flatten_Simple_Values()
    {
        var source = new Dictionary<string, object?> {
            { "factuurNummer", "0123456" },
            { "factuurDatum", "2019-11-19" }
        };
        var target = DictionaryUtility.Flatten(source);
        Assert.AreEqual(source["factuurNummer"], target["factuurNummer"]);
        Assert.AreEqual(source["factuurDatum"], target["factuurDatum"]);
    }
    [Test]
    public void Flatten_CollectionValues()
    {
        var source = new Dictionary<string, object?> {
            { "factuurLijnen", new[] {
                new Dictionary<string, object?> {
                    { "omschrijving", "Vervoer" },
                    { "eenheidsPrijs", 160.50 }
                },
                new Dictionary<string, object?> {
                    { "omschrijving", "Diensten" },
                    { "eenheidsPrijs", 180.50 }
                }
            } }
        };
        var target = DictionaryUtility.Flatten(source);
        Assert.IsFalse(target.ContainsKey("factuurLijnen"));
        Assert.IsTrue(target.ContainsKey("factuurLijnen.0.omschrijving"));
        var sourceFactuurLijnen = (IDictionary<string, object?>[])source["factuurLijnen"];
        Assert.AreEqual(sourceFactuurLijnen[0]["omschrijving"], target["factuurLijnen.0.omschrijving"]);
    }
    [Test]
    public void Flatten_Ignore_CollectionValues()
    {
        var source = new Dictionary<string, object?> {
            { "factuurLijnen", new[] {
                new Dictionary<string, object?> {
                    { "omschrijving", "Vervoer" },
                    { "eenheidsPrijs", 160.50 }
                },
                new Dictionary<string, object?> {
                    { "omschrijving", "Diensten" },
                    { "eenheidsPrijs", 180.50 }
                }
            } }
        };
        var target = DictionaryUtility.Flatten(source, new FlattenOptions { IgnoreCollections = true });
        Assert.IsTrue(target.ContainsKey("factuurLijnen"));
        Assert.IsNotEmpty((IList<IDictionary<string, object?>>)target["factuurLijnen"]);
    }
    [Test]
    public void Flatten_Nested_CollectionValues()
    {
        var source = new Dictionary<string, object?> {
            { "factuurLijnen", new[] {
                new Dictionary<string, object?> {
                    { "nested", new Dictionary<string,object> {
                        { "first", "1st nested" },
                        { "second", "2nd nested" }
                    } }
                }
            } }
        };
        var target = DictionaryUtility.Flatten(source);
        var sourceFactuurLijnen = (IDictionary<string, object?>[])source["factuurLijnen"];
        var sourceLijn = sourceFactuurLijnen.First();
        var sourceFirstNested = (IDictionary<string, object?>)sourceLijn["nested"];
        Assert.AreEqual(sourceFirstNested["first"], target["factuurLijnen.0.nested.first"]);
    }
    [Test]
    public void Flatten_Nested_Ignore_Collections()
    {
        var source = new Dictionary<string, object?> {
            { "factuurLijnen", new[] {
                new Dictionary<string, object?> {
                    { "nested", new Dictionary<string,object> {
                        { "first", "1st nested" },
                        { "second", "2nd nested" }
                    } }
                }
            } }
        };
        var target = DictionaryUtility.Flatten(source, new FlattenOptions { IgnoreCollections = true });
        var sourceFactuurLijnen = (IDictionary<string, object?>[])source["factuurLijnen"];
        var sourceLijn = sourceFactuurLijnen.First();
        var sourceFirstNested = (IDictionary<string, object?>)sourceLijn["nested"];
        var targetFactuurLijnen = (IList<IDictionary<string, object?>>)target["factuurLijnen"];
        Assert.AreEqual(sourceFirstNested["first"], targetFactuurLijnen[0]["nested.first"]);
    }
    [Test]
    public void Flatten_Removes_Hierarchy()
    {
        var source = new Dictionary<string, object?> {
            { "leverancier", new Dictionary<string,object?> {
                { "code", null },
                { "naam", "LEVERANCIER BVBA" },
                { "telefoon", "003239999999" }
            } }
        };
        // only works for NewtonSoft.Json for now...
        //var json = _serializer.Serialize(source);
        //source = _serializer.Deserialize<Dictionary<string, object>>(json);
        var target = DictionaryUtility.Flatten(source);
        Assert.IsFalse(target.ContainsKey("leverancier"));
        var sourceLeverancier = (IDictionary<string, object?>)source["leverancier"];
        Assert.AreEqual(sourceLeverancier["naam"], target["leverancier.naam"]);
    }
    [Test]
    public void Flatten_Removes_Nested_Hierarchy()
    {
        var source = new Dictionary<string, object?> {
            { "leverancier", new Dictionary<string, object?> {
                { "adres", new Dictionary<string, object?> {
                    { "straat", "Kerkstraat 1" },
                    { "postcode", "2000" },
                    { "gemeente", "Antwerpen" }
                } }
            } }
        };
        var target = DictionaryUtility.Flatten(source);
        var sourceLeverancier = (IDictionary<string, object?>)source["leverancier"];
        Assert.IsTrue(sourceLeverancier.ContainsKey("adres"));
        Assert.IsInstanceOf<IDictionary<string, object?>>(sourceLeverancier["adres"]);
        var sourceLeverancierAdres = (IDictionary<string, object?>)sourceLeverancier["adres"];
        Assert.AreEqual(sourceLeverancierAdres["straat"], target["leverancier.adres.straat"]);
    }
    [Test]
    public void Flatten_Nested_Collections_In_Collections()
    {
        var source = new Dictionary<string, object?> {
            { "factuurLijnen", new[] {
                new Dictionary<string, object?> {
                    { "omschrijving", "Vervoer" },
                    { "eenheidsPrijs", 160.50 }
                },
                new Dictionary<string, object?> {
                    { "nested", new Dictionary<string, object?> {
                        { "first", "1st nested" },
                        { "second", new[] {
                            new Dictionary<string, object?> {
                                { "foo", new Dictionary<string, object?> {
                                    { "value", "fooValue" }
                                } }
                            }
                        } },
                        { "third", "3th nested" }
                    } }
                }
            } }
        };
        var target = DictionaryUtility.Flatten(source);
        var sourceFactuurLijnen = ((IEnumerable<IDictionary<string, object?>>)source["factuurLijnen"]).ToArray();
        var sourceLijn2 = sourceFactuurLijnen[1];
        var sourceNestedProp = (IDictionary<string, object?>)sourceLijn2["nested"];
        Assert.AreEqual(sourceNestedProp["first"], target["factuurLijnen.1.nested.first"]);
        var sourceNestedSecond = ((IEnumerable<IDictionary<string, object?>>)sourceNestedProp["second"]).ToList();
        var sourceNestedSecondFoo = (IDictionary<string, object?>)sourceNestedSecond[0]["foo"];
        Assert.AreEqual(sourceNestedSecondFoo["value"], target["factuurLijnen.1.nested.second.0.foo.value"]);
    }
    [Test]
    public void Flatten_Full_Unflattened_Sample()
    {
        IDictionary<string, object?> source = new Dictionary<string, object?> {
            { "factuurNummer", "0123456" },
            { "factuurDatum", "2019-11-19" },
            { "factuurType", 0 },
            { "vervalDatum", "2019-12-04" },
            { "betalingsVoorwaarden", "Xxx" },
            { "leverancier.rekeningNummer.iban", "BEXXXXXXXXXXXXXX" },
            { "leverancier.rekeningNummer.bic", "KREDBEBB" },
            { "leverancier.kbo", "BEXXXXXXXXXX" },
            { "leverancier.naam", "LEVERANCIER BVBA" },
            { "leverancier.telefoon", "003239999999" },
            { "leverancier.email", "facturatie@xxx.be" },
            { "leverancier.adres.straat", "Kerkstraat 1" },
            { "leverancier.adres.postcode", "2000" },
            { "leverancier.adres.gemeente", "Antwerpen" },
            { "factuurLijnen", new[] {
                    new Dictionary<string, object?> {
                        { "omschrijving", "Vervoer" },
                        { "eenheidsPrijs", 160.50 },
                        { "nested.first", "1st nested" },
                        { "nested.second", "2nd nested" }
                    },
                    new Dictionary<string, object?> {
                        { "omschrijving", "Diensten" },
                        { "eenheidsPrijs", 180.50 },
                        { "nested.first", "1st nested" },
                        { "nested.second", new[] {
                            new Dictionary<string, object?> {
                                { "foo.value", "fooValue" }
                            }
                        } },
                        { "nested.third", "3th nested" }
                    }
                }
            }
        };
        source = DictionaryUtility.Unflatten(source);
        var target = DictionaryUtility.Flatten(source);

        Assert.AreEqual(source["factuurNummer"], target["factuurNummer"]);
        Assert.AreEqual(source["factuurDatum"], target["factuurDatum"]);

        var sourceFactuurLijnen = (IList<IDictionary<string, object?>>)source["factuurLijnen"];
        var sourceLijn = sourceFactuurLijnen.First();
        var sourceFirstNested = (IDictionary<string, object?>)sourceLijn["nested"];
        Assert.AreEqual(sourceFirstNested["first"], target["factuurLijnen.0.nested.first"]);

        var sourceLeverancier = (IDictionary<string, object?>)source["leverancier"];
        Assert.IsTrue(sourceLeverancier.ContainsKey("adres"));
        Assert.IsInstanceOf<IDictionary<string, object?>>(sourceLeverancier["adres"]);
        var sourceLeverancierAdres = (IDictionary<string, object?>)sourceLeverancier["adres"];
        Assert.AreEqual(sourceLeverancierAdres["straat"], target["leverancier.adres.straat"]);

        var sourceLijn2 = sourceFactuurLijnen[1];
        var sourceNestedProp = (IDictionary<string, object?>)sourceLijn2["nested"];
        Assert.AreEqual(sourceNestedProp["first"], target["factuurLijnen.1.nested.first"]);
        var sourceNestedSecond = ((IEnumerable<IDictionary<string, object?>>)sourceNestedProp["second"]).ToList();
        var sourceNestedSecondFoo = (IDictionary<string, object?>)sourceNestedSecond[0]["foo"];
        Assert.AreEqual(sourceNestedSecondFoo["value"], target["factuurLijnen.1.nested.second.0.foo.value"]);
    }
    //        [Test]
    //        public void Flatten_Json_Sample()
    //        {
    //            var json = @"{
    //  ""factuurNummer"": ""0123456"",
    //  ""leverancier"": {
    //    ""naam"": ""Company X"",
    //    ""adres"": {
    //      ""straat"": ""Straat 111"",
    //      ""gemeente"": ""Antwerpen""
    //    }
    //  },
    //  ""factuurLijnen"": [
    //    {
    //      ""volgnummer"": 1,
    //      ""eenheidsPrijs"": 160.5,
    //      ""btw"": {
    //        ""tarief"": 0
    //      }
    //    },
    //    {
    //      ""volgnummer"": 2,
    //      ""eenheidsPrijs"": 180.5,
    //      ""btw"": {
    //        ""tarief"": 0
    //      }
    //    }
    //  ]
    //}";
    //            // only works for NewtonSoft.Json for now...
    //            var source = JsonSerializer.Deserialize<IDictionary<string, object?>>(json)!;
    //            var target = DictionaryUtility.Flatten(source);
    //            //var flatJson = JsonSerializer.Serialize(target);
    //            Assert.AreEqual(source["factuurNummer"], target["factuurNummer"]);
    //            var leverancier = (IDictionary<string, object?>)source["leverancier"];
    //            var adres = (IDictionary<string, object?>)leverancier["adres"];
    //            Assert.AreEqual(adres["straat"], target["leverancier.adres.straat"]);
    //            var factuurLijnen = (IList<IDictionary<string, object?>>)source["factuurLijnen"];
    //            Assert.AreEqual(factuurLijnen[0]["volgnummer"], target["factuurLijnen.0.volgnummer"]);
    //            var firstBtw = (IDictionary<string, object?>)factuurLijnen.First()["btw"];
    //            Assert.AreEqual(firstBtw["tarief"], target["factuurLijnen.0.btw.tarief"]);
    //        }
    #endregion

    #region Unflatten tests
    [Test]
    public void Unflatten_Returns_Dictionary()
    {
        var source = new Dictionary<string, object?>();
        var target = DictionaryUtility.Unflatten(source);
        Assert.IsNotNull(target);
        Assert.IsInstanceOf<IDictionary<string, object?>>(target);
    }
    [Test]
    public void Unflatten_Simple_Values()
    {
        var source = new Dictionary<string, object?> {
            {"factuurNummer", "0123456"},
            {"factuurDatum", "2019-11-19"}
        };
        var target = DictionaryUtility.Unflatten(source);
        Assert.AreEqual(source["factuurNummer"], target["factuurNummer"]);
        Assert.AreEqual(source["factuurDatum"], target["factuurDatum"]);
    }
    [Test]
    public void Unflatten_CollectionValues()
    {
        var source = new Dictionary<string, object?> {
            {
                "factuurLijnen", new[] {
                    new Dictionary<string, object?> {
                        {"omschrijving", "Vervoer"},
                        {"eenheidsPrijs", 160.50}
                    },
                    new Dictionary<string, object?> {
                        {"omschrijving", "Diensten"},
                        {"eenheidsPrijs", 180.50}
                    }
                }
            }
        };
        var target = DictionaryUtility.Unflatten(source);
        var targetFactuurLijnen = target["factuurLijnen"] as IEnumerable<IDictionary<string, object?>>;
        Assert.IsInstanceOf<IEnumerable<IDictionary<string, object?>>>(targetFactuurLijnen);
        // ReSharper disable once AssignNullToNotNullAttribute
        var factuurlijnen = targetFactuurLijnen!.ToList();
        var lijn1 = factuurlijnen[0];
        var sourceFactuurLijnen = (IDictionary<string, object?>[])source["factuurLijnen"];
        Assert.AreEqual(sourceFactuurLijnen[0]["omschrijving"], lijn1["omschrijving"]);
    }
    [Test]
    public void Unflatten_Flat_CollectionValues()
    {
        var source = new Dictionary<string, object?> {
            {"factuurLijnen.0.omschrijving", "Vervoer"},
            {"factuurLijnen.0.eenheidsPrijs", 160.50},
            {"factuurLijnen.1.omschrijving", "Diensten"},
            {"factuurLijnen.1.eenheidsPrijs", 180.50}
        };
        var target = DictionaryUtility.Unflatten(source);
        var targetFactuurLijnen = target["factuurLijnen"] as IEnumerable<IDictionary<string, object?>>;
        Assert.IsInstanceOf<IEnumerable<IDictionary<string, object?>>>(targetFactuurLijnen);
        // ReSharper disable once AssignNullToNotNullAttribute
        var factuurlijnen = targetFactuurLijnen.ToList();
        var lijn1 = factuurlijnen[0];
        Assert.AreEqual(source["factuurLijnen.0.omschrijving"], lijn1["omschrijving"]);
    }
    [Test]
    public void Unflatten_Nested_CollectionValues()
    {
        var source = new Dictionary<string, object?> {
            { "factuurLijnen", new[] {
                new Dictionary<string, object?> {
                    { "nested.first", "1st nested" },
                    { "nested.second", "2nd nested" }
                }
            } }
        };
        var target = DictionaryUtility.Unflatten(source);
        var sourceFactuurLijnen = (IDictionary<string, object?>[])source["factuurLijnen"]!;
        var sourceLijn = sourceFactuurLijnen[0];
        var targetFactuurLijnen = (IList<IDictionary<string, object?>>)target["factuurLijnen"]!;
        var targetLijn = targetFactuurLijnen[0];
        var targetNested = (IDictionary<string, object?>)targetLijn["nested"]!;
        Assert.That(targetNested["first"], Is.EqualTo(sourceLijn["nested.first"]));
    }
    [Test]
    public void Unflatten_Creates_Hierarchy()
    {
        var source = new Dictionary<string, object?> {
            { "leverancier.naam", "LEVERANCIER BVBA" },
            { "leverancier.telefoon", "003239999999" },
        };
        var target = DictionaryUtility.Unflatten(source);
        Assert.IsTrue(target.ContainsKey("leverancier"));
        Assert.IsInstanceOf<IDictionary<string, object?>>(target["leverancier"]);
        var targetLeverancier = (IDictionary<string, object?>)target["leverancier"]!;
        Assert.That(targetLeverancier["naam"], Is.EqualTo(source["leverancier.naam"]));
    }
    [Test]
    public void Unflatten_Creates_Nested_Hierarchy()
    {
        var source = new Dictionary<string, object?> {
            { "leverancier.adres.straat", "Kerkstraat 1" },
            { "leverancier.adres.postcode", "2000" },
            { "leverancier.adres.gemeente", "Antwerpen" }
        };
        var target = DictionaryUtility.Unflatten(source);
        var targetLeverancier = (IDictionary<string, object?>)target["leverancier"]!;
        Assert.IsTrue(targetLeverancier.ContainsKey("adres"));
        Assert.IsInstanceOf<IDictionary<string, object?>>(targetLeverancier["adres"]);
        var targetLeverancierAdres = (IDictionary<string, object?>)targetLeverancier["adres"]!;
        Assert.AreEqual(source["leverancier.adres.straat"], targetLeverancierAdres["straat"]);
    }
    [Test]
    public void Collections_Remain_Collections()
    {
        var source = new Dictionary<string, object?> {
            { "factuurLijnen", new[] {
                new Dictionary<string, object?> {
                    {"omschrijving", "Vervoer"},
                    {"eenheidsPrijs", 160.50}
                },
                new Dictionary<string, object?> {
                    {"omschrijving", "Diensten"},
                    {"eenheidsPrijs", 180.50}
                }
            } }
        };
        var target = DictionaryUtility.Unflatten(source);
        var targetFactuurLijnen = target["factuurLijnen"] as IEnumerable<IDictionary<string, object?>>;
        Assert.IsInstanceOf<IEnumerable<IDictionary<string, object?>>>(targetFactuurLijnen);
        // ReSharper disable once AssignNullToNotNullAttribute
        var factuurlijnen = targetFactuurLijnen.ToList();
        var lijn1 = factuurlijnen[0];
        var sourceFactuurLijnen = (IDictionary<string, object?>[])source["factuurLijnen"];
        Assert.AreEqual(sourceFactuurLijnen[0]["omschrijving"], lijn1["omschrijving"]);
    }
    [Test]
    public void Unflatten_Nested_Collections_In_Collections()
    {
        var source = new Dictionary<string, object?> {
            { "factuurLijnen", new[] {
                new Dictionary<string, object?> {
                    { "omschrijving", "Vervoer" },
                    { "eenheidsPrijs", 160.50 }
                },
                new Dictionary<string, object?> {
                    { "nested.first", "1st nested" },
                    { "nested.second", new[] {
                        new Dictionary<string, object?> {
                            { "foo.value", "fooValue" }
                        }
                    } },
                    { "nested.third", "3th nested" }
                }
            } }
        };
        var target = DictionaryUtility.Unflatten(source);
        var targetFactuurLijnen = ((IEnumerable<IDictionary<string, object?>>)target["factuurLijnen"]).ToArray();
        var sourceFactuurLijnen = (IDictionary<string, object?>[])source["factuurLijnen"];
        var targetLijn2 = targetFactuurLijnen[1];
        var sourceLijn2 = sourceFactuurLijnen[1];
        Assert.IsNotNull(targetLijn2["nested"] as IDictionary<string, object?>);
        var targetNestedProp = (IDictionary<string, object?>)targetLijn2["nested"];
        Assert.AreEqual(sourceLijn2["nested.first"], targetNestedProp["first"]);
        var targetNestedSecond = ((IEnumerable<IDictionary<string, object?>>)targetNestedProp["second"]).ToList();
        var sourceNestedSecond = ((IEnumerable<IDictionary<string, object?>>)sourceLijn2["nested.second"]).ToList();
        var targetNestedSecondFoo = (IDictionary<string, object?>)targetNestedSecond[0]["foo"];
        Assert.AreEqual(sourceNestedSecond[0]["foo.value"], targetNestedSecondFoo["value"]);
    }
    [Test]
    public void Unflatten_Full_Sample_With_Custom_Separator()
    {
        var source = new Dictionary<string, object?> {
            { "factuurNummer", "0123456" },
            { "factuurDatum", "2019-11-19" },
            { "factuurType", 0 },
            { "vervalDatum", "2019-12-04" },
            { "betalingsVoorwaarden", "Xxx" },
            { "leverancier:rekeningNummer:iban", "BEXXXXXXXXXXXXXX" },
            { "leverancier:rekeningNummer:bic", "KREDBEBB" },
            { "leverancier:kbo", "BEXXXXXXXXXX" },
            { "leverancier:naam", "LEVERANCIER BVBA" },
            { "leverancier:telefoon", "003239999999" },
            { "leverancier:email", "facturatie@xxx:be" },
            { "leverancier:adres:straat", "Kerkstraat 1" },
            { "leverancier:adres:postcode", "2000" },
            { "leverancier:adres:gemeente", "Antwerpen" },
            { "factuurLijnen:0:omschrijving", "Vervoer" },
            { "factuurLijnen:0:eenheidsPrijs", 160.50 },
            { "factuurLijnen:0:nested:first", "1st nested" },
            { "factuurLijnen:0:nested:second", "2nd nested" },
            { "factuurLijnen:1:omschrijving", "Diensten" },
            { "factuurLijnen:1:eenheidsPrijs", 180.50 },
            { "factuurLijnen:1:nested:first", "1st nested" },
            { "factuurLijnen:1:nested:second:0:foo:value", "fooValue" },
            { "factuurLijnen:2:nested:nested:third", "3th nested" }
        };
        var target = DictionaryUtility.Unflatten(source, new FlattenOptions { Separator = ":" });

        var targetLeverancier = (IDictionary<string, object?>)target["leverancier"];
        Assert.IsTrue(targetLeverancier.ContainsKey("adres"));
        Assert.IsInstanceOf<IDictionary<string, object?>>(targetLeverancier["adres"]);
        var targetLeverancierAdres = (IDictionary<string, object?>)targetLeverancier["adres"];
        Assert.AreEqual(source["leverancier:adres:straat"], targetLeverancierAdres["straat"]);

        var targetFactuurLijnen = ((IEnumerable<IDictionary<string, object?>>)target["factuurLijnen"]).ToArray();
        var targetLijn2 = targetFactuurLijnen[1];
        Assert.IsNotNull(targetLijn2["nested"] as IDictionary<string, object?>);
        var targetNestedProp = (IDictionary<string, object?>)targetLijn2["nested"];
        Assert.AreEqual(source["factuurLijnen:0:nested:first"], targetNestedProp["first"]);
        var targetNestedSecond = ((IEnumerable<IDictionary<string, object?>>)targetNestedProp["second"]).ToList();
        var targetNestedSecondFoo = (IDictionary<string, object?>)targetNestedSecond[0]["foo"];
        Assert.AreEqual(source["factuurLijnen:1:nested:second:0:foo:value"], targetNestedSecondFoo["value"]);
    }
    #endregion
}