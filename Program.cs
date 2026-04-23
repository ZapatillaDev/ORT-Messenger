using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Reflection.Metadata;

namespace ORT_Messenger
{
    internal class Program
    {
        static string version = "0.6";
        static bool conectado = false;
        static string miIP;
        static int port;
        static void Main(string[] args)
        {
            Thread mainThread = Thread.CurrentThread;
            miIP = GetLocalIPAddress();
            Console.WriteLine($"ORT Messenger v{version}\r\nTu IP es {miIP}");
            Console.WriteLine("\r\nEscribí la dirección de puerto");
            port = int.Parse(Console.ReadLine());
            Thread server = new Thread(SettingServer);
            server.Start();
            Thread mandar = new Thread(SendMessages);
            mandar.Start();

            static string GetLocalIPAddress()
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    // 1. Filtramos que la interfaz esté activa (Up)
                    // 2. Que no sea de tipo Loopback (127.0.0.1)
                    // 3. Que no sea una interfaz virtual (evita vEthernet de Hyper-V/Docker)
                    if (ni.OperationalStatus == OperationalStatus.Up &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        !ni.Description.ToLower().Contains("virtual") &&
                        !ni.Name.ToLower().Contains("virtual"))
                    {
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            // Solo queremos IPv4
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
                throw new Exception("No estás conectado a Internet");
            }


        }
        public static void SendMessages()
        {
            string encriptado(string mensaje)
            {
                char[] array = mensaje.ToCharArray();
                int[] key = { 1, 8, 1, 2, 2, 0, 2, 2 };
                for (int i = 0; i < array.Length; i++)
                {
                    char temp = (char)(array[i] + key[i % 8]);
                    array[i] = temp;

                }
                string texto = new string(array);
                return texto;


            }
            bool allowMessages = true;
            int Color = 7;
            Console.WriteLine("Introducí la IP del destinatario");
            string ipServidor = Console.ReadLine();
            TcpClient client = new(ipServidor, port);
            Console.WriteLine("User:");
            string user = Console.ReadLine();
            Console.WriteLine("Escribí tu mensaje:");

            while (true)
            {
                string msg = Console.ReadLine();
                string antiBlank = msg.Replace(" ", "");
                if (antiBlank == "")
                {
                    Console.WriteLine("No puedes mandar un mensaje vacio");
                    allowMessages = false;
                    Task.Delay(10);
                    allowMessages= true;
                    //Esto no anda hay que arreglarlo
                }
                if (msg == "/clear")
                {
                    Console.Clear();
                    Console.WriteLine($"ORT Messenger v{version}\r\nTu IP es {miIP}");
                }
                if (msg[0] == '/' && msg[1] == 'C')
                {
                    //string[] colorList = msg.Split(' ');    
                    //if (hola1[1] == "list")
                    //{
                    //    Console.WriteLine("");
                    //}
                    //int newColor = int.Parse(colorList[1]);
                    //ConsoleColor color = ConsoleColor.DarkCyan;
                    //ConsoleColor.colorList[1];
                    
                    
                }
                else if (allowMessages == true)
                {
                    string mensaje = user + ": " + msg;
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write($"{user}: ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(msg);
                    string mensajeEncriptado = encriptado(mensaje);
                    byte[] data = Encoding.UTF8.GetBytes(mensajeEncriptado);
                    NetworkStream stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                }
            }
        }

        public static void SettingServer()
        {
            string desencriptar(string mensaje)
            {
                char[] array = mensaje.ToCharArray();
                int[] key = { 1, 8, 1, 2, 2, 0, 2, 2 };
                for (int i = 0; i < array.Length; i++)
                {
                    char temp = (char)(array[i] - key[i % 8]);
                    array[i] = temp;

                }
                string texto = new string(array);
                return texto;

            }
            while (true)
            {
                TcpListener server = new TcpListener(IPAddress.Any, port);
                server.Start();

                // Aceptamos al cliente
                using TcpClient client = server.AcceptTcpClient();
                conectado = true;

                while (true)
                {
                    // Leemos el mensaje
                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    string mensaje = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string mensajeDesencriptado = desencriptar(mensaje);
                    string[] userFinal = mensajeDesencriptado.Split(':');

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write($"{userFinal[0]}:");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    string final67 = mensajeDesencriptado.Replace(userFinal[0], "");
                    final67 = final67.Replace(":", "");
                    Console.WriteLine(final67);
                    Console.Beep();
                }
            }

        }
    }
}




