namespace Itp.SimpleHL7.Utilities;

internal static class IListExtensions
{
    public static void EnsureSize<T>(this IList<T> list, int size, T? defaultValue = default)
    {
        if (list is List<T> l) l.EnsureCapacity(size);

        while (list.Count < size)
        {
            list.Add(defaultValue!);
        }
    }

    public static void EnsureSizeAndSet(this IList<object?> list, int i, object? value)
    {
        list.EnsureSize(i + 1);
        list[i] = value;
    }

    public static object? UnwrapIf<TUnwrap>(this object? obj, int offset0Based)
        where TUnwrap : List<object?>
        => obj.UnwrapIf<TUnwrap>(offset0Based, out _);

    public static object? UnwrapIf<TUnwrap>(this object? obj, int offset0Based, out bool wasOutOfRange)
        where TUnwrap : List<object?>
    {
        wasOutOfRange = false;
        if (obj is TUnwrap l)
        {
            if (offset0Based >= l.Count)
            {
                wasOutOfRange = true;
                return null;
            }

            return l[offset0Based];
        }
        if (offset0Based == 0) return obj;
        return null;
    }

    public static void Consume(this ref HL7Lexer lexer, HL7TokenType type)
    {
        if (lexer.Type != type)
        {
            throw new InvalidOperationException($"Unexpected token {lexer.Type}, expected {type}");
        }
        if (!lexer.MoveNext())
        {
            throw new InvalidOperationException("Unexpected end of message");
        }
    }

    public static void Consume(this ref HL7Lexer lexer)
    {
        if (!lexer.MoveNext())
        {
            throw new InvalidOperationException("Unexpected end of message");
        }
    }

    public static bool IsFieldOrValue(this HL7TokenType type)
        => type == HL7TokenType.FieldSeparator
        || type == HL7TokenType.RepeatSeparator
        || type == HL7TokenType.ComponentSeparator
        || type == HL7TokenType.SubcomponentSeparator
        || type == HL7TokenType.Value;
}
