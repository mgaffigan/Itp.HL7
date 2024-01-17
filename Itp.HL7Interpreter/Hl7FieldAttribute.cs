using Itp.HL7Interpreter.Formatters;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Itp.HL7Interpreter;

[AttributeUsage(AttributeTargets.Property)]
public class Hl7FieldAttribute : Attribute
{
    private static readonly Regex rCamel = new Regex("([^A-Z])(\\B[A-Z])");

    public IFormatter Formatter { get; set; }

    public int Length { get; set; }

    public bool Required { get; set; }

    public int Sequence { get; set; }

    public Hl7FieldAttribute() : this(0, 0, false)
    {
    }

    public Hl7FieldAttribute(int seq, int len, bool req)
        : this(seq, len, req, null)
    {
    }

    public Hl7FieldAttribute(int seq, int len, bool req, Type? formatter)
    {
        this.Sequence = seq;
        this.Length = len;
        this.Required = req;
        if (formatter != null)
        {
            this.Formatter = (IFormatter)formatter.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public).GetValue(null, null);
        }
        else
        {
            this.Formatter = HL7StringFormatter.Instance;
        }
    }

    public static string CamelToProperCase(string ifn)
    {
        return rCamel.Replace(ifn, "$1 $2");
    }

    public static object? GetDefault(Type type)
    {
        return !type.IsValueType ? null : Activator.CreateInstance(type);
    }

    public static void JoinFields(string? separator, object target, StringBuilder sb, int fieldsToIgnore)
    {
        JoinFields(separator ?? "|", target, sb, fieldsToIgnore, true);
    }

    public static void JoinFields(string separator, object target, StringBuilder sb, int fieldsToIgnore, bool lastSep)
    {
        int maxField = 0;
        var values = new Dictionary<int, string>();
        foreach (var prop in target.GetType().GetProperties())
        {
            foreach (var attr in prop.GetCustomAttributes<Hl7FieldAttribute>(inherit: true))
            {
                object value = prop.GetValue(target, null);
                if (value is not null && !Equals(value, GetDefault(prop.PropertyType)))
                {
                    string str = attr.Formatter.Encode(value);
                    if (!string.IsNullOrWhiteSpace(str) || attr.Required)
                    {
                        maxField = Math.Max(maxField, attr.Sequence);
                        values.Add(attr.Sequence, str);
                    }
                }
            }
        }

        for (int i = 0; i < maxField; i++)
        {
            if (values.TryGetValue(i, out var s))
            {
                sb.Append(s);
            }
            if (i != maxField - 1)
            {
                sb.Append(separator);
            }
        }
    }

    public static void SplitFields(string data, char separator, object target)
    {
        SplitFields(data, separator, target, 0);
    }

    public static void SplitFields(string data, char separator, object target, int fieldsToIgnore)
    {
#if NET
        var fields = new Dictionary<int, HL7FieldInfo>(
#else
        var fields = (
#endif
            from prop in target.GetType().GetProperties()
            from attr in prop.GetCustomAttributes<Hl7FieldAttribute>(inherit: true)
            select new KeyValuePair<int, HL7FieldInfo>(attr.Sequence - fieldsToIgnore, new HL7FieldInfo(attr, prop))
#if NET
        );
#else
        ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
#endif

        var strArrays = data.Split(new char[] { separator });
        for (int field = 0; field < (int)strArrays.Length; field++)
        {
            if (!fields.TryGetValue(field, out var attribute))
            {
                continue;
            }

            // HL7 Empty string is ""
            var fieldValue = strArrays[field];
            if (fieldValue == "\"\"")
            {
                continue;
            }
            if (string.IsNullOrWhiteSpace(fieldValue))
            {
                continue;
            }
            fieldValue = fieldValue.Trim();

            try
            {
                if (fields[field].Property.PropertyType.IsSubclassOf(typeof(Part)))
                {
                    var part = (Part)Activator.CreateInstance(fields[field].Property.PropertyType);
                    part.Parse(fieldValue);
                    fields[field].Property.SetValue(target, part, null);
                }
                else
                {
                    object obj = attribute.Attribute.Formatter.Decode(fieldValue);
                    fields[field].Property.SetValue(target, obj, null);
                }
            }
            catch (Exception ex)
            {
                throw new FormatException($"Could not parse field {attribute.Attribute.Sequence} ({fields[field].Property.Name}) from value '{strArrays[field]}'", ex);
            }
        }
    }

    public static void ToHumaneReadable(string separator, object target, StringBuilder sb, int fieldsToIgnore)
    {
        int colWidth = 0;
        var strs = new Dictionary<string, string>();
        foreach (var pi in target.GetType().GetProperties())
        {
            foreach (var attr in pi.GetCustomAttributes<Hl7FieldAttribute>(inherit: true))
            {
                object value = pi.GetValue(target, null);
                if (value is null || Equals(value, GetDefault(pi.PropertyType)))
                {
                    continue;
                }

                string str = value.ToString();
                if (string.IsNullOrWhiteSpace(str) && !attr.Required)
                {
                    continue;
                }

                string fieldName = rCamel.Replace(pi.Name, "$1 $2");
                colWidth = Math.Max(colWidth, fieldName.Length);
                strs.Add(fieldName, str);
            }
        }

        foreach (var keyValuePair in strs)
        {
            sb.AppendFormat(string.Concat("{0,", colWidth.ToString(), "} : {1}\n"), keyValuePair.Key, keyValuePair.Value);
        }
    }

    private class HL7FieldInfo
    {
        public Hl7FieldAttribute Attribute;

        public PropertyInfo Property;

        public HL7FieldInfo(Hl7FieldAttribute attribute, PropertyInfo property)
        {
            this.Attribute = attribute;
            this.Property = property;
        }
    }
}