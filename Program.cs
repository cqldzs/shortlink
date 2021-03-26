using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace shortlink
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!"+ Directory.GetCurrentDirectory());

            try
            {
                WebServer.Start();
            }
            catch (Exception)//忽略异常
            {

                //throw;
            }

            Console.Read();
        }
    }
}
