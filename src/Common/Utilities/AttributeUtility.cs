using System.Linq.Expressions;
using System.Reflection;

namespace Regira.Utilities;

public static class AttributeUtility
{
    public static TValue? GetPropertyAttributeValue<T, TOut, TAttribute, TValue>
        (Expression<Func<T, TOut>> propertyExpression, Func<TAttribute, TValue> valueSelector, bool inherited = true)
        where TAttribute : Attribute
    {
        var expression = (MemberExpression)propertyExpression.Body;
        var propertyInfo = (PropertyInfo)expression.Member;
        return propertyInfo.GetCustomAttributes(typeof(TAttribute), inherited).FirstOrDefault() is TAttribute attr ? valueSelector(attr) : default;
    }
}