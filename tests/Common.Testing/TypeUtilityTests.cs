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
        Assert.IsTrue(TypeUtility.IsSimpleType(typeof(int)));
        Assert.IsTrue(TypeUtility.IsSimpleType(typeof(string)));
        Assert.IsTrue(TypeUtility.IsSimpleType(typeof(DateTime)));
        Assert.IsTrue(TypeUtility.IsSimpleType(typeof(TestEnum)));

        Assert.IsTrue(TypeUtility.IsSimpleType(typeof(int?)));
        Assert.IsTrue(TypeUtility.IsSimpleType(typeof(string)));
        Assert.IsTrue(TypeUtility.IsSimpleType(typeof(DateTime?)));
        Assert.IsTrue(TypeUtility.IsSimpleType(typeof(TestEnum?)));

        Assert.IsFalse(TypeUtility.IsSimpleType(typeof(TestStruct)));
        Assert.IsFalse(TypeUtility.IsSimpleType(typeof(TestStruct?)));
        Assert.IsFalse(TypeUtility.IsSimpleType(typeof(ITestCore<>)));
    }
    [Test]
    public void Test_IsNullableType()
    {
        Assert.IsFalse(TypeUtility.IsNullableType(typeof(int)));
        Assert.IsFalse(TypeUtility.IsNullableType(typeof(string)));
        Assert.IsFalse(TypeUtility.IsNullableType(typeof(DateTime)));
        Assert.IsFalse(TypeUtility.IsNullableType(typeof(TestEnum)));
        Assert.IsFalse(TypeUtility.IsNullableType(typeof(TestStruct)));

        Assert.IsTrue(TypeUtility.IsNullableType(typeof(int?)));
        Assert.IsTrue(TypeUtility.IsNullableType(typeof(DateTime?)));
        Assert.IsTrue(TypeUtility.IsNullableType(typeof(TestEnum?)));
        Assert.IsTrue(TypeUtility.IsNullableType(typeof(TestStruct?)));

        Assert.IsFalse(TypeUtility.IsNullableType(typeof(ITestCore<>)));
    }

    [Test]
    public void Test_IsEnumerable()
    {
        var aStringCollection = "this string is a collection of chars";
        var aIntCollection = new[] { 1, 2, 3, 4, 5 };
        var test1Class = new Test1Class { Structs = new TestStruct[] { new(), new() } };

        Assert.IsTrue(TypeUtility.IsTypeEnumerable(aStringCollection.GetType()));
        Assert.IsTrue(TypeUtility.IsTypeEnumerable<string>(aStringCollection.GetType()));
        Assert.IsTrue(TypeUtility.IsTypeEnumerable(aStringCollection.GetType(), typeof(IEnumerable<char>)));
        Assert.IsFalse(TypeUtility.IsTypeEnumerable<IList>(aStringCollection.GetType()));

        Assert.IsTrue(TypeUtility.IsTypeEnumerable(aIntCollection.GetType()));
        Assert.IsTrue(TypeUtility.IsTypeEnumerable(aIntCollection.GetType(), typeof(int[])));
        Assert.IsTrue(TypeUtility.IsTypeEnumerable(aIntCollection.GetType(), typeof(IList<int>)));

        Assert.IsFalse(TypeUtility.IsTypeEnumerable(typeof(int)));
        Assert.IsFalse(TypeUtility.IsTypeEnumerable(typeof(Test1Class)));
        Assert.IsFalse(TypeUtility.IsTypeEnumerable(typeof(TestStruct)));
        Assert.IsTrue(TypeUtility.IsTypeEnumerable(typeof(ArrayList)));

        Assert.IsTrue(TypeUtility.IsTypeEnumerable(test1Class.Structs.GetType()));
        Assert.IsTrue(TypeUtility.IsTypeEnumerable<IList>(test1Class.Structs.GetType()));
        Assert.IsTrue(TypeUtility.IsTypeEnumerable<TestStruct[]>(test1Class.Structs.GetType()));
        Assert.IsTrue(TypeUtility.IsTypeEnumerable<IList<TestStruct>>(test1Class.Structs.GetType()));
        Assert.IsTrue(TypeUtility.IsTypeEnumerable<ICollection<TestStruct>>(test1Class.Structs.GetType()));
        Assert.IsFalse(TypeUtility.IsTypeEnumerable<List<TestStruct>>(test1Class.Structs.GetType()));
        Assert.IsFalse(TypeUtility.IsTypeEnumerable<IList<Test1Class>>(test1Class.Structs.GetType()));
    }

    [Test]
    public void Test_IsCollection()
    {
        var aStringCollection = "this string is a collection of chars";
        var aIntCollection = new[] { 1, 2, 3, 4, 5 };
        var test1Class = new Test1Class { Structs = new TestStruct[] { new(), new() } };

        Assert.IsTrue(TypeUtility.IsTypeACollection(aStringCollection.GetType()));
        Assert.IsFalse(TypeUtility.IsTypeACollection(aStringCollection.GetType(), typeof(IList<char>)));
        Assert.IsFalse(TypeUtility.IsTypeACollection<IList>(aStringCollection.GetType()));

        Assert.IsTrue(TypeUtility.IsTypeACollection(aIntCollection.GetType()));
        Assert.IsTrue(TypeUtility.IsTypeACollection(aIntCollection.GetType(), typeof(int[])));
        Assert.IsTrue(TypeUtility.IsTypeACollection(aIntCollection.GetType(), typeof(IList<int>)));

        Assert.IsFalse(TypeUtility.IsTypeACollection(typeof(int)));
        Assert.IsFalse(TypeUtility.IsTypeACollection(typeof(Test1Class)));
        Assert.IsFalse(TypeUtility.IsTypeACollection(typeof(TestStruct)));
        Assert.IsTrue(TypeUtility.IsTypeACollection(typeof(ArrayList)));

        Assert.IsTrue(TypeUtility.IsTypeACollection(test1Class.Structs.GetType()));
        Assert.IsTrue(TypeUtility.IsTypeACollection<IList>(test1Class.Structs.GetType()));
        Assert.IsTrue(TypeUtility.IsTypeACollection<TestStruct[]>(test1Class.Structs.GetType()));
        Assert.IsTrue(TypeUtility.IsTypeACollection<IList<TestStruct>>(test1Class.Structs.GetType()));
        Assert.IsTrue(TypeUtility.IsTypeACollection<ICollection<TestStruct>>(test1Class.Structs.GetType()));
        Assert.IsFalse(TypeUtility.IsTypeACollection<List<TestStruct>>(test1Class.Structs.GetType()));
        Assert.IsFalse(TypeUtility.IsTypeACollection<IList<Test1Class>>(test1Class.Structs.GetType()));
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