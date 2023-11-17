namespace Reaper.Attributes;

/// <summary>
/// This defines that the endpoint is scoped, enabling scoped ctor dependency injection
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ReaperScopedAttribute : Attribute
{
    
}