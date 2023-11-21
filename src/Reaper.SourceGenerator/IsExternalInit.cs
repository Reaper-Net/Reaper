using System.ComponentModel;

namespace System.Runtime.CompilerServices
{

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IsExternalInit
    {
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class RequiredMemberAttribute : Attribute
    {
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string name)
        {
        }
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Constructor)]
    public class SetsRequiredMembersAttribute : Attribute
    {
    }
}