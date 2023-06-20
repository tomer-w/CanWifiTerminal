using System.Net;
using System.Text;

public class CanTerminal
{
    CanWifiClient client;
    private CanTerminal(CanWifiClient client)
    {
        this.client = client;
    }

    public static async Task<CanTerminal> Create(IPAddress ip, int port)
    {
        var client = await CanWifiClient.Connect(ip, port);
        Console.WriteLine ($"Connected successfully to: {ip}:{port}");
        return new CanTerminal(client);
    }

    private async Task HandleNextMessage()
    {
        try
        {
            var message =  await client.ReceiveMessageAsync();
            Console.WriteLine(message);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Parsing failed: {e}");
        }
        Task ignoredTask = HandleNextMessage();
    }

    private async Task HandleConsoleInput(Stream stream)
    {
        try
        {
            var buffer = new byte[100];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var input = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            if (input.Length == 0)
            {
                Console.WriteLine($"Valid input is can json");
            }
            else if(input.StartsWith("dump"))
            {
                var fileName = input.Remove(0, 5).Trim();
                Console.WriteLine($"Dumping to: {fileName}");
                client.StartDump(fileName);
            }
            else if(input.StartsWith("stop dump"))
            {
                Console.WriteLine($"Dump stopped");
                client.StopDump();
            }
            else
            {
                var canMessage = CanMessage.Deserialize(input);
                await client.SendMessageAsync(canMessage);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed: {e}");
        }
        Task ignoredTask = HandleConsoleInput(stream);
    }

    public async Task Loop()
    {
        Task receiveTask = HandleNextMessage();
        using (var stream = Console.OpenStandardInput())
        {
            Task consoleInputTask = HandleConsoleInput(stream);
            await Task.Delay(1000*1000);
        }
    }
}
