using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProgrammServer
{
    public class UdpHandler
    {
        private UdpClient UdpClient;
        private int Port;
        public event EventHandler<string> OnUserEventReceived;

        public UdpHandler(int port)
        {
            Port = port;
        }

        public async Task StartAsync()
        {
            try
            {
                UdpClient = new UdpClient(Port);

                while (true)
                {
                    var result = await UdpClient.ReceiveAsync();
                    string message = Encoding.UTF8.GetString(result.Buffer);
                    OnUserEventReceived?.Invoke(this, message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка UDP: {ex.Message}");
            }
        }

        public void Dispose()
        {
            UdpClient.Dispose();
        }
    }
}