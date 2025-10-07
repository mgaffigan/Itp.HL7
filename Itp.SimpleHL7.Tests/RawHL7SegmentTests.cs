using Itp.SimpleHL7.Raw;
using System.Text;

namespace Itp.SimpleHL7.Tests;

[TestClass]
public class RawHL7SegmentTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_ValidSegmentId_SetsSegmentID()
    {
        var segment = new HL7Segment("PID");
        Assert.AreEqual("PID", segment.SegmentID);
    }

    [TestMethod]
    public void Constructor_WithFields_SetsSegmentIDAndFields()
    {
        var segment = new HL7Segment("PID", "field1", "field2", "field3");
        Assert.AreEqual("PID", segment.SegmentID);
        Assert.AreEqual("field1", segment[1]);
        Assert.AreEqual("field2", segment[2]);
        Assert.AreEqual("field3", segment[3]);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_NullSegmentId_ThrowsArgumentNullException()
    {
        new HL7Segment(null!);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("P")]
    [DataRow("PI")]
    [DataRow("PIDX")]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Constructor_InvalidSegmentIdLength_ThrowsArgumentOutOfRangeException(string segmentId)
    {
        new HL7Segment(segmentId);
    }

    [TestMethod]
    public void ZeroFields_Serializes()
    {
        var segment = new HL7Segment("PID");
        Assert.AreEqual(0, segment.FieldCount);
        Assert.AreEqual("PID", segment.ToString());
    }

    [TestMethod]
    public void Length_Specified_Field()
    {
        var segment = HL7Segment.Parse("PID|||3");
        Assert.AreEqual(3, segment.FieldCount);
        Assert.AreEqual(1, segment.Length[1]);
        Assert.AreEqual(1, segment.Length[3]);
        Assert.AreEqual(0, segment.Length[4]);
    }

    [TestMethod]
    public void Length_Specified_Repeat()
    {
        var segment = HL7Segment.Parse("PID|~1");
        Assert.AreEqual(1, segment.FieldCount);
        Assert.AreEqual(2, segment.Length[1]);
        Assert.AreEqual(1, segment.Length[1, 1]);
        Assert.AreEqual(0, segment.Length[1, 3]);
    }

    [TestMethod]
    public void Length_Specified_Component()
    {
        var segment = HL7Segment.Parse("PID|^1");
        Assert.AreEqual(1, segment.FieldCount);
        Assert.AreEqual(2, segment.Length[1, 1]);
        Assert.AreEqual(1, segment.Length[1, 1, 1]);
        Assert.AreEqual(0, segment.Length[1, 1, 3]);
    }

    [TestMethod]
    public void Length_Specified_Subcomponent()
    {
        var segment = HL7Segment.Parse("PID|&1");
        Assert.AreEqual(1, segment.FieldCount);
        Assert.AreEqual(2, segment.Length[1, 1, 1]);
    }

    [TestMethod]
    public void ZeroFields_Parses()
    {
        var segment = HL7Segment.Parse("PID");
        Assert.AreEqual("PID", segment.SegmentID);
        Assert.AreEqual(0, segment.FieldCount);
    }

    [TestMethod]
    public void AllEmpty_Parses()
    {
        var segment = HL7Segment.Parse("PID|");
        Assert.AreEqual("PID", segment.SegmentID);
        Assert.AreEqual(1, segment.FieldCount);
    }

    [TestMethod]
    public void AllEmpty_Parses_3()
    {
        var segment = HL7Segment.Parse("PID|||");
        Assert.AreEqual("PID", segment.SegmentID);
        Assert.AreEqual(3, segment.FieldCount);
    }

    [TestMethod]
    public void AllEmpty_Parses_Subparts()
    {
        // Verify on the right
        var segment = HL7Segment.Parse("PID|~^^&&&||||");
        Assert.AreEqual(5, segment.FieldCount);
        Assert.AreEqual(2, segment.Length[1]);
        Assert.AreEqual(1, segment.Length[1, 1]);
        Assert.AreEqual(0, segment.Length[1, 3]);
        Assert.AreEqual(3, segment.Length[1, 2]);
        Assert.AreEqual(4, segment.Length[1, 2, 3]);

        // Verify on the left
        segment = HL7Segment.Parse("PID|||||&&&^^~");
        Assert.AreEqual(5, segment.FieldCount);
        Assert.AreEqual(2, segment.Length[5]);
        Assert.AreEqual(3, segment.Length[5, 1]);
        Assert.AreEqual(1, segment.Length[5, 2]);
        Assert.AreEqual(4, segment.Length[5, 1, 1]);
    }

    [TestMethod]
    public void TrailingData_Parses()
    {
        var segment = HL7Segment.Parse("PID||||asdf");
        Assert.AreEqual("PID", segment.SegmentID);
        Assert.AreEqual(4, segment.FieldCount);
        Assert.AreEqual("asdf", segment[4]);
    }

    #endregion

    #region Single Value Field Tests

    [TestMethod]
    public void Indexer_GetSimpleField_ReturnsValue()
    {
        var segment = new HL7Segment("PID");
        segment[1] = "John";
        Assert.AreEqual("John", segment[1]);
        Assert.AreEqual(1, segment.FieldCount);
    }

    [TestMethod]
    public void Indexer_GetNonExistentField_ReturnsEmptyString()
    {
        var segment = new HL7Segment("PID");
        Assert.AreEqual(string.Empty, segment[5]);
        Assert.AreEqual(0, segment.FieldCount);
    }

    [TestMethod]
    public void Indexer_SetNonExistentField_ReturnsEmptyString()
    {
        var segment = new HL7Segment("PID");
        segment[5] = "value";
        Assert.AreEqual(string.Empty, segment[1]);
        Assert.AreEqual("value", segment[5]);
        Assert.AreEqual(5, segment.FieldCount);
    }

    [TestMethod]
    public void Indexer_SetAndGetMultipleFields_ReturnsCorrectValues()
    {
        var segment = new HL7Segment("PID");
        segment[1] = "John";
        segment[3] = "Doe";
        segment[2] = "Middle";

        Assert.AreEqual("John", segment[1]);
        Assert.AreEqual("Middle", segment[2]);
        Assert.AreEqual("Doe", segment[3]);
        Assert.AreEqual(3, segment.FieldCount);
    }

    #endregion

    #region Repeated Field Tests

    [TestMethod]
    public void Add_SingleValue_CreatesRepeat()
    {
        var segment = new HL7Segment("PID");
        segment.Add(1, "First");
        segment.Add(1, "Second");

        Assert.AreEqual(1, segment.FieldCount);
        Assert.AreEqual(2, segment.Length[1]);
        Assert.AreEqual("First", segment[1]);
        Assert.AreEqual("First", segment[1, 1]);
        Assert.AreEqual("Second", segment[1, 2]);
        Assert.AreEqual(string.Empty, segment[1, 1, 2]);
        Assert.AreEqual(string.Empty, segment[2]);
    }

    [TestMethod]
    public void Indexer_SetRepeatedValues_WorksCorrectly()
    {
        var segment = new HL7Segment("PID");
        segment[1, 1] = "First";
        segment[1, 2] = "Second";

        Assert.AreEqual("First", segment[1, 1]);
        Assert.AreEqual("Second", segment[1, 2]);
        Assert.AreEqual(2, segment.Length[1]);
    }

    [TestMethod]
    public void FieldRepeats_AccessRepeatedValues_ReturnsCorrectCount()
    {
        var segment = new HL7Segment("PID");
        segment.Add(1, "First");
        segment.Add(1, "Second");
        segment.Add(1, "Third");

        var repeats = segment.FieldRepeats[1].ToList();
        Assert.AreEqual(3, repeats.Count);
        Assert.AreEqual("First", repeats[0]);
        Assert.AreEqual("Second", repeats[1]);
        Assert.AreEqual("Third", repeats[2]);
    }

    #endregion

    #region Component Tests

    [TestMethod]
    public void Add_Components_CreatesComponentStructure()
    {
        var segment = new HL7Segment("PID");
        segment.Add(1, "Last", "First", "Middle");

        Assert.AreEqual("Last", segment[1, 1, 1]);
        Assert.AreEqual("First", segment[1, 1, 2]);
        Assert.AreEqual("Middle", segment[1, 1, 3]);
        Assert.AreEqual(3, segment.Length[1, 1]);
    }

    [TestMethod]
    public void Set_Components_CreatesComponentStructure()
    {
        var segment = new HL7Segment("PID");
        segment.Set(1, "Last", "First", "Middle");

        Assert.AreEqual("Last", segment[1, 1, 1]);
        Assert.AreEqual("First", segment[1, 1, 2]);
        Assert.AreEqual("Middle", segment[1, 1, 3]);
        Assert.AreEqual(3, segment.Length[1, 1]);
    }

    [TestMethod]
    public void Set_Overwrites()
    {
        var segment = new HL7Segment("PID");
        segment.Set(1, "Last", "First", "Middle");
        Assert.AreEqual("Middle", segment[1, 1, 3]);
        segment.Set(1, "ABC");
        Assert.AreEqual("", segment[1, 1, 3]);
    }

    [TestMethod]
    public void Indexer_SetComponents_WorksCorrectly()
    {
        var segment = new HL7Segment("PID");
        segment[1, 1, 1] = "Last";
        segment[1, 1, 2] = "First";
        segment[1, 1, 3] = "Middle";

        Assert.AreEqual("Last", segment[1, 1, 1]);
        Assert.AreEqual("First", segment[1, 1, 2]);
        Assert.AreEqual("Middle", segment[1, 1, 3]);
    }

    [TestMethod]
    public void Add_ComponentsWithRepeats_CreatesCorrectStructure()
    {
        var segment = new HL7Segment("PID");
        segment.Add(1, "Last1", "First1");
        segment.Add(1, "Last2", "First2");

        Assert.AreEqual("Last1", segment[1, 1, 1]);
        Assert.AreEqual("First1", segment[1, 1, 2]);
        Assert.AreEqual("Last2", segment[1, 2, 1]);
        Assert.AreEqual("First2", segment[1, 2, 2]);
        Assert.AreEqual(2, segment.Length[1]);
    }

    #endregion

    #region Subcomponent Tests

    [TestMethod]
    public void Add_Subcomponents_CreatesSubcomponentStructure()
    {
        var segment = new HL7Segment("PID");
        segment.Add(1, new SetHL7Subcomponents("Last", "Suffix"), "First");

        Assert.AreEqual("Last", segment[1, 1, 1, 1]);
        Assert.AreEqual("Suffix", segment[1, 1, 1, 2]);
        Assert.AreEqual("First", segment[1, 1, 2, 1]);
        Assert.AreEqual(2, segment.Length[1, 1, 1]);
        Assert.AreEqual(1, segment.Length[1, 1, 2]);
    }

    [TestMethod]
    public void Indexer_SetSubcomponents_WorksCorrectly()
    {
        var segment = new HL7Segment("PID");
        segment[1, 1, 1, 1] = "Last";
        segment[1, 1, 1, 2] = "Suffix";
        segment[1, 1, 2, 1] = "First";

        Assert.AreEqual("Last", segment[1, 1, 1, 1]);
        Assert.AreEqual("Suffix", segment[1, 1, 1, 2]);
        Assert.AreEqual("First", segment[1, 1, 2, 1]);
    }

    #endregion

    #region Complex Structure Tests

    [TestMethod]
    public void ComplexStructure_RepeatedFieldsWithComponentsAndSubcomponents_WorksCorrectly()
    {
        var segment = new HL7Segment("PID");

        // First repeat: Last^First with subcomponents
        segment[1, 1, 1, 1] = "Smith";
        segment[1, 1, 1, 2] = "Jr";
        segment[1, 1, 2, 1] = "John";
        segment[1, 1, 3, 1] = "Unique Components";
        segment[1, 1, 4, 1] = "Unique Components";

        // Second repeat: Different name
        segment[1, 2, 1, 1] = "Doe";
        segment[1, 2, 2, 1] = "Jane";
        segment[1, 3, 3, 1] = "Unique Repeat";

        // Third repeat
        segment[1, 3] = "";

        Assert.AreEqual("Smith", segment[1, 1, 1, 1]);
        Assert.AreEqual("Jr", segment[1, 1, 1, 2]);
        Assert.AreEqual("John", segment[1, 1, 2, 1]);
        Assert.AreEqual("Doe", segment[1, 2, 1, 1]);
        Assert.AreEqual("Jane", segment[1, 2, 2, 1]);

        Assert.AreEqual(3, segment.Length[1]); // 2 repeats
        Assert.AreEqual(4, segment.Length[1, 1]); // 2 components in first repeat
        Assert.AreEqual(2, segment.Length[1, 1, 1]); // 2 subcomponents in first component
    }

    #endregion

    #region Length Accessor Tests

    [TestMethod]
    public void Length_EmptyField_ReturnsZero()
    {
        var segment = new HL7Segment("PID");
        Assert.AreEqual(0, segment.Length[1]);
    }

    [TestMethod]
    public void Length_SingleValue_ReturnsOne()
    {
        var segment = new HL7Segment("PID");
        segment[1] = "value";
        Assert.AreEqual(1, segment.Length[1]);
    }

    [TestMethod]
    public void Length_WithComponents_ReturnsCorrectCount()
    {
        var segment = new HL7Segment("PID");
        segment.Add(1, "comp1", "comp2", "comp3");
        Assert.AreEqual(1, segment.Length[1]); // 1 repeat
        Assert.AreEqual(3, segment.Length[1, 1]); // 3 components
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Length_InvalidFieldIndex_ThrowsArgumentOutOfRangeException(int field)
    {
        var segment = new HL7Segment("PID");
        _ = segment.Length[field];
    }

    #endregion

    #region Parse Tests

    [TestMethod]
    public void Parse_SimpleSegment_ParsesCorrectly()
    {
        var segment = HL7Segment.Parse("PID|field1|field2|field3");

        Assert.AreEqual("PID", segment.SegmentID);
        Assert.AreEqual("field1", segment[1]);
        Assert.AreEqual("field2", segment[2]);
        Assert.AreEqual("field3", segment[3]);
    }

    [TestMethod]
    public void Parse_SegmentWithComponents_ParsesCorrectly()
    {
        var segment = HL7Segment.Parse("PID|Last^First^Middle");

        Assert.AreEqual("Last", segment[1, 1, 1]);
        Assert.AreEqual("First", segment[1, 1, 2]);
        Assert.AreEqual("Middle", segment[1, 1, 3]);
    }

    [TestMethod]
    public void Parse_SegmentWithRepeats_ParsesCorrectly()
    {
        var segment = HL7Segment.Parse("PID|value1~value2~value3");

        Assert.AreEqual("value1", segment[1, 1]);
        Assert.AreEqual("value2", segment[1, 2]);
        Assert.AreEqual("value3", segment[1, 3]);
        Assert.AreEqual(3, segment.Length[1]);
    }

    [TestMethod]
    public void Parse_SegmentWithSubcomponents_ParsesCorrectly()
    {
        var segment = HL7Segment.Parse("PID|Last&Jr^First");

        Assert.AreEqual("Last", segment[1, 1, 1, 1]);
        Assert.AreEqual("Jr", segment[1, 1, 1, 2]);
        Assert.AreEqual("First", segment[1, 1, 2, 1]);
    }

    [TestMethod]
    public void Parse_MSHSegment_ParsesCorrectly()
    {
        var segment = HL7Segment.Parse("MSH|^~\\&|SendingApp|SendingFacility");

        Assert.AreEqual("MSH", segment.SegmentID);
        Assert.AreEqual("|", segment[1]);
        Assert.AreEqual("^~\\&", segment[2]);
        Assert.AreEqual("SendingApp", segment[3]);
        Assert.AreEqual("SendingFacility", segment[4]);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Parse_MSHWithCustomSeparators_ThrowsInvalidOperationException()
    {
        var customSeparators = new HL7Separators('|', '^', '&', '~', '\\');
        HL7Segment.Parse("MSH|^~\\&|test", customSeparators, Encoding.UTF8);
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_SimpleSegment_SerializesCorrectly()
    {
        var segment = new HL7Segment("PID");
        segment[1] = "field1";
        segment[2] = "field2";

        var result = segment.ToString();
        Assert.AreEqual("PID|field1|field2", result);
    }

    [TestMethod]
    public void ToString_SegmentWithComponents_SerializesCorrectly()
    {
        var segment = new HL7Segment("PID");
        segment.Add(1, "Last", "First", "Middle");

        var result = segment.ToString();
        Assert.AreEqual("PID|Last^First^Middle", result);
    }

    [TestMethod]
    public void ToString_SegmentWithRepeats_SerializesCorrectly()
    {
        var segment = new HL7Segment("PID");
        segment.Add(1, "value1");
        segment.Add(1, "value2");

        var result = segment.ToString();
        Assert.AreEqual("PID|value1~value2", result);
    }

    [TestMethod]
    public void ToString_SegmentWithSubcomponents_SerializesCorrectly()
    {
        var segment = new HL7Segment("PID");
        segment[1, 1, 1, 1] = "Last";
        segment[1, 1, 1, 2] = "Jr";
        segment[1, 1, 2, 1] = "First";

        var result = segment.ToString();
        Assert.AreEqual("PID|Last&Jr^First", result);
    }

    #endregion

    #region Round-trip Tests

    [TestMethod]
    [DataRow("PID|")]
    [DataRow("PID||")]
    [DataRow("PID|field1|field2|field3")]
    [DataRow("PV1|1|I|ICU^101^A")]
    [DataRow("OBX|1|ST|value1~value2~value3")]
    [DataRow("NK1|1|Last&Jr^First^Middle")]
    [DataRow("MSH|^~\\&|SendingApp|SendingFacility")]
    [DataRow("AL1|1|DA|^Penicillin")]
    [DataRow("DG1|1||A01.1^Typhoid fever")]
    [DataRow("PID|~^^&&&||||")]
    [DataRow("PID|||||&&&^^~")]
    public void RoundTrip_VariousSegments_MaintainsData(string input)
    {
        var parsed = HL7Segment.Parse(input);
        var serialized = parsed.ToString();

        Assert.AreEqual(input, serialized);
    }

    [TestMethod]
    public void RoundTrip_ComplexSegment_MaintainsStructure()
    {
        var original = "PID|1||Last&Jr^First^Middle~Alt&Sr^Jane^Marie";
        var parsed = HL7Segment.Parse(original);
        var serialized = parsed.ToString();

        Assert.AreEqual(original, serialized);

        // Verify structure is maintained
        Assert.AreEqual("Last", parsed[3, 1, 1, 1]);
        Assert.AreEqual("Jr", parsed[3, 1, 1, 2]);
        Assert.AreEqual("First", parsed[3, 1, 2, 1]);
        Assert.AreEqual("Alt", parsed[3, 2, 1, 1]);
        Assert.AreEqual("Sr", parsed[3, 2, 1, 2]);
        Assert.AreEqual("Jane", parsed[3, 2, 2, 1]);
    }

    #endregion

    #region Error Scenarios

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer_InvalidFieldIndex_ThrowsArgumentOutOfRangeException(int field)
    {
        var segment = new HL7Segment("PID");
        _ = segment[field];
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer_InvalidRepeatIndex_ThrowsArgumentOutOfRangeException(int repeat)
    {
        var segment = new HL7Segment("PID");
        _ = segment[1, repeat];
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Add_InvalidFieldIndex_ThrowsArgumentOutOfRangeException(int field)
    {
        var segment = new HL7Segment("PID");
        segment.Add(field, "value");
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Parse_InvalidSegmentFormat_ThrowsInvalidOperationException()
    {
        HL7Segment.Parse("PID|field1\rEXTRA_DATA");
    }

    #endregion

    #region FieldRepeatAccessor Tests

    [TestMethod]
    public void FieldRepeatAccessor_ImplicitStringConversion_WorksCorrectly()
    {
        var segment = new HL7Segment("PID");
        segment[1, 1, 1] = "TestValue";

        string value = segment.FieldRepeats[1].First();
        Assert.AreEqual("TestValue", value);
    }

    [TestMethod]
    public void FieldRepeatAccessor_ComponentAccess_WorksCorrectly()
    {
        var segment = new HL7Segment("PID");
        segment[1, 1, 1] = "First";
        segment[1, 1, 2] = "Second";

        var repeat = segment.FieldRepeats[1].First();
        Assert.AreEqual("First", repeat[1]);
        Assert.AreEqual("Second", repeat[2]);
    }

    [TestMethod]
    public void FieldRepeats_EmptyField_ReturnsEmptyEnumerable()
    {
        var segment = new HL7Segment("PID");
        var repeats = segment.FieldRepeats[1];

        Assert.AreEqual(0, repeats.Count());
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void Segment_EmptyValues_HandledCorrectly()
    {
        var segment = new HL7Segment("PID");
        segment[1] = "";
        segment[2] = "value";
        segment[3] = "";

        Assert.AreEqual("", segment[1]);
        Assert.AreEqual("value", segment[2]);
        Assert.AreEqual("", segment[3]);
    }

    [TestMethod]
    public void Segment_SparseFields_HandledCorrectly()
    {
        var segment = new HL7Segment("PID");
        segment[1] = "field1";
        segment[5] = "field5"; // Skip fields 2-4

        Assert.AreEqual("field1", segment[1]);
        Assert.AreEqual("", segment[2]);
        Assert.AreEqual("", segment[3]);
        Assert.AreEqual("", segment[4]);
        Assert.AreEqual("field5", segment[5]);
    }

    [TestMethod]
    public void Parse_EmptyFields_HandledCorrectly()
    {
        var segment = HL7Segment.Parse("PID|value1||value3||value5");

        Assert.AreEqual("value1", segment[1]);
        Assert.AreEqual("", segment[2]);
        Assert.AreEqual("value3", segment[3]);
        Assert.AreEqual("", segment[4]);
        Assert.AreEqual("value5", segment[5]);
    }

    #endregion
}