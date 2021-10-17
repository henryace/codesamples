using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Extensions.Configuration;

namespace Ploeh.Samples.HelloDI.Console
{
    public static class Program
    {
        private static void Main()
        {
            EarlyBindingExample();
            LateBindingExample();

            System.Console.ReadLine();
        }

        private static void EarlyBindingExample()
        {
            // SecureMessageWriter 實作應用程式安全功能
            // ConsoleMessageWriter 實作與使用者介面之間關聯
            IMessageWriter writer =
                new SecureMessageWriter(
                    writer: new ConsoleMessageWriter(),
                    identity: GetIdentity());

            var salutation = new Salutation(writer);

            salutation.Exclaim();
        }

        private static void LateBindingExample()
        {
            // read from appsettings.json file
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string typeName = configuration["messageWriter"];
            Type type = Type.GetType(typeName, throwOnError: true);

            // SecureMessageWriter 實作應用程式安全功能，經過授權才能輸出
            // LateBinding
            //     ConsoleMessageWriter() => (IMessageWriter)Activator.CreateInstance(type)
            IMessageWriter writer =
                new SecureMessageWriter(
                    writer: (IMessageWriter)Activator.CreateInstance(type),
                    identity: GetIdentity());

            var salutation = new Salutation(writer);

            salutation.Exclaim();
        }

        private static IIdentity GetIdentity()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsIdentity.GetCurrent();
            }
            else
            {
                // For non-Windows OSes, like Mac and Linux.
                return new GenericIdentity(
                    Environment.GetEnvironmentVariable("USERNAME")
                    ?? Environment.GetEnvironmentVariable("USER"));
            }
        }
    }
}