using System.Data;
using System.Text;

namespace Reaper.SourceGenerator.Internal;

public class CodeWriter
{
    private readonly StringBuilder builder = new();
    private readonly int indentAmount = 4;
    private int indentLevel;
    private bool indentPending = true;
    
    public void In()
    {
        indentLevel++;
    }

    public void Out()
    {
        indentLevel--;
    }
    
    public void Namespace(string name)
    {
        builder.Append("namespace ");
        builder.Append(name);
        builder.AppendLine(";");
    }

    public void StartClass(string name, string accessModifier = "public", string? bases = null)
    {
        AppendLine(GeneratorStatics.GeneratedCodeAttribute);
        Append(accessModifier);
        Append(" class ");
        Append(name);
        if (bases != null)
        {
            Append(" : ");
            Append(bases);
        }

        AppendLine("");
        AppendLine("{");
        In();
    }

    public void EndClass()
    {
        Out();
        AppendLine("}");
    }
    
    public void Append(string value)
    {
        if (indentPending)
        {
            indentPending = false;
            builder.Append(' ', indentLevel * indentAmount);
        }
        builder.Append(value);
    }
    
    public void AppendLine(string value)
    {
        if (indentPending)
        {
            builder.Append(' ', indentLevel * indentAmount);
        }
        builder.AppendLine(value);
    }
}