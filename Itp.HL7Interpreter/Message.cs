using Itp.HL7Interpreter.Segments;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Itp.HL7Interpreter;

public class Message : List<Segment>
{
    public const string FileSeparator = "\u001c";

    public const string MessageStart = "\v";

    public const string MessageEnd = "\r";

    public MessageHeader? Header { get; private set; }

    public Segment? this[string s]
    {
        get
        {
            if (s.Equals("MSH", StringComparison.CurrentCultureIgnoreCase))
            {
                return this.Header;
            }

            int num = 0;
            while (num < Count)
            {
                if (!base[num].MessageIdentifier.Equals(s, StringComparison.CurrentCultureIgnoreCase))
                {
                    num++;
                }
                return base[num];
            }

            throw new IndexOutOfRangeException();
        }
        set
        {
            throw new NotImplementedException();
        }
    }

    public Message()
    {
    }

    public new void Add(Segment s)
    {
        if (s is MessageHeader h)
        {
            this.Header = h;
            foreach (Segment segment in this)
            {
                segment.HeaderSegment = h;
            }
            base.Add(s);
        }
        else
        {
            s.HeaderSegment = this.Header!;
            base.Add(s);
        }
    }

    public new void AddRange(IEnumerable<Segment> segments)
    {
        foreach (Segment segment in segments)
        {
            this.Add(segment);
        }
    }

    public static Message FromString(string data, char defaultSeparator = '|', SegmentTypeResolver? typeResolver = null)
    {
        if (string.IsNullOrWhiteSpace(data) || data.IndexOf("MSH") > 5)
        {
            throw new ArgumentException(data);
        }

        char segSep = defaultSeparator;
        if (data.StartsWith("MSH"))
        {
            segSep = data[3];
        }

        var message = new Message();
        foreach (var segText in data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (string.IsNullOrWhiteSpace(segText) || segText[0] == '\u001C')
            {
                continue;
            }

            var segment = Segment.Parse(segText, segSep, typeResolver);
            if (segment != null)
            {
                message.Add(segment);
            }
            else
            {
                Debug.WriteLine(string.Concat("Could not find a segment for: ", segText));
            }
        }
        return message;
    }

    public T GetSegment<T>(int index)
        where T : Segment
    {
        if (typeof(T).Equals(typeof(MessageHeader)))
        {
            return (T)(object)this.Header!;
        }

        return this.OfType<T>().ElementAt(index);
    }

    public T GetSegment<T>()
        where T : Segment
    {
        return this.GetSegment<T>(0);
    }

    public bool HasSegment<T>()
    {
        if (typeof(T).Equals(typeof(MessageHeader)))
        {
            return true;
        }

        return this.OfType<T>().Any();
    }

    public string ToHumanReadable()
    {
        var stringBuilder = new StringBuilder();
        foreach (var segment in this)
        {
            segment.ToHumanReadable(stringBuilder);
        }
        return stringBuilder.ToString();
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(this.Header?.ToString());
        stringBuilder.Append("\r");
        foreach (Segment segment in this)
        {
            if (segment is MessageHeader)
            {
                continue;
            }

            stringBuilder.Append(segment.ToString());
            stringBuilder.Append("\r");
        }

        return stringBuilder.ToString();
    }
}