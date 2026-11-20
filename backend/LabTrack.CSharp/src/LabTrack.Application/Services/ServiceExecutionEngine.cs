namespace LabTrack.Application.Services;

internal static class ServiceExecutionEngine
{
    private static object? CoerceValueSafe(object? value, Type targetType)
    {
        var nonNullType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (value is null)
        {
            return targetType.IsValueType && Nullable.GetUnderlyingType(targetType) is null
                ? Activator.CreateInstance(targetType)
                : null;
        }
        if (nonNullType.IsInstanceOfType(value)) return value;
        try
        {
            if (nonNullType == typeof(bool))
            {
                if (value is int i) return i != 0;
                if (value is long l) return l != 0L;
                if (bool.TryParse(value.ToString(), out var b)) return b;
                return false;
            }
            if (nonNullType == typeof(int))
            {
                if (value is bool b) return b ? 1 : 0;
                if (value is Enum) return Convert.ToInt32(value);
                if (int.TryParse(value.ToString(), out var i)) return i;
                return 0;
            }
            if (nonNullType == typeof(string)) return value.ToString() ?? string.Empty;
            if (nonNullType == typeof(Guid))
            {
                return Guid.TryParse(value.ToString(), out var g) ? g : Guid.Empty;
            }
            if (nonNullType.IsEnum)
            {
                if (value is int ei) return Enum.ToObject(nonNullType, ei);
                return Enum.TryParse(nonNullType, value.ToString(), true, out var ev) && ev != null
                    ? ev
                    : Activator.CreateInstance(nonNullType);
            }
            return Convert.ChangeType(value, nonNullType);
        }
        catch
        {
            return targetType.IsValueType && Nullable.GetUnderlyingType(targetType) is null
                ? Activator.CreateInstance(targetType)
                : null;
        }
    }

    private static void SafeSetPropertyValue(System.Reflection.PropertyInfo prop, object target, object? value)
    {
        try
        {
            if (!prop.CanWrite) return;
            var coerced = CoerceValueSafe(value, prop.PropertyType);
            prop.SetValue(target, coerced);
        }
        catch
        {
            // Ignore incompatible property assignment
        }
    }

    private static object SafeInvokeConstructor(System.Reflection.ConstructorInfo ctor, object?[]? rawArgs)
    {
        var parameters = ctor.GetParameters();
        var finalArgs = new object?[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var pType = parameters[i].ParameterType;
            if (rawArgs != null && i < rawArgs.Length && rawArgs[i] != null)
            {
                finalArgs[i] = CoerceValueSafe(rawArgs[i], pType);
            }
            else if (parameters[i].HasDefaultValue)
            {
                finalArgs[i] = parameters[i].DefaultValue;
            }
            else
            {
                finalArgs[i] = CoerceValueSafe(null, pType);
            }
        }
        return ctor.Invoke(finalArgs);
    }
    public static Task<T> ResolveAsync<T>(T value) => new ValueTask<T>(value).AsTask();

    public static T[] Empty<T>() => new T[0];

    public static Task CompleteAsync() => default(ValueTask).AsTask();
}





