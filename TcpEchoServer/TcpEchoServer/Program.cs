using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace TcpEchoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting echo server...");

            int port = 1234;
            TcpListener listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();

            List<TcpClient> clientList = new List<TcpClient>();

            if (listener.Pending())
            {
                clientList.Add(new Client(listener));
            }
            //TcpClient client = listener.AcceptTcpClient();
            //NetworkStream stream = client.GetStream();
            //StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
            //StreamReader reader = new StreamReader(stream, Encoding.ASCII);

            //while (true)
            //{
            //	string inputLine = "";
            //	//string stop = "Exit";
            //	while (inputLine != null)
            //	{
            //		//if()
            //		inputLine = reader.ReadLine();
            //		writer.WriteLine("Echoing string: " + inputLine);
            //		Console.WriteLine("Echoing string: " + inputLine);
            //	}
            //	Console.WriteLine("Server saw disconnect from client.");
            //}
        }
    }
    //utf8
    class Client
    {
        public TcpClient client;
        public NetworkStream stream;
        public StreamWriter writer;
        public StreamReader reader;

        public Client(TcpListener listener)
        {
            client = listener.AcceptTcpClient();
            stream = client.GetStream();
            writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
            reader = new StreamReader(stream, Encoding.ASCII);

            bool connectionEstablished = false;
            while (!connectionEstablished)
            {
                string inputLine = "";
                while (!connectionEstablished)
                {
                    inputLine = reader.ReadLine();
                    string[] answerArr = inputLine.Split(' ', 2);
                    if (answerArr[0].ToLower() == "connect")
                    {
                        answerArr[1].Trim();
                    }
                    else
                    {
                        writer.WriteLine("Please connect with a username");
                        writer.WriteLine("Syntax: ");
                        writer.WriteLine("    CONNECT user_name");
                    }
                }
            }
        }
    }
}
