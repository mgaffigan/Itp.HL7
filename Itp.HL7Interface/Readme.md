# Itp.HL7Interface

A simple MLLP bidirectional server and client for sending and receiving
HL7 messages over TCP/IP, secured or unsecured.

## Example MLLP Client
```csharp
var hl7text = @"MSH|^~\&|HIS|RIH|EKG|EKG|200202150930||ADT^A01|MSG00001|P|2.4\rPID|1||123456^^^Hospital^MR||Doe^John||19600101|M|||123 Main St^^Metropolis^IL^12345||(555)555-2004|(555)555-2004||S||123456789|987-65-4320||||||||||||||||N\r"
    + "PV1|1|I|2000^2012^01||||004777^Smith^John^A^^Dr.|||||||||||V123456789|A0|\r";

using var connection = await MllpClient.ConnectAsync("some.ehr.server.com", 15725, default);
var response = await connection.TransceiveAsync(hl7text, default);
Console.WriteLine($"Received response: {response}");
```

## Example MLLP Server
```csharp
var tcs = new TaskCompletionSource();
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

var ipe = new IPEndPoint(IPAddress.Any, 1234);
using var connection = new MllpListener(ipe, Handler,
    null, loggerFactory.CreateLogger<ILogger<MllpListener>>());
Console.WriteLine($"Listening on {ipe}");

await tcs.Task;

async Task Handler(IMllpConnection connection)
{
    Console.WriteLine($"Connected to {connection.RemoteEndPoint}");
    while (true)
    {
        var message = await connection.ReceiveAsStringAsync(default);
        if (message is null)
        {
            Console.WriteLine("Connection closed.");
            break;
        }
        Console.WriteLine("< " + message);

        var ack = "MSH|^~\\&|RECEIVER|HOSPITAL|SENDER|CLINIC|202406101200||ACK^A01|123456|P|2.5\r" +
                  "MSA|AA|123456\r";
        await connection.SendAsync(ack, default);
    }
}
```

## Security

The MLLP client and server support TLS encryption.  To use TLS, call `ConnectTlsAsync`
on the client and provide a certificate on the server.  mTLS is not supported.

## Parsing and Creating HL7 Messages

This library does not include HL7 parsing and building functionality.  Use
[Itp.SimpleHl7](https://www.nuget.org/packages/Itp.SimpleHl7) or some other
HL7 library for that purpose.