using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Text;
using Winit.Modules.ApprovalEngine.DL.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Int_CommonMethods.Model.Classes;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;
using Winit.Modules.User.Model.Constants;
using Winit.Modules.User.Model.Interface;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ApprovalEngine.DL.Classes
{
    public class MSSQLApprovalEngineDL : Base.DL.DBManager.SqlServerDBManager, IApprovalEngineDL
    {

        public MSSQLApprovalEngineDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        { }
        public async Task<IEnumerable<Model.Interfaces.IApprovalLog>> GetApprovalLog(string RequestId)
        {
            try
            {

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"RequestId",  RequestId}
                };

                var sql = @"SELECT
                                approverId AS ApproverId,
                                level AS Level,
                                status AS Status,
                                comments AS Comments,
                                modifiedBy AS ModifiedBy,
                                reassignTo AS ReassignTo,
                                createdOn AS CreatedOn
                            FROM ApprovalLogs where requestId=@RequestId";

                IEnumerable<Model.Interfaces.IApprovalLog> ApprovalLog = await ExecuteQueryAsync<Model.Interfaces.IApprovalLog>(sql, parameters);
                return ApprovalLog;
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<ISelectionItem>> GetRoleNames()
        {
            try
            {
                var sql = @"SELECT code AS Code, role_name_en AS Label FROM roles";
                List<ISelectionItem> roles = await ExecuteQueryAsync<ISelectionItem>(sql);
                return roles;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<IApprovalHierarchy>> GetApprovalHierarchy(string ruleId)
        {
            try
            {

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"RuleId",  ruleId}
                };

                var sql = @"SELECT
                                approverid AS ApproverId,
                                level AS Level,
                                nextapproverid AS NextApprover
                            FROM approvalhierarchy where ruleId=@RuleId";

                IEnumerable<IApprovalHierarchy> approvalHierarchy = await ExecuteQueryAsync<IApprovalHierarchy>(sql, parameters);
                return approvalHierarchy;
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<Winit.Modules.ApprovalEngine.Model.Interfaces.IApprovalRuleMap>> DropDownsForApprovalMapping()
        {
            try
            {
                var query = @"Select Name,description,ruleid from ruleparameters
                              Select Distinct UID From Org
                              Select Distinct ruleid as UID From APPROVALHIERARCHY";
                DataSet ds = await ExecuteQueryDataSetAsync(query);
                List<IApprovalRuleMap> ApprovalRuleMap = new List<IApprovalRuleMap>();
                //ApprovalRuleMap.Type = new List<ISelectionItem>();
                //ApprovalRuleMap.TypeCode = new List<ISelectionItem>();
                //ApprovalRuleMap.RuleId = new List<ISelectionItem>();

                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                ApprovalRuleMap approvalRuleMap = new ApprovalRuleMap
                                {
                                    Type = row["Name"].ToString(),
                                    TypeCode = row["description"].ToString(),
                                    RuleId = Convert.ToInt32(row["ruleid"])
                                };

                                ApprovalRuleMap.Add(approvalRuleMap);
                                // Extract values from the DataRow
                                //string name = row["Name"].ToString();          // Assuming the column is "Name"
                                //string description = row["Description"].ToString();  // Assuming the column is "Description"
                                //string ruleId = row["RuleId"].ToString();      // Assuming the column is "RuleId"

                                //// Create a new instance of ISelectionItem and populate it
                                //ISelectionItem typeItem = new SelectionItem
                                //{
                                //    UID = name,    // UID and Label will be set to 'Name'
                                //    Label = name,
                                //    Code = name,   // Code will also be set to 'Name'
                                //};

                                //ISelectionItem typeCodeItem = new SelectionItem
                                //{
                                //    UID = description,   // UID and Label will be set to 'Description'
                                //    Label = description,
                                //    Code = description,  // Code will also be set to 'Description'
                                //};

                                //ISelectionItem ruleIdItem = new SelectionItem
                                //{
                                //    UID = ruleId,   // UID and Label will be set to 'RuleId'
                                //    Label = ruleId,
                                //    Code = ruleId,  // Code will also be set to 'RuleId'
                                //};

                                //// Add the items to their respective lists
                                //ApprovalRuleMap.Type.Add(typeItem);
                                //ApprovalRuleMap.TypeCode.Add(typeCodeItem);
                                //ApprovalRuleMap.RuleId.Add(ruleIdItem);
                            }
                        }
                        // For OrgUids
                        if (ds.Tables[1].Rows.Count > 0)
                        {
                            //foreach (DataRow row in ds.Tables[0].Rows)
                            //{
                            //    // Assuming the UID is in the first column of the DataRow
                            //    var selectionItem = ConvertDataTableToObject<Winit.Shared.Models.Common.SelectionItem>(row);
                            //    selectionItem.Label = row["UID"].ToString();  // Bind the UID to the Label
                            //    ApprovalRuleMap.OrgUids.Add(selectionItem);
                            //}
                        }

                        // For RuleId
                        if (ds.Tables[2].Rows.Count > 0)
                        {
                            //foreach (DataRow row in ds.Tables[1].Rows)
                            //{
                            //    // Assuming the UID is in the "ruleid" column of the DataRow
                            //    var selectionItem = ConvertDataTableToObject<Winit.Shared.Models.Common.SelectionItem>(row);
                            //    selectionItem.Label = row["UID"].ToString();  // Bind the UID to the Label
                            //    ApprovalRuleMap.RuleId.Add(selectionItem);
                            //}
                        }
                    }
                }
                return ApprovalRuleMap;
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> IntegrateCreatedRule(IApprovalRuleMapping approRuleMap)
        {
            int retVal = -1;
            try
            {
                string query = @"Insert Into ApprovalRuleMapping(uid,rule_id,type,type_code,created_by,created_time,modified_time)
                         values (@UID,@RuleId,@Type,@TypeCode,@CreatedBy,@CreatedTime,@ModifiedTime)";
                retVal = await ExecuteNonQueryAsync(query, approRuleMap);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                throw new Exception("You can not integrate two rule with same type and type code.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while inserting the approval rule.", ex);
            }
            return retVal;
        }


        public async Task<int> GetRuleId(string type, string typeCode)
        {
            int ruleId;
            try
            {
                var parameters = new Dictionary<string, object>()
            {
                {"type",type },
                {"typeCode",typeCode },
            };
                string query = @"select rule_id from ApprovalRuleMapping where type=@type and type_code=@typeCode";
                ruleId = await ExecuteScalarAsync<int>(query, parameters);
            }

            catch (Exception)
            {
                throw;
            }
            return ruleId;
        }
        public async Task<IEnumerable<IViewChangeRequestApproval>> GetAllChangeRequest()
        {
            try
            {

                var sqlQuery = @"SELECT
                                uid AS UID,
                                emp_uid AS EmpUid,
                                linked_item_type AS LinkedItemType,
                                channel_partner_code AS ChannelPartnerCode,
                                channel_partner_name AS ChannelPartnerName,
                                linked_item_uid AS LinkedItemUid,
                                request_date AS RequestDate,
                                approved_date AS ApprovedDate,
                                status AS Status,
                                operation AS OperationType,
                                created_by AS RequestedBy,
                                reference AS Reference,
                                change_data AS ChangedRecord,
                                row_recognizer AS RowRecognizer
                            FROM 
                                change_requests WITH (NOLOCK)  -- Use NOLOCK to prevent locking on read queries (use carefully)
                            ORDER BY 
                                request_date DESC  
                            ";
                IEnumerable<IViewChangeRequestApproval> viewChangeRequestApproval = await ExecuteQueryAsync<IViewChangeRequestApproval>(sqlQuery);
                return viewChangeRequestApproval;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally { }
        }
        //public async Task<PagedResponse<IViewChangeRequestApproval>> GetChangeRequestData(PagingRequest pagingRequest)
        //{
        //    try
        //    {
        //        var sqlQuery = new StringBuilder(@"
        //  select * from(  SELECT
        //        uid AS UID,
        //        emp_uid AS EmpUid,
        //        linked_item_type AS LinkedItemType,
        //        channel_partner_code AS ChannelPartnerCode,
        //        channel_partner_name AS ChannelPartnerName,
        //        linked_item_uid AS LinkedItemUid,
        //        request_date AS RequestDate,
        //        approved_date AS ApprovedDate,
        //        status AS Status,
        //        operation AS OperationType,
        //        created_by AS RequestedBy,
        //        reference AS Reference,
        //        change_data AS ChangedRecord,
        //        row_recognizer AS RowRecognizer
        //    FROM 
        //        change_requests WITH (NOLOCK)) as subquery
        //");

        //        var sqlCount = new StringBuilder();
        //        if (pagingRequest.IsCountRequired)
        //        {
        //            sqlCount.Append("SELECT COUNT(1) AS TotalCount FROM change_requests WITH (NOLOCK)");
        //        }

        //        var parameters = new Dictionary<string, object>();

        //        // Apply filtering
        //        if (pagingRequest.FilterCriterias != null && pagingRequest.FilterCriterias.Count > 0)
        //        {
        //            StringBuilder filterBuilder = new StringBuilder(" WHERE ");
        //            AppendFilterCriteria<IViewChangeRequestApproval>(pagingRequest.FilterCriterias, filterBuilder, parameters);
        //            sqlQuery.Append(filterBuilder);
        //            if (pagingRequest.IsCountRequired)
        //            {
        //                sqlCount.Append(filterBuilder);
        //            }
        //        }

        //        // Apply sorting
        //        if (pagingRequest.SortCriterias != null && pagingRequest.SortCriterias.Count > 0)
        //        {
        //            sqlQuery.Append(" ORDER BY ");
        //            AppendSortCriteria(pagingRequest.SortCriterias, sqlQuery);
        //        }
        //        else
        //        {
        //            sqlQuery.Append(" ORDER BY RequestDate DESC");
        //        }

        //        // Apply pagination
        //        if (pagingRequest.PageNumber > 0 && pagingRequest.PageSize > 0)
        //        {
        //            sqlQuery.Append($" OFFSET {(pagingRequest.PageNumber - 1) * pagingRequest.PageSize} ROWS FETCH NEXT {pagingRequest.PageSize} ROWS ONLY");
        //        }

        //        // Execute the query
        //        IEnumerable<IViewChangeRequestApproval> data = await ExecuteQueryAsync<IViewChangeRequestApproval>(sqlQuery.ToString(), parameters);

        //        // Execute the count query if required
        //        int totalCount = 0;
        //        if (pagingRequest.IsCountRequired)
        //        {
        //            totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
        //        }

        //        // Construct and return the paged response
        //        var pagedResponse = new PagedResponse<IViewChangeRequestApproval>
        //        {
        //            PagedData = data,
        //            TotalCount = totalCount,
        //        };

        //        return pagedResponse;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}


        public async Task<PagedResponse<IViewChangeRequestApproval>> GetChangeRequestData(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                StringBuilder sql = new(@"select * from(SELECT
                uid AS UID,
                emp_uid AS EmpUid,
                linked_item_type AS LinkedItemType,
                channel_partner_code AS ChannelPartnerCode,
                channel_partner_name AS ChannelPartnerName,
                linked_item_uid AS LinkedItemUid,
                request_date AS RequestDate,
                approved_date AS ApprovedDate,
                status AS Status,
                operation AS OperationType,
                created_by AS RequestedBy,
                reference AS Reference,
                change_data AS ChangedRecord,
                row_recognizer AS RowRecognizer
            FROM 
                change_requests )as sub_query");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT
                uid AS UID,
                emp_uid AS EmpUid,
                linked_item_type AS LinkedItemType,
                channel_partner_code AS ChannelPartnerCode,
                channel_partner_name AS ChannelPartnerName,
                linked_item_uid AS LinkedItemUid,
                request_date AS RequestDate,
                approved_date AS ApprovedDate,
                status AS Status,
                operation AS OperationType,
                created_by AS RequestedBy,
                reference AS Reference,
                change_data AS ChangedRecord,
                row_recognizer AS RowRecognizer
            FROM 
                change_requests )as sub_query");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<IViewChangeRequestApproval>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($" ORDER BY RequestDate Desc OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }

                }
                IEnumerable<IViewChangeRequestApproval> approvalDetails = await ExecuteQueryAsync<IViewChangeRequestApproval>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<IViewChangeRequestApproval> pagedResponse = new PagedResponse<IViewChangeRequestApproval>
                {
                    PagedData = approvalDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<Winit.Modules.ApprovalEngine.Model.Interfaces.IAllApprovalRequest?> GetApprovalDetailsByLinkedItemUid(string linkItemUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
             {
                   { "LinkItemUID", linkItemUID }
             };
            var sqlQuery = @"
                            SELECT
                                linkedItemType AS LinkedItemType,
                                linkedItemUID AS UID,
                                requestID AS RequestID,
                                ApprovalUserDetail AS ApprovalUserDetail
                            FROM 
                                AllApprovalRequest
                            WHERE 
                                linkedItemUID = @LinkItemUID";
            Winit.Modules.ApprovalEngine.Model.Interfaces.IAllApprovalRequest? approvalLevel = (await ExecuteQueryAsync<Winit.Modules.ApprovalEngine.Model.Interfaces.IAllApprovalRequest>(sqlQuery, parameters)).FirstOrDefault();
            return approvalLevel;
        }
        public async Task<IViewChangeRequestApproval> GetChangeRequestDataByUid(string requestUid)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                 {
                       { "UID", requestUid }
                 };

                var sqlQuery = @"SELECT
                                uid AS UID,
                                emp_uid AS EmpUid,
                                linked_item_type AS LinkedItemType,
                                linked_item_uid AS LinkedItemUid,
                                request_date AS RequestDate,
                                approved_date AS ApprovedDate,
                                status AS Status,
                                change_data AS ChangedRecord,
                                row_recognizer AS RowRecognizer,
                                created_by AS RequestedBy,
                                operation AS OperationType,
                                reference AS Reference

                            FROM 
                                change_requests WITH (NOLOCK)  -- Use NOLOCK to prevent locking on read queries (use carefully)
                            where uid = @UID
                            ORDER BY 
                                request_date DESC  
                            ";
                IViewChangeRequestApproval? viewChangeRequestApproval = (await ExecuteQueryAsync<IViewChangeRequestApproval>(sqlQuery, parameters)).FirstOrDefault();
                return viewChangeRequestApproval!;
            }
            catch (Exception ex) { }
            return default;

        }

        #region Request Change Logic
        public async Task<int> UpdateChangesInMainTable(IViewChangeRequestApproval? viewChangeRequestApproval)
        {
            try
            {
                int retVal = -1;
                // Deserialize the ChangeRecordDTOs from the ChangedRecord property
                List<ChangeRecordDTO> ChangeRecordDTOs = JsonConvert.DeserializeObject<List<ChangeRecordDTO>>(viewChangeRequestApproval.ChangedRecord)!;

                foreach (ChangeRecordDTO changeRecordDTO in ChangeRecordDTOs)
                {
                    string tableName = GetTableName(changeRecordDTO.ScreenModelName);

                    // Switch based on the Action property of changeRecordDTO
                    switch (changeRecordDTO.Action.ToLower())
                    {
                        case "update":
                            retVal = await UpdateRecord(changeRecordDTO, tableName);
                            break;

                        case "delete":
                            retVal = await DeleteRecord(changeRecordDTO.UID, tableName);
                            break;

                        case "create":
                            retVal = await CreateRecord(changeRecordDTO, tableName, viewChangeRequestApproval.EmpUid);
                            break;

                        default:
                            throw new ArgumentException($"Unsupported action: {changeRecordDTO.Action}");
                    }
                }

                // If changes were made, update the status of the ChangeRequest table
                if (retVal > 0)
                {
                    retVal = await ChangeStatusOfChangeRequestTable(viewChangeRequestApproval);
                }

                return retVal;
            }
            catch (Exception ex) { throw new ApplicationException($"Error while inserting the record: {ex.Message}"); }
        }
        private async Task<int> UpdateRecord(ChangeRecordDTO changeRecordDTO, string tableName)
        {
            try
            {
                int retVal = -1;
                var currentTime = DateTime.Now; // Get the current time

                foreach (var item in changeRecordDTO.ChangeRecords)
                {
                    // Add the query to update the field and the timestamps
                    var updateQuery = $@"UPDATE {tableName}
                             SET {item.FieldName} = @FieldValue, 
                                 modified_time = @ModifiedTime,
                                 server_modified_time = @ServerModifiedTime
                             WHERE uid = @UID";

                    // Define the parameters, including the new time values
                    var updateParameters = new Dictionary<string, object>
                {
                    { "@FieldValue", item.NewValue },
                    { "@UID", changeRecordDTO.UID },
                    { "@ModifiedTime", currentTime },
                    { "@ServerModifiedTime", currentTime }
                };

                    // Execute the query and store the result
                    retVal = await ExecuteNonQueryAsync(updateQuery, updateParameters);
                    if (retVal > 0 && tableName == DbTableName.Store)
                    {
                        if (item.FieldName == "classification_type")
                        {
                            await IntegrateAddressInOracle(changeRecordDTO, tableName, "Store");
                        }
                    }

                }
                if (retVal > 0 && tableName == DbTableName.Address)
                {
                    await IntegrateAddressInOracle(changeRecordDTO, tableName, "CustomerAddress");
                }


                return retVal;
            }
            catch (Exception ex) { throw new ApplicationException($"Error while inserting the record: {ex.Message}"); }

        }

        private async Task<int> DeleteRecord(string uid, string tableName)
        {
            var deleteQuery = $@"DELETE FROM {tableName} WHERE uid = @UID";
            var deleteParameters = new Dictionary<string, object>()
            {
                { "@UID", uid }
            };

            return await ExecuteNonQueryAsync(deleteQuery, deleteParameters);
        }
        private async Task<int> CreateRecord(ChangeRecordDTO changeRecordDTO, string tableName, string createdBy)
        {
            try
            {
                // Get the current time for all time-related fields
                var currentTime = DateTime.Now;

                // Initialize field names, values, and parameters with UID and timestamps
                var fieldNames = new List<string> { "uid", "created_by", "modified_by", "created_time", "modified_time", "server_add_time", "server_modified_time" };
                var fieldValues = new List<string> { "@UID", "@CreatedBy", "@ModifiedBy", "@CreatedTime", "@ModifiedTime", "@ServerAddTime", "@ServerModifiedTime" };
                var insertParameters = new Dictionary<string, object>
                    {
                        { "@UID", changeRecordDTO.UID },
                        { "@CreatedBy", createdBy },
                        { "@ModifiedBy", createdBy },
                        { "@CreatedTime", currentTime },
                        { "@ModifiedTime", currentTime },
                        { "@ServerAddTime", currentTime },
                        { "@ServerModifiedTime", currentTime }
                    };

                // Add LinkedItemUID if present
                if (!string.IsNullOrEmpty(changeRecordDTO.LinkedItemUID))
                {
                    fieldNames.Add("linked_item_type");
                    fieldNames.Add("linked_item_uid");
                    fieldValues.Add("@LinkedItemType");
                    fieldValues.Add("@LinkedItemUID");
                    insertParameters.Add("@LinkedItemUID", changeRecordDTO.LinkedItemUID);
                    insertParameters.Add("@LinkedItemType", "store");
                }

                // Collect all field names and values from ChangeRecords for insertion
                foreach (var item in changeRecordDTO.ChangeRecords)
                {
                    fieldNames.Add(item.FieldName);
                    fieldValues.Add($"@{item.FieldName}");
                    insertParameters.Add($"@{item.FieldName}", item.NewValue ?? DBNull.Value);
                }

                // Build and execute the insert query
                var insertQuery = $@"INSERT INTO {tableName} ({string.Join(",", fieldNames)}) 
                             VALUES ({string.Join(",", fieldValues)})";

                return await ExecuteNonQueryAsync(insertQuery, insertParameters);
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                throw new ApplicationException("Error while inserting the record", ex);
            }
        }


        private async Task<int> ChangeStatusOfChangeRequestTable(IViewChangeRequestApproval? viewChangeRequestApproval)
        {

            // Get the current time for the timestamps
            var currentTime = DateTime.Now;
            string uid = viewChangeRequestApproval?.UID!;
            // Define the update query to change the status and update the timestamps
            var statusQuery = $@"UPDATE change_requests 
                         SET Status = @NewStatus,
                             modified_time = @ModifiedTime,
                             server_modified_time = @ServerModifiedTime,
                             approved_date = @ApprovedDate
                         WHERE uid = @UID";

            // Define the parameters, including the new status and timestamps
            var statusParameters = new Dictionary<string, object>
                {
                    { "@NewStatus", viewChangeRequestApproval?.Status! },  // Example: Set status to "Approved"
                    { "@ModifiedTime", currentTime },
                    { "@ServerModifiedTime", currentTime },
                    { "@ApprovedDate", currentTime },
                    { "@UID", uid }
                };

            // Execute the query and return the result
            return await ExecuteNonQueryAsync(statusQuery, statusParameters);
        }
        private async Task<int> IntegrateAddressInOracle(ChangeRecordDTO changeRecordDTO, string tableName, string linkedItemType)
        {
            try
            {

                IPendingDataRequest pendingRequestData = new PendingDataRequest
                {
                    LinkedItemUid = changeRecordDTO.UID,
                    LinkedItemType = linkedItemType,
                    Status = "Pending"
                };
                int retVal = await InsertPendingData(pendingRequestData);
                return retVal;
            }
            catch
            {
                throw new Exception("Integration Failed");
            }
        }
        public async Task<int> InsertPendingData(IPendingDataRequest pendingData)
        {
            try
            {
                var syncLogDetailUpdate = new StringBuilder($@"insert into int_pushed_data_status (uid,linked_item_uid,status,linked_item_type)
                        values (newid(),@LinkedItemUid,@Status,@LinkedItemType)");
                int syncLogDetailCount = await ExecuteNonQueryAsync(syncLogDetailUpdate.ToString(), pendingData);
                return syncLogDetailCount;
            }
            catch
            {
                throw;
            }
        }
        public string GetTableName(string sectionName)
        {
            switch (sectionName)
            {
                case OnboardingScreenConstant.CustomInfoStore:
                    return DbTableName.Store;

                case OnboardingScreenConstant.Contact:
                    return DbTableName.Contact;

                case OnboardingScreenConstant.CustomInfoStoreAdditionInfo:
                    return DbTableName.StoreAdditionalInfo;

                case OnboardingScreenConstant.EmployeeDetails:
                case OnboardingScreenConstant.ServiceCenterDetail:
                case OnboardingScreenConstant.ShowroomDetails:
                case OnboardingScreenConstant.BankersDetails:
                case OnboardingScreenConstant.BusinessDetails:
                case OnboardingScreenConstant.DistBusinessDetails:
                case OnboardingScreenConstant.EarlierWorkWithCMI:
                case OnboardingScreenConstant.AreaOfDistAgreed:
                case OnboardingScreenConstant.AreaofOperationAgreed:
                case OnboardingScreenConstant.CustomInfoStoreAdditionInfoCmi:
                    return DbTableName.StoreAdditionalInfoCMI;

                case OnboardingScreenConstant.BilltoAddress:
                case OnboardingScreenConstant.ShiptoAddress:
                    return DbTableName.Address;

                default:
                    return default;
            }
        }
        #endregion

        #region Approval Delete Logic
        public async Task<int> DeleteApprovalRequest(string RequestId)
        {
            var requestId = int.Parse(RequestId);
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"requestId",requestId}
            };
            var sql = @"DELETE  FROM AllApprovalRequest WHERE requestID = @requestId";

            return await ExecuteNonQueryAsync(sql, parameters);
        }
        #endregion
        public async Task<int> CreateAllApprovalRequest(IAllApprovalRequest allApprovalRequest)
        {
            int retVal = -1;
            try
            {
                string query =
                    @"Insert Into AllApprovalRequest(linkedItemType,linkedItemUID,requestID,ApprovalUserDetail)
                                 values(@linkedItemType,@linkedItemUID,@requestID,@ApprovalUserDetail)";
                retVal = await ExecuteNonQueryAsync(query, allApprovalRequest);
            }
            catch
            {
                throw;
            }

            return retVal;
        }
        public async Task<List<IApprovalRuleMaster>> GetApprovalRuleMasterData()
        {
            try
            {

                var sqlQuery = @"
                SELECT
                    id AS RuleId,
                    name AS RuleName
                FROM 
                    rulemaster";
                List<IApprovalRuleMaster> approvalRuleMaster = await ExecuteQueryAsync<IApprovalRuleMaster>(sqlQuery);
                if (approvalRuleMaster == null || !approvalRuleMaster.Any())
                {
                    throw new ApplicationException("No data found in the rulemaster table.");
                }

                return approvalRuleMaster;
            }
            catch (SqlException sqlEx) // SQL-specific exceptions
            {
                throw new ApplicationException($"Database error occurred: {sqlEx.Message}", sqlEx);
            }
            catch (InvalidOperationException invEx) // Handle issues like query execution
            {
                throw new ApplicationException($"Operation error occurred while fetching data: {invEx.Message}", invEx);
            }
            catch (Exception ex) // General exceptions
            {
                throw new ApplicationException($"An unexpected error occurred: {ex.Message}", ex);
            }

        }
        public async Task<List<IUserHierarchy>> GetUserHierarchyForRule(string hierarchyType, string hierarchyUID, int ruleId)
        {
            try
            {
                IDictionary<string, object?> parameters = new Dictionary<string, object?>
        {
            {"HierarchyType", hierarchyType},
            {"RuleId", ruleId }
        };
                switch (hierarchyType)
                {
                    case UserHierarchyTypeConst.Emp:
                        parameters["HierarchyUID"] = hierarchyUID;
                        break;
                    case UserHierarchyTypeConst.StoreBM:
                        string sqlStoreBM = """
                    SELECT JP.emp_uid FROM job_position JP
                    INNER JOIN store S ON S.uid = @HierarchyUID 
                    INNER JOIN address A ON A.linked_item_type = 'store' AND A.type = 'Billing' 
                    AND A.is_default = 1 AND A.linked_item_uid = @HierarchyUID 
                    AND JP.branch_uid = A.branch_uid
                    AND JP.user_role_uid = CASE WHEN S.broad_classification = 'Service' THEN 'BSEM' ELSE 'BM' END 
                    """;
                        parameters["HierarchyUID"] = await ExecuteSingleAsync<string>(sqlStoreBM, new { HierarchyUID = hierarchyUID });
                        break;
                    case UserHierarchyTypeConst.Store:
                        string sqlStore = "SELECT  reporting_emp_uid FROM store WHERE uid = @HierarchyUID;";
                        parameters["HierarchyUID"] = await ExecuteSingleAsync<string>(sqlStore, new { HierarchyUID = hierarchyUID });
                        break;
                    case UserHierarchyTypeConst.StoreShipTo:
                        string sqlStoreShipTo = "SELECT  asm_emp_uid FROM Address WHERE type = 'Shipping' AND uid = @HierarchyUID AND is_default = 1;";
                        parameters["HierarchyUID"] = await ExecuteSingleAsync<string>(sqlStoreShipTo, new { HierarchyUID = hierarchyUID });
                        break;
                }

                string sql = @"
            WITH 
            ApplicableRoles AS
            (
               	SELECT R.code AS RoleCode, R.uid AS RoleUID, AH.level
               	FROM approvalhierarchy AH
               	INNER JOIN roles R ON R.code = AH.approverid
               	WHERE AH.ruleId = @RuleId
            ),
            user_hierarchy AS
            (
                SELECT JP.user_role_uid, JP.uid, JP.emp_uid, JP.reports_to_uid, 0 AS level_no
               	FROM job_position JP
               	--INNER JOIN ApplicableRoles AR ON AR.RoleUID = JP.user_role_uid
               	WHERE JP.emp_uid = @HierarchyUID
               	UNION ALL
               	SELECT JP.user_role_uid, JP.uid, JP.emp_uid, JP.reports_to_uid, UH.level_no + 1
                FROM job_position JP
                INNER JOIN user_hierarchy UH ON UH.reports_to_uid = /*JP.emp_uid*/ JP.uid
            )
            SELECT T.RoleCode, T.EmpUID, AR.level LevelNo, T.EmpCode, T.EmpName 
            FROM ApplicableRoles AR
            INNER JOIN (
            SELECT R.uid AS RoleUID,R.code AS RoleCode, JP.emp_uid AS EmpUID, 0 AS LevelNo,
            E.Code AS EmpCode, E.Name AS EmpName--, AR.level AS ApprovalLevel
            FROM job_position JP
            INNER JOIN roles R ON R.uid = JP.user_role_uid and R.is_branch_applicable = 1
            AND R.uid IN ('BC','BSEM') 
            AND JP.branch_uid IN (SELECT branch_uid FROM job_position where emp_uid = @HierarchyUID)
            INNER JOIN Emp E ON E.uid = JP.emp_uid
            UNION
            SELECT R.uid AS RoleUID,R.code AS RoleCode, JP.emp_uid AS EmpUID, 0 AS LevelNo,
            E.Code AS EmpCode, E.Name AS EmpName--, AR.level AS ApprovalLevel
            FROM job_position JP
            INNER JOIN roles R ON R.uid = JP.user_role_uid
            INNER JOIN Emp E ON E.uid = JP.emp_uid 
            --AND R.is_admin = 1
            AND jp.user_role_uid IN ('HOTAX','APF','FC','CH','CFO','HR','NSEH') 
            UNION
            SELECT R.uid AS RoleUID,R.Code RoleCode, UH.emp_uid EmpUID, UH.level_no AS LevelNo,
            E.Code AS EmpCode, E.Name AS EmpName--, AR.level AS ApprovalLevel
            FROM user_hierarchy UH
            INNER JOIN roles R ON R.uid = UH.user_role_uid
            INNER JOIN Emp E ON E.uid = UH.emp_uid AND UH.user_role_uid NOT IN ('BC','BSEM','HOTAX','APF','FC','CH','CFO','HR','NSEH') 
            ) T ON T.RoleUID = AR.RoleUID
            ";

                return await ExecuteQueryAsync<IUserHierarchy>(sql, parameters);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
