using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SMS_App.Utilities.ShortMessageService;
public class SmsApiService
{
    private readonly HttpClient _httpClient;

    public SmsApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetSmsDashboardData()
    {


        var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        string vendorAPILink = MyConfig.GetValue<string>("PhoneSMSSetup:PhoneSMSVendorAPILink");
        string token = MyConfig.GetValue<string>("PhoneSMSSetup:Token");

        var url = $"{vendorAPILink}token={token}&balance&expiry&rate&tokensms&totalsms&monthlysms&tokenmonthlysms&json";

        var response = await _httpClient.GetAsync(url);

        return await response.Content.ReadAsStringAsync();
    }
}