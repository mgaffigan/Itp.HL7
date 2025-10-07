using Itp.SimpleHL7.Utilities;
using System.Globalization;
using System.Text;

namespace Itp.SimpleHL7.Raw;

public class HL7Segment
{
    // Each field can be:
    // string                       1       A simple value
    // RawHL7FieldRepeatList        1~2     A simple value with repeats
    // RawHL7ComponentList          A field with components and repeats
    // RawHL7SubcomponentList       A field with components, subcomponents and repeats
    // or any nesting of RawHL7FieldRepeatList -> RawHL7ComponentList -> RawHL7SubcomponentList
    private readonly List<object?> _fields = new();
    public string SegmentID { get; }

    public HL7Segment(string segmentId)
    {
        ArgumentNullException.ThrowIfNull(segmentId);
        if (segmentId.Length != 3)
        {
            throw new ArgumentOutOfRangeException(nameof(segmentId), $"Invalid segment ID: {segmentId}");
        }

        this.SegmentID = segmentId;
    }

    public HL7Segment(string segmentId, params IEnumerable<SetHL7Repeats> fields)
        : this(segmentId)
    {
        _fields.AddRange(fields.Select(s => s.Value));
    }

    #region Field Accessors

    public string this[int field, int repeat = 1, int component = 1, int subcomponent = 1]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(field, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(repeat, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(component, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(subcomponent, 1);

            var fieldValue = _fields.Count >= field ? _fields[field - 1] : null;
            return fieldValue
                .UnwrapIf<HL7FieldRepeatList>(repeat - 1)
                .UnwrapIf<HL7ComponentList>(component - 1)
                .UnwrapIf<HL7SubcomponentList>(subcomponent - 1)
                as string ?? string.Empty;
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(field, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(repeat, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(component, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(subcomponent, 1);

            _fields.EnsureSize(field);
            _fields[field - 1] = HL7FieldRepeatList.Set(
                _fields[field - 1], repeat - 1, value,
                HL7ComponentList.CreateSetter(component - 1,
                    HL7SubcomponentList.CreateSetter(subcomponent - 1)
                )
            );
        }
    }

    public int FieldCount => _fields.Count;

    public LengthAccessor Length => new(this);

    public struct LengthAccessor(HL7Segment @this)
    {
        public int this[int field, int repeat = -1, int component = -1]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(field, 1);
                if (!(repeat == -1 || repeat >= 1)) throw new ArgumentOutOfRangeException(nameof(repeat));
                if (!(component == -1 || component >= 1)) throw new ArgumentOutOfRangeException(nameof(component));

                if (field > @this._fields.Count) return 0;
                var fieldValue = @this._fields[field - 1];
                if (fieldValue is null) return 1;

                if (repeat == -1)
                {
                    if (fieldValue is HL7FieldRepeatList l1) return l1.Count;
                    return 1;
                }

                fieldValue = fieldValue.UnwrapIf<HL7FieldRepeatList>(repeat - 1, out var wasOutOfRange);
                if (fieldValue is null) return wasOutOfRange ? 0 : 1;

                if (component == -1)
                {
                    if (fieldValue is HL7ComponentList l2) return l2.Count;
                    return 1;
                }

                fieldValue = fieldValue.UnwrapIf<HL7ComponentList>(component - 1, out wasOutOfRange);
                if (fieldValue is null) return wasOutOfRange ? 0 : 1;

                if (fieldValue is HL7SubcomponentList l3) return l3.Count;
                return 1;
            }
        }
    }

    public FieldRepeatsAccessor FieldRepeats => new(this);

    public struct FieldRepeatsAccessor(HL7Segment @this)
    {
        public IEnumerable<FieldRepeatAccessor> this[int field]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(field, 1);

                var fieldValue = field > @this._fields.Count ? null : @this._fields[field - 1];
                if (fieldValue is null)
                {
                    yield break;
                }

                if (fieldValue is HL7FieldRepeatList l)
                {
                    for (int i = 0; i < l.Count; i++)
                    {
                        yield return new FieldRepeatAccessor(@this, field, i + 1);
                    }
                }
                else
                {
                    yield return new FieldRepeatAccessor(@this, field, 1);
                }
            }
        }
    }

    public struct FieldRepeatAccessor(HL7Segment @this, int field, int repeat)
    {
        public string this[int component, int subcomponent = 1] => @this[field, repeat, component, subcomponent];

        public static implicit operator string(FieldRepeatAccessor accessor) => accessor[1, 1];
    }

    public void Add(int field, string value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(field, 1);

        _fields.EnsureSize(field);
        _fields[field - 1] = HL7FieldRepeatList.Add(_fields[field - 1], value);
    }

    public void Add(int field, params SetHL7Subcomponents[] components)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(field, 1);

        _fields.EnsureSize(field);
        _fields[field - 1] = HL7FieldRepeatList.Add(_fields[field - 1],
            new SetHL7Components(components).Value);
    }

    public void AddSubcomponents(int field, SetHL7Components repeat)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(field, 1);

        _fields.EnsureSize(field);
        _fields[field - 1] = HL7FieldRepeatList.Add(_fields[field - 1], repeat.Value);
    }

    public void Set(int field, string value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(field, 1);

        _fields.EnsureSize(field);
        _fields[field - 1] = value;
    }

    public void Set(int field, SetHL7Repeats repeats)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(field, 1);
        _fields.EnsureSize(field);
        _fields[field - 1] = repeats.Value;
    }

    public void Set(int field, params SetHL7Subcomponents[] repeats)
        => Set(field, new SetHL7Components(repeats));

    #endregion

    #region Parse/Serialize

    public override string ToString()
    {
        var sb = new StringBuilder();
        ToString(sb, HL7Separators.Default);
        return sb.ToString();
    }

    public void ToString(StringBuilder sb, HL7Separators separators)
    {
        sb.Append(separators.EncodeString(SegmentID));
        for (int i = 0; i < _fields.Count; i++)
        {
            sb.Append(separators.Field);
            if (SegmentID == "MSH" && i == 0)
            {
                // Special case MSH-2, which is the encoding characters
                sb.Append(separators.MSH2);
                i += 1;
            }
            else
            {
                HL7FieldRepeatList.ToString(sb, separators, _fields[i]);
            }
        }
    }

    public static HL7Segment Parse(string s)
        => Parse(s, Encoding.UTF8);

    public static HL7Segment Parse(string s, Encoding enc)
    {
        if (s.StartsWith("MSH", StringComparison.Ordinal))
        {
            var lexer = new HL7Lexer(s, enc);
            var result = HL7Segment.ReadMsh(ref lexer);
            if (lexer.Type != HL7TokenType.EndOfMessage)
            {
                throw new InvalidOperationException("Unexpected data after MSH segment");
            }
            return result;
        }
        else
        {
            return Parse(s, HL7Separators.Default, enc);
        }
    }

    public static HL7Segment Parse(string s, HL7Separators separators, Encoding enc)
    {
        if (s.StartsWith("MSH", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Cannot specify separators when parsing MSH segment");
        }

        var lexer = new HL7Lexer($"MSH{separators.Field}{separators.MSH2}\r{s}", enc);

        // Get rid of our header
        _ = HL7Segment.ReadMsh(ref lexer);

        var result = HL7Segment.ReadOther(ref lexer);
        if (lexer.Type != HL7TokenType.EndOfMessage)
        {
            throw new InvalidOperationException("Unexpected data after segment");
        }
        return result;
    }

    // Expects StartOfMessage, Exits with start of segment (Value) or EndOfMessage
    internal static HL7Segment ReadMsh(ref HL7Lexer lexer)
    {
        // StartOfMessage == MSH|^~\&
        // Next token: FieldSeparator, but could be any valid end of segment token
        lexer.Consume(HL7TokenType.StartOfMessage);

        var segment = new HL7Segment("MSH");
        segment[1] = lexer.Separators.Field.ToString();
        segment[2] = lexer.Separators.MSH2;
        segment.ReadFields(ref lexer, 3);
        return segment;
    }

    // Expects start of segment (Value), Exits with start of segment (Value) or EndOfMessage
    internal static HL7Segment ReadOther(ref HL7Lexer lexer)
    {
        if (lexer.Type != HL7TokenType.Value)
        {
            throw new InvalidOperationException($"Expected segment ID, got {lexer.Type}");
        }

        var segment = new HL7Segment(lexer.Value.ToString());
        segment.ReadFields(ref lexer, 0);
        return segment;
    }

    // Exits with start of segment (Value) or EndOfMessage
    private void ReadFields(ref HL7Lexer lexer, int field)
    {
        var repeat = 1;
        var component = 1;
        var subcomponent = 1;
        while (lexer.Type.IsFieldOrValue())
        {
            lexer.Consume();
            if (lexer.Type == HL7TokenType.Value)
            {
                this[field, repeat, component, subcomponent] = lexer.Value.ToString();
            }
            else if (lexer.Type == HL7TokenType.FieldSeparator)
            {
                field++;
                repeat = 1;
                component = 1;
                subcomponent = 1;
                this[field, repeat, component, subcomponent] = null!;
            }
            else if (lexer.Type == HL7TokenType.RepeatSeparator)
            {
                repeat++;
                component = 1;
                subcomponent = 1;
                this[field, repeat, component, subcomponent] = null!;
            }
            else if (lexer.Type == HL7TokenType.ComponentSeparator)
            {
                component++;
                subcomponent = 1;
                this[field, repeat, component, subcomponent] = null!;
            }
            else if (lexer.Type == HL7TokenType.SubcomponentSeparator)
            {
                subcomponent++;
                this[field, repeat, component, subcomponent] = null!;
            }
        }
        if (lexer.Type != HL7TokenType.EndOfMessage)
        {
            lexer.Consume(HL7TokenType.EndOfSegment);
        }
    }

    #endregion

    public static HL7Segment CreateMSH(
        string sendingApp, string sendingFac,
        string receivingApp, string receivingFac,
        string msgCode, string msgEvent, string msgStruct,
        string? messageControlId = default, DateTime messageDateTime = default,
        string processingId = "P", string versionId = "2.5",
        HL7Separators? separators = null)
    {
        separators ??= HL7Separators.Default;
        messageDateTime = messageDateTime == default ? DateTime.UtcNow : messageDateTime;
        messageControlId ??= Guid.NewGuid().ToString("n").Substring(0, 20);

        return new HL7Segment("MSH")
        {
            [1] = separators.Field.ToString(),
            [2] = separators.MSH2,
            [3] = sendingApp,
            [4] = sendingFac,
            [5] = receivingApp,
            [6] = receivingFac,
            [7] = messageDateTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            [9, component: 1] = msgCode,
            [9, component: 2] = msgEvent,
            [9, component: 3] = msgStruct,
            [10] = messageControlId,
            [11] = processingId,
            [12] = versionId
        };
    }
}

internal delegate object? FieldValueMutator(object? existing, object? value);
