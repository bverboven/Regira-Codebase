using NUnit.Framework.Legacy;
using Regira.Utilities;
using System.Collections;

namespace Common.Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class TypeUtilityTests
{
    #region Models

    public enum TestEnum
    {
        Value1,
        Value2
    }

    public struct TestStruct
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public interface ITestCore<T>
    {
        T Id { get; set; }
        DateTime Created { get; set; }
        DateTime? LastModified { get; set; }
    }
    public interface ITestWithDescription
    {
        string? Description { get; set; }
    }
    public interface ITestWithParent<T>
    {
        ITestCore<T> Parent { get; set; }
    }
    public abstract class TestClassBase<T> : ITestCore<T>
    {
        public T Id { get; set; } = default!;
        public string? Title { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? LastModified { get; set; }
    }

    public class Test1Class : TestClassBase<int>, ITestWithDescription
    {
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public ICollection<TestStruct> Structs { get; set; } = null!;
    }

    public class Test2Class : TestClassBase<string>, ITestWithParent<string>
    {
        public ITestCore<string>? Parent { get; set; }
    }
    #endregion

    [Test]
    public void Test_IsSimpleType()
    {
        ClassicAssert.IsTrue(TypeUtility.IsSimpleType(typeof(int)));
        ClassicAssert.IsTrue(TypeUtility.IsSimpleType(typeof(string)));
        ClassicAssert.IsTrue(TypeUtility.IsSimpleType(typeof(DateTime)));
        ClassicAssert.IsTrue(TypeUtility.IsSimpleType(typeof(TestEnum)));

        ClassicAssert.IsTrue(TypeUtility.IsSimpleType(typeof(int?)));
        ClassicAssert.IsTrue(TypeUtility.IsSimpleType(typeof(string)));
        ClassicAssert.IsTrue(TypeUtility.IsSimpleType(typeof(DateTime?)));
        ClassicAssert.IsTrue(TypeUtility.IsSimpleType(typeof(TestEnum?)));

        ClassicAssert.IsFalse(TypeUtility.IsSimpleType(typeof(TestStruct)));
        ClassicAssert.IsFalse(TypeUtility.IsSimpleType(typeof(TestStruct?)));
        ClassicAssert.IsFalse(TypeUtility.IsSimpleType(typeof(ITestCore<>)));
    }
    [Test]
    public void Test_IsNullableType()
    {
        ClassicAssert.IsFalse(TypeUtility.IsNullableType(typeof(int)));
        ClassicAssert.IsFalse(TypeUtility.IsNullableType(typeof(string)));
        ClassicAssert.IsFalse(TypeUtility.IsNullableType(typeof(DateTime)));
        ClassicAssert.IsFalse(TypeUtility.IsNullableType(typeof(TestEnum)));
        ClassicAssert.IsFalse(TypeUtility.IsNullableType(typeof(TestStruct)));

        ClassicAssert.IsTrue(TypeUtility.IsNullableType(typeof(int?)));
        ClassicAssert.IsTrue(TypeUtility.IsNullableType(typeof(DateTime?)));
        ClassicAssert.IsTrue(TypeUtility.IsNullableType(typeof(TestEnum?)));
        ClassicAssert.IsTrue(TypeUtility.IsNullableType(typeof(TestStruct?)));

        ClassicAssert.IsFalse(TypeUtility.IsNullableType(typeof(ITestCore<>)));
    }

    [Test]
    public void Test_IsEnumerable()
    {
        var aStringCollection = "this string is a collection of chars";
        var aIntCollection = new[] { 1, 2, 3, 4, 5 };
        var test1Class = new Test1Class { Structs = new TestStruct[] { new(), new() } };

        ClassicAssert.IsTrue(TypeUtility.IsTypeEnumerable(aStringCollection.GetType()));
        ClassicAssert.IsTrue(TypeUtility.IsTypeEnumerable<string>(aStringCollection.GetType()));
        ClassicAssert.IsTrue(TypeUtility.IsTypeEnumerable(aStringCollection.GetType(), typeof(IEnumerable<char>)));
        ClassicAssert.IsFalse(TypeUtility.IsTypeEnumerable<IList>(aStringCollection.GetType()));

        ClassicAssert.IsTrue(TypeUtility.IsTypeEnumerable(aIntCollection.GetType()));
        ClassicAssert.IsTrue(TypeUtility.IsTypeEnumerable(aIntCollection.GetType(), typeof(int[])));
        ClassicAssert.IsTrue(TypeUtility.IsTypeEnumerable(aIntCollection.GetType(), typeof(IList<int>)));

        ClassicAssert.IsFalse(TypeUtility.IsTypeEnumerable(typeof(int)));
        ClassicAssert.IsFalse(TypeUtility.IsTypeEnumerable(typeof(Test1Class)));
        ClassicAssert.IsFalse(TypeUtility.IsTypeEnumerable(typeof(TestStruct)));
        ClassicAssert.IsTrue(TypeUtility.IsTypeEnumerable(typeof(ArrayList)));

        ClassicAssert.IsTrue(TypeUtility.IsTypeEnumerable(test1Class.Structs.GetType()));
        ClassicAssert.IsTrue(TypeUtility.IsTypeEnumerable<IList>(test1Class.Structs.GetType()));
        ClassicAssert.IsTrue(TypeUtility.IsTypeEnumerable<TestStruct[]>(test1Class.Structs.GetType()));
        ClassicAssert.IsTrue(TypeUtility.IsTypeEnumerable<IList<TestStruct>>(test1Class.Structs.GetType()));
        ClassicAssert.IsTrue(TypeUtility.IsTypeEnumerable<ICollection<TestStruct>>(test1Class.Structs.GetType()));
        ClassicAssert.IsFalse(TypeUtility.IsTypeEnumerable<List<TestStruct>>(test1Class.Structs.GetType()));
        ClassicAssert.IsFalse(TypeUtility.IsTypeEnumerable<IList<Test1Class>>(test1Class.Structs.GetType()));
    }

    [Test]
    public void Test_IsCollection()
    {
        var aStringCollection = "this string is a collection of chars";
        var aIntCollection = new[] { 1, 2, 3, 4, 5 };
        var test1Class = new Test1Class { Structs = new TestStruct[] { new(), new() } };

        ClassicAssert.IsTrue(TypeUtility.IsTypeACollection(aStringCollection.GetType()));
        ClassicAssert.IsFalse(TypeUtility.IsTypeACollection(aStringCollection.GetType(), typeof(IList<char>)));
        ClassicAssert.IsFalse(TypeUtility.IsTypeACollection<IList>(aStringCollection.GetType()));

        ClassicAssert.IsTrue(TypeUtility.IsTypeACollection(aIntCollection.GetType()));
        ClassicAssert.IsTrue(TypeUtility.IsTypeACollection(aIntCollection.GetType(), typeof(int[])));
        ClassicAssert.IsTrue(TypeUtility.IsTypeACollection(aIntCollection.GetType(), typeof(IList<int>)));

        ClassicAssert.IsFalse(TypeUtility.IsTypeACollection(typeof(int)));
        ClassicAssert.IsFalse(TypeUtility.IsTypeACollection(typeof(Test1Class)));
        ClassicAssert.IsFalse(TypeUtility.IsTypeACollection(typeof(TestStruct)));
        ClassicAssert.IsTrue(TypeUtility.IsTypeACollection(typeof(ArrayList)));

        ClassicAssert.IsTrue(TypeUtility.IsTypeACollection(test1Class.Structs.GetType()));
        ClassicAssert.IsTrue(TypeUtility.IsTypeACollection<IList>(test1Class.Structs.GetType()));
        ClassicAssert.IsTrue(TypeUtility.IsTypeACollection<TestStruct[]>(test1Class.Structs.GetType()));
        ClassicAssert.IsTrue(TypeUtility.IsTypeACollection<IList<TestStruct>>(test1Class.Structs.GetType()));
        ClassicAssert.IsTrue(TypeUtility.IsTypeACollection<ICollection<TestStruct>>(test1Class.Structs.GetType()));
        ClassicAssert.IsFalse(TypeUtility.IsTypeACollection<List<TestStruct>>(test1Class.Structs.GetType()));
        ClassicAssert.IsFalse(TypeUtility.IsTypeACollection<IList<Test1Class>>(test1Class.Structs.GetType()));
    }

    [Test]
    public void Test_BaseTypes()
    {
        var baseTypes = TypeUtility.GetBaseTypes(typeof(Test2Class))
            .Distinct()
            .ToArray();
        var expectedBaseTypes = new[]
        {
            typeof(TestClassBase<string>),
            typeof(ITestWithParent<string>),
            typeof(ITestCore<string>),
            typeof(object)
        };
        CollectionAssert.IsNotEmpty(baseTypes);
        CollectionAssert.AreEquivalent(expectedBaseTypes, baseTypes);
    }

    [Test]
    public void Test_Get_PropertyName_By_Expression()
    {
        var test1Class = new Test1Class { Structs = Array.Empty<TestStruct>() };

        Assert.That(TypeUtility.GetPropertyName(() => test1Class.Price!), Is.EqualTo(nameof(Test1Class.Price)));
        Assert.That(TypeUtility.GetPropertyName<Test1Class>(x => x.Price!), Is.EqualTo(nameof(Test1Class.Price)));
        Assert.That(TypeUtility.GetPropertyName<Test1Class>(x => x.Description!), Is.EqualTo(nameof(Test1Class.Description)));
        Assert.That(TypeUtility.GetPropertyName<Test1Class>(x => x.Structs.First()), Is.EqualTo(null));
        Assert.That(TypeUtility.GetPropertyName<Test1Class>(x => x.Structs.First().Name), Is.EqualTo(nameof(TestStruct.Name)));
    }
}