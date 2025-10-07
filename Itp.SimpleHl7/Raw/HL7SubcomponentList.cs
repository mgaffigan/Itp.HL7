using Itp.SimpleHL7.Utilities;
using System.Text;

namespace Itp.SimpleHL7.Raw;

internal class HL7SubcomponentList : List<object?>
{
    public static object? FromList(string?[] subcomponents)
    {
        if (subcomponents.Length == 0) return null;
        if (subcomponents.Length == 1) return subcomponents[0];

        var subcomps = new HL7SubcomponentList();
        subcomps.AddRange(subcomponents);
        return subcomps;
    }

    public static FieldValueMutator CreateSetter(int offset0Based) => (existing, value) =>
    {
        if (existing is HL7SubcomponentList l)
        {
            l.EnsureSizeAndSet(offset0Based, value);
            return l;
        }
        else if (offset0Based == 0)
        {
            return value;
        }
        else
        {
            var l2 = new HL7SubcomponentList() { existing };
            l2.EnsureSizeAndSet(offset0Based, value);
            return l2;
        }
    };

    public override string ToString()
    {
        var sb = new StringBuilder();
        ToString(sb, HL7Separators.Default, this);
        return sb.ToString();
    }

    public static void ToString(StringBuilder sb, HL7Separators separators, object? subcomponentValue)
    {
        if (subcomponentValue is null) return;
        if (subcomponentValue is HL7SubcomponentList l)
        {
            for (int i = 0; i < l.Count; i++)
            {
                if (i > 0) sb.Append(separators.Subcomponent);
                if (l[i] is string s) sb.Append(separators.EncodeString(s));
            }
        }
        else if (subcomponentValue is string s)
        {
            sb.Append(separators.EncodeString(s));
        }
        else
        {
            throw new InvalidOperationException($"Invalid subcomponent value type {subcomponentValue.GetType().FullName}");
        }
    }
}

// Opaque struct for setting a field.  Dual of RawHL7ComponentList
// 
// Can be:
// - RawHL7SubcomponentList
// - string
public struct SetHL7Subcomponents
{
    internal object? Value;

    public SetHL7Subcomponents(string? value)
    {
        Value = value;
    }

    public SetHL7Subcomponents(params string?[] subcomponents)
    {
        Value = HL7SubcomponentList.FromList(subcomponents);
    }

    public static implicit operator SetHL7Subcomponents(string? value)
        => new SetHL7Subcomponents(value);
}
