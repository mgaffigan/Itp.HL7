using Itp.SimpleHL7.Utilities;
using System.Text;

namespace Itp.SimpleHL7.Raw;

internal class HL7ComponentList : List<object?>
{
    public static object? FromList(string?[][] componentsAndSubcomponents)
    {
        if (componentsAndSubcomponents.Length == 0) return null;
        if (componentsAndSubcomponents.Length == 1 && componentsAndSubcomponents[0].Length == 1)
            return componentsAndSubcomponents[0][0];

        var comps = new HL7ComponentList();
        comps.AddRange(componentsAndSubcomponents.Select(HL7SubcomponentList.FromList));
        return comps;
    }

    public static object? FromList(string?[] components)
    {
        if (components.Length == 0) return null;
        if (components.Length == 1) return components[0];

        var comps = new HL7ComponentList();
        comps.AddRange(components);
        return comps;
    }

    public static FieldValueMutator CreateSetter(int offset0Based, FieldValueMutator mutator) => (existing, value) =>
    {
        if (existing is HL7ComponentList l)
        {
            l.EnsureSize(offset0Based + 1);
            l[offset0Based] = mutator(l[offset0Based], value);
            return l;
        }
        else if (offset0Based == 0)
        {
            return mutator(existing, value);
        }
        else
        {
            var l2 = new HL7ComponentList() { existing };
            l2.EnsureSizeAndSet(offset0Based, mutator(null, value));
            return l2;
        }
    };

    public override string ToString()
    {
        var sb = new StringBuilder();
        ToString(sb, HL7Separators.Default, this);
        return sb.ToString();
    }

    public static void ToString(StringBuilder sb, HL7Separators separators, object? componentValue)
    {
        if (componentValue is null) return;
        if (componentValue is HL7ComponentList l)
        {
            for (int i = 0; i < l.Count; i++)
            {
                if (i > 0) sb.Append(separators.Component);
                HL7SubcomponentList.ToString(sb, separators, l[i]);
            }
        }
        else
        {
            HL7SubcomponentList.ToString(sb, separators, componentValue);
        }
    }
}

// Opaque struct for setting a field.  Dual of RawHL7ComponentList
// 
// Can be:
// - RawHL7ComponentList
// - RawHL7SubcomponentList
// - string
public struct SetHL7Components
{
    internal object? Value;

    public SetHL7Components(string? value)
    {
        Value = value;
    }

    public SetHL7Components(params SetHL7Subcomponents[] components)
    {
        if (components == null || components.Length == 0)
        {
            Value = null;
        }
        else if (components.Length == 1)
        {
            Value = components[0].Value;
        }
        else
        {
            var compList = new HL7ComponentList();
            compList.AddRange(components.Select(c => c.Value));
            Value = compList;
        }
    }

    public static implicit operator SetHL7Components(SetHL7Subcomponents value)
        => new SetHL7Components(value);

    public static implicit operator SetHL7Components(string? value)
        => new SetHL7Components(value);
}
