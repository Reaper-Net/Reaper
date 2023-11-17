namespace Reaper.Context;

public class ReaperExecutionContextProvider : IReaperExecutionContextProvider
{
    private static readonly AsyncLocal<ReaperExecutionContext> executionContext = new();
    
    public ReaperExecutionContext Context
    {
        get => executionContext.Value ?? throw new InvalidOperationException("Reaper execution context is not available.");
        internal set => executionContext.Value = value;
    }

    public void SetContext(ReaperExecutionContext context)
    {
        Context = context;
    }
}