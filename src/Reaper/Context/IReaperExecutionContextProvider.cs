namespace Reaper.Context;

public interface IReaperExecutionContextProvider
{
    public IReaperExecutionContext Context { get; }

    internal void SetContext(IReaperExecutionContext context);
}