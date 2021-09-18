using System;

namespace SecureClient
{
    class Program
    {
        static void Main(string[] args)
        {
            AuthConfig authConfig = AuthConfig.ReadJsonFromFile("appsettings.json");

            Console.WriteLine($"Authority: {authConfig.Authority}");
        }
    }
}
