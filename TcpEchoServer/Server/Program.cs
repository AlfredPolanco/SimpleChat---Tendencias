using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace Server
{
    class Program
    {
        public static List<(string, TcpClient, ClientHandler)> clientsList = new List<(string, TcpClient, ClientHandler)>();

        static void Main(string[] args)
        {
            IPHostEntry host = Dns.GetHostEntry("User");
            IPAddress ipAddress = host.AddressList[3];
            TcpListener serverSocket = new TcpListener(ipAddress, 8000);
            TcpClient clientSocket = default;
            int counter = 0;

            serverSocket.Start();
            Console.WriteLine("Chat Server Started ...");
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
                    StreamWriter writer = new StreamWriter(networkStream, Encoding.ASCII) { AutoFlush = true };
                    StreamReader reader = new StreamReader(networkStream, Encoding.ASCII);
                    string entryString = reader.ReadLine();
                    if (entryString.ToLower().StartsWith("connect "))
                    {
                        entryString = entryString.Substring(8);
                        entryString = entryString.Split(' ')[0];
                        if (!clientsList.Any(item => item.Item1 == entryString))
                        {
                            dataFromClient = entryString;
                            writer.WriteLine("Successfully connected to server!");
                            Broadcast(dataFromClient + " Joined ", dataFromClient, false);

                            Console.WriteLine(dataFromClient + " Joined chat room ");
                            ClientHandler client = new ClientHandler();
                            clientsList.Add((entryString, clientSocket, client));
                            client.StartClient(clientSocket, dataFromClient, ref clientsList);
                        }
                        else
                        {
                            writer.WriteLine("Connection Dismissed, another user has this username");
                        }
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public static void Broadcast(string msg, string userName, bool flag)
        {
            foreach (var item in clientsList)
            {
                string broadcastMessage;
                if (flag == true)
                {
                    broadcastMessage = userName + " says : " + msg;
                }
                else
                {
                    broadcastMessage = msg;
                }


                if (!item.Item1.Equals(userName))
                {
                    item.Item3.writer.WriteLine(broadcastMessage, 0, broadcastMessage.Length);
                    item.Item3.writer.Flush();
                }

            }
        }
    }


    public class ClientHandler
    {
        TcpClient ClientSocket;
        string ID;
        List<(string, TcpClient, ClientHandler)> ClientList;
        public StreamWriter writer;

        public void StartClient(TcpClient clientSocket, string clientUser, ref List<(string, TcpClient, ClientHandler)> clientList)
        {
            ClientSocket = clientSocket;
            ID = clientUser;
            ClientList = clientList;
            Thread clientThread = new Thread(ChatHandler);
            clientThread.Start();
        }
        ~ClientHandler()
        {
            Console.WriteLine("Client named " + ID + " left the chat room");
            Program.Broadcast("Client named " + ID + " left the chat room", ID, false);
        }
        private void ChatHandler()
        {
            int requestCount = 0;
            string dataFromClient;
            string rCount;
            NetworkStream networkStream = ClientSocket.GetStream();
            writer = new StreamWriter(networkStream, Encoding.ASCII) { AutoFlush = true };
            StreamReader reader = new StreamReader(networkStream, Encoding.ASCII);
            bool x = true;
            while ((x))
            {
                try
                {
                    requestCount = requestCount + 1;
                    dataFromClient = reader.ReadLine();

                    //List_Users
                    if (dataFromClient.ToLower().Trim(' ').Contains("list_users"))
                    {
                        string userList = "";

                        for (int i = 0; i < ClientList.LongCount(); i++)
                        {
                            var item = ClientList[i];
                            if (i == 0)
                            {
                                userList += "User List: ";
                            }
                            if (i < ClientList.LongCount() - 1)
                            {
                                userList += item.Item1 + ", ";
                            }
                            else
                            {
                                userList += item.Item1;
                            }
                        }
                        writer.WriteLine(userList);
                        rCount = Convert.ToString(requestCount);
                    }
                    //Message
                    else if (dataFromClient.Split(' ')[0].ToLower() == "send_message" && ClientList.Any(item => item.Item1 == dataFromClient.Split(' ')[1].ToLower()))
                    {
                        (string, TcpClient, ClientHandler) clientSelected = ClientList.Find(item => item.Item1 == dataFromClient.Split(' ')[1].ToLower());
                        clientSelected.Item3.writer.WriteLine("From client - " + ID + " : " + dataFromClient.Substring(dataFromClient.IndexOf(clientSelected.Item1) + clientSelected.Item1.Length));
                        Console.WriteLine("From client - " + ID + " : " + "\"" + dataFromClient + "\"" + " to " + clientSelected.Item1);
                        rCount = Convert.ToString(requestCount);
                    }

                    else if (dataFromClient.Split(' ')[0].ToLower() == "exit")
                    {
                        writer.Close();
                        reader.Close();
                        networkStream.Close();
                        ClientSocket.Close();
                    }
                    else
                    {
                        Console.WriteLine("From client - " + ID + " : " + dataFromClient);
                        rCount = Convert.ToString(requestCount);
                        Program.Broadcast(dataFromClient, ID, true);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(this.ID + " has left the server");
                    ClientList.RemoveAll(item => item.Item1.Equals(this.ID));
                    Program.Broadcast(this.ID + " left the server", this.ID, false);
                    x = false;
                    writer.Close();
                    reader.Close();
                    networkStream.Close();
                    ClientSocket.Close();

                }
            }
        }
    }
}
