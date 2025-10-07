using System.Collections;
using System.Text;

namespace Itp.SimpleHL7.Raw;

public class HL7Message : IList<HL7Segment>
{
    private List<HL7Segment> _segments = new();

    #region IList<RawHL7Segment>

    public HL7Segment this[int index] { get => ((IList<HL7Segment>)_segments)[index]; set => ((IList<HL7Segment>)_segments)[index] = value; }

    public int Count => ((ICollection<HL7Segment>)_segments).Count;

    public bool IsReadOnly => ((ICollection<HL7Segment>)_segments).IsReadOnly;

    public void Add(HL7Segment item)
    {
        ((ICollection<HL7Segment>)_segments).Add(item);
    }

    public void Clear()
    {
        ((ICollection<HL7Segment>)_segments).Clear();
    }

    public bool Contains(HL7Segment item)
    {
        return ((ICollection<HL7Segment>)_segments).Contains(item);
    }

    public void CopyTo(HL7Segment[] array, int arrayIndex)
    {
        ((ICollection<HL7Segment>)_segments).CopyTo(array, arrayIndex);
    }

    public IEnumerator<HL7Segment> GetEnumerator()
    {
        return ((IEnumerable<HL7Segment>)_segments).GetEnumerator();
    }

    public int IndexOf(HL7Segment item)
    {
        return ((IList<HL7Segment>)_segments).IndexOf(item);
    }

    public void Insert(int index, HL7Segment item)
    {
        ((IList<HL7Segment>)_segments).Insert(index, item);
    }

    public bool Remove(HL7Segment item)
    {
        return ((ICollection<HL7Segment>)_segments).Remove(item);
    }

    public void RemoveAt(int index)
    {
        ((IList<HL7Segment>)_segments).RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_segments).GetEnumerator();
    }

    #endregion

    #region First Segment Accessor

    public HL7Segment MSH => First("MSH");

    public HL7Segment this[string segmentId]
    {
        get => First(segmentId);
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            for (int i = 0; i < _segments.Count; i++)
            {
                if (_segments[i].SegmentID == segmentId)
                {
                    _segments[i] = value;
                    return;
                }
            }
            this.Add(value);
        }
    }

    #endregion

    #region Get Helpers

    public IEnumerable<HL7Segment> OfSegment(string segmentId)
        => _segments.Where(s => s.SegmentID == segmentId);

    public HL7Segment? FirstOrDefault(string segmentId)
        => OfSegment(segmentId).FirstOrDefault();

    public HL7Segment First(string segmentId)
        => OfSegment(segmentId).First();

    public HL7Segment? SingleOrDefault(string segmentId)
        => OfSegment(segmentId).SingleOrDefault();

    public HL7Segment Single(string segmentId)
        => OfSegment(segmentId).Single();

    #endregion

    public HL7Segment Add(string segmentId, params SetHL7Repeats[] fields)
    {
        var seg = new HL7Segment(segmentId, fields);
        Add(seg);
        return seg;
    }

    public static HL7Message Parse(string s)
        => Parse(s, Encoding.UTF8);

    public static HL7Message Parse(string s, Encoding enc)
    {
        var lexer = new HL7Lexer(s, enc);
        var msg = new HL7Message();
        msg.Add(HL7Segment.ReadMsh(ref lexer));
        while (lexer.Type != HL7TokenType.EndOfMessage)
        {
            msg.Add(HL7Segment.ReadOther(ref lexer));
        }
        return msg;
    }

    public override string ToString()
    {
        var msh = FirstOrDefault("MSH");
        var separators = msh is not null ? HL7Separators.FromMSH12(msh[1], msh[2]) : HL7Separators.Default;

        var sb = new StringBuilder();
        foreach (var seg in _segments)
        {
            if (sb.Length > 0) sb.Append('\r');
            seg.ToString(sb, separators);
        }
        return sb.ToString();
    }

    public HL7Message CreateAck(string code = "AA", string? textMessage = null)
    {
        var ack = new HL7Message();
        var msh = this.MSH;
        ack.Add(HL7Segment.CreateMSH(
            sendingApp: msh[5], sendingFac: msh[6],
            receivingApp: msh[3], receivingFac: msh[4],
            msgCode: "ACK", msgEvent: msh[9, component: 2], msgStruct: "ACK"));
        var msa = ack.Add("MSA", code, msh[9 /* control number */]);
        if (textMessage is not null) msa[3] = textMessage;
        return ack;
    }
}
