using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using Winit.Modules.Common.Model.Constants.Notification;
using Winit.Modules.SMS.DL.Interfaces;
using Winit.Modules.SMS.Model.Classes;
using Winit.Modules.SMS.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.SMS.DL.Classes
{
    public class PGSQLSMSDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ISMSDL
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private string? SmsApiUrl { get; set; }
        private string? SmsKey { get; set; }
        public PGSQLSMSDL(IServiceProvider serviceProvider, IConfiguration configuration, HttpClient httpClient) : base(serviceProvider, configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            SmsApiUrl = _configuration.GetSection("ApiSettings:SMSApiUrl").Value;
            SmsKey = _configuration.GetSection("ApiSettings:SMSKey").Value;
        }
        public async Task<int> CheckExistsOrNot(ISms smsRequest)
        {
            int count = -1;
            try
            {
                string? existingUID = await CheckIfUIDExistsInDB(DbTableName.SmsRequest, smsRequest.UID);
                if (existingUID != null)
                {
                    count = await UpdateSmsRequest(smsRequest);
                }
                else
                {
                    count = await CreateSmsRequest(smsRequest);
                }

                return count;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> CreateSmsRequest(ISms smsModel)
        {
            try
            {
                var sql =
                    @"INSERT INTO sms_request(uid, sender, priority, message_type, content, request_status, request_time, sent_time, 
                     error_details, retry_count, gateway_provider, response_code, response_time, response_status, 
                     response_message, batch_id) 
                    VALUES (@UID, @Sender, @Priority, @MessageType, @Content, @RequestStatus, @RequestTime, @SentTime, 
                     @ErrorDetails, @RetryCount, @GatewayProvider, @ResponseCode, @ResponseTime, @ResponseStatus, 
                     @ResponseMessage, @BatchId);";
                int Result = await ExecuteNonQueryAsync(sql, smsModel);
                await CreateSmsRequestReceivers(smsModel);
                return Result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<int> CreateSmsRequestReceivers(ISms model)
        {
            try
            {
                var requestBodies = model.Receivers.Select(receiver => new SmsModelReceiver
                {
                    UID = Guid.NewGuid().ToString(),
                    SmsRequestUID = model.UID,
                    Receiver = receiver.Receiver,
                }).ToList();

                var sql = @"INSERT INTO sms_request_receivers(uid,sms_request_uid,receiver)
                            VALUES(@UID,@SmsRequestUID,@Receiver);";
                return await ExecuteNonQueryAsync(sql, requestBodies);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<int> UpdateSmsRequest(ISms model)
        {
            try
            {
                var sql = @"UPDATE sms_request
                            SET request_status = @RequestStatus, sent_time = @SentTime, error_details = @ErrorDetails, retry_count = @RetryCount,response_code = @ResponseCode,
                            response_time = @ResponseTime, response_status = @ResponseStatus, response_message = @ResponseMessage, batch_id = @BatchId
                            WHERE uid = @UID;";
                return await ExecuteNonQueryAsync(sql, model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<ISmsModel>> GetSmsRequest(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "UID", UID  },
                };
                var sql = @"SELECT uid AS UID, sender AS Sender, priority AS Priority, message_type AS MessageType, 
                            content AS Content, request_status AS RequestStatus, request_time AS RequestTime, 
                            sent_time AS SentTime, error_details AS ErrorDetails, retry_count AS RetryCount, 
                            gateway_provider AS GatewayProvider, response_code AS ResponseCode, 
                            response_time AS ResponseTime, response_status AS ResponseStatus, 
                            response_message AS ResponseMessage, batch_id AS BatchId
                            FROM sms_request where UID = @UID; ";
                return await ExecuteQueryAsync<ISmsModel>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<ISmsRequestModel>> GetSmsRequestFromJob()
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                };
                var sql = @"select 
                            sr.uid as UID,
                            sr.content as Content, 
                            STRING_AGG(srr.receiver, ',') as Receivers, 
                            sr.retry_count as RetryCount
                            from sms_request sr
                            inner join sms_request_receivers srr on sr.uid = srr.sms_request_uid
                            where sr.request_status in ('Pending', 'Failed') and sr.retry_count < 3
                            group by sr.uid, sr.content, sr.retry_count;";
                List<SmsRequestDTO> smsRequestDTOs = await ExecuteQueryAsync<SmsRequestDTO>(sql, parameters);
                return smsRequestDTOs.Select(r => new SmsRequestModel
                {
                    UID = r.UID,
                    Content = r.Content,
                    RetryCount = r.RetryCount,
                    Receivers = r.Receivers?.Split(',').ToList() ?? new List<string>()
                }).ToList<ISmsRequestModel>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<INotificationPOData> GetNotificationDataForPO(string TemplateName, string OrderUID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "OrderUID", OrderUID  },
                };
                var sql = "";
                switch (TemplateName)
                {
                    case NotificationTemplateNames.INVOICE_RECEIVED_FROM_ORACLE_SEND_FOR_INFO:
                        sql = @"Select channel_partner, order_no, order_qty,invoice_no,invoice_qty,invoice_amount from table where order_uid = @OrderUID";
                        break;
                    default:
                        sql = @"SELECT s.name AS ChannelPartnerName,poh.uid as OrderUID, COALESCE(poh.order_number, poh.draft_order_number) AS OrderNo, poh.qty_count OrderQty, poh.qty_count ApprovedQty, poh.net_amount OrderValue, e.name CreatedBy,
                                ISNULL(c.mobile, c.mobile2) AS ChannelPartnerMobileNo, c.email AS ChannelPartnerEmail, eiasm.phone AS ASMMobileNo, eiasm.email AS ASMEmail,
                                eibm.phone AS BMMobileNo, eibm.email AS BMEmail, eiasm.emp_uid, eibm_emp.name AS ApprovedBy, 
								jpasm.user_role_uid as Role,
								CASE 
									WHEN jpasm.user_role_uid = 'ASM' THEN e.name 
								END AS ASMName
                                FROM purchase_order_header poh 
                                INNER JOIN store s on s.uid = poh.org_uid 
                                INNER JOIN emp e on e.uid = poh.created_by
                                AND poh.uid = @OrderUID
                                LEFT JOIN contact c on c.linked_item_type = 'Store' and c.linked_item_uid = s.uid AND c.is_default = 1
                                LEFT JOIN emp_info eiasm on eiasm.emp_uid = poh.reporting_emp_uid 
                                LEFT JOIN job_position jpasm on jpasm.emp_uid = poh.reporting_emp_uid 
                                LEFT JOIN job_position jpbm on jpbm.uid = jpasm.reports_to_uid
                                LEFT JOIN emp_info eibm on eibm.emp_uid = jpbm.emp_uid
                                LEFT JOIN emp eibm_emp ON eibm.emp_uid = eibm_emp.uid";
                        break;
                }
                List<INotificationPOData> ReceiverDetails = await ExecuteQueryAsync<INotificationPOData>(sql, parameters);
                return ReceiverDetails.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return new NotificationPOData();
            }
        }

        public async Task<ISms> GetSmsRequestForPO(string notificationTemplateNames, INotificationPOData smsFields, string UniqueUID)
        {
            try
            {
                string SmsContent = string.Empty;
                switch (notificationTemplateNames)
                {
                    case NotificationTemplateNames.INVOICE_RECEIVED_FROM_ORACLE_SEND_FOR_INFO:
                        SmsContent = $" Dear Sir/Madam, Invoice generated as per below details Channel Partner {smsFields.ChannelPartnerName}. Order No {smsFields.OrderNo} Invoice No {smsFields.InvoiceNumber} Order Qty  {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)} Invoice Qty {CommonFunctions.GetStringInNumberFormat(smsFields.InvoiceQty)} Invoice Amount  {CommonFunctions.FormatNumberInIndianStyleWithoutSymbol(Convert.ToDecimal(smsFields.InvoiceValue))} Please Login to CMI-Saarthi for more information. {smsFields.Url} Carrier Midea India Pvt. Ltd. ";
                        return await PopulateSmsModal(smsFields, RoleNames.ChannelPartner, SmsContent, notificationTemplateNames, UniqueUID);
                    case NotificationTemplateNames.PO_APPROVED_BY_LAST_LEVEL_SEND_TO_CP_FOR_INFO:
                        SmsContent = $" Dear Channel Partner, Channel Partner Name {smsFields.ChannelPartnerName}. The below order is accepted. Order No {smsFields.OrderNo} Requested Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)} Ordered Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)} Order Value  {CommonFunctions.FormatNumberInIndianStyleWithoutSymbol(Convert.ToDecimal(smsFields.OrderValue))} Please Login to CMI-Saarthi for more information. {smsFields.Url} Carrier Midea India Pvt. Ltd. ";
                        return await PopulateSmsModal(smsFields, RoleNames.ChannelPartner, SmsContent, notificationTemplateNames, UniqueUID);
                    case NotificationTemplateNames.PO_CREATED_BY_CP_SEND_TO_BM_FOR_INFO:
                        SmsContent = $" Dear Sir/Madam, below order is Created in CMI-Saarthi Channel Partner Name  {smsFields.ChannelPartnerName}. Order No {smsFields.OrderNo} Created by  {smsFields.CreatedBy} Order Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)} Order Value   {CommonFunctions.FormatNumberInIndianStyleWithoutSymbol(Convert.ToDecimal(smsFields.OrderValue))} Please Login for more information. Carrier Midea India Pvt. Ltd. ";
                        return await PopulateSmsModal(smsFields, RoleNames.BranchManager, SmsContent, notificationTemplateNames, UniqueUID);
                    case NotificationTemplateNames.PO_CREATED_BY_CP_SEND_TO_ASM_FOR_APPROVAL:
                        SmsContent = $" Dear Sir/Madam, below order is Created in CMI-Saarthi Channel Partner {smsFields.ChannelPartnerName}. Order No {smsFields.OrderNo} Created by  {smsFields.CreatedBy} Order Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)} Order Value {CommonFunctions.FormatNumberInIndianStyleWithoutSymbol(Convert.ToDecimal(smsFields.OrderValue))} Please Login for Approval. Carrier Midea India Pvt. Ltd. ";
                        return await PopulateSmsModal(smsFields, RoleNames.ASM, SmsContent, notificationTemplateNames, UniqueUID);
                    case NotificationTemplateNames.PO_APPROVED_BY_BM_SEND_TO_ASM_FOR_INFO:
                        SmsContent = $"Dear Sir, The below order approved by {smsFields.ApprovedBy} Channel Partner Name {smsFields.ChannelPartnerName}. Order No {smsFields.OrderNo} Order Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)} Approved Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.ApprovedQty)} Order Value {CommonFunctions.FormatNumberInIndianStyleWithoutSymbol(Convert.ToDecimal(smsFields.OrderValue))} Please Login for more information. Carrier Midea India Pvt. Ltd. ";
                        return await PopulateSmsModal(smsFields, RoleNames.ASM, SmsContent, notificationTemplateNames, UniqueUID);
                    case NotificationTemplateNames.PO_APPROVED_BY_CP_SEND_TO_BM_FOR_APPROVAL:
                        SmsContent = $" Dear Sir/Madam, below order confirmed by Channel Partner Order No {smsFields.OrderNo} Created by {smsFields.CreatedBy} Order Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)} Order Value {CommonFunctions.FormatNumberInIndianStyleWithoutSymbol(Convert.ToDecimal(smsFields.OrderValue))} Channel Partner {smsFields.ChannelPartnerName}. Please Login for Approval. Carrier Midea India Pvt. Ltd. ";
                        return await PopulateSmsModal(smsFields, RoleNames.BranchManager, SmsContent, notificationTemplateNames, UniqueUID);
                    case NotificationTemplateNames.PO_CREATED_BY_ASM_SEND_TO_BM_FOR_INFO:
                        SmsContent = $"Dear Sir/Madam, below order is Created in CMI-Saarthi Order No {smsFields.OrderNo} Created by {smsFields.CreatedBy} Order Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)} Order Value {CommonFunctions.FormatNumberInIndianStyleWithoutSymbol(Convert.ToDecimal(smsFields.OrderValue))} Channel Partner Name {smsFields.ChannelPartnerName}. Please Login for more information. Carrier Midea India Pvt. Ltd. ";
                        return await PopulateSmsModal(smsFields, RoleNames.BranchManager, SmsContent, notificationTemplateNames, UniqueUID);
                    case NotificationTemplateNames.PO_CREATED_BY_ASM_SEND_TO_CP_FOR_APPROVAL:
                        SmsContent = @$" Dear Sir/Madam, below order is Created in CMI-Saarthi Order No {smsFields.OrderNo} Created by {smsFields.CreatedBy} Order Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)} Order Value {CommonFunctions.FormatNumberInIndianStyleWithoutSymbol(Convert.ToDecimal(smsFields.OrderValue), 2, " ")} Please Login and confirm the order. Carrier Midea India Pvt. Ltd. ";
                        return await PopulateSmsModal(smsFields, RoleNames.ChannelPartner, SmsContent, notificationTemplateNames, UniqueUID);
                    default:
                        return new Sms();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<Sms> PopulateSmsModal(INotificationPOData smsTemplateFields, string Receiver, string Content, string notificationTemplateNames, string UniqueUID)
        {
            try
            {
                switch (Receiver)
                {
                    case RoleNames.ChannelPartner:
                        return new Sms
                        {
                            UID = UniqueUID,
                            Content = Content,
                            Sender = "",
                            MessageType = "Transactional",
                            Priority = 1,
                            RequestStatus = "Pending",
                            RequestTime = DateTime.Now,
                            Receivers = new List<SmsModelReceiver>
                            {
                                new SmsModelReceiver
                                {
                                    Receiver = smsTemplateFields.ChannelPartnerMobileNo
                                },
                            }
                        };
                    case RoleNames.BranchManager:
                        return new Sms
                        {
                            UID = UniqueUID,
                            Sender = "",
                            Content = Content,
                            MessageType = "Transactional",
                            Priority = 1,
                            RequestStatus = "Pending",
                            RequestTime = DateTime.Now,
                            Receivers = new List<SmsModelReceiver>
                            {
                                new SmsModelReceiver
                                {
                                    Receiver = smsTemplateFields.BMMobileNo
                                },
                            }
                        };
                    case RoleNames.ASM:
                        return new Sms
                        {
                            UID = UniqueUID,
                            Sender = "",
                            Content = Content,
                            MessageType = "Transactional",
                            Priority = 1,
                            RequestStatus = "Pending",
                            RequestTime = DateTime.Now,
                            Receivers = new List<SmsModelReceiver>
                            {
                                new SmsModelReceiver
                                {
                                    Receiver = smsTemplateFields.ASMMobileNo
                                },
                            }
                        };
                    default:
                        return new Sms();
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<SMSApiResponse> SendOtp(ISms smsRequest)
        {
            try
            {
                var RequestBody = new SMSRequest
                {
                    ver = SmsRequestConstants.ApiVersion,
                    key = SmsKey,
                    messages = new List<SMSSubRequest>
                    {
                        new SMSSubRequest
                        {
                            dest = new List<string> { smsRequest.Receivers.FirstOrDefault().Receiver },
                            text = smsRequest.Content,
                            send = SmsRequestConstants.SenderID,
                            type = SmsRequestConstants.TypeOfMessage
                        }
                    }
                };
                var content = new StringContent(CommonFunctions.ConvertToJson(RequestBody), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync(SmsApiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<SMSApiResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    throw new Exception($"HTTP Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {

                throw new Exception($"Exception: {ex}"); 
            }
        }
    }
}
