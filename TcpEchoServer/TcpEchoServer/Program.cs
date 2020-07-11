using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TcpEchoServer
{
    class Program
    {
        public static List<(string, TcpClient)> clientsList = new List<(string, TcpClient)>();

        static void Main(string[] args)
        {
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            //IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
            TcpListener serverSocket = new TcpListener(ipAddress, 1234);
            TcpClient clientSocket = default;
            int counter = 0;

            serverSocket.Start();
            Console.WriteLine("Chat Server Started ....");
            counter = 0;
            while ((true))
            {
                try
                {
                    counter += 1;
                    clientSocket = serverSocket.AcceptTcpClient();

                    byte[] bytesFrom = new byte[65536];
                    string dataFromClient = null;

                    NetworkStream networkStream = clientSocket.GetStream();
                    StreamWriter writer = new StreamWriter(networkStream, Encoding.UTF32);
                    StreamReader reader = new StreamReader(networkStream, Encoding.UTF32);
                    string entryString = reader.ReadLine();
                    if (entryString.ToLower().StartsWith("connect "))
                    {
                        entryString = entryString.Substring(8);
                        entryString = entryString.Split(' ')[0];
                        if (!clientsList.Any(item => item.Item1 == entryString))
                        {
                            clientsList.Add((entryString, clientSocket));
                            dataFromClient = entryString;
                            writer.WriteLine("Successfully connected to server!");
                            Broadcast(dataFromClient + " Joined ", dataFromClient, false);

                            Console.WriteLine(dataFromClient + " Joined chat room ");
                            ClientHandler client = new ClientHandler();
                            client.StartClient(clientSocket, dataFromClient, ref clientsList);

                        }
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine("exit");
            Console.ReadLine();
        }

        public static void Broadcast(string msg, string userName, bool flag)
        {
            foreach (var item in clientsList)
            {
                TcpClient broadcastSocket;
                broadcastSocket = item.Item2;
                NetworkStream broadcastStream = broadcastSocket.GetStream();
                byte[] broadcastMessage;
                if (flag == true)
                {
                    broadcastMessage = Encoding.UTF32.GetBytes(userName + " says : " + msg);
                }
                else
                {
                    broadcastMessage = Encoding.UTF32.GetBytes(msg);
                }

                broadcastStream.Write(broadcastMessage, 0, broadcastMessage.Length);
                broadcastStream.Flush();
            }
        }
    }


    public class ClientHandler
    {
        TcpClient ClientSocket;
        string ID;
        List<(string, TcpClient)> ClientList;

        public void StartClient(TcpClient clientSocket, string clientUser, ref List<(string, TcpClient)> clientList)
        {
            ClientSocket = clientSocket;
            ID = clientUser;
            ClientList = clientList;
            Thread clientThread = new Thread(ChatHandler);
            clientThread.Start();
        }

        private void ChatHandler()
        {
            int requestCount = 0;
            string dataFromClient;
            string rCount;

            while ((true))
            {
                try
                {
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = ClientSocket.GetStream();
                    StreamWriter writer = new StreamWriter(networkStream, Encoding.UTF32);
                    StreamReader reader = new StreamReader(networkStream, Encoding.UTF32);
                    dataFromClient = reader.ReadLine();
                    if (dataFromClient.Split(' ')[0].ToLower() == "list_users")
                    {
                        writer.WriteLine("User List:");
                        foreach (var item in ClientList)
                        {
                            writer.WriteLine(item);
                        }
                    }

                    if (dataFromClient.Split(' ')[0].ToLower() == "send_message" && ClientList.Any(item => item.Item1 == dataFromClient.Split(' ')[1].ToLower()))
                    {

                        Console.WriteLine("From client - " + ID + " : " + "\"" + dataFromClient + "\"" + " to ");
                    }
                    Console.WriteLine("From client - " + ID + " : " + dataFromClient);
                    rCount = Convert.ToString(requestCount);

                    Program.Broadcast(dataFromClient, ID, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}