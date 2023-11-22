using System.Globalization;

namespace Reaper.RequestDelegateSupport;

public static class RequestHelpers
{
    public static bool TryConvertValue<T>(object value, out T? result)
    {
        string sv = value.ToString()!;
        
        if (typeof(T) == typeof(string))
        {
            result = (T)(object)sv;
            return true;
        }
        if (typeof(T) == typeof(int))
        {
            if (int.TryParse(sv, CultureInfo.InvariantCulture, out var iv))
            {
                result = (T)(object)iv;
                return true;
            }
        } else if (typeof(T) == typeof(double))
        {
            if (double.TryParse(sv, CultureInfo.InvariantCulture, out var dv))
            {
                result = (T)(object)dv;
                return true;
            }
        } else if (typeof(T) == typeof(float))
        {
            if (float.TryParse(sv, CultureInfo.InvariantCulture, out var fv))
            {
                result = (T) (object)fv;
                return true;
            }
        } else if (typeof(T) == typeof(decimal)) {
            if (decimal.TryParse(sv, CultureInfo.InvariantCulture, out var dv))
            {
                result = (T) (object)dv;
                return true;
            }
        } else if (typeof(T) == typeof(bool))
        {
            if (bool.TryParse(sv, out var bv))
            {
                result = (T)(object)bv;
                return true;
            }
        } else if (typeof(T) == typeof(DateTime))
        {
            if (DateTime.TryParse(sv, CultureInfo.InvariantCulture, out var dtv))
            {
                result = (T)(object)dtv;
                return true;
            }
        } else if (typeof(T) == typeof(Guid))
        {
            if (Guid.TryParse(sv, CultureInfo.InvariantCulture, out var g))
            {
                result = (T)(object)g;
                return true;
            }
        } else if (typeof(T).IsEnum)
        {
            if (Enum.TryParse(typeof(T), sv, true, out var ev))
            {
                result = (T) ev;
                return true;
            }
        }
        
        result = default;
        return false;
    }
}