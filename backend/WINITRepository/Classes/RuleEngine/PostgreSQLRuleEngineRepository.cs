using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITRepository.Interfaces.RuleEngine;
using WINITSharedObjects.Models.RuleEngine;
using Action = WINITSharedObjects.Models.RuleEngine.Action;

namespace WINITRepository.Classes.RuleEngine
{
    public class PostgreSQLRuleEngineRepository : IRuleEngineRepository
    {
        private readonly string _connectionString;
        public PostgreSQLRuleEngineRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("PostgreSQL");
        }
        public async Task<List<Rule>> RetrieveAllRuleAsync()
        {
            List<Rule> Rules = null;
            try
            {
                DBManager.PostgresDBManager<Rule> dbManager = new DBManager.PostgresDBManager<Rule>(_connectionString);
                var sql = @"select *  FROM rulemaster order by id desc";
                Rules = await dbManager.ExecuteQueryAsync(sql, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in RetrieveRuleAsync for rule.", ex);
            }
            return Rules;
        }
        public async Task<Rule> RetrieveRuleAsync(int ruleId)
        {
            Rule Rule = null;
            try
            {
                DBManager.PostgresDBManager<Rule> dbManager = new DBManager.PostgresDBManager<Rule>(_connectionString);
                var sql = @"select *  FROM rulemaster WHERE id = @Id";
                Rule = await dbManager.ExecuteSingleAsync(sql, new Dictionary<string, object>
            {
                {"Id",  ruleId}
            });
            }
            catch (Exception ex)
            {
                throw new Exception("Error in RetrieveRuleAsync for rule.", ex);
            }
            return Rule;
        }

        public async Task<List<Action>> RetrieveActionsAsync(int ruleId)
        {
            List<Action> actions = new List<Action>();

            try
            {
                DBManager.PostgresDBManager<Action> dbManager = new DBManager.PostgresDBManager<Action>(_connectionString);
                var sql = @"select *  FROM ruleactions WHERE ruleId = @Id";
                actions = await dbManager.ExecuteQueryAsync(sql, new Dictionary<string, object>
            {
                {"Id",  ruleId}
            });
            }
            catch (Exception ex)
            {
                throw new Exception("Error in RetrieveActionsAsync for rule.", ex);
            }

            return actions;
        }
        public async Task<List<Condition>> RetrieveConditionsAsync(int ruleId)
        {
            List<Condition> conditions = new List<Condition>();


            try
            {
                DBManager.PostgresDBManager<Condition> dbManager = new DBManager.PostgresDBManager<Condition>(_connectionString);
                var sql = @"select c.*,p.name as ParameterName,p.datatype  FROM conditions c join ruleparameters p on p.id=c.parameterid WHERE c.ruleId = @Id";
                conditions = await dbManager.ExecuteQueryAsync(sql, new Dictionary<string, object>
            {
                {"Id",  ruleId}
            });
            }
            catch (Exception ex)
            {
                throw new Exception("Error in RetrieveConditionsAsync for rule.", ex);

            }
            return conditions;
        }

        public async Task<int> InsertApprovalRequestAsync(ApprovalRequest approvalRequest)
        {
            int res = 0;
            try
            {
                DBManager.PostgresDBManager<ApprovalRequest> dbManager = new DBManager.PostgresDBManager<ApprovalRequest>(_connectionString);
                var sql = @"
                    INSERT INTO approvalrequest (RuleId, RequesterId, Status, Comments, ModifiedBy,CreatedOn ,ModifiedOn)
                    VALUES (@ruleId, @userId, @status, @remarks, @ModifiedBy,@current_date,@current_date)
                    RETURNING Id";

                res = await dbManager.ExecuteScalarAsync<int>(sql, new Dictionary<string, object>
            {
                {"ruleId",  approvalRequest.RuleId},
                {"userId",approvalRequest.RequesterId },
                {"status",  approvalRequest.Status},
                {"remarks",approvalRequest.Remarks },
                {"ModifiedBy",  approvalRequest.ModifiedBy},
                {"current_date",DateTime.UtcNow }
            });
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Inserting request.", ex);

            }
            return res;

        }
        public async Task<int> UpdateApprovalRequestStatusAsync(int requestId, int ruleId, string status, string modifyBy)
        {
            int res = 0;
            try
            {
                DBManager.PostgresDBManager<Condition> dbManager = new DBManager.PostgresDBManager<Condition>(_connectionString);
                var sql = @"UPDATE ApprovalRequest SET Status = @status,ModifiedBy=@ModifiedBy,ModifiedOn=current_date WHERE id = @requestId AND RuleId = @ruleId";
                res = await dbManager.ExecuteNonQueryAsync(sql, new Dictionary<string, object>
            {
                {"ruleId",  ruleId},
                {"requestId",requestId },
                {"status",  status},
                 {"ModifiedBy",  modifyBy }
            });
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating status.", ex);

            }
            return res;
        }
        public async Task<int> InsertApprovalStatusAsync(ApprovalStatus approvalStatus)
        {
            int res = 0;
            try
            {
                DBManager.PostgresDBManager<ApprovalRequest> dbManager = new DBManager.PostgresDBManager<ApprovalRequest>(_connectionString);
                var sql = @"
                    INSERT INTO ApprovalStatus(ApprovalRequestId, ApproverId, Status, ActionId, CreatedOn, Remarks)
                    VALUES(@approvalRequestId, @approverId, @status, @actionId, @createdOn, @remarks)";

                res = await dbManager.ExecuteNonQueryAsync(sql, new Dictionary<string, object>
                {
                    { "approvalRequestId", approvalStatus.ApprovalRequestId},
                    { "approverId", approvalStatus.ApproverId},
                    { "status", approvalStatus.Status},
                    { "actionId", approvalStatus.ActionId},
                    { "createdOn", approvalStatus.CreatedOn},
                    {"remarks", approvalStatus.Remarks??""}
                }
            );
            }
            catch (Exception ex)
            {
                throw new Exception("Error in inserting status.", ex);

            }

            return res;
        }
        public async Task<int> UpdateApprovalStatusAsync(ApprovalStatus approvalStatus)
        {
            int res = 0;
            try
            {
                DBManager.PostgresDBManager<Condition> dbManager = new DBManager.PostgresDBManager<Condition>(_connectionString);
                var sql = @"UPDATE ApprovalStatus SET Status = @status WHERE ApprovalRequestId = @approvalRequestId AND ApproverId = @approverId";
                res = await dbManager.ExecuteNonQueryAsync(sql, new Dictionary<string, object>
            {
               { "approvalRequestId", approvalStatus.ApprovalRequestId},
               { "approverId", approvalStatus.ApproverId},
               { "status", approvalStatus.Status}
            });
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating approval status.", ex);

            }
            return res;
        }
        public async Task<List<ApprovalHierarchy>> RetrieveApprovalHierarchyAsync(int ruleId)
        {
            List<ApprovalHierarchy> approvalHierarchy = new List<ApprovalHierarchy>();


            try
            {
                DBManager.PostgresDBManager<ApprovalHierarchy> dbManager = new DBManager.PostgresDBManager<ApprovalHierarchy>(_connectionString);
                var sql = @"WITH RECURSIVE ApprovalHierarchyCTE AS (
                          SELECT Id, RuleId, ApproverId,  NextApproverId, 1 AS Level
                          FROM ApprovalHierarchy
                          WHERE RuleId = @id -- Replace <your_rule_id> with the desired RuleId to start the hierarchy
                          UNION ALL
                          SELECT AH.Id, AH.RuleId, AH.ApproverId, AH.NextApproverId, AHCTE.Level + 1
                          FROM ApprovalHierarchy AH
                          INNER JOIN ApprovalHierarchyCTE AHCTE ON AH.ApproverId = AHCTE.NextApproverId
                        )
                        SELECT c.*,u.email FROM ApprovalHierarchyCTE c join users u on u.id=ApproverId;";
                approvalHierarchy = await dbManager.ExecuteQueryAsync(sql, new Dictionary<string, object>
            {
                {"Id",  ruleId}
            });
            }
            catch (Exception ex)
            {
                throw new Exception("Error in RetrieveApprovalHierarchyAsync for rule.", ex);

            }
            return approvalHierarchy;
        }
        public async Task<int> InsertApproverAsync(int approvalRequestId, string ApproverId)
        {
            int res = 0;
            try
            {
                DBManager.PostgresDBManager<ApprovalRequest> dbManager = new DBManager.PostgresDBManager<ApprovalRequest>(_connectionString);
                var sql = @"
                    INSERT INTO approval_request_approver(ApprovalRequestId, ApproverId)
                    VALUES(@approvalRequestId, @approverId)";

                res = await dbManager.ExecuteNonQueryAsync(sql, new Dictionary<string, object>
                {
                    { "approvalRequestId", approvalRequestId},
                    { "approverId", ApproverId}
                }
            );
            }
            catch (Exception ex)
            {
                throw new Exception("Error in inserting status.", ex);

            }

            return res;
        }
        public async Task<int> InsertApprovalRequestActionAsync(int approvalRequestId, long ActionId)
        {
            int res = 0;
            try
            {
                DBManager.PostgresDBManager<ApprovalRequest> dbManager = new DBManager.PostgresDBManager<ApprovalRequest>(_connectionString);
                var sql = @"
                    INSERT INTO Approval_Request_Action(ApprovalRequestId, ActionId)
                    VALUES(@approvalRequestId, @ActionId)";

                res = await dbManager.ExecuteNonQueryAsync(sql, new Dictionary<string, object>
                {
                    { "approvalRequestId", approvalRequestId},
                    { "ActionId", ActionId}
                }
            );
            }
            catch (Exception ex)
            {
                throw new Exception("Error in inserting status.", ex);

            }

            return res;
        }
        public async Task<List<Action>> RetrieveActionsByRequestIdAsync(int requestId)
        {
            List<Action> actions = new List<Action>();

            try
            {
                DBManager.PostgresDBManager<Action> dbManager = new DBManager.PostgresDBManager<Action>(_connectionString);
                var sql = @"select a.*  FROM ruleactions a join Approval_Request_Action a2 on a.id=a2.ActionId WHERE a2.ApprovalRequestId =@Id";
                actions = await dbManager.ExecuteQueryAsync(sql, new Dictionary<string, object>
            {
                {"Id",  requestId}
            });
            }
            catch (Exception ex)
            {
                throw new Exception("Error in RetrieveActionsAsync for rule.", ex);
            }

            return actions;
        }
        public async Task<List<ApprovalHierarchy>> RetrieveApprovalHierarchyByRequestIdAsync(int requestId)
        {
            List<ApprovalHierarchy> approvalHierarchy = new List<ApprovalHierarchy>();

            try
            {
                DBManager.PostgresDBManager<ApprovalHierarchy> dbManager = new DBManager.PostgresDBManager<ApprovalHierarchy>(_connectionString);
                var sql = @"select a3.*,email FROM ApprovalStatus a 
                            join ApprovalRequest a2 on a.ApprovalRequestId =a2.id
                            join Users u on u.id=a.ApproverId
                            join ApprovalHierarchy a3 on a3.ruleid=a2.ruleid and a3.ApproverId=a.ApproverId
                            WHERE a.ApprovalRequestId = @Id and a.Status not in('Approved','Rejected') order by level";
                approvalHierarchy = await dbManager.ExecuteQueryAsync(sql, new Dictionary<string, object>
            {
                {"Id",  requestId}
            });
            }
            catch (Exception ex)
            {
                throw new Exception("Error in RetrieveApprovalHierarchyByRequestIdAsync for rule.", ex);

            }
            return approvalHierarchy;
        }
        public async Task<Users> RetrieveRequesterDetailsByRequestIdAsync(int requestId)
        {
            Users user = new Users();


            try
            {
                DBManager.PostgresDBManager<Users> dbManager = new DBManager.PostgresDBManager<Users>(_connectionString);
                var sql = @"select U.* from ApprovalRequest a join Users u on u.id=a.RequesterId WHERE a.id = @Id";
                user = await dbManager.ExecuteSingleAsync(sql, new Dictionary<string, object>
            {
                {"Id",  requestId}
            });
            }
            catch (Exception ex)
            {
                throw new Exception("Error in RetrieveRequesterDetailsByRequestIdAsync for rule.", ex);

            }
            return user;
        }
        public async Task<int> InsertRuleMaster(RuleMaster ruleMaster)
        {

            int res = 0;
            try
            {
                DBManager.PostgresDBManager<RuleMaster> dbManager = new DBManager.PostgresDBManager<RuleMaster>(_connectionString);
                string insertQuery = "INSERT INTO RuleMaster (Name, Description, CreatedOn, CreatedBy) VALUES (@Name, @Description, @CreatedOn, @CreatedBy) RETURNING Id";


                res = await dbManager.ExecuteScalarAsync<int>(insertQuery, new Dictionary<string, object>
                {
                    { "Name", ruleMaster.Name},
                    { "Description", ruleMaster.Description},
                    {"CreatedOn", ruleMaster.CreatedOn },
                    {"CreatedBy", ruleMaster.CreatedBy }
                }
            );
            }
            catch (Exception ex)
            {
                throw new Exception("Error in inserting RuleMaster.", ex);

            }

            return res;
        }
        public async Task<int> UpsertRuleParameter(RuleParameter ruleParameter)
        {
            int res = 0;
            try
            {
                DBManager.PostgresDBManager<RuleParameter> dbManager = new DBManager.PostgresDBManager<RuleParameter>(_connectionString);
                //string insertQuery = "INSERT INTO RuleParameters (RuleId, Name, DataType, Description) VALUES (@RuleId, @Name, @DataType, @Description)  RETURNING Id";
                string upsertQuery = @"
                INSERT INTO RuleParameters (RuleId, Name, DataType, Description)
                VALUES (@RuleId, @Name, @DataType, @Description)
                RETURNING Id";

                if (ruleParameter.Id > 0)
                    upsertQuery = @"UPDATE RuleParameters SET
                RuleId = @RuleId,
                Name = @Name,
                DataType = @DataType,
                Description = @Description where Id=@Id
                RETURNING Id";
                res = await dbManager.ExecuteScalarAsync<int>(upsertQuery, new Dictionary<string, object>
                {
                    { "Id", ruleParameter.Id},
                    { "RuleId", ruleParameter.RuleId},
                    { "Name", ruleParameter.Name},
                    {"DataType", ruleParameter.DataType},
                    {"Description", ruleParameter.Description??""}
                }
            );
            }
            catch (Exception ex)
            {
                throw new Exception("Error in UpsertRuleParameter.", ex);

            }

            return res;
        }
        public async Task<int> UpsertCondition(Condition condition)
        {
            int res = 0;
            try
            {
                DBManager.PostgresDBManager<Condition> dbManager = new DBManager.PostgresDBManager<Condition>(_connectionString);
                string upsertQuery = @"INSERT INTO Conditions (RuleId, ParentConditionId, IsGroup, Operator, ParameterId, Value)
                                   VALUES (@RuleId, @ParentConditionId, @IsGroup, @Operator, @ParameterId, @Value)
                                   RETURNING Id";
                if (condition.Id > 0)
                    upsertQuery = @"UPDATE Conditions SET
                                   RuleId = @RuleId,
                                   ParentConditionId = @ParentConditionId,
                                   IsGroup = @IsGroup,
                                   Operator = @Operator,
                                   ParameterId = @ParameterId,
                                   Value = @Value where Id=@Id
                                   RETURNING Id";

                res = await dbManager.ExecuteScalarAsync<int>(upsertQuery, new Dictionary<string, object>
                {
                    {"Id", condition.Id},
                    {"RuleId", condition.RuleId},
                    {"ParentConditionId", condition.ParentConditionId ?? (object)DBNull.Value},
                    {"IsGroup", condition.IsGroup},
                    { "Operator", condition.Operator},
                    { "ParameterId", condition.ParameterId},
                    {"Value", condition.Value ?? (object)DBNull.Value}
                }
            );
            }
            catch (Exception ex)
            {
                throw new Exception("Error in UpsertCondition.", ex);

            }

            return res;

        }
        public async Task<int> UpsertRuleAction(RuleAction ruleAction)
        {
            int res = 0;
            try
            {
                DBManager.PostgresDBManager<RuleAction> dbManager = new DBManager.PostgresDBManager<RuleAction>(_connectionString);
                string upsertQuery = @"
                    INSERT INTO RuleActions ( RuleId, ConditionId, ActionType, emailtemplate,notificationtemplate,smstemplate)
                    VALUES ( @RuleId, @ConditionId, @ActionType, @Emailtemplate,@Notificationtemplate,@Smstemplate)
                    RETURNING Id";
                if (ruleAction.Id > 0)
                    upsertQuery = @"
                   UPDATE RuleActions SET
                        RuleId = @RuleId,
                        ConditionId = @ConditionId,
                        ActionType = @ActionType,
                        emailtemplate = @Emailtemplate,
                        notificationtemplate = @Notificationtemplate,
                        smstemplate = @Smstemplate where Id=@Id
                    RETURNING Id";

                res = await dbManager.ExecuteScalarAsync<int>(upsertQuery, new Dictionary<string, object>
                {
                    {"Id", ruleAction.Id},
                    {"RuleId", ruleAction.RuleId},
                    {"ActionType", ruleAction.ActionType},
                    {"ConditionId", ruleAction.ConditionId},
                    {"Emailtemplate",  ruleAction.ActionType=="Email"?ruleAction.Template:""},
                    {"Notificationtemplate",  ruleAction.ActionType=="Notification"?ruleAction.Template:""},
                    {"Smstemplate",  ruleAction.ActionType=="SMS"?ruleAction.Template:""}
                }
            );
            }
            catch (Exception ex)
            {
                throw new Exception("Error in UpsertRuleAction.", ex);

            }

            return res;

        }
        public async Task<int> UpsertApprovalHierarchy(ApprovalHierarchy approvalHierarchy)
        {
            int res = 0;
            try
            {
                DBManager.PostgresDBManager<ApprovalHierarchy> dbManager = new DBManager.PostgresDBManager<ApprovalHierarchy>(_connectionString);
                string upsertQuery = @"
                    INSERT INTO ApprovalHierarchy ( RuleId, ApproverId, Level, NextApproverId)
                    VALUES ( @RuleId, @ApproverId, @Level, @NextApproverId)
                    RETURNING Id";
                if(approvalHierarchy.id>0)
                    upsertQuery = @"
                    UPDATE ApprovalHierarchy SET
                        RuleId = @RuleId,
                        ApproverId = @ApproverId,
                        Level = @Level,
                        NextApproverId = @NextApproverId where id=@Id
                    RETURNING Id";

                res = await dbManager.ExecuteScalarAsync<int>(upsertQuery, new Dictionary<string, object>
                {
                    {"Id", approvalHierarchy.id},
                    {"RuleId", approvalHierarchy.RuleId},
                    {"ApproverId", approvalHierarchy.ApproverId},
                    {"Level", approvalHierarchy.Level},
                    {"NextApproverId", approvalHierarchy.NextApproverId ?? (object)DBNull.Value}
                }
            );
            }
            catch (Exception ex)
            {
                throw new Exception("Error in UpsertApprovalHierarchy.", ex);

            }

            return res;
        }
        public async Task<List<KeyValueObject<string, string>>> RetrieveApproverAllAsync()
        {
            List<KeyValueObject<string, string>> users = new List<KeyValueObject<string, string>>();

            try
            {
                DBManager.PostgresDBManager<KeyValueObject<string, string>> dbManager = new DBManager.PostgresDBManager<KeyValueObject<string, string>>(_connectionString);
                var sql = @"select id as Key,name as Value from users";
                users = await dbManager.ExecuteQueryAsync(sql, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in RetrieveApproverAllAsync", ex);
            }

            return users;
        }

    }
}
