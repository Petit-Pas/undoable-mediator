using System.Reflection;

namespace UndoableMediator.Mediators;

/// <summary>
///     Reflection utilities used by <see cref="Mediator"/> for runtime generic method dispatch.
/// </summary>
internal static class ReflectionHelper
{
    /// <summary>
    ///     Finds a private instance method on <typeparamref name="TClass"/> by name and generic arity,
    ///     then closes it with the supplied type arguments.
    /// </summary>
    public static MethodInfo GetClosedPrivateMethod<TClass>(string methodName, int genericArity, params Type[] typeArguments)
    {
        var openMethod = typeof(TClass)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .First(m => m.Name == methodName && m.GetGenericArguments().Length == genericArity);

        return openMethod.MakeGenericMethod(typeArguments);
    }

    /// <summary>
    ///     Invokes <paramref name="method"/> on <paramref name="instance"/>,
    ///     unwrapping any <see cref="TargetInvocationException"/> so the original exception propagates.
    /// </summary>
    public static object InvokeUnwrapped(MethodInfo method, object instance, params object[] args)
    {
        try
        {
            return method.Invoke(instance, args)!;
        }
        catch (TargetInvocationException e)
        {
            throw e.InnerException ?? e;
        }
    }

    /// <summary>
    ///     Returns the first generic type argument of an interface matching <paramref name="openGenericInterface"/>
    ///     implemented by <paramref name="type"/>, or <c>null</c> if not found.
    /// </summary>
    public static Type? GetGenericInterfaceArgument(Type type, Type openGenericInterface)
    {
        foreach (var iface in type.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == openGenericInterface)
            {
                return iface.GenericTypeArguments[0];
            }
        }
        return null;
    }
}
