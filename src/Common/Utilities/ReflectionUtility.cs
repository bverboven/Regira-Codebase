using System.Reflection;

namespace Regira.Utilities;

public static class ReflectionUtility
{
    public static T CastToHelper<T>(this object o) => (T)o;
    public static dynamic? CastTo(this object o, Type type)
    {
        // https://stackoverflow.com/questions/18052562/cast-to-a-reflected-type-in-c-sharp#55852845
        var methodInfo = typeof(ReflectionUtility).GetMethod(nameof(CastToHelper), BindingFlags.Static | BindingFlags.Public);
        var genericArguments = new[] { type };
        var genericMethodInfo = methodInfo?.MakeGenericMethod(genericArguments);
        return genericMethodInfo?.Invoke(null, new[] { o });
    }
        
    public static Task<object?> InvokeAsync(this MethodInfo methodInfo, object obj, params object[] parameters)
        => InvokeAsync<object?>(methodInfo, obj, parameters);
    public static async Task<TResult> InvokeAsync<TResult>(this MethodInfo methodInfo, object obj, params object[] parameters)
    {
        // https://stackoverflow.com/questions/39674988/how-to-call-a-generic-async-method-using-reflection#39679855
        var task = (Task)methodInfo.Invoke(obj, parameters)!;
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        return (TResult)resultProperty!.GetValue(task)!;
    }
}