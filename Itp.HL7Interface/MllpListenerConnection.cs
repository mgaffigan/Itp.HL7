using Itp.HL7Interpreter.Messages;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text;

namespace Itp.HL7Interface;

internal sealed class MllpListenerConnection
{
    private readonly MllpListener host;
    private readonly List<byte> incomingMessage = new List<byte>();
    private readonly byte[] buffer = new byte[1024];
    private readonly ILogger _logger;

    private static class MllpFraming
    {
        public const byte StartOfMessageByte = 11;
        public const byte EndOfMessageByte = 28;
        public const byte EndOfMessageExtraByte = 13;
    }

    private Socket socket { get; }

    public MllpListenerConnection(TcpClient client, MllpListener host, ILogger logger)
    {
        this.host = host;
        this.socket = client.Client;
        this._logger = logger;
    }

    public void ReadAsync()
    {
        try
        {
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReadCallback), null);
        }
        catch (Exception ex)
        {
            host.Logger.LogWarning(ex, "Exception while beginning a read from client.");
        }
    }

    private void ReadCallback(IAsyncResult iar)
    {
        int count = 0;
        try
        {
            count = socket.EndReceive(iar);
        }
        catch (Exception ex)
        {
            host.Logger.LogWarning(ex, "Exception while reading from client.");
        }

        if (count > 0)
        {
            incomingMessage.AddRange(buffer.Take(count));
            if (incomingMessage.Contains(MllpFraming.StartOfMessageByte) && incomingMessage.Contains(MllpFraming.EndOfMessageByte))
            {
                // this 'should' parse.
                // could start with the 'cr' from the last message skip until we find the marker.
                var msgStart = incomingMessage.SkipWhile(b => b != MllpFraming.StartOfMessageByte);
                var msg = msgStart.Skip(1).TakeWhile(b => b != MllpFraming.EndOfMessageByte).ToArray();
                if (msgStart.Skip(msg.Length).Any() && msgStart.Skip(msg.Length + 1).First() == MllpFraming.EndOfMessageByte)
                {
                    // found start and end markers.
                    // msg is valid, remove from head all the way to end frame
                    host.HandleMessage(Encoding.ASCII.GetString(msg), this);
                    incomingMessage.RemoveRange(0, incomingMessage.IndexOf(MllpFraming.EndOfMessageByte) + 1);
                    // message should end eith 'cr', if it does just eat it.
                    if (incomingMessage.Any() && incomingMessage.First() == MllpFraming.EndOfMessageExtraByte)
                    {
                        // consume this too.
                        incomingMessage.RemoveAt(0);
                    }
                }
            }
            ReadAsync();
        }
        else
        {
            // connection is being closed
            // the trailing <CR> could have been received by itself due to packet size or sender implementation or buffer boundry, or network fragmentation
            // lots of reasons.
            if (incomingMessage.Any() && incomingMessage.First() == MllpFraming.EndOfMessageExtraByte)
            {
                // consume this too.
                incomingMessage.RemoveAt(0);
            }

            // check to ensure the buffer is empty
            // log if not.
            if (incomingMessage.Any())
            {
                // either the message was processed or corrupt
                // either way no need to inform the host.
                _logger.LogWarning("MllpConnection partial message received content: {0}", Encoding.ASCII.GetString(incomingMessage.ToArray()));
            }
        }
    }

    internal void SendAck(AcknowledgementMessage ack)
    {
        try
        {
            byte[] bytes = Encoding.ASCII.GetBytes(ack.ToString());
            socket.Send(new byte[1] { MllpFraming.StartOfMessageByte });
            socket.Send(bytes);
            socket.Send(new byte[2] { MllpFraming.EndOfMessageByte, MllpFraming.EndOfMessageExtraByte });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send ack: {ack}", ack.ToString());
        }
    }
}