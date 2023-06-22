using System.Net;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("ECAN-W01S terminal");

        if (args.Length != 2)
        {
            Console.WriteLine("CanWifiTerminal ip port");
            return;
        }
        var terminal = await CanTerminal.Create(IPAddress.Parse(args[0]), Int32.Parse(args[1]));
        await terminal.Loop();
    }
}