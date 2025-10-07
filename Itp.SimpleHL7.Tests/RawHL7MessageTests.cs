using Itp.SimpleHL7.Raw;
using System.ComponentModel;
using System.Text;

namespace Itp.SimpleHL7.Tests;

[TestClass]
public class RawHL7MessageTests
{
    #region Parse Tests

    [TestMethod]
    public void Parse_SimpleMessage_ParsesCorrectly()
    {
        var input = "MSH|^~\\&|SendingApp|SendingFacility|ReceivingApp|ReceivingFacility|20231001120000||ADT^A01|12345|P|2.5\r" +
                   "PID|1||123456789^^^MR||Doe^John^Middle||19800101|M|||123 Main St^^Anytown^ST^12345\r" +
                   "PV1|1|I|ICU^101^A|||||||||||||||12345";

        var message = HL7Message.Parse(input);

        Assert.AreEqual(3, message.Count);
        Assert.AreEqual("MSH", message[0].SegmentID);
        Assert.AreEqual("PID", message[1].SegmentID);
        Assert.AreEqual("PV1", message[2].SegmentID);

        Assert.AreEqual("SendingApp", message.MSH[3]);
        Assert.AreEqual("123456789", message["PID"][3, 1, 1]);
        Assert.AreEqual("ICU", message["PV1"][3, 1, 1]);
    }

    [TestMethod]
    public void Parse_MessageWithRepeatedSegments_ParsesCorrectly()
    {
        var input = "MSH|^~\\&|App|Facility||||||\r" +
                   "OBX|1|ST|Code1|SubId1|Value1\r" +
                   "OBX|2|ST|Code2|SubId2|Value2\r" +
                   "OBX|3|ST|Code3|SubId3|Value3";

        var message = HL7Message.Parse(input);

        Assert.AreEqual(4, message.Count);
        var obxSegments = message.OfSegment("OBX").ToList();
        Assert.AreEqual(3, obxSegments.Count);
        Assert.AreEqual("Value1", obxSegments[0][5]);
        Assert.AreEqual("Value2", obxSegments[1][5]);
        Assert.AreEqual("Value3", obxSegments[2][5]);
    }

    [TestMethod]
    public void Parse_MSHOnly_ParsesCorrectly()
    {
        var input = "MSH|^~\\&|SendingApp|SendingFacility";

        var message = HL7Message.Parse(input);

        Assert.AreEqual(1, message.Count);
        Assert.AreEqual("MSH", message[0].SegmentID);
        Assert.AreEqual("SendingApp", message.MSH[3]);
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_SimpleMessage_SerializesCorrectly()
    {
        var message = new HL7Message();
        message.Add(new HL7Segment("MSH", "|", "^~\\&", "SendingApp", "SendingFacility"));
        message.Add(new HL7Segment("PID", "1", "", "123456789"));

        var result = message.ToString();
        var expected = "MSH|^~\\&|SendingApp|SendingFacility\r" +
                      "PID|1||123456789";

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ToString_EmptyMessage_ReturnsEmptyString()
    {
        var message = new HL7Message();
        var result = message.ToString();
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void ToString_MessageWithoutMSH_UsesDefaultSeparators()
    {
        var message = new HL7Message();
        message.Add(new HL7Segment("PID", "field1", "field2"));

        var result = message.ToString();
        Assert.AreEqual("PID|field1|field2", result);
    }

    #endregion

    #region Round-trip Tests

    [TestMethod]
    [DataRow("MSH|^~\\&|App|Facility\rPID|1||123")]
    [DataRow("MSH|^~\\&|App|Facility||||||\rOBX|1|ST|Code^Display~Code2^Display2")]
    [DataRow("MSH|^~\\&|App|Facility\rPID|1||Last&Jr^First^Middle\rPV1|1|I|ICU^101^A")]
    public void RoundTrip_VariousMessages_MaintainsData(string input)
    {
        var parsed = HL7Message.Parse(input);
        var serialized = parsed.ToString();

        Assert.AreEqual(input, serialized);
    }

    #endregion

    #region IList Implementation Tests

    [TestMethod]
    public void IList_BasicOperations_WorkCorrectly()
    {
        var message = new HL7Message();
        var mshSegment = new HL7Segment("MSH");
        var pidSegment = new HL7Segment("PID");

        // Add
        message.Add(mshSegment);
        message.Add(pidSegment);
        Assert.AreEqual(2, message.Count);

        // Index access
        Assert.AreEqual("MSH", message[0].SegmentID);
        Assert.AreEqual("PID", message[1].SegmentID);

        // Contains
        Assert.IsTrue(message.Contains(mshSegment));
        Assert.IsTrue(message.Contains(pidSegment));

        // IndexOf
        Assert.AreEqual(0, message.IndexOf(mshSegment));
        Assert.AreEqual(1, message.IndexOf(pidSegment));

        // Remove
        Assert.IsTrue(message.Remove(pidSegment));
        Assert.AreEqual(1, message.Count);
        Assert.IsFalse(message.Contains(pidSegment));

        // Clear
        message.Clear();
        Assert.AreEqual(0, message.Count);
    }

    [TestMethod]
    public void IList_InsertAndRemoveAt_WorkCorrectly()
    {
        var message = new HL7Message();
        var mshSegment = new HL7Segment("MSH");
        var pidSegment = new HL7Segment("PID");
        var pv1Segment = new HL7Segment("PV1");

        message.Add(mshSegment);
        message.Add(pv1Segment);

        // Insert
        message.Insert(1, pidSegment);
        Assert.AreEqual(3, message.Count);
        Assert.AreEqual("PID", message[1].SegmentID);
        Assert.AreEqual("PV1", message[2].SegmentID);

        // RemoveAt
        message.RemoveAt(1);
        Assert.AreEqual(2, message.Count);
        Assert.AreEqual("PV1", message[1].SegmentID);
    }

    [TestMethod]
    public void IList_CopyTo_WorksCorrectly()
    {
        var message = new HL7Message();
        message.Add(new HL7Segment("MSH"));
        message.Add(new HL7Segment("PID"));

        var array = new HL7Segment[3];
        message.CopyTo(array, 1);

        Assert.IsNull(array[0]);
        Assert.AreEqual("MSH", array[1].SegmentID);
        Assert.AreEqual("PID", array[2].SegmentID);
    }

    #endregion

    #region Segment Accessors Tests

    [TestMethod]
    public void MSH_Property_ReturnsFirstMSHSegment()
    {
        var message = new HL7Message();
        message.Add(new HL7Segment("MSH", "|", "^~\\&", "App1"));
        message.Add(new HL7Segment("MSH", "|", "^~\\&", "App2")); // Shouldn't happen but testing

        Assert.AreEqual("App1", message.MSH[3]);
    }

    [TestMethod]
    public void SegmentIndexer_GetExistingSegment_ReturnsFirstMatch()
    {
        var message = new HL7Message();
        message.Add(new HL7Segment("PID", "value1"));
        message.Add(new HL7Segment("PID", "value2"));

        Assert.AreEqual("value1", message["PID"][1]);
    }

    [TestMethod]
    public void SegmentIndexer_SetExistingSegment_ReplacesFirstMatch()
    {
        var message = new HL7Message();
        message.Add(new HL7Segment("PID", "original"));

        var newSegment = new HL7Segment("PID", "replaced");
        message["PID"] = newSegment;

        Assert.AreEqual(1, message.Count);
        Assert.AreEqual("replaced", message["PID"][1]);
    }

    [TestMethod]
    public void SegmentIndexer_SetNonExistentSegment_AddsSegment()
    {
        var message = new HL7Message();
        var newSegment = new HL7Segment("PID", "new");

        message["PID"] = newSegment;

        Assert.AreEqual(1, message.Count);
        Assert.AreEqual("new", message["PID"][1]);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void SegmentIndexer_GetNonExistentSegment_ThrowsInvalidOperationException()
    {
        var message = new HL7Message();
        _ = message["PID"];
    }

    #endregion

    #region Helper Methods Tests

    [TestMethod]
    public void OfSegment_FiltersBySegmentID_ReturnsCorrectSegments()
    {
        var message = new HL7Message();
        message.Add(new HL7Segment("MSH"));
        message.Add(new HL7Segment("PID", "1"));
        message.Add(new HL7Segment("OBX", "1"));
        message.Add(new HL7Segment("OBX", "2"));
        message.Add(new HL7Segment("PV1"));

        var obxSegments = message.OfSegment("OBX").ToList();

        Assert.AreEqual(2, obxSegments.Count);
        Assert.AreEqual("1", obxSegments[0][1]);
        Assert.AreEqual("2", obxSegments[1][1]);
    }

    [TestMethod]
    public void FirstOrDefault_ExistingSegment_ReturnsFirstMatch()
    {
        var message = new HL7Message();
        message.Add(new HL7Segment("PID", "first"));
        message.Add(new HL7Segment("PID", "second"));

        var result = message.FirstOrDefault("PID");

        Assert.IsNotNull(result);
        Assert.AreEqual("first", result[1]);
    }

    [TestMethod]
    public void FirstOrDefault_NonExistentSegment_ReturnsNull()
    {
        var message = new HL7Message();
        var result = message.FirstOrDefault("PID");

        Assert.IsNull(result);
    }

    [TestMethod]
    public void SingleOrDefault_ExistingUniqueSegment_ReturnsSegment()
    {
        var message = new HL7Message();
        message.Add(new HL7Segment("MSH"));

        var result = message.SingleOrDefault("MSH");

        Assert.IsNotNull(result);
        Assert.AreEqual("MSH", result.SegmentID);
    }

    [TestMethod]
    public void SingleOrDefault_NonExistentSegment_ReturnsNull()
    {
        var message = new HL7Message();
        var result = message.SingleOrDefault("PID");

        Assert.IsNull(result);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Single_MultipleSegments_ThrowsInvalidOperationException()
    {
        var message = new HL7Message();
        message.Add(new HL7Segment("OBX", "1"));
        message.Add(new HL7Segment("OBX", "2"));

        _ = message.Single("OBX");
    }

    #endregion

    #region Building Messages Tests

    [TestMethod]
    public void Add_WithSegmentIdAndFields_CreatesSegmentCorrectly()
    {
        var message = new HL7Message()
        {
            { "MSH", "", "", "SendingApp", "SendingFacility" },
            { "PID", "1", "", "123456789", new SetHL7Components("Doe", "John") }
        };

        Assert.AreEqual(2, message.Count);
        var msh = message.MSH;
        var segment = message["PID"];
        Assert.AreEqual("MSH", msh.SegmentID);
        Assert.AreEqual("SendingApp", msh[3]);
        Assert.AreEqual("PID", segment.SegmentID);
        Assert.AreEqual("1", segment[1]);
        Assert.AreEqual("", segment[2]);
        Assert.AreEqual("123456789", segment[3]);
        Assert.AreEqual("Doe", segment[4, 1, 1]);
        Assert.AreEqual("John", segment[4, 1, 2]);
    }

    [TestMethod]
    public void Add_WithSegmentIdAndFields_CreatesSegmentCorrectly2()
    {
        var message = new HL7Message()
        {
            HL7Segment.CreateMSH(
                "fromapp", "fromfac", "toapp", "tofac",
                "ADT", "A08", "ADT_A01",
                "1234567", new DateTime(2000, 01, 01)),
            new HL7Segment("PID")
            {
                [1] = "1",
                [2] = "",
                [3] = "123456789",
                [4, 1, 1] = "Doe",
                [4, 1, 2] = "John"
            }
        };

        Assert.AreEqual(2, message.Count);
        var msh = message.MSH;
        var segment = message["PID"];
        Assert.AreEqual("MSH", msh.SegmentID);
        Assert.AreEqual("fromapp", msh[3]);
        Assert.AreEqual("tofac", msh[6]);
        Assert.AreEqual("PID", segment.SegmentID);
        Assert.AreEqual("1", segment[1]);
        Assert.AreEqual("", segment[2]);
        Assert.AreEqual("123456789", segment[3]);
        Assert.AreEqual("Doe", segment[4, 1, 1]);
        Assert.AreEqual("John", segment[4, 1, 2]);
    }

    [TestMethod]
    public void BuildMessage_UsingVariousMethods_CreatesCorrectStructure()
    {
        var message = new HL7Message();

        // Add MSH using constructor
        message.Add(new HL7Segment("MSH", "|", "^~\\&", "SendingApp", "SendingFacility"));

        // Add PID using helper method
        message.Add("PID", "1", "", "123456789");

        // Add PV1 by creating and adding
        var pv1 = new HL7Segment("PV1");
        pv1[1] = "1";
        pv1[2] = "I";
        pv1.Add(3, "ICU", "101", "A");
        message.Add(pv1);

        // Add multiple OBX segments
        message.Add("OBX", "1", "ST", "CODE1", "", "Value1");
        message.Add("OBX", "2", "ST", "CODE2", "", "Value2");

        Assert.AreEqual(5, message.Count);
        Assert.AreEqual("MSH", message[0].SegmentID);
        Assert.AreEqual("PID", message[1].SegmentID);
        Assert.AreEqual("PV1", message[2].SegmentID);
        Assert.AreEqual("OBX", message[3].SegmentID);
        Assert.AreEqual("OBX", message[4].SegmentID);

        // Verify content
        Assert.AreEqual("SendingApp", message.MSH[3]);
        Assert.AreEqual("123456789", message["PID"][3]);
        Assert.AreEqual("ICU", message["PV1"][3, 1, 1]);
        Assert.AreEqual("Value1", message.OfSegment("OBX").First()[5]);
        Assert.AreEqual("Value2", message.OfSegment("OBX").Last()[5]);
    }

    #endregion

    #region Enumeration Tests

    [TestMethod]
    public void Enumeration_WorksCorrectly()
    {
        var message = new HL7Message();
        message.Add(new HL7Segment("MSH"));
        message.Add(new HL7Segment("PID"));
        message.Add(new HL7Segment("PV1"));

        var segmentIds = new List<string>();
        foreach (var segment in message)
        {
            segmentIds.Add(segment.SegmentID);
        }

        CollectionAssert.AreEqual(new[] { "MSH", "PID", "PV1" }, segmentIds);
    }

    #endregion

    #region Error Scenarios

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SegmentIndexer_SetNull_ThrowsArgumentNullException()
    {
        var message = new HL7Message();
        message["PID"] = null!;
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void First_NonExistentSegment_ThrowsInvalidOperationException()
    {
        var message = new HL7Message();
        _ = message.First("PID");
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Single_NonExistentSegment_ThrowsInvalidOperationException()
    {
        var message = new HL7Message();
        _ = message.Single("PID");
    }

    #endregion

    #region Documentation Examples

    [TestMethod]
    public void Docs_BuildMessage()
    {
        var message = new HL7Message()
        {
            // Use a helper to create the MSH segment
            HL7Segment.CreateMSH(
                "fromapp", "fromfac", "toapp", "tofac",
                "ADT", "A08", "ADT_A01",
                "1234567", new DateTime(2000, 01, 01)),
            // Create segments using object initializer syntax
            new HL7Segment("PID")
            {
                [1] = "1",
                [2] = "",
                [3, 1, 1] = "123456789",
                [3, 1, 4] = "MR",
                [3, 2, 1] = "766-45-4320",
                [3, 2, 4] = "SS",
                // Set repeats, components, and subcomponents using indexers
                [5, component: 1] = "Doe",
                [5, component: 2] = "John",
                [7] = "19800101",
                [8] = "M",
                [11, component: 1] = "123 Main St",
                [11, component: 3] = "Anytown",
                [11, component: 4] = "ST",
                [11, component: 5] = "12345"
            },
            // Or use the Add method with field values to create segments in one line
            { "PV1", "1", "I", new SetHL7Components("West", "102", "A", "Caring Acres") },
            { "AL1", "1", "DA", new SetHL7Components("", "Penicillin"), "Severe"  },
            { "AL1", "2", "FA", new SetHL7Components("", "Peanuts"), "Mild" }
        };

        // Serialize to HL7 string
        var hl7text = message.ToString();
        Console.WriteLine(hl7text);

        // Deserialize from string
        var parsed = HL7Message.Parse(hl7text);
        Assert.AreEqual("fromapp", parsed.MSH[3]);
        var pid = parsed["PID"];
        Assert.AreEqual("123456789", pid[3, 1, 1]);

        // Enumerate repeated fields
        foreach (var id in pid.FieldRepeats[3])
        {
            // 123456789 (MR)
            // 766-45-4320 (SS)
            Console.WriteLine($"{id[1, 1]} ({id[1, 4]})");
        }

        // Enumerate segments
        foreach (var al1 in parsed.OfSegment("AL1"))
        {
            // Penicillin (Severe)
            // Peanuts (Mild)
            Console.WriteLine($"{al1[3, component: 2]} ({al1[4]})");
        }
    }

    #endregion
}