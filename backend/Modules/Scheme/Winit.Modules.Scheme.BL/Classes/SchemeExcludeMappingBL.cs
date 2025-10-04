using iTextSharp.text;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class SchemeExcludeMappingBL : ISchemeExcludeMappingBL
    {
        ISchemeExcludeMappingDL _schemeExcludeMappingDL;
        public SchemeExcludeMappingBL(ISchemeExcludeMappingDL schemeExcludeMappingDL)
        {
            _schemeExcludeMappingDL = schemeExcludeMappingDL;
        }
        public async Task<List<SchemeExcludeMapping>> BulkImport(List<SchemeExcludeMapping> schemeExcludeMappings)
        {
            try
            {
                DateTime now = DateTime.Now;
                List<SchemeKey> schemeKeys = schemeExcludeMappings
                    .Select(d => new SchemeKey { SchemeType = d.SchemeType, SchemeUID = d.SchemeUID, StoreUID = d.StoreUID })
                    .Distinct()
                    .ToList();

                List<ISchemeExcludeMapping> existingRecords = await _schemeExcludeMappingDL.GetSchemeExcludeMappings(schemeKeys);

                List<ISchemeExcludeMapping> toInsert = new List<ISchemeExcludeMapping>();
                List<ISchemeExcludeMapping> toUpdate = new List<ISchemeExcludeMapping>();

                foreach (var newRecord in schemeExcludeMappings)
                {
                    // Adjust StartDate if it's today
                    if (newRecord.StartDate.Date == now.Date)
                        newRecord.StartDate = DateTime.Now;

                    var overlappingRecord = existingRecords
                        .Where(d => d.SchemeType == newRecord.SchemeType &&
                                    d.SchemeUID == newRecord.SchemeUID &&
                                    d.StoreUID == newRecord.StoreUID)   //&& d.EndDate > newRecord.StartDate
                        .OrderByDescending(d => d.EndDate)
                        .FirstOrDefault();

                    if (overlappingRecord != null)
                    {
                        // Expire previous record
                        //overlappingRecord.EndDate = newRecord.StartDate.AddSeconds(-1);
                        //if (overlappingRecord.EndDate.Date == now.Date)

                        overlappingRecord.EndDate = DateTime.Now.AddSeconds(-1);
                        overlappingRecord.ExpiredOn = DateTime.Now;
                        overlappingRecord.IsActive = false;
                        overlappingRecord.ModifiedBy = newRecord.CreatedBy;
                        overlappingRecord.ModifiedTime = DateTime.Now;
                        toUpdate.Add(overlappingRecord);
                    }

                    newRecord.EndDate = newRecord.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    toInsert.Add(newRecord);
                }

                // Perform batch updates
                if (toUpdate.Any())
                {
                    await _schemeExcludeMappingDL.UpdateSchemeExcludeMapping(toUpdate);
                }

                // Perform batch inserts
                if (toInsert.Any())
                {
                    await _schemeExcludeMappingDL.InsertSchemeExcludeMapping(toInsert);
                }
                return schemeExcludeMappings;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<PagedResponse<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping>> SelectAllSchemeExcludeMapping(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _schemeExcludeMappingDL.SelectAllSchemeExcludeMapping(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<PagedResponse<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping>> SelectAllSchemeExcludeMappingHistory(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _schemeExcludeMappingDL.SelectAllSchemeExcludeMappingHistory(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<int> CheckSchemeExcludeMappingExists(string storeUID, DateTime currentDate)
        {
            return await _schemeExcludeMappingDL.CheckSchemeExcludeMappingExists(storeUID, currentDate);
        }
        public async Task<string?> CheckIfUIDExistsInDB(string TableName, string UID)
        {
            return await _schemeExcludeMappingDL.CheckUIDExistsInDB(TableName, UID);
        }
    }
}
