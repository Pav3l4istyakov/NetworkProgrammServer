using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProgrammServer
{
    public class TcpServer
    {
        private int TcpPort = 8080;
        private int UdpPort = 8081;
        private List<NetworkStream> Clients = new();
        private UdpHandler UdpHandler;

        public TcpServer()
        {
            UdpHandler = new UdpHandler(UdpPort);
            UdpHandler.OnUserEventReceived += HandleUserEvent;
        }

        public async Task StartAsync()
        {
            
            var backgroundUdpTask = UdpHandler.StartAsync();

            try
            {
                var listener = new TcpListener(IPAddress.Any, TcpPort);
                listener.Start();
               
                Console.WriteLine("Ожидание клиентов...");

                while (true)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    var stream = client.GetStream();

                    lock (Clients)
                    {
                        Clients.Add(stream);
                    }

                    Console.WriteLine($" Подключён: {client.Client.RemoteEndPoint}");

                    Task.Run(() => ProcessClient(stream, client));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Ошибка сервера: {ex.Message}");
            }
        }

        private void ProcessClient(NetworkStream stream, TcpClient client)
        {
            try
            {
                while (client.Connected)
                {
                    string message = ReceiveMessage(stream);
                    if (string.IsNullOrEmpty(message))
                    {
                        break;
                    }

                    string response = $"OFFLINE";
                    SendMessageToAllClients(response);
                }
            }
            catch
            {
                
            }
            finally
            {
                RemoveClient(stream);
                client?.Close();
            }
        }

        private void HandleUserEvent(object sender, string eventData)
        {
            if (eventData.Contains(':'))
            {
                string[] parts = eventData.Split(':', 2);
                string username = parts[0].Trim();
                string status = parts[1].Trim().ToUpper();
                string message = $"{username} {status}";
                SendMessageToAllClients(message);
            }
        }

        private string ReceiveMessage(NetworkStream stream)
        {
            byte[] lengthBytes = new byte[4];
            try
            {
                stream.ReadExactly(lengthBytes, 0, 4);
                int length = BitConverter.ToInt32(lengthBytes, 0);

                byte[] messageBytes = new byte[length];
                stream.ReadExactly(messageBytes, 0, length);

                return Encoding.UTF8.GetString(messageBytes);
            }
            catch
            {
                return null;
            }
        }

        private void SendMessageToAllClients(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            byte[] header = BitConverter.GetBytes(data.Length);

            lock (Clients)
            {
                for (int i = Clients.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        var stream = Clients[i];
                        stream.Write(header, 0, 4);
                        stream.Write(data, 0, data.Length);
                    }
                    catch
                    {
                        Clients.RemoveAt(i);
                    }
                }
            }
        }

        private void RemoveClient(NetworkStream stream)
        {
            lock (Clients)
            {
                Clients.Remove(stream);
            }
        }
    }
}
