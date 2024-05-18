using System.Diagnostics.CodeAnalysis;

namespace Reaper.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ReaperRouteAttribute : Attribute
{
    public string Verb { get; }
    public string Route { get; }
    
    public ReaperRouteAttribute(string verb, [StringSyntax("Route")] string route)
    {
        Verb = verb;
        Route = route;
    }
}