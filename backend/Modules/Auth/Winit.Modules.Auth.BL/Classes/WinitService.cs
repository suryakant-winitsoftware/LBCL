using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace Winit.Modules.Auth.BL.Classes;

public class WinitService
{
    private readonly HttpClient _httpClient;
    private IConfiguration _configuration;
    public WinitService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;

    }

    public async Task<bool> CallSOAPServiceAsync()
    {
        string url = _configuration["WinitURL:ServiceURL"]!;// "http://dev.winitmobile.com/winitapps/services.asmx"; // Change to HTTPS if available
        string key = _configuration["WinitURL:AppKey"]!;
        string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
        <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                       xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
                       xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
          <soap:Body>
            <GetAppStatus xmlns=""http://tempuri.org/"">
              <AppName>{key}</AppName>
            </GetAppStatus>
          </soap:Body>
        </soap:Envelope>";

        var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
        content.Headers.Add("SOAPAction", "http://tempuri.org/GetAppStatus");

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode(); // Throws exception if HTTP error
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var envelope = DeserializeXml<SoapResponse.Envelope>(responseBody);
                return envelope.Body.GetAppStatusResponse.GetAppStatusResult.Status.Equals("Active", StringComparison.OrdinalIgnoreCase);
            }

        }
        catch (HttpRequestException ex)
        {
            throw ex;
        }

        return false;
    }
    public static T DeserializeXml<T>(string xml)
    {
        var serializer = new XmlSerializer(typeof(T));
        using (var reader = new StringReader(xml))
        {
            return (T)serializer.Deserialize(reader);
        }
    }
}

public class SoapResponse
{
    [XmlRoot(Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Envelope
    {
        [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public Body Body { get; set; }
    }

    public class Body
    {
        [XmlElement(ElementName = "GetAppStatusResponse", Namespace = "http://tempuri.org/")]
        public GetAppStatusResponse GetAppStatusResponse { get; set; }
    }

    public class GetAppStatusResponse
    {
        [XmlElement(ElementName = "GetAppStatusResult", Namespace = "http://tempuri.org/")]
        public GetAppStatusResult GetAppStatusResult { get; set; }
    }

    public class GetAppStatusResult
    {
        [XmlElement(ElementName = "Status")] public string Status { get; set; }
    }
}
