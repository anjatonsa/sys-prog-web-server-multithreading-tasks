using System;
using System.Net;
using System.Text;
using System.Threading;

namespace DrugiProjekat
{
    internal class Program
    {
        static string port = "5000/";
        static string url = "http://localhost:" + port;
        static string root = "..\\..\\..\\..\\Root\\";

        static async Task Main()
        {
            Console.WriteLine("Main thread...");

            string[] arguments = { url, root };
        
            WebServer server = new WebServer(arguments);
            await server.Start();

        }
    }
}