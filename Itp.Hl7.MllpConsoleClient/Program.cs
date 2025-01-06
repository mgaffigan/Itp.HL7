using Itp.HL7Interface;
using System.Text;

string host;
int port;
if (args.Length >= 2)
{
    host = args[0];
    port = int.Parse(args[1]);
}
else
{
    Console.Write("Host> ");
    host = Console.ReadLine()!;
    Console.Write("Port> ");
    port = int.Parse(Console.ReadLine()!);
}

using var connection = await MllpClient.ConnectAsync(host, port, default);
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
        if ("QUIT".Equals(line, StringComparison.OrdinalIgnoreCase)) return;
        if (line == ".") break;
        sb.AppendLine(line);
    }

    var message = sb.ToString();
    await connection.SendAsync(message, default);
}