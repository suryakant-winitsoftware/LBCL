using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Winit.Modules.Base.BL;
using Winit.Modules.ServiceAndCallRegistration.Model.Classes;
using Winit.Modules.ServiceAndCallRegistration.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using WINITServices.Interfaces.CacheHandler;

namespace WINITAPI.Controllers.ServiceAndCallRegistration
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceAndCallRegistrationController : WINITBaseController
    {
        IConfiguration _configuration { get; }
        public ServiceAndCallRegistrationController(IServiceProvider serviceProvider, IConfiguration configuration) : base(serviceProvider)
        {
            _configuration = configuration;
        }

        [HttpPost("LogACall")]
        public async Task<IActionResult> LogACall([FromBody] CallRegistration request)
        {
            string soapEndpoint = _configuration["CallRegistrationUserCredintials:CallRegistrationEndPoint"];
            request.Username = _configuration["CallRegistrationUserCredintials:Username"];
            request.Password = _configuration["CallRegistrationUserCredintials:Password"];
            request.DeviceId = _configuration["CallRegistrationUserCredintials:DeviceId"];
            request.RelationshipWithCmi = CommonFunctions.GetIntValue(_configuration["CallRegistrationUserCredintials:RelationshipWithCmi"]);
            // Dynamically create the SOAP envelope
            string soapEnvelope = $@"
                <soap:Envelope xmlns:soap='http://www.w3.org/2003/05/soap-envelope' xmlns:xsd='http://www.smsws.com/thirdparty/xsd'>
                   <soap:Header/>
                   <soap:Body>
                      <xsd:logACall>
                         <xsd:custType>{request.CustomerType}</xsd:custType>
                         <xsd:custName>{request.CustomerName}</xsd:custName>
                         <xsd:contactPerson>{request.ContactPerson}</xsd:contactPerson>
                         <xsd:custAddress>{request.Address}</xsd:custAddress>
                         <xsd:custPincode>{request.Pincode}</xsd:custPincode>
                         <xsd:custMobile>{request.MobileNumber}</xsd:custMobile>
                         <xsd:custEmail>{request.EmailID}</xsd:custEmail>
                         <xsd:cmiRelationShipNo>{request.CmiRelationshipNumber}</xsd:cmiRelationShipNo>
                         <xsd:productCategoryCode>{request.ProductCategoryCode}</xsd:productCategoryCode>
                         <xsd:modelCode>{request.ModelCode}</xsd:modelCode>
                         <xsd:serviceType>{request.ServiceType}</xsd:serviceType>
                         <xsd:custCoverageClaim>{request.WarrantyStatus}</xsd:custCoverageClaim>
                         <xsd:sellerName>{request.ResellerName}</xsd:sellerName>
                         <xsd:purchaseDate>{request.PurchaseDate}</xsd:purchaseDate>
                         <xsd:custRemarks>{request.Remarks}</xsd:custRemarks>
                         <xsd:userName>{request.Username}</xsd:userName>
                         <xsd:password>{request.Password}</xsd:password>
                         <xsd:deviceId>{request.DeviceId}</xsd:deviceId>
                         <xsd:serviveRequestorMobile>{request.ServiceRequestorMobile}</xsd:serviveRequestorMobile>
                         <xsd:relationshipWithCMI>{request.RelationshipWithCmi}</xsd:relationshipWithCMI>
                      </xsd:logACall>
                   </soap:Body>
                </soap:Envelope>";

            try
            {
                using (var client = new HttpClient())
                {
                    var content = new StringContent(soapEnvelope, Encoding.UTF8, "application/soap+xml");

                    // Send the request
                    HttpResponseMessage response = await client.PostAsync(soapEndpoint, content);

                    string responseContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        // Parse successful response
                        ICallRegistrationResponce callresult = ParseCallRegistrationSoapResponse(responseContent);
                        return CreateOkApiResponse(callresult);
                    }
                    else
                    {
                        // Handle SOAP fault or server errors
                        var faultMessage = ParseSoapFault(responseContent);
                        return StatusCode((int)response.StatusCode, new { Error = faultMessage });
                    }
                    // Parse the response into the model
                    //ICallRegistrationResponce result = ParseCallRegistrationSoapResponse(responseContent);
                    //return CreateOkApiResponse(new
                    //{
                    //    Data = new
                    //    {
                    //        CallID = result.CallID
                    //    },
                    //    StatusCode = result.StatusCode,
                    //    Errors = result.Errors
                    //});
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"SOAP API call failed: {ex.Message}");
            }
        }
        [HttpGet]
        public Winit.Modules.ServiceAndCallRegistration.Model.Interfaces.ICallRegistrationResponce ParseCallRegistrationSoapResponse(string responseContent)
        {
            var responseXml = XDocument.Parse(responseContent);
            XNamespace ns = "http://www.smsws.com/thirdparty/xsd";

            var statusCode = int.Parse(responseXml.Descendants(ns + "statusCode").FirstOrDefault()?.Value ?? "0");
            var callId = responseXml.Descendants(ns + "callId").FirstOrDefault()?.Value;
            var errors = responseXml.Descendants(ns + "error").Select(e => e.Value).ToList();

            return new CallRegistrationResponce
            {
                StatusCode = statusCode,
                CallID = callId,
                Errors = errors
            };
        }






        [HttpGet("ServiceStatus")]
        public async Task<IActionResult> ServiceStatus([FromQuery] string callId)
        {
            string soapEndpoint = _configuration["CallRegistrationUserCredintials:CallRegistrationEndPoint"];
            string password = _configuration["CallRegistrationUserCredintials:Password"];
            string userName = _configuration["CallRegistrationUserCredintials:Username"];
            string deviceId = _configuration["CallRegistrationUserCredintials:DeviceId"];

            string soapEnvelope = $@"
                    <soap:Envelope xmlns:soap='http://www.w3.org/2003/05/soap-envelope' xmlns:xsd='http://www.smsws.com/thirdparty/xsd'>
                       <soap:Header/>
                       <soap:Body>
                          <xsd:serviceStatus>
                             <xsd:callId>{callId}</xsd:callId>
                             <xsd:deviceId>{deviceId}</xsd:deviceId>
                             <xsd:userName>{userName}</xsd:userName>
                             <xsd:password>{password}</xsd:password>
                          </xsd:serviceStatus>
                       </soap:Body>
                    </soap:Envelope>";
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new StringContent(soapEnvelope, Encoding.UTF8, "application/soap+xml");

                    // Send the request
                    HttpResponseMessage response = await client.PostAsync(soapEndpoint, content);

                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        // Parse successful response
                        IServiceRequestStatusResponce result = ParseServiceStatusSoapResponse(responseContent);
                        return CreateOkApiResponse(result);
                    }
                    else
                    {
                        // Handle SOAP fault or server errors
                        var faultMessage = ParseSoapFault(responseContent);
                        return StatusCode((int)response.StatusCode, new { Error = faultMessage });
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"SOAP API call failed: {ex.Message}");
            }
        }

        [HttpGet]
        public IServiceRequestStatusResponce ParseServiceStatusSoapResponse(string responseContent)
        {
            var responseXml = XDocument.Parse(responseContent);
            XNamespace ns = "http://www.smsws.com/thirdparty/xsd";

            return new ServiceRequestStatusResponce
            {
                StatusCode = int.Parse(responseXml.Descendants(ns + "statusCode").FirstOrDefault()?.Value ?? "0"),
                CallLoggedDate = responseXml.Descendants(ns + "callLoggedDate").FirstOrDefault()?.Value,
                ItemName = responseXml.Descendants(ns + "itemName").FirstOrDefault()?.Value,
                ItemCode = responseXml.Descendants(ns + "itemCode").FirstOrDefault()?.Value,
                EquipmentNo = responseXml.Descendants(ns + "equipmentNo").FirstOrDefault()?.Value,
                EngineerName = responseXml.Descendants(ns + "engineerName").FirstOrDefault()?.Value,
                ServiceStatus = responseXml.Descendants(ns + "serviceStatus").FirstOrDefault()?.Value,
                ServiceOutcome = responseXml.Descendants(ns + "serviceOutcome").FirstOrDefault()?.Value,
                PendingReason = responseXml.Descendants(ns + "pendingReason").FirstOrDefault()?.Value,
                CallStatus = responseXml.Descendants(ns + "callStatus").FirstOrDefault()?.Value,
                ServiceCompletionDate = responseXml.Descendants(ns + "serviceCompletionDate").FirstOrDefault()?.Value,
                CmiRelationshipNo = responseXml.Descendants(ns + "cmiRelationshipNo").FirstOrDefault()?.Value,
                Errors = responseXml.Descendants(ns + "error").Select(e => e.Value).ToList()
            };
        }

        [HttpGet]
        private string ParseSoapFault(string responseContent)
        {
            var responseXml = XDocument.Parse(responseContent);
            XNamespace soapEnv = "http://www.w3.org/2003/05/soap-envelope";

            var faultText = responseXml.Descendants(soapEnv + "Text").FirstOrDefault()?.Value;

            return faultText ?? "Unknown SOAP fault occurred.";
        }



    }
}
