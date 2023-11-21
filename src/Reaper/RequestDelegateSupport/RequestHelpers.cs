using System.Runtime.CompilerServices;
using Microsoft.Extensions.Primitives;

namespace Reaper.RequestDelegateSupport;

public static class RequestHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static object? TryConvertValue(object value, Type destination)
    {
        string sv;
        if (value is StringValues)
        {
            sv = value.ToString();
        }
        else
        {
            sv = (string)value;
        }
        if (destination == typeof(string))
        {
            return sv.ToString();
        }
        else if (destination == typeof(int))
        {
            return int.Parse(sv);
        }
        else if (destination == typeof(long))
        {
            return long.Parse(sv);
        }
        else if (destination == typeof(short))
        {
            return short.Parse(sv);
        }
        else if (destination == typeof(byte))
        {
            return byte.Parse(sv);
        }
        else if (destination == typeof(uint))
        {
            return uint.Parse(sv);
        }
        else if (destination == typeof(ulong))
        {
            return ulong.Parse(sv);
        }
        else if (destination == typeof(ushort))
        {
            return ushort.Parse(sv);
        }
        else if (destination == typeof(sbyte))
        {
            return sbyte.Parse(sv);
        }
        else if (destination == typeof(float))
        {
            return float.Parse(sv);
        }
        else if (destination == typeof(double))
        {
            return double.Parse(sv);
        }
        else if (destination == typeof(decimal))
        {
            return decimal.Parse(sv);
        }
        else if (destination == typeof(bool))
        {
            return bool.Parse(sv);
        }
        else if (destination == typeof(Guid))
        {
            return Guid.Parse(sv);
        }
        else if (destination == typeof(DateTime))
        {
            return DateTime.Parse(sv);
        }
        else if (destination == typeof(DateTimeOffset))
        {
            return DateTimeOffset.Parse(sv);
        }
        else if (destination == typeof(TimeSpan))
        {
            return TimeSpan.Parse(sv);
        }
        else if (destination == typeof(Uri))
        {
            return new Uri(sv);
        }
        else if (destination == typeof(char))
        {
            return char.Parse(sv);
        }
        else if (destination == typeof(byte[]))
        {
            return Convert.FromBase64String(sv);
        }
        else
        {
            return null;
        }
    }
}