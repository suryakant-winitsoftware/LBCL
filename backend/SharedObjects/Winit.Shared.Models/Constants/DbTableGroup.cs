using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Constants
{
    public class DbTableGroup
    {
        public const string Master = "master";
        public const string Sales = "sales";
        public const string Merchandiser = "merchandiser";
        public const string Return = "return";
        public const string StockRequest = "stock_request";
        public const string Collection = "collection";
        public const string CollectionDeposit = "collection_deposit";
        public const string StoreCheck = "store_check";
        public const string SurveyResponse = "survey_response";
        public const string FileSys = "file_sys";
        public const string Address = "address";
        /// <summary>
        /// When uploading to server replace value to _prod
        /// When committing in svn replae value to _dev
        /// When working in local change to your name
        /// </summary>
        public const string Suffix = "_ramana";
    }
}
