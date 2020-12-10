using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessangerServer
{
    class Program
    {
        private static string adres = "192.168.88.186";
        private static int port = 8005;
        private static TcpListener tcpListener;

        static void Main(string[] args)
        {
            try
            {
                string path = @"D:\Games\Stellaris Galaxy Edition\ebook\Stellaris Digital Artbook.pdf";
                byte[] package = null;
                var indx = PreparePackage(ref package, path);


                tcpListener = new TcpListener(IPAddress.Parse(adres), port);
                tcpListener.Start();
                Console.WriteLine("server is running");
                
                var client = tcpListener.AcceptTcpClient();
                if (client.Connected)
                    Console.WriteLine("device connected");
                var stream = client.GetStream();
                stream.WriteTimeout = 600000000;

                int bufferSize = 1024;
                int bytesSent = 0;
                byte[] buffer = new byte[bufferSize];

                long bytesLeft = new FileInfo(path).Length;

                stream.Write(package, 0, indx);

                using (FileStream fl = new FileStream(path, FileMode.Open))
                {
                    while (bytesLeft > 0)
                    {
                        int nextpacketsize = 0;
                        if (bytesLeft > bufferSize)
                        {
                            nextpacketsize = bufferSize;
                        }
                        else
                        {
                            nextpacketsize = (int) bytesLeft;
                        }
                        var bytesread = fl.Read(buffer, 0, nextpacketsize);
                        stream.Write(buffer, 0, bytesread);
                        bytesLeft -= bytesread;
                        Console.WriteLine(bytesLeft);
                    }
                }

                //stream.Write(package, 0, package.Length);

                //while (bytesLeft > 0)
                //{

                //    int nextpacketsize = 0;
                //    if (bytesLeft > bufferSize)
                //    {
                //        nextpacketsize = bufferSize;
                //    }
                //    else
                //    {
                //        nextpacketsize = bytesLeft;
                //    }

                //    stream.Write(package, bytesSent, nextpacketsize);
                //    bytesSent += nextpacketsize;
                //    bytesLeft -= nextpacketsize;
                //    Console.WriteLine(bytesSent / 1000000);
                //}

                stream.Close();
                client.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
            }

            Console.ReadLine();
        }


        public static int PreparePackage(ref byte[] newpackage, string filepath, string name = null)
        {
            string path = filepath;
            string newname;

            FileInfo fileInfo = new FileInfo(path);

            if (string.IsNullOrEmpty(name))
            {
                newname = Path.GetFileName(path);
            }
            else
            {
                newname = name;
            }
            
            //byte[] data = File.ReadAllBytes(path);
            byte[] filename = Encoding.UTF8.GetBytes(newname);
            byte[] package = new byte[8 + 4 + filename.Length];
            newpackage = package;
            byte[] datalength = BitConverter.GetBytes(fileInfo.Length);
            byte[] filenamelength = BitConverter.GetBytes(filename.Length);
            datalength.CopyTo(package, 0);
            filenamelength.CopyTo(package, 8);
            filename.CopyTo(package, 12);
            //data.CopyTo(package, filename.Length + 8);

            return filename.Length+12;
        }
    }
}
