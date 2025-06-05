using System.Net.NetworkInformation;

namespace Service.Helper;

public static class NetworkHelper
{
    public static async Task<bool> IsNetworkAvailableAsync()
    {
        try
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return false;

            using var ping = new Ping();
            var reply = await ping.SendPingAsync("8.8.8.8", 3000); // Google DNS, timeout 3 seconds
            
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }
}