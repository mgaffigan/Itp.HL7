# Simple HL7

A lightweight, easy to use, high-performance .NET library for parsing 
and building HL7 messages.

## Quick Start

`HL7Message` contains `HL7Segment`s.  Fields, repeats, components, and subcomponents
are accessed directly on `HL7Segment` using indexers.  Parse and serialize directly.

### Build a message
```csharp
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
```

### Parse a message
```csharp
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
```

### Generate an ack
```csharp
var ack = parsed.CreateAck("AA");
Console.WriteLine(ack.ToString());
```