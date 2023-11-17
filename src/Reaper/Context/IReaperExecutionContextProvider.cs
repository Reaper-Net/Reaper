namespace Reaper.Context;

public interface IReaperExecutionContextProvider
{
    public ReaperExecutionContext Context { get; }

    internal void SetContext(ReaperExecutionContext context);
}