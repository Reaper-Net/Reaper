namespace Reaper.Context;

public class ReaperExecutionContextProvider : IReaperExecutionContextProvider
{
    private static readonly AsyncLocal<IReaperExecutionContext> executionContext = new();
    
    public IReaperExecutionContext Context
    {
        get => executionContext.Value ?? throw new InvalidOperationException("Reaper execution context is not available.");
        internal set => executionContext.Value = value;
    }

    public void SetContext(IReaperExecutionContext context)
    {
        Context = context;
    }
}