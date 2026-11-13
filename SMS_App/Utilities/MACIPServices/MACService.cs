using System.Net.NetworkInformation;

namespace SMS_App.Utilities.MACIPServices
{
    public static class MACService
    {
        public static string GetMAC()
        {
            string macAddress = "";
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddress += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }
            return macAddress;
        }
    }
}
