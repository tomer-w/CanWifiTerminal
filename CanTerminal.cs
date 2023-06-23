using System.Net;
using System.Text;
using System.Text.RegularExpressions;

public class CanTerminal
{
    CanWifiClient client;
    bool consolePrint = false;
    bool filter = false;
    long messageReceived = 0;

    private Dictionary<ulong, ulong> idsMap = new Dictionary<ulong, ulong>();
    private HashSet<ulong> filters = new HashSet<ulong>();

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
            ulong count;
            idsMap.TryGetValue(message.ID, out count);
            idsMap[message.ID] = ++count;
            bool idFirstTime = idsMap[message.ID] == 1;
            bool filtered = (filter == true) && filters.Contains(message.ID);
            messageReceived++;
            if (consolePrint || idFirstTime || filtered)
            {
                if (idFirstTime)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                if (filtered)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                Console.WriteLine($"{count}:{message}");
                Console.ResetColor();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Parsing failed: {e}");
        }
        Task ignoredTask = HandleNextMessage();
    }

    private async Task HandleTimer()
    {
        await Task.Delay(TimeSpan.FromSeconds(10));
        Console.WriteLine($"Messages received: {messageReceived}");
        Task ignoredTask = HandleTimer();
    }

    //Split by spaces but keep together qouted text. Authored by ChatGPT: "c# regex match quoted string with spaces"
    private IList<string> splitInput(String input)
    {
        var result = Regex.Matches(input, @"(?<="")[^""]+(?="")|(?<=')[^']+(?=')|[^'""\s]+");
        List<string> list = result.Select(m => m.Value).ToList();
        return list;
    }

    private async Task HandleConsoleInput(Stream stream)
    {
        try
        {
            var buffer = new byte[100];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var input = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            var argc = splitInput(input);
            if(argc.Count == 3 && argc[0] == "dump" && argc[1] == "start")
            {
                var fileName = argc[2];
                Console.WriteLine($"Dumping to: {fileName}");
                client.StartDump(fileName);
            }
            else if(argc.Count == 2 && argc[0] == "dump" && argc[1] == "stop")
            {
                Console.WriteLine($"Dump stopped");
                client.StopDump();
            }
            else if(argc.Count == 2 && argc[0] == "print" && argc[1] == "start")
            {
                Console.WriteLine($"Console print started");
                consolePrint = true;
            }
            else if(argc.Count == 2 && argc[0] == "print" && argc[1] == "stop")
            {
                Console.WriteLine($"Console print stopped");
                consolePrint = true;
            }
            else if(argc.Count == 2 && argc[0] == "filter" && argc[1] == "off")
            {
                Console.WriteLine($"Console filter off");
                filter = false;
            }
            else if(argc.Count == 2 && argc[0] == "filter" && argc[1] == "on")
            {
                Console.WriteLine($"Console filter on");
                filter = true;
            }
            else if(argc.Count == 2 && argc[0] == "filter" && argc[1] == "clear")
            {
                Console.WriteLine($"Filter reset");
                filters.Clear();
            }
            else if(argc.Count == 2 && argc[0] == "filter")
            {
                ulong val = ulong.Parse(argc[1]);
                if (filters.Contains(val))
                {
                    Console.WriteLine($"Filter {val} removed");
                    filters.Remove(val);
                }
                else
                {
                    Console.WriteLine($"Filter {val} added");
                    filters.Add(val);
                }
            }
            else if(input.StartsWith("{") && input.EndsWith("}"))
            {
                var canMessage = CanMessage.Deserialize(input);
                await client.SendMessageAsync(canMessage);
            }
            else
            {
                Console.WriteLine("Available options:");
                Console.WriteLine(" dump start <file name>");
                Console.WriteLine(" dump stop");
                Console.WriteLine(" print start");
                Console.WriteLine(" print stop");
                Console.WriteLine(" filter on");
                Console.WriteLine(" filter off");
                Console.WriteLine(" filter clear");
                Console.WriteLine(" filter <id>");
                Console.WriteLine(" {json CAN message} to send");
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
        Task timerTask = HandleTimer();

        using (var stream = Console.OpenStandardInput())
        {
            Task consoleInputTask = HandleConsoleInput(stream);
            await Task.Delay(1000*100000);
        }
    }
}
