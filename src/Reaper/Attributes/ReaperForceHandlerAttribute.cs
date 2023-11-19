namespace Reaper.Attributes;

/// <summary>
/// This attribute states that it should use the Reaper handler instead of the default Minimal API handler.
///
/// This is typically automatically discovered in certain cases, but you can force it for performance and/or simplicity reasons.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ReaperForceHandlerAttribute : Attribute
{
    
}