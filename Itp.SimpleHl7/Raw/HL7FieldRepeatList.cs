using Itp.SimpleHL7.Utilities;
using System.Text;

namespace Itp.SimpleHL7.Raw;

internal class HL7FieldRepeatList : List<object?>
{
    public static object? Add(object? existing, object? addl)
    {
        if (existing is null) return addl;
        if (existing is HL7FieldRepeatList l)
        {
            l.Add(addl);
            return l;
        }
        return new HL7FieldRepeatList() { existing, addl };
    }

    public static object? Set(object? existing, int offset0Based, object? value, FieldValueMutator mutator)
    {
        if (existing is HL7FieldRepeatList l)
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
            var l2 = new HL7FieldRepeatList() { existing };
            l2.EnsureSizeAndSet(offset0Based, mutator(null, value));
            return l2;
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        ToString(sb, HL7Separators.Default, this);
        return sb.ToString();
    }

    public static void ToString(StringBuilder sb, HL7Separators separators, object? fieldValue)
    {
        if (fieldValue is null) return;

        if (fieldValue is HL7FieldRepeatList l)
        {
            for (int i = 0; i < l.Count; i++)
            {
                if (i > 0) sb.Append(separators.Repeat);
                HL7ComponentList.ToString(sb, separators, l[i]);
            }
        }
        else
        {
            HL7ComponentList.ToString(sb, separators, fieldValue);
        }
    }
}

// Opaque struct for setting a field.  Dual of RawHL7FieldRepeatList
// 
// Can be:
// - RawHL7FieldRepeatList
// - RawHL7ComponentList
// - RawHL7SubcomponentList
// - string
public struct SetHL7Repeats
{
    internal object? Value;

    public SetHL7Repeats(string? value)
    {
        Value = value;
    }

    public SetHL7Repeats(params SetHL7Components[] repeats)
    {
        if (repeats == null || repeats.Length == 0)
        {
            Value = null;
        }
        else if (repeats.Length == 1)
        {
            Value = repeats[0].Value;
        }
        else
        {
            var compList = new HL7FieldRepeatList();
            compList.AddRange(repeats.Select(c => c.Value));
            Value = compList;
        }
    }

    public static implicit operator SetHL7Repeats(SetHL7Components value)
        => new SetHL7Repeats() { Value = value.Value };

    public static implicit operator SetHL7Repeats(SetHL7Subcomponents value)
        => new SetHL7Repeats() { Value = value.Value };

    public static implicit operator SetHL7Repeats(string? value)
        => new SetHL7Repeats(value);
}
