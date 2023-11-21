using System.Text;

namespace Reaper.SourceGenerator.Internal;

public class CodeWriter
{
    private readonly StringBuilder builder = new();
    private readonly int indentAmount = 4;
    private int indentLevel;
    private bool indentPending = true;
    
    public CodeWriter() { }

    public CodeWriter(CodeWriter baseWriter)
    {
        indentLevel = baseWriter.indentAmount;
    }
    
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
        builder.AppendLine(name);
        builder.AppendLine("{");
        In();
    }

    public void OpenBlock()
    {
        AppendLine("{");
        In();
    }

    public void CloseBlock()
    {
        Out();
        AppendLine("}");
    }

    public void StartClass(string name, string accessModifier = "public", string? bases = null, bool skipGenCode = false)
    {
        if (!skipGenCode)
        {
            AppendLine(GeneratorStatics.GeneratedCodeAttribute);
        }

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
    
    public void Append(string value)
    {
        if (indentPending)
        {
            indentPending = false;
            builder.Append(' ', indentLevel * indentAmount);
        }
        builder.Append(value);
    }

    public void Append(int value)
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
        indentPending = true;
    }

    public override string ToString() => builder.ToString();
}