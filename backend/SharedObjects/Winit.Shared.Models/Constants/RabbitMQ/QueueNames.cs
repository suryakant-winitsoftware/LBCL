using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Constants.RabbitMQ
{
    public class QueueNames
    {
        public const string SalesOrderQueue = "sales_order_queue";
        public const string MerchandiserQueue = "merchandiser_queue";
        public const string CollectionQueue = "collection_queue";
        public const string SurveyQueue = "survey_queue";
        public const string MasterQueue = "master_queue";
        public const string FileSys = "file_sys_queue";
        public const string ReturnOrderQueue = "return_order_queue";
    }
}
