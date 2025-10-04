using MongoDB.Bson;
using MongoDB.Driver;
using System.Globalization;
using System.Text.RegularExpressions;
using Winit.Modules.AuditTrail.DL.Interfaces;
using Winit.Modules.AuditTrail.Model.Classes;
using Winit.Shared.Models.Common;
//using WINITSharedObjects.Enums;
using Winit.Shared.Models.Enums;
namespace Winit.Modules.AuditTrail.DL.Classes
{
    public class AuditTrailServiceDL : IAuditTrailServiceDL
    {
        private readonly IMongoCollection<AuditTrailEntry> _collection;

        public AuditTrailServiceDL(string connectionString, string databaseName, string collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<AuditTrailEntry>(collectionName);
        }

        public async Task CreateAuditTrailAsync(AuditTrailEntry auditTrailEntry)
        {
            auditTrailEntry.Id = Guid.NewGuid().ToString(); // Ensure unique _id
            auditTrailEntry.ServerCommandDate = DateTime.UtcNow;
            await _collection.InsertOneAsync(auditTrailEntry);
        }

        public async Task<List<AuditTrailEntry>> GetAuditTrailsAsync(string linkedItemType, string linkedItemUID)
        {
            return await _collection.Find(audit => audit.LinkedItemType == linkedItemType && audit.LinkedItemUID == linkedItemUID).ToListAsync();
        }
        public async Task<AuditTrailEntry> GetAuditTrailByIdAsync(string id, bool isChangeDataRequired)
        {
            var filter = Builders<AuditTrailEntry>.Filter.And(
                Builders<AuditTrailEntry>.Filter.Eq(audit => audit.Id, id)
            );

            var projection = Builders<AuditTrailEntry>.Projection
                    .Include(a => a.Id)
                    .Include(a => a.ServerCommandDate)
                    .Include(a => a.LinkedItemType)
                    .Include(a => a.LinkedItemUID)
                    .Include(a => a.CommandType)
                    .Include(a => a.CommandDate)
                    .Include(a => a.DocNo)
                    .Include(a => a.JobPositionUID)
                    .Include(a => a.EmpUID)
                    .Include(a => a.EmpName)
                    .Include(a => a.NewData)
                    .Include(a => a.OriginalDataId)
                    .Include(a => a.HasChanges);
            if (isChangeDataRequired)
            {
                projection = projection.Include(a => a.ChangeData);
            }

            AuditTrailEntry auditTrailEntry = await _collection.Find(filter)
                .SortByDescending(d => d.ServerCommandDate)
                .Project<AuditTrailEntry>(projection)
                .FirstOrDefaultAsync();

            return auditTrailEntry;
        }
        public async Task<PagedResponse<AuditTrailEntry>> GetAuditTrailsAsyncByPaging(
      List<SortCriteria> sortCriterias, int pageNumber, int pageSize,
      List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var filterBuilder = Builders<AuditTrailEntry>.Filter;
                var filterDefinitions = new List<FilterDefinition<AuditTrailEntry>>();

                if (filterCriterias?.Count > 0)
                {
                    foreach (var criteria in filterCriterias)
                    {
                        var field = criteria.Name;
                        var value = criteria.Value?.ToString();
                        if (string.IsNullOrWhiteSpace(value)) continue;

                        switch (criteria.Type)
                        {
                            case FilterType.Equal:
                                filterDefinitions.Add(filterBuilder.Regex(field, new BsonRegularExpression($"^{Regex.Escape(value)}$", "i")));
                                break;
                            case FilterType.NotEqual:
                                filterDefinitions.Add(filterBuilder.Not(filterBuilder.Regex(field, new BsonRegularExpression($"^{Regex.Escape(value)}$", "i"))));
                                break;
                            case FilterType.GreaterThan:
                                filterDefinitions.Add(filterBuilder.Gt(field, value));
                                break;
                            case FilterType.LessThan:
                                filterDefinitions.Add(filterBuilder.Lt(field, value));
                                break;
                            case FilterType.GreaterThanOrEqual:
                                filterDefinitions.Add(filterBuilder.Gte(field, value));
                                break;
                            case FilterType.LessThanOrEqual:
                                filterDefinitions.Add(filterBuilder.Lte(field, value));
                                break;
                            case FilterType.Contains:
                                filterDefinitions.Add(filterBuilder.Regex(field, new BsonRegularExpression($".*{Regex.Escape(value)}.*", "i")));
                                break;
                            case FilterType.Between:
                                if (criteria.Value is IEnumerable<object> dateRangeObj)
                                {
                                    var dateRangeList = dateRangeObj.Select(d => d.ToString()).ToList();

                                    if (dateRangeList.Count == 2 &&
                                        DateTime.TryParseExact(dateRangeList[0], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var lowerBound) &&
                                        DateTime.TryParseExact(dateRangeList[1], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var upperBound))
                                    {
                                        // Ensure lowerBound is at start of day
                                        lowerBound = lowerBound.Date;

                                        //Ensure upperBound is at end of day (23:59:59)
                                        upperBound = upperBound.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                                        filterDefinitions.Add(filterBuilder.Gte(field, lowerBound));
                                        filterDefinitions.Add(filterBuilder.Lte(field, upperBound));
                                    }
                                    else
                                    {
                                        Console.WriteLine("ERROR: Date parsing failed");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("ERROR: criteria.Value is not an IEnumerable<object>");
                                }
                                break;

                        }
                    }
                }

                var filter = filterDefinitions.Count > 0 ? filterBuilder.And(filterDefinitions) : filterBuilder.Empty;

                var sortBuilder = Builders<AuditTrailEntry>.Sort;
                var sort = sortCriterias?.Count > 0
                    ? sortBuilder.Combine(sortCriterias.Select(c => c.Direction == Winit.Shared.Models.Enums.SortDirection.Asc
                        ? sortBuilder.Ascending(c.SortParameter)
                        : sortBuilder.Descending(c.SortParameter)))
                    : sortBuilder.Descending(a => a.ServerCommandDate);

                var skip = (pageNumber - 1) * pageSize;
                var limit = pageSize;

                var projection = Builders<AuditTrailEntry>.Projection
                   .Include(a => a.Id)
                   .Include(a => a.ServerCommandDate)
                   .Include(a => a.LinkedItemType)
                   .Include(a => a.LinkedItemUID)
                   .Include(a => a.CommandType)
                   .Include(a => a.CommandDate)
                   .Include(a => a.DocNo)
                   .Include(a => a.JobPositionUID)
                   .Include(a => a.EmpUID)
                   .Include(a => a.EmpName)
                   .Include(a => a.OriginalDataId)
                   .Include(a => a.HasChanges);

                var auditTrailsTask = _collection.Find(filter)
                    .Sort(sort)
                    .Skip(skip)
                    .Limit(limit)
                    .Project<AuditTrailEntry>(projection)
                    .ToListAsync();

                var countTask = isCountRequired ? _collection.CountDocumentsAsync(filter) : Task.FromResult(0L);

                await Task.WhenAll(auditTrailsTask, countTask);

                return new PagedResponse<AuditTrailEntry>
                {
                    PagedData = auditTrailsTask.Result,
                    TotalCount = (int)countTask.Result
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAuditTrailsAsyncByPaging: {ex.Message}");
                throw;
            }
        }



        public async Task UpdateAuditTrailAsync(string id, AuditTrailEntry updatedAuditTrailEntry)
        {
            var filter = Builders<AuditTrailEntry>.Filter.Eq(audit => audit.Id, id);
            await _collection.ReplaceOneAsync(filter, updatedAuditTrailEntry);
        }

        public async Task DeleteAuditTrailAsync(string id)
        {
            var filter = Builders<AuditTrailEntry>.Filter.Eq(audit => audit.Id, id);
            await _collection.DeleteOneAsync(filter);
        }

        public async Task<AuditTrailEntry> GetLastAuditTrailAsync(string linkedItemType, string linkedItemUID,
            bool isChangeDataRequired)
        {
            // Step 1: Retrieve old data from MongoDB
            var filter = Builders<AuditTrailEntry>.Filter.And(
                Builders<AuditTrailEntry>.Filter.Eq(audit => audit.LinkedItemType, linkedItemType),
                Builders<AuditTrailEntry>.Filter.Eq(audit => audit.LinkedItemUID, linkedItemUID)
            );

            var projection = Builders<AuditTrailEntry>.Projection
                    .Include(a => a.Id)
                    .Include(a => a.ServerCommandDate)
                    .Include(a => a.LinkedItemType)
                    .Include(a => a.LinkedItemUID)
                    .Include(a => a.CommandType)
                    .Include(a => a.CommandDate)
                    .Include(a => a.DocNo)
                    .Include(a => a.JobPositionUID)
                    .Include(a => a.EmpUID)
                    .Include(a => a.EmpName)
                    .Include(a => a.NewData)
                    .Include(a => a.OriginalDataId);
            if (isChangeDataRequired)
            {
                projection.Include(a => a.ChangeData);
            }

            AuditTrailEntry auditTrailEntry = await _collection.Find(filter)
                .SortByDescending(d => d.ServerCommandDate)
                .Project<AuditTrailEntry>(projection)
                .FirstOrDefaultAsync();

            return auditTrailEntry;
        }
    }
}
