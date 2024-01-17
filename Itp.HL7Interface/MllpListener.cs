using Itp.HL7Interpreter;
using Itp.HL7Interpreter.Messages;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace Itp.HL7Interface;

public delegate AcknowledgementMessage MessageReceiveHandler(Message message, string ackControlNumber);

public class MllpListener : IDisposable
{
    private int MessageSeqNo;

    private readonly TcpListener listener;
    private readonly IPEndPoint ListenEndpoint;
    internal readonly ILogger Logger;
    private readonly SegmentTypeResolver? TypeResolver;
    private readonly MessageReceiveHandler MessageReceived;
    private readonly string? ArchivePath;

    public MllpListener(IPEndPoint endpoint, string archivePath, SegmentTypeResolver? typeResolver, MessageReceiveHandler handler, ILogger log)
    {
        TypeResolver = typeResolver;
        ArchivePath = archivePath;
        ListenEndpoint = endpoint;
        MessageReceived = handler;
        Logger = log;

        listener = new TcpListener(ListenEndpoint);
        listener.Start();
        acceptNewConAsync();
    }

    public void Dispose()
    {
        listener.Stop();
    }

    private void acceptNewConAsync()
    {
        Logger.LogDebug("Listening for a new connection...");
        try
        {
            listener.BeginAcceptTcpClient(new AsyncCallback(handleConnection), null);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred while accepting connection for {0}", ListenEndpoint);
        }
    }

    private void handleConnection(IAsyncResult result)
    {
        Logger.LogDebug("Handling a new connection...");
        acceptNewConAsync();
        try
        {
            TcpClient client = listener.EndAcceptTcpClient(result);
            if (!client.Connected)
            {
                Logger.LogInformation("Connection closed before processing.");
            }
            else
            {
                Logger.LogDebug("New connection accepted from {0}", client.Client.RemoteEndPoint);
                new MllpListenerConnection(client, this, Logger).ReadAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error while accepting socket");
        }
    }

    internal void HandleMessage(string p, MllpListenerConnection con)
    {
        int seqNo = Math.Abs(Interlocked.Increment(ref MessageSeqNo) % 1000000);
        writeToArchive(seqNo, p, out var path);
        path ??= "<< Not saved >>";

        // Parse
        Message message;
        try
        {
            message = Message.FromString(p, '|', TypeResolver);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Exception while parsing message.  Message is stored at '{0}'", path);
            return;
        }

        // Handle and respond
        var AckControlNo = string.Format("{0:yyyyMMdd}{1:000000}", DateTime.Today, seqNo);
        try
        {
            var ack = MessageReceived(message, AckControlNo);
            Logger.LogDebug("Sending ack for {0}", AckControlNo);
            con.SendAck(ack);
        }
        catch (ErrorAcknowledgementException ex)
        {
            Logger.LogWarning(ex, "Error processing message");
            var ack = new AcknowledgementMessage("ITP", "HL7IF", AckControlNo,
                AcknowledgementType.Error,
                AcknowledgementErrorType.ApplicationInternalError, "Error Processing Message");
            con.SendAck(ack);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Exception while processing message.  Message is stored at '{0}'", path);
        }
    }

    private void writeToArchive(int seqNo, string contents, out string? path)
    {
        if (ArchivePath == null)
        {
            path = null;
            return;
        }

        path = Path.Combine(ArchivePath, string.Format("rec-{0:yyyy'-'MM'-'dd'T'HHmmss}-{1:0000}.hl7", DateTime.Now, seqNo));
        File.WriteAllText(path, contents);
    }
}