using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.RSSMailQueue.DL.Interfaces;
using Winit.Modules.RSSMailQueue.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.RSSMailQueue.DL.Classes;

public class PGSQLRSSMailQueueDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IRSSMailQueueDL
{
    public PGSQLRSSMailQueueDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {
    }

    public async Task<int> CreateRSSMailQueue(Winit.Modules.RSSMailQueue.Model.Interfaces.IRSSMailQueue rSSMailQueue)
    {
        try
        {
            var sql = @"INSERT INTO rss_mail_queue (uid, created_time, modified_time, server_add_time,
                           server_modified_time, linked_item_type, linked_item_uid, mail_status, type, subject, body,
                           from_mail, cc_mail, to_mail, has_attachment, attachment_format_code,format_code  , 
                           error_message) VALUES (@uid, @CreatedTime, @ModifiedTime, @ServerAddTime,
                           @ServerModifiedTime, @LinkedItemType, @LinkedItemuid, @MailStatus, @Type, @Subject, @Body, @FromMail,
                           @CCMail, @ToMail, @HasAttachment, @AttachmentFormatCode, @FormatCode, @ErrorMessage);";
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@uid", rSSMailQueue.UID },
                    { "@CreatedBy", rSSMailQueue.CreatedBy },
                    { "@CreatedTime", rSSMailQueue.CreatedTime },
                    { "@ModifiedBy", rSSMailQueue.ModifiedBy },
                    { "@ModifiedTime", rSSMailQueue.ModifiedTime },
                    { "@ServerAddTime", rSSMailQueue.ServerAddTime },
                    { "@ServerModifiedTime", rSSMailQueue.ServerModifiedTime },
                    { "@LinkedItemType", rSSMailQueue.LinkedItemType },
                    { "@LinkedItemuid", rSSMailQueue.LinkedItemUID },
                    { "@MailStatus", rSSMailQueue.MailStatus },
                    { "@Type", rSSMailQueue.Type },
                    { "@Subject", rSSMailQueue.Subject },
                    { "@Body", rSSMailQueue.Body },
                    { "@FromMail", rSSMailQueue.FromMail },
                    { "@CCMail", rSSMailQueue.CCMail },
                    { "@ToMail", rSSMailQueue.ToMail },
                    { "@HasAttachment", rSSMailQueue.HasAttachment },
                    { "@AttachmentFormatCode", rSSMailQueue.AttachmentFormatCode },
                    { "@FormatCode", rSSMailQueue.FormatCode },
                    { "@ErrorMessage", rSSMailQueue.ErrorMessage },
                };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public Task<int> DeleteRSSMailQueue(string uid)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResponse<IRSSMailQueue>> SelectAllRSSMailQueueDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        throw new NotImplementedException();
    }

    public Task<IRSSMailQueue> SelectRSSMailQueueByUID(string uid)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateRSSMailQueue(IRSSMailQueue rSSMailQueueDetails)
    {
        throw new NotImplementedException();
    }
}

