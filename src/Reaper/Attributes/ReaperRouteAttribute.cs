using System.Diagnostics.CodeAnalysis;

namespace Reaper.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ReaperRouteAttribute : Attribute
{
    public ReaperRouteAttribute(string verb, [StringSyntax("Route")] string route) { }
}