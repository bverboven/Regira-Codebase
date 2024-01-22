﻿using NUnit.Framework.Legacy;
using Regira.Normalizing;
using Regira.Normalizing.Abstractions;

namespace Normalizing.Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class NormalizeTests
{
    private readonly DefaultNormalizer _normalizer;
    private readonly ObjectNormalizer _objectNormalizer;
    public NormalizeTests()
    {
        _normalizer = new DefaultNormalizer();
        _objectNormalizer = new ObjectNormalizer();
    }

    [Test]
    public void Test_Without_Attribute()
    {
        var obj = new { Value = "Testing without the normalized attribute" };
        var sourceValue = obj.Value;
        _objectNormalizer.HandleNormalize(obj);
        Assert.That(obj.Value, Is.EqualTo(sourceValue));
    }
    [Test]
    public void Test_Without_SourceProp()
    {
        var obj = new NormalizableObject1 { NormalizedProp = "Testing the normalized-attribute" };
        var sourceValue = obj.NormalizedProp;
        _objectNormalizer.HandleNormalize(obj);
        Assert.That(obj.NormalizedProp, Is.Not.EqualTo(sourceValue));
        Assert.That(obj.NormalizedProp, Is.EqualTo(_normalizer.Normalize(sourceValue)));
    }
    [Test]
    public void Test_SourceProp_And_DefaultNormalizer()
    {
        var obj = new NormalizableObject2 { SourceProp = "Testing the normalized-attribute" };
        ClassicAssert.IsNull(obj.NormalizedProp);
        _objectNormalizer.HandleNormalize(obj);
        Assert.That(obj.NormalizedProp, Is.Not.EqualTo(obj.SourceProp));
        Assert.That(obj.NormalizedProp, Is.EqualTo(_normalizer.Normalize(obj.SourceProp)));
    }
    [Test]
    public void Test_SourceProp_And_CustomNormalizer()
    {
        var obj = new NormalizableObject3 { SourceProp = "Testing the normalized attribute" };
        ClassicAssert.IsNull(obj.NormalizedProp);
        _objectNormalizer.HandleNormalize(obj);
        Assert.That(obj.NormalizedProp, Is.Not.EqualTo(obj.SourceProp));
        var normalizer = new TestNormalizer();
        Assert.That(obj.NormalizedProp, Is.EqualTo(normalizer.Normalize(obj.SourceProp)));
    }
    [Test]
    public void Test_Multiple_Properties()
    {
        var obj = new NormalizableObject4
        {
            SourceProp1 = "Testing-the-normalized-attribute.",
            SourceProp2 = "Testing the normalized attribute",
            NormalizedProp3 = "Testing the normalized attribute"
        };
        ClassicAssert.IsNull(obj.NormalizedProp1);
        ClassicAssert.IsNull(obj.NormalizedProp2);
        var sourceProp3 = obj.NormalizedProp3;
        _objectNormalizer.HandleNormalize(obj);
        Assert.That(obj.NormalizedProp1, Is.Not.EqualTo(obj.SourceProp1));
        Assert.That(obj.NormalizedProp2, Is.Not.EqualTo(obj.SourceProp2));
        var normalizer = new TestNormalizer();
        Assert.That(obj.NormalizedProp1, Is.EqualTo(_normalizer.Normalize(obj.SourceProp1)));
        Assert.That(obj.NormalizedProp2, Is.EqualTo(normalizer.Normalize(obj.SourceProp2)));
        Assert.That(obj.NormalizedProp3, Is.EqualTo(normalizer.Normalize(sourceProp3)));
    }

    [Test]
    public void Test_Multiple_SourceProperties()
    {
        var obj = new NormalizableObject4
        {
            SourceProp1 = "Testing-the-normalized-attribute.",
            SourceProp2 = "Testing the normalized attribute"
        };
        _objectNormalizer.HandleNormalize(obj);
        Assert.That(obj.NormalizedContent, Is.EqualTo($"{_normalizer.Normalize(obj.SourceProp1)} {_normalizer.Normalize(obj.SourceProp2)}"));
    }
    [Test]
    public void Test_Collection()
    {
        var arr = new List<NormalizableObject2> {
            new() {SourceProp = "1. Testing the normalized attribute"},
            new() {SourceProp = "2. Testing the normalized attribute"}
        };
        _objectNormalizer.HandleNormalize(new
        {
            Collection = arr
        });
        Assert.That(arr[0].NormalizedProp, Is.EqualTo(_normalizer.Normalize(arr[0].SourceProp)));
        Assert.That(arr[1].NormalizedProp, Is.EqualTo(_normalizer.Normalize(arr[1].SourceProp)));
    }
    [Test]
    public void Test_Nested_Objects()
    {
        var obj = new NestedObject
        {
            //Obj1 = new NormalizableObject1 { NormalizedProp = "Testing the normalized-attribute" },
            Obj2 = new NormalizableObject2 { SourceProp = "Testing the normalized-attribute" },
            Obj3 = new NestedObject
            {
                Obj2 = new NormalizableObject2 { SourceProp = "Testing the normalized-attribute" },
                Collection = new List<NestedObject>
                {
                    //new NestedObject {
                    //    Obj1 = new NormalizableObject1 { NormalizedProp = "Testing the normalized-attribute" }
                    //},
                    new()
                    {
                        Obj2 = new NormalizableObject2 { SourceProp = "Testing the normalized-attribute" }
                    }
                }
            }
        };
        _objectNormalizer.HandleNormalize(obj);
        Assert.That(obj.Obj2.NormalizedProp, Is.Not.EqualTo(obj.Obj2.SourceProp));
        Assert.That(obj.Obj3.Obj2.NormalizedProp, Is.Not.EqualTo(obj.Obj3.Obj2.SourceProp));
        Assert.That(obj.Obj2.NormalizedProp, Is.EqualTo(_normalizer.Normalize(obj.Obj2.SourceProp)));
        Assert.That(obj.Obj3.Obj2.NormalizedProp, Is.EqualTo(_normalizer.Normalize(obj.Obj3.Obj2.SourceProp)));
        Assert.That(obj.Obj3.Collection.Last().Obj2!.NormalizedProp, Is.EqualTo(_normalizer.Normalize(obj.Obj3.Collection.Last().Obj2!.SourceProp)));
    }

    #region Infrastructure
    public class TestNormalizer : INormalizer
    {
        public string? Normalize(string? input)
        {
            return input?.ToUpper().Replace(" ", "_");
        }
    }

    class NormalizableObject1
    {
        [Normalized]
        public string? NormalizedProp { get; set; }
    }
    class NormalizableObject2
    {
        public string? SourceProp { get; set; }
        [Normalized(SourceProperty = nameof(SourceProp), SourceProperties = new[] { nameof(SourceProp) })]
        public string? NormalizedProp { get; set; }
    }
    class NormalizableObject3
    {
        public string? SourceProp { get; set; }
        [Normalized(SourceProperty = nameof(SourceProp), Normalizer = typeof(TestNormalizer))]
        public string? NormalizedProp { get; set; }
    }
    class NormalizableObject4
    {
        public string? SourceProp1 { get; set; }
        [Normalized(SourceProperty = nameof(SourceProp1))]
        public string? NormalizedProp1 { get; set; }
        public string? SourceProp2 { get; set; }
        [Normalized(SourceProperty = nameof(SourceProp2), Normalizer = typeof(TestNormalizer))]
        public string? NormalizedProp2 { get; set; }
        [Normalized(Normalizer = typeof(TestNormalizer))]
        public string? NormalizedProp3 { get; set; }
        [Normalized(SourceProperties = new[] { nameof(SourceProp1), nameof(SourceProp2) })]
        public string? NormalizedContent { get; set; }
    }

    class NestedObject
    {
        //public NormalizableObject1? Obj1 { get; set; }
        public NormalizableObject2? Obj2 { get; set; }
        public NestedObject? Obj3 { get; set; }
        public ICollection<NestedObject>? Collection { get; set; }
    }
    #endregion
}