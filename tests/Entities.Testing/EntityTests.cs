using Entities.Testing.Infrastructure.Data;
using Regira.Entities.Extensions;

namespace Entities.Testing;

[TestFixture]
public class EntityTests
{
    [Test]
    public void IsNew_For_Int()
    {
        var item = new Category();
        var isNew = item.IsNew();
        Assert.That(isNew, Is.True);
    }
    [Test]
    public void IsNew_For_String()
    {
        var item = new User();
        var isNew = item.IsNew();
        Assert.That(isNew, Is.True);
    }

    [Test]
    public void Not_IsNew_For_Int()
    {
        var item = new Category { Id = 1 };
        var isNew = item.IsNew();
        Assert.That(isNew, Is.False);
    }
    [Test]
    public void Not_IsNew_For_String()
    {
        var item = new User { Id = Guid.NewGuid().ToString("N") };
        var isNew = item.IsNew();
        Assert.That(isNew, Is.False);
    }
}
