using NUnit.Framework.Legacy;
using Regira.Utilities;

namespace Common.Testing;

[Flags]
public enum TestEnum
{
    None = 0,           // 0
    One = 1 << 0,       // 1
    Two = 1 << 1,       // 2
    Three = One | Two,  // 3
    Four = 1 << 2,      // 4
    Five = One | Four,  // 5
    Seven = None | One | Two | Four, // 7
    SixtyFour = 1 << 6,
    SixtyNine = SixtyFour | Five,
    //All = ~0            // -1 -> not working as expected with integers
}

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class EnumTests
{
    private readonly TestEnum[] _validFlags = { TestEnum.None, TestEnum.One, TestEnum.Two, TestEnum.Four, TestEnum.SixtyFour };
    private readonly TestEnum[] _combos = new[] { 3, 5, 6, 7, 65, 66, 67, 68, 69, 70, 71 }.Cast<TestEnum>().ToArray();

    [Test]
    public void List_Bitmask_FlagValues()
    {
        var values = EnumUtility.ListValidFlagValues<TestEnum>().ToArray();
        var values2 = EnumUtility.ListFlagValues<TestEnum>(/*false*/);
        CollectionAssert.AreEquivalent(values, values2);
        CollectionAssert.AreEquivalent(_validFlags, values);
    }
    [Test]
    public void List_All_FlagValues()
    {
        var values = EnumUtility.ListFlagValues<TestEnum>(true).ToArray();
        var expected = _validFlags.Concat(_combos);
        CollectionAssert.AreEquivalent(expected, values);
    }
    [Test]
    public void List_Also_Undefined_FlagValues()
    {
        var values = EnumUtility.ListFlagValues<TestEnum>(true, false).ToArray();
        var maxValue = (int)EnumUtility.GetMaxFlagValue<TestEnum>();
        var expected = Enumerable.Range(0, maxValue + 1).Select(v => (TestEnum)v);
        CollectionAssert.AreEquivalent(expected, values);
    }

    [Test]
    public void Only_Valid_Flag_Values_Are_Included()
    {
        var allCombinations = EnumUtility.ListValidFlagValues<TestEnum>()
            .ToArray();
        CollectionAssert.Contains(allCombinations, TestEnum.One);
        CollectionAssert.Contains(allCombinations, TestEnum.Two);
        CollectionAssert.Contains(allCombinations, TestEnum.Four);

        CollectionAssert.Contains(allCombinations, TestEnum.None);
        var firstAndSecond = TestEnum.One | TestEnum.Two;
        CollectionAssert.DoesNotContain(allCombinations, firstAndSecond);
        var secondAndThird = TestEnum.Two | TestEnum.Four;
        CollectionAssert.DoesNotContain(allCombinations, secondAndThird);
        var firstAndThird = TestEnum.One | TestEnum.Four;
        CollectionAssert.DoesNotContain(allCombinations, firstAndThird);
        CollectionAssert.DoesNotContain(allCombinations, TestEnum.Three);
        CollectionAssert.DoesNotContain(allCombinations, TestEnum.Seven);
        CollectionAssert.DoesNotContain(allCombinations, TestEnum.SixtyNine);

        CollectionAssert.DoesNotContain(allCombinations, (TestEnum)8);
    }
    [Test]
    public void All_Combinations_Are_Included()
    {
        var allCombinations = EnumUtility.ListFlagValues<TestEnum>(true)
            .ToArray();
        CollectionAssert.Contains(allCombinations, TestEnum.None);
        CollectionAssert.Contains(allCombinations, TestEnum.One);
        CollectionAssert.Contains(allCombinations, TestEnum.Two);
        CollectionAssert.Contains(allCombinations, TestEnum.Four);
        var firstAndSecond = TestEnum.One | TestEnum.Two;
        CollectionAssert.Contains(allCombinations, firstAndSecond);
        var secondAndThird = TestEnum.Two | TestEnum.Four;
        CollectionAssert.Contains(allCombinations, secondAndThird);
        var firstAndThird = TestEnum.One | TestEnum.Four;
        CollectionAssert.Contains(allCombinations, firstAndThird);
        CollectionAssert.Contains(allCombinations, TestEnum.Three);
        CollectionAssert.Contains(allCombinations, TestEnum.Seven);
        CollectionAssert.Contains(allCombinations, TestEnum.SixtyNine);
    }

    [Test]
    public void Max_FlagValue_Equals_SumOfAll()
    {
        var maxValue = (int)EnumUtility.GetMaxFlagValue<TestEnum>();
        var all = TestEnum.One | TestEnum.Two | TestEnum.Four | TestEnum.SixtyFour;
        Assert.That(maxValue, Is.EqualTo((int)(all)));
    }
    [Test]
    public void Max_FlagValue()
    {
        var maxValue = EnumUtility.GetMaxFlagValue<TestEnum>();
        Assert.That(maxValue.HasFlag(TestEnum.None | TestEnum.One | TestEnum.Two | TestEnum.Four), Is.True);
    }
}