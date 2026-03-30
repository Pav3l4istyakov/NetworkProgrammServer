namespace NetworkProgrammServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var server = new TcpServer();
            await server.StartAsync();
        }
    }
}
