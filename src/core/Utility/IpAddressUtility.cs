using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace core.Utility
{
    public static class IpAddressUtility
    {
        public static IPAddress LocalIP()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            return host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }        
    }
}