using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static string command = "";
        static List<string> input = new List<string>();
        static string userInput = "";
        static string messageToServer = "";
        static void Main(string[] args)
        {
            Console.WriteLine("Simple Chat Client-Server");
            System.Console.WriteLine("=====================");
            System.Console.WriteLine();
            System.Console.WriteLine("Lista de comandos:");
            System.Console.WriteLine("- Para conectarse al servidor: CONNECT [nombre de usuario]");
            System.Console.WriteLine("- Para ver lista de usuarios: LIST_USERS");
            System.Console.WriteLine("- Para enviar mensaje: SEND_MESSAGE [nombre del receptor] [mensaje]");
            System.Console.WriteLine("- Para terminar conexion: EXIT");
            System.Console.WriteLine("============================");
            System.Console.WriteLine();

            GetUserInput();

            while (isCommandInvalid())
            {
                System.Console.WriteLine("Comando invalido. Por favor intente de nuevo");
                GetUserInput();
            }

            System.Console.WriteLine();

            int port = 8000;
            TcpClient client = new TcpClient();
            client.Connect("1.1.1.1", 8000);
            NetworkStream stream = client.GetStream();
            StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
            Thread serverReaderThread = new Thread(() => ReaderHandler(ref stream));
            serverReaderThread.Start();
            while (true)
            {
                Console.WriteLine("Sending to server: " + "'" + messageToServer + "'");
                writer.WriteLine(messageToServer);
                /*
                Para evitar que se imprima "Escriba un comando" en la misma linea que
                "Received from server: ...". 
                */
                Thread.Sleep(500);
                GetUserInput();
                if (messageToServer.Equals("exit"))
                {
                    Console.WriteLine("Sending to server: " + "'" + messageToServer + "'");
                    writer.WriteLine(messageToServer);
                    break;
                }
            }
        }

        static void ReaderHandler(ref NetworkStream networkStream)
        {
            StreamReader reader = new StreamReader(networkStream, Encoding.ASCII);
            while ((true))
            {
                try
                {
                    string lineReceived = reader.ReadLine();
                    Console.WriteLine("Received from server: " + lineReceived);
                    if (messageToServer.Equals("exit"))
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

        }
        static void GetUserInput()
        {
            System.Console.Write("Escriba un comando: ");
            userInput = Console.ReadLine();
            input = userInput.Split(' ').ToList().Select(e =>
            {
                return e.Trim();
            }).ToList();
            command = input[0];
            messageToServer = string.Join(" ", input.ToArray());
        }

        static bool isCommandInvalid()
        {
            bool isInvalid = false;
            if (command != "connect" && command != "list_users" && command != "send_message" && command != "exit")
            {
                return true;
            }
            switch (command)
            {
                case "connect":
                    if (input.LongCount() > 2)
                    {
                        System.Console.WriteLine("El nombre de usuario no puede tener espacios. Usa '_' o '-' para separar.");
                        isInvalid = true;
                    }
                    else
                    {
                        isInvalid = false;
                    }
                    break;
                case "list_users":
                    break;
                case "send_message":
                    if (input.LongCount() < 3)
                    {
                        System.Console.WriteLine("El formato para enviar mensajes: SEND_MESSAGE [nombre del receptor(sin espacios)] [mensaje]");
                        isInvalid = true;
                    }
                    else if (input[1].Split(' ').LongCount() > 1)
                    {
                        System.Console.WriteLine("El nombre de usuario no puede tener espacios. Usa '_' o '-' para separar.");
                        isInvalid = true;
                    }
                    else
                    {
                        isInvalid = false;
                    }
                    break;
                case "exit":
                    break;
            }
            return isInvalid;
        }
    }
}
