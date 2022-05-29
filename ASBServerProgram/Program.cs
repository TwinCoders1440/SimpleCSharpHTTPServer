using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.IO.Compression;

namespace ASBServerProgram
{
    class Program
    {
        private static IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        private static Int32 port = 8080;
        private static int byteArreySize = 50000;
        private static int counter = 0;

        //program start from here
        static void Main(string[] args)
        {
            if(args.Length < 2)
            {
                Console.WriteLine("Usage: appName.exe ip port");
                Console.ReadKey();
                return;
            }

            localAddr = IPAddress.Parse(args[0]);
            port = Int32.Parse(args[1]);

            TcpListener serverSocket = new TcpListener(localAddr, port);

            serverSocket.Start();
            Console.WriteLine(">> ASB's Server Started");

            for (int i = 0; i < 100; i++)
            {
                Thread acceptThread = new Thread(new ParameterizedThreadStart(Program.Fun));

                acceptThread.Start(serverSocket);

                acceptThread.Join();

            }

        }//end Main function

        
        static void Fun(Object listener)
        {
            TcpListener serSocket = (TcpListener)listener;
            TcpClient clientSocket = new TcpClient();

            clientSocket = serSocket.AcceptTcpClient();
            Console.WriteLine(">> Accept connection from a Client");

            while (true)
            {
                try
                {
                    NetworkStream browserNetworkStream = clientSocket.GetStream();

                    byte[] bytesFromClient = new byte[byteArreySize];
                    string bytesFromClientString = null;
                    Thread.Sleep(50);

                    if (browserNetworkStream.DataAvailable)
                    {
                        while (browserNetworkStream.DataAvailable)
                            browserNetworkStream.Read(bytesFromClient, 0, bytesFromClient.Length);


                        bytesFromClientString = System.Text.Encoding.ASCII.GetString(bytesFromClient);
                        Console.WriteLine("\nThe header is: " + bytesFromClientString.Substring(0,
                            bytesFromClientString.IndexOf("\r\n\r\n")));
                    }
                    else
                    {

                        break;

                    }//end else

                    string hostRecognationString = bytesFromClientString;
                    string hostString = null;
                    hostString = hostRecognationString.Substring(hostRecognationString.IndexOf("Host: ") + 6);
                    hostRecognationString = hostRecognationString.Substring(hostRecognationString.IndexOf("Host: ") + 6);
                    hostString = hostRecognationString.Substring(0, hostRecognationString.IndexOf("\r\n"));

                    Console.WriteLine("Host is: " + hostString);

                    string portRecegnationString = ASCIIEncoding.ASCII.GetString(bytesFromClient);
                    Int32 hostPort = 80;

                    portRecegnationString = portRecegnationString.Substring(0, portRecegnationString.IndexOf(" HTTP"));
                    if (portRecegnationString.Contains(":"))
                    {
                        portRecegnationString = portRecegnationString.Substring(portRecegnationString.IndexOf(":") + 1,
                            portRecegnationString.IndexOf(" HTTP") - portRecegnationString.IndexOf(":"));

                        hostPort = Convert.ToInt32(portRecegnationString);

                    }//end if

                    Console.WriteLine("Port is: " + hostPort);

                    IPAddress[] hostAddr = new IPAddress[1];
                    hostAddr = Dns.GetHostAddresses(hostString);
                    Console.WriteLine("the dns: " + Convert.ToString(hostAddr[0]));

                    TcpClient bindingToHostSocket = new TcpClient();
                    bindingToHostSocket.Connect(Convert.ToString(hostAddr[0]), hostPort);

                    NetworkStream HostNetworkStream = bindingToHostSocket.GetStream();

                    HostNetworkStream.Write(bytesFromClient, 0, bytesFromClient.Length);
                    HostNetworkStream.Flush();

                    byte[] byteFromHost = new byte[byteArreySize];
                    int numberOfBytesRead = 0;
                    Thread.Sleep(50);
                    if (HostNetworkStream.DataAvailable)
                    {
                        while (HostNetworkStream.DataAvailable)
                            numberOfBytesRead += HostNetworkStream.Read(byteFromHost, 0, byteFromHost.Length);

                        String bytesFromHostString = Encoding.ASCII.GetString(byteFromHost);
                        bytesFromHostString = bytesFromHostString.Substring(0,
                            bytesFromHostString.IndexOf("\r\n\r\n"));

                        Console.WriteLine("\nThe Host Response: " + bytesFromHostString);

                        browserNetworkStream.Write(byteFromHost, 0, numberOfBytesRead);
                        browserNetworkStream.Flush();
                    }

                    else
                    {
                        bindingToHostSocket.Close();
                        HostNetworkStream.Close();

                        browserNetworkStream.Close();

                        break;

                    }//end else

                }//end try

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                }//end catch

            }//end while

            Console.WriteLine(">> Connection lost.");

        }//end fun()
    
    }//end Program class

}//end ASBServerProgram namespace
