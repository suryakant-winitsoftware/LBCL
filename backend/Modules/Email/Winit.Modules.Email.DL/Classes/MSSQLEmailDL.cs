using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SQLitePCL;
using System.Web;
using Winit.Modules.Common.Model.Constants.Notification;
using Winit.Modules.Email.DL.Interfaces;
using Winit.Modules.Email.DL.UtilityClasses;
using Winit.Modules.Email.Model.Classes;
using Winit.Modules.Email.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.Email.DL.Classes
{
    public class MSSQLEmailDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IEmailDL
    {
        private readonly EmailUtility _emailUtility;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private string? FromEmailConfig { get; set; }
        private string? CMIDMSPortalLink { get; set; }
        private string? CMIDMSPortalLink1 { get; set; }
        public MSSQLEmailDL(EmailUtility emailUtility, IServiceProvider serviceProvider, IConfiguration configuration, HttpClient httpClient) : base(serviceProvider, configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _emailUtility = emailUtility;
            FromEmailConfig = _configuration.GetSection("MailSettings:FromEmail").Value;
            CMIDMSPortalLink1 = _configuration.GetSection("MailSettings:CMIDMSPortalLink").Value;
            CMIDMSPortalLink = "http://cmisarathitest.carriermidea.in/winitapp/";
        }
        public async Task<int> CreateEmailRequest(IMailRequest mailRequest)
        {
            try
            {
                var sql =
                    @"INSERT INTO mail_request (uid, priority, message_type, subject, content, linked_item_type, linked_item_uid, 
                          has_attachment, attachment_format, mail_format, request_status, sent_time, 
                          error_details, retry_count, batch_id, from_email) 
                        VALUES (@UID, @Priority, @MessageType, @Subject, @Content, @LinkedItemType, @LinkedItemUID, 
                        @HasAttachment, @AttachmentFormat, @MailFormat,'Pending', @SentTime, 
                        @ErrorDetails, @RetryCount, @BatchId, @FromEmail);";
                int Result = await ExecuteNonQueryAsync(sql, mailRequest);
                await CreateEmailRequestReceivers(mailRequest);
                return Result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> CheckWithUIDInMailRequest(IMailRequest mailRequest)
        {
            try
            {
                var sql =
                    @"INSERT INTO mail_request (uid, priority, message_type, subject, content, linked_item_type, linked_item_uid, 
                          has_attachment, attachment_format, mail_format, request_status, sent_time, 
                          error_details, retry_count, batch_id, from_email) 
                        VALUES (@UID, @Priority, @MessageType, @Subject, @Content, @LinkedItemType, @LinkedItemUID, 
                        @HasAttachment, @AttachmentFormat, @MailFormat, @RequestStatus, @SentTime, 
                        @ErrorDetails, @RetryCount, @BatchId, @FromEmail);";
                int Result = await ExecuteNonQueryAsync(sql, mailRequest);
                await CreateEmailRequestReceivers(mailRequest);
                return Result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> CheckExistsOrNot(IMailRequest mailRequest)
        {
            int count = -1;
            try
            {
                string? existingUID = await CheckIfUIDExistsInDB(DbTableName.MailRequest, mailRequest.UID);
                if (existingUID != null)
                {
                    count = await UpdateSuccessEmailRequest(mailRequest);
                }
                else
                {
                    count = await CreateEmailRequest(mailRequest);
                }

                return count;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> CreateEmailRequestReceivers(IMailRequest model)
        {
            try
            {
                var requestBodies = new EmailModelReceiver
                {
                    UID = Guid.NewGuid().ToString(),
                    EmailRequestUID = model.UID,
                    ToEmail = model.Receivers?.FirstOrDefault()?.ToEmail?.ToString() ?? "", // Assign corresponding ToEmail
                    CcEmail = model.Receivers?.FirstOrDefault()?.BccEmail?.ToString() ?? "", // Optional: Use null-safe navigation for nullable collections
                    BccEmail = model.Receivers?.FirstOrDefault()?.CcEmail?.ToString() ?? "" // Optional: Use null-safe navigation
                };

                var sql = @"INSERT INTO mail_request_receivers (uid, mail_request_uid, to_email, cc_email, bcc_email) 
                            VALUES (@UID, @EmailRequestUID, @ToEmail, @CcEmail, @BccEmail);";
                return await ExecuteNonQueryAsync(sql, requestBodies);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> UpdateSuccessEmailRequest(IMailRequest model)
        {
            try
            {
                var sql = @"UPDATE mail_request 
                            SET request_status = 'Success' WHERE uid = @UID";
                return await ExecuteNonQueryAsync(sql, model);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> UpdateFailureEmailRequest(IMailRequest model)
        {
            try
            {
                var sql = @"UPDATE mail_request 
                            SET request_status = 'Failed', error_details = @ErrorDetails, 
                            retry_count = @RetryCount WHERE uid = @UID";
                return await ExecuteNonQueryAsync(sql, model);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<List<IEmailRequestModel>> GetEmailRequestFromJob()
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                };
                var sql = @"select mr.uid as UID,
                            mr.content as Content,  
                            mr.retry_count as RetryCount,
                            mr.mail_format as MailFormat,
                            mr.from_email as FromMail,
                            mr.subject as Subject,
							mrr.mail_request_uid as MailRequestUID,
                            STRING_AGG(mrr.to_email, ',') as ToMail,
                            STRING_AGG(mrr.cc_email, ',') as CcMail,
                            STRING_AGG(mrr.bcc_email, ',') as BccMail
                            from mail_request mr
                            inner join mail_request_receivers mrr on mr.uid = mrr.mail_request_uid
                            where mr.request_status in ('Pending', 'Failed') and mr.retry_count < 3
                            group by mr.uid, mr.content, mr.retry_count, mr.mail_format, mr.from_email, mr.subject, mrr.mail_request_uid;";
                List<EmailRequestDTO> smsRequestDTOs = await ExecuteQueryAsync<EmailRequestDTO>(sql, parameters);
                return smsRequestDTOs.Select(r => new EmailRequestModel
                {
                    UID = r.UID,
                    MailRequestUID = r.MailRequestUID,
                    Content = r.Content,
                    RetryCount = r.RetryCount,
                    Subject = r.Subject,
                    MailFormat = r.MailFormat,
                    FromMail = r.FromMail,
                    ToMail = r.ToMail?.Split(',').ToList() ?? new List<string>(),
                    CcMail = r.CcMail?.Split(',').ToList() ?? new List<string>(),
                    BccMail = r.BccMail?.Split(',').ToList() ?? new List<string>(),
                }).ToList<IEmailRequestModel>();
            }
            catch (Exception ex)
            {
                return new List<IEmailRequestModel>();
            }
        }
        public async Task<bool> SendEmail(IMailRequest MailDetails)
        {
            try
            {
                bool res = _emailUtility.SendDefaultMail(MailDetails);
                return res;
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
                    //AND poh.status = 'inprocesserp'  commented for now later will add in query
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
        public async Task<IMailRequest> GetMailRequestForPO(string notificationTemplateNames, INotificationPOData smsFields, string UniqueUID)
        {
            try
            {
                string SmsContent = string.Empty;
                switch (notificationTemplateNames)
                {
                    case NotificationTemplateNames.INVOICE_RECEIVED_FROM_ORACLE_SEND_FOR_INFO:
                        SmsContent = $"""
                                       <p>Dear Sir/Madam,</p>
                                       <p>Invoice generated as per below details<br />
                                       Channel Partner {smsFields.ChannelPartnerName}.<br />
                                       Order No {smsFields.OrderNo}<br /> Invoice No {smsFields.InvoiceNumber}<br /> Order Qty  {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)}<br />
                                       Invoice Qty {smsFields.InvoiceQty}<br /> Invoice Amount  {smsFields.InvoiceValue}<br /></p>
                                       <p>Please <a href='{CMIDMSPortalLink}' target='_blank'>Login</a> to CMI-Sarathi for more information. {smsFields.Url}</p>
                                     """;
                        return await PopulateMailModal(smsFields, RoleNames.ChannelPartner, SmsContent, notificationTemplateNames, UniqueUID);
                    case NotificationTemplateNames.PO_APPROVED_BY_LAST_LEVEL_SEND_TO_CP_FOR_INFO:
                        SmsContent = $"""
                                        <p>Dear Channel Partner, {smsFields.ChannelPartnerName}.</p>
                                        <p>The below order is accepted.</p>
                                        <p>Order No {smsFields.OrderNo}<br /> Requested Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)}<br /> Ordered Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)}<br />
                                        Order Value  {CommonFunctions.FormatNumberInIndianStyle(Convert.ToDecimal(smsFields.OrderValue))}<br /></p>
                                        <p>Please <a href='{CMIDMSPortalLink}' target='_blank'>Login</a> to CMI-Sarathi for more information. {smsFields.Url}</p>
                                     """;
                        return await PopulateMailModal(smsFields, RoleNames.ChannelPartner, SmsContent, notificationTemplateNames, UniqueUID);
                    case NotificationTemplateNames.PO_CREATED_BY_CP_SEND_TO_BM_FOR_INFO:
                        SmsContent = $"""
                                        <p>Dear Sir/Madam,</p> <p>Below order is Created in CMI-Sarathi</p>
                                        <p>Channel Partner {smsFields.ChannelPartnerName}.<br />
                                        Order No {smsFields.OrderNo}<br /> Created by  {smsFields.CreatedBy}<br /> Order Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)}<br />
                                        Order Value   {CommonFunctions.FormatNumberInIndianStyle(Convert.ToDecimal(smsFields.OrderValue))}</p>
                                        <p>Please <a href='{CMIDMSPortalLink}' target='_blank'>Login</a> for more information.</p>
                                     """;
                        return await PopulateMailModal(smsFields, RoleNames.BranchManager, SmsContent, notificationTemplateNames, UniqueUID);
                    case NotificationTemplateNames.PO_CREATED_BY_CP_SEND_TO_ASM_FOR_APPROVAL:
                        SmsContent = $"""
                                        <p>Dear Sir/Madam,</p> <p>Below order is Created in CMI-Sarathi</p>
                                        <p>Channel Partner {smsFields.ChannelPartnerName}.<br />
                                        Order No {smsFields.OrderNo}<br /> Created by  {smsFields.CreatedBy}<br /> Order Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)}<br />
                                        Order Value {CommonFunctions.FormatNumberInIndianStyle(Convert.ToDecimal(smsFields.OrderValue))}<br /></p>
                                        <p>Please <a href='{CMIDMSPortalLink}' target='_blank'>Login</a> for Approval.</p>
                                     """;
                        return await PopulateMailModal(smsFields, RoleNames.ASM, SmsContent, notificationTemplateNames, UniqueUID);
                    case NotificationTemplateNames.PO_APPROVED_BY_BM_SEND_TO_ASM_FOR_INFO:
                        SmsContent = $"""
                                        <p>Dear Sir,</p> </p>Below order is approved by {smsFields.ApprovedBy}</p>
                                        <p>Channel Partner Name {smsFields.ChannelPartnerName}.<br /> Order No {smsFields.OrderNo}<br /> Order Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)}<br /> Approved Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.ApprovedQty)}<br /> Order Value {CommonFunctions.FormatNumberInIndianStyle(Convert.ToDecimal(smsFields.OrderValue))}</p>
                                        <p>Please <a href='{CMIDMSPortalLink}' target='_blank'>Login</a> for more information.</p>
                                      """;
                        return await PopulateMailModal(smsFields, RoleNames.ASM, SmsContent, notificationTemplateNames, UniqueUID);
                    case NotificationTemplateNames.PO_APPROVED_BY_CP_SEND_TO_BM_FOR_APPROVAL:
                        SmsContent = $"""
                                        <p>Dear Sir/Madam,</p> <p>Below order confirmed by Channel Partner</p>
                                        <p>Order No {smsFields.OrderNo}<br /> Created by {smsFields.CreatedBy}<br /> Order Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)}<br /> Order Value {CommonFunctions.FormatNumberInIndianStyle(Convert.ToDecimal(smsFields.OrderValue))}<br />
                                        Channel Partner {smsFields.ChannelPartnerName}.</p>
                                        <p>Please <a href='{CMIDMSPortalLink}' target='_blank'>Login</a> for Approval.</p>
                                      """;
                        return await PopulateMailModal(smsFields, RoleNames.BranchManager, SmsContent, notificationTemplateNames, UniqueUID);
                    case NotificationTemplateNames.PO_CREATED_BY_ASM_SEND_TO_BM_FOR_INFO:
                        SmsContent = $"""
                                        <p>Dear Sir/Madam,</p> <p>Below order is Created in  CMI-Sarathi</p>
                                        <p>Order No {smsFields.OrderNo}<br /> Created by {smsFields.CreatedBy}<br /> Order Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)}<br /> Order Value {CommonFunctions.FormatNumberInIndianStyle(Convert.ToDecimal(smsFields.OrderValue))}<br />
                                        Channel Partner Name {smsFields.ChannelPartnerName}.</p>
                                        <p>Please <a href='{HttpUtility.HtmlEncode(CMIDMSPortalLink)}' target='_blank'>Login</a> for more information.</p>
                                      """;
                        return await PopulateMailModal(smsFields, RoleNames.BranchManager, SmsContent, notificationTemplateNames, UniqueUID);
                    case NotificationTemplateNames.PO_CREATED_BY_ASM_SEND_TO_CP_FOR_APPROVAL:
                        SmsContent = $"""
                                        <p>Dear Sir/Madam,</p> <p>Below order is Created in CMI-Sarathi</p>
                                        <p>Order No {smsFields.OrderNo}<br /> Created by {smsFields.CreatedBy}<br /> Order Qty {CommonFunctions.GetStringInNumberFormatExcludingZero(smsFields.OrderQty)}<br /> Order Value {CommonFunctions.FormatNumberInIndianStyle(Convert.ToDecimal(smsFields.OrderValue))}<br /></p>
                                        <p>Please <a href='{CMIDMSPortalLink}' target='_blank'>Login</a> and confirm the order.</p>
                                      """;
                        return await PopulateMailModal(smsFields, RoleNames.ChannelPartner, SmsContent, notificationTemplateNames, UniqueUID);
                    default:
                        return new MailRequest();
                }
            }
            catch (Exception ex)
            {
                return new MailRequest();
            }
        }
        public async Task<MailRequest> PopulateMailModal(INotificationPOData smsTemplateFields, string Receiver, string Content, string notificationTemplateNames, string UniqueUID)
        {
            try
            {
                switch (Receiver)
                {
                    case RoleNames.ChannelPartner:
                        return new MailRequest
                        {
                            UID = UniqueUID,
                            FromEmail = FromEmailConfig ?? "",
                            Content = Content,
                            MailFormat = "General",
                            MessageType = "Transactional",
                            Priority = 1 ,
                            LinkedItemType = "PurchaseOrder" ,
                            LinkedItemUID = smsTemplateFields.OrderUID,
                            Subject = PopulateSubject(notificationTemplateNames, smsTemplateFields),
                            Receivers = new List<EmailModelReceiver>
                            {
                                new EmailModelReceiver
                                {
                                    ToEmail = smsTemplateFields.ChannelPartnerEmail
                                },
                            }
                        };
                    case RoleNames.BranchManager:
                        return new MailRequest
                        {
                            UID = UniqueUID,
                            FromEmail = FromEmailConfig ?? "",
                            Content = Content,
                            MailFormat = "General",
                            MessageType = "Transactional",
                            Priority = 1,
                            LinkedItemType = "PurchaseOrder",
                            LinkedItemUID = smsTemplateFields.OrderUID,
                            Subject = PopulateSubject(notificationTemplateNames, smsTemplateFields),
                            Receivers = new List<EmailModelReceiver>
                            {
                                new EmailModelReceiver
                                {
                                    ToEmail = smsTemplateFields.BMEmail
                                },
                            },
                        };
                    case RoleNames.ASM:
                        return new MailRequest
                        {
                            UID = UniqueUID,
                            FromEmail = FromEmailConfig ?? "",
                            Content = Content,
                            MailFormat = "General",
                            MessageType = "Transactional",
                            Priority = 1,
                            LinkedItemType = "PurchaseOrder",
                            LinkedItemUID = smsTemplateFields.OrderUID,
                            Subject = PopulateSubject(notificationTemplateNames, smsTemplateFields),
                            Receivers = new List<EmailModelReceiver>
                            {
                                new EmailModelReceiver
                                {
                                    ToEmail = smsTemplateFields.ASMEmail
                                },
                            },
                        };
                    default:
                        return new MailRequest();
                }

            }
            catch (Exception ex)
            {
                return new MailRequest();
            }
        }
        public string PopulateSubject(string notificationTemplateNames, INotificationPOData smsTemplateFields)
        {
            try
            {
                string Subject = string.Empty;
                switch (notificationTemplateNames)
                {
                    case NotificationTemplateNames.INVOICE_RECEIVED_FROM_ORACLE_SEND_FOR_INFO:
                        Subject = EmailSubjectsConstants.InvoiceSubject;
                        Subject = Subject.Replace("{INVOICE_NO}", smsTemplateFields.InvoiceNumber);
                        Subject = Subject.Replace("{ORDER_NO}", smsTemplateFields.OrderNo);
                        return Subject;
                    case NotificationTemplateNames.PO_APPROVED_BY_LAST_LEVEL_SEND_TO_CP_FOR_INFO:
                        Subject = EmailSubjectsConstants.CPLastLevelApprovalSubject;
                        Subject = Subject.Replace("{ORDER_NO}", smsTemplateFields.OrderNo);
                        return Subject;
                    case NotificationTemplateNames.PO_CREATED_BY_CP_SEND_TO_BM_FOR_INFO:
                        Subject = EmailSubjectsConstants.BMInfoSubject;
                        Subject = Subject.Replace("{CHANNEL_PARTNER}", smsTemplateFields.ChannelPartnerName);
                        Subject = Subject.Replace("{ORDER_NO}", smsTemplateFields.OrderNo);
                        return Subject;
                    case NotificationTemplateNames.PO_CREATED_BY_CP_SEND_TO_ASM_FOR_APPROVAL:
                        Subject = EmailSubjectsConstants.ASMConfirmationSubject;
                        Subject = Subject.Replace("{CHANNEL_PARTNER}", smsTemplateFields.ChannelPartnerName);
                        Subject = Subject.Replace("{ORDER_NO}", smsTemplateFields.OrderNo);
                        return Subject;
                    case NotificationTemplateNames.PO_APPROVED_BY_BM_SEND_TO_ASM_FOR_INFO:
                        Subject = EmailSubjectsConstants.ASMInfoSubject;
                        Subject = Subject.Replace("{APPROVED_BY}", smsTemplateFields.ApprovedBy);
                        Subject = Subject.Replace("{ORDER_NO}", smsTemplateFields.OrderNo);
                        return Subject;
                    case NotificationTemplateNames.PO_APPROVED_BY_CP_SEND_TO_BM_FOR_APPROVAL:
                        Subject = EmailSubjectsConstants.BMApprovalSubject;
                        Subject = Subject.Replace("{CHANNEL_PARTNER}", smsTemplateFields.ChannelPartnerName);
                        Subject = Subject.Replace("{ORDER_NO}", smsTemplateFields.OrderNo);
                        return Subject;
                    case NotificationTemplateNames.PO_CREATED_BY_ASM_SEND_TO_BM_FOR_INFO:
                        Subject = EmailSubjectsConstants.BMInfoSubject;
                        Subject = Subject.Replace("{CHANNEL_PARTNER}", smsTemplateFields.ChannelPartnerName);
                        Subject = Subject.Replace("{ORDER_NO}", smsTemplateFields.OrderNo);
                        return Subject;
                    case NotificationTemplateNames.PO_CREATED_BY_ASM_SEND_TO_CP_FOR_APPROVAL:
                        Subject = EmailSubjectsConstants.CPInfoSubject;
                        Subject = Subject.Replace("{ASM_NAME}", smsTemplateFields.ASMName);
                        Subject = Subject.Replace("{ORDER_NO}", smsTemplateFields.OrderNo);
                        return Subject;
                    default:
                        return Subject;
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}
