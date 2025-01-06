using Itp.HL7Interface;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

// create log builder for console
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
    Console.WriteLine("Enter data to send, then transmit the message with a line containing a single \".\"");
    Console.WriteLine("Enter QUIT to exit");

    var reader = Task.Run(async () =>
    {
        while (true)
        {
            var message = await connection.ReceiveAsStringAsync(default);
            if (message is null)
            {
                Console.WriteLine("Connection closed.");
                break;
            }
            Console.WriteLine("< " + message);
        }
    });

    while (true)
    {
        // accumulate lines
        var sb = new StringBuilder();
        while (true)
        {
            var line = Console.ReadLine();
            if ("QUIT".Equals(line, StringComparison.OrdinalIgnoreCase))
            {
                tcs!.SetResult();
                return;
            }
            if (line == ".") break;
            sb.AppendLine(line);
        }

        var message = sb.ToString();
        await connection.SendAsync(message, default);
    }
}