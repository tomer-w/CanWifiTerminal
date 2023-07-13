using System.Net;
using System.Net.Sockets;

public class CanWifiClient
{
    private TcpClient tcpClient = new TcpClient();
    private StreamWriter? dumpWriter;

    public bool Connected {get => tcpClient.Connected;}

    public static async Task<CanWifiClient> Connect(IPAddress ip, int port)
    {
        var canWifiClient = new CanWifiClient();
        await canWifiClient.tcpClient.ConnectAsync(ip, port);

        return canWifiClient;
    }

    private async Task SendMessageAsync(byte[] message)
    {
        await tcpClient.GetStream().WriteAsync(message, 0, message.Length);
    }

    public async Task SendMessageAsync(CanMessage message)
    {
        await SendMessageAsync(message.ToBinary());
    }

    public async Task<CanMessage> ReceiveMessageAsync()
    {
        //read response from stream
        var buffer = new byte[13];
        int left = 13;
        while (left > 0)
        {
            int byteCount = await tcpClient.GetStream().ReadAsync(buffer, 13 - left, left);
            left -= byteCount;
        }
        var canMessage = new CanMessage(buffer);
        if (dumpWriter != null)
        {
            await dumpWriter.WriteLineAsync($"({DateTime.Now}):{canMessage.Serialize()}");
        }
        return canMessage;
    } 

    public void StartDump(String fileName)
    {
        StopDump();
        dumpWriter = new StreamWriter(fileName, true);
    }

    public void StopDump()
    {
        if (dumpWriter != null)
        {
            dumpWriter.Close();
            dumpWriter = null;
        }
    }
}