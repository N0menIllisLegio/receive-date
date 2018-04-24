using System;
using System.Net;
using System.Net.Sockets;

namespace Date
{
    public class DayTimeClient : TcpClient
    {
        public static void GetNetworkTime()
        {
            const string ntpServer = "time.windows.com";
            
            var ntpData    = new byte[48];     
            var ntpSecData = new byte[8];

            // (Индикатор коррекции: 0 - без коррекции, 4 - номер версии, Режим: 3 - клиент)
            ntpData[0] = Convert.ToByte("043", 8); 
            
            var addresses = Dns.GetHostEntry(ntpServer).AddressList;

            var ipEndPoint = new IPEndPoint(addresses[0], 123);

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(ipEndPoint);
                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();
            }

            Array.Copy(ntpData, 40, ntpSecData, 0, 8);
            var milliseconds = GetMilliSeconds(ntpSecData);
            var networkDateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((long) milliseconds);
            Console.WriteLine(networkDateTime.ToLocalTime());
        }

        public static ulong GetMilliSeconds(byte[] ntpTime)
        {
            // целое число секунд и их дробная часть
            ulong intpart = 0, fractpart = 0;

            for (var i = 0; i < 4; i++)
            {
                intpart = 256 * intpart + ntpTime[i];
            }

            for (var i = 4; i < 8; i++)
            {
                fractpart = 256 * fractpart + ntpTime[i];
            }

            return intpart * 1000 + ((fractpart * 1000) / 0x100000000L);
        }

        public static void Main()
        {
            GetNetworkTime();
            Console.ReadKey();
        }
    }
}
