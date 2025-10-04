using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Winit.Modules.CaptureCompetitor.Model.Classes;
using Winit.Modules.CaptureCompetitor.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Dapper;
using Npgsql;

namespace WINITAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitorBrandController : WINITBaseController
    {
        private readonly string _connectionString;

        public CompetitorBrandController(IServiceProvider serviceProvider, IConfiguration configuration) : base(serviceProvider)
        {
            _connectionString = configuration.GetConnectionString(ConnectionStringName.PostgreSQL);
        }

        // GET: api/CompetitorBrand/GetMappings
        [HttpGet("GetMappings")]
        public async Task<IActionResult> GetCompetitorBrandMappings(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string categoryCode = null,
            [FromQuery] string brandCode = null)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                
                var query = @"
                    SELECT 
                        cbcm.id,
                        cbcm.uid,
                        CONCAT('[', c.category_code, '] ', c.category_name) as CategoryName,
                        c.category_code as CategoryCode,
                        b.brand_code as BrandCode,
                        b.brand_name as BrandName,
                        cbcm.competitor_code as CompetitorCompany,
                        cbcm.created_by as CreatedBy,
                        cbcm.created_time as CreatedTime,
                        cbcm.modified_by as ModifiedBy,
                        cbcm.modified_time as ModifiedTime
                    FROM category_brand_competitor_mapping cbcm
                    JOIN category_brand_mapping cbm ON cbm.uid = cbcm.category_brand_uid
                    JOIN vw_category c ON c.category_code = cbm.category_code
                    JOIN vw_brand b ON b.brand_code = cbm.brand_code 
                    WHERE cbcm.ss = 1";

                var parameters = new DynamicParameters();
                
                if (!string.IsNullOrEmpty(categoryCode))
                {
                    query += " AND c.category_code = @CategoryCode";
                    parameters.Add("CategoryCode", categoryCode);
                }
                
                if (!string.IsNullOrEmpty(brandCode))
                {
                    query += " AND b.brand_code = @BrandCode";
                    parameters.Add("BrandCode", brandCode);
                }

                query += " ORDER BY c.category_code, b.brand_code";
                query += " LIMIT @PageSize OFFSET @Offset";
                
                parameters.Add("PageSize", pageSize);
                parameters.Add("Offset", (pageNumber - 1) * pageSize);

                var mappings = await connection.QueryAsync<dynamic>(query, parameters);

                // Get total count
                var countQuery = @"
                    SELECT COUNT(*) 
                    FROM category_brand_competitor_mapping cbcm
                    JOIN category_brand_mapping cbm ON cbm.uid = cbcm.category_brand_uid
                    WHERE cbcm.ss = 1";

                var totalCount = await connection.ExecuteScalarAsync<int>(countQuery);

                return Ok(new
                {
                    Data = mappings,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // POST: api/CompetitorBrand/Create
        [HttpPost("Create")]
        public async Task<IActionResult> CreateCompetitorBrandMapping([FromBody] CompetitorBrandMappingDto model)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();

                try
                {
                    // Check if category_brand_mapping exists
                    var checkQuery = @"
                        SELECT uid 
                        FROM category_brand_mapping 
                        WHERE category_code = @CategoryCode 
                          AND brand_code = @BrandCode 
                          AND ss = 1";

                    var existingUid = await connection.ExecuteScalarAsync<string>(
                        checkQuery,
                        new { model.CategoryCode, model.BrandCode },
                        transaction);

                    string categoryBrandUid;

                    if (string.IsNullOrEmpty(existingUid))
                    {
                        // Create category_brand_mapping
                        categoryBrandUid = Guid.NewGuid().ToString();
                        var insertCBMQuery = @"
                            INSERT INTO category_brand_mapping (
                                uid, category_code, brand_code, created_by, created_time,
                                modified_by, modified_time, server_add_time, server_modified_time, ss
                            ) VALUES (
                                @Uid, @CategoryCode, @BrandCode, @CreatedBy, @CreatedTime,
                                @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @SS
                            )";

                        await connection.ExecuteAsync(insertCBMQuery, new
                        {
                            Uid = categoryBrandUid,
                            model.CategoryCode,
                            model.BrandCode,
                            CreatedBy = model.CreatedBy ?? "TB7628", // Default employee UID
                            CreatedTime = DateTime.UtcNow,
                            ModifiedBy = model.CreatedBy ?? "TB7628",
                            ModifiedTime = DateTime.UtcNow,
                            ServerAddTime = DateTime.UtcNow,
                            ServerModifiedTime = DateTime.UtcNow,
                            SS = 1
                        }, transaction);
                    }
                    else
                    {
                        categoryBrandUid = existingUid;
                    }

                    // Check for duplicate competitor mapping
                    var duplicateCheckQuery = @"
                        SELECT COUNT(*) 
                        FROM category_brand_competitor_mapping 
                        WHERE category_brand_uid = @CategoryBrandUid 
                          AND competitor_code = @CompetitorCode 
                          AND ss = 1";

                    var duplicateCount = await connection.ExecuteScalarAsync<int>(
                        duplicateCheckQuery,
                        new { CategoryBrandUid = categoryBrandUid, model.CompetitorCode },
                        transaction);

                    if (duplicateCount > 0)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { Error = "This competitor mapping already exists" });
                    }

                    // Insert competitor mapping
                    var mappingUid = Guid.NewGuid().ToString();
                    var insertMappingQuery = @"
                        INSERT INTO category_brand_competitor_mapping (
                            uid, category_brand_uid, competitor_code, created_by, created_time,
                            modified_by, modified_time, server_add_time, server_modified_time, ss
                        ) VALUES (
                            @Uid, @CategoryBrandUid, @CompetitorCode, @CreatedBy, @CreatedTime,
                            @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @SS
                        )";

                    await connection.ExecuteAsync(insertMappingQuery, new
                    {
                        Uid = mappingUid,
                        CategoryBrandUid = categoryBrandUid,
                        model.CompetitorCode,
                        CreatedBy = model.CreatedBy ?? "TB7628",
                        CreatedTime = DateTime.UtcNow,
                        ModifiedBy = model.CreatedBy ?? "TB7628",
                        ModifiedTime = DateTime.UtcNow,
                        ServerAddTime = DateTime.UtcNow,
                        ServerModifiedTime = DateTime.UtcNow,
                        SS = 1
                    }, transaction);

                    await transaction.CommitAsync();

                    return Ok(new { Success = true, Uid = mappingUid });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // PUT: api/CompetitorBrand/Update
        [HttpPut("Update")]
        public async Task<IActionResult> UpdateCompetitorBrandMapping([FromBody] CompetitorBrandUpdateDto model)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                var updateQuery = @"
                    UPDATE category_brand_competitor_mapping
                    SET 
                        competitor_code = @CompetitorCode,
                        modified_by = @ModifiedBy,
                        modified_time = @ModifiedTime,
                        server_modified_time = @ServerModifiedTime
                    WHERE uid = @Uid AND ss = 1";

                var affectedRows = await connection.ExecuteAsync(updateQuery, new
                {
                    model.Uid,
                    model.CompetitorCode,
                    ModifiedBy = model.ModifiedBy ?? "TB7628",
                    ModifiedTime = DateTime.UtcNow,
                    ServerModifiedTime = DateTime.UtcNow
                });

                if (affectedRows == 0)
                {
                    return NotFound(new { Error = "Mapping not found or already deleted" });
                }

                return Ok(new { Success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // DELETE: api/CompetitorBrand/Delete/{uid}
        [HttpDelete("Delete/{uid}")]
        public async Task<IActionResult> DeleteCompetitorBrandMapping(string uid, [FromQuery] string deletedBy = null)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                // Soft delete - set ss to 0
                var deleteQuery = @"
                    UPDATE category_brand_competitor_mapping
                    SET 
                        ss = 0,
                        modified_by = @ModifiedBy,
                        modified_time = @ModifiedTime,
                        server_modified_time = @ServerModifiedTime
                    WHERE uid = @Uid AND ss = 1";

                var affectedRows = await connection.ExecuteAsync(deleteQuery, new
                {
                    Uid = uid,
                    ModifiedBy = deletedBy ?? "TB7628",
                    ModifiedTime = DateTime.UtcNow,
                    ServerModifiedTime = DateTime.UtcNow
                });

                if (affectedRows == 0)
                {
                    return NotFound(new { Error = "Mapping not found or already deleted" });
                }

                return Ok(new { Success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // GET: api/CompetitorBrand/GetCategories
        [HttpGet("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                
                var query = @"
                    SELECT DISTINCT 
                        category_code as Value,
                        CONCAT('[', category_code, '] ', category_name) as Label
                    FROM vw_category
                    ORDER BY category_code";

                var categories = await connection.QueryAsync<dynamic>(query);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // GET: api/CompetitorBrand/GetBrandsByCategory/{categoryCode}
        [HttpGet("GetBrandsByCategory/{categoryCode}")]
        public async Task<IActionResult> GetBrandsByCategory(string categoryCode)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                
                var query = @"
                    SELECT DISTINCT
                        brand_code as Value,
                        CONCAT('[', brand_code, '] ', brand_name) as Label
                    FROM vw_brand
                    WHERE category_code = @CategoryCode
                    ORDER BY brand_code";

                var brands = await connection.QueryAsync<dynamic>(query, new { CategoryCode = categoryCode });
                return Ok(brands);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // GET: api/CompetitorBrand/GetCompetitors
        [HttpGet("GetCompetitors")]
        public async Task<IActionResult> GetCompetitors()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                
                // Get unique competitor codes from existing data
                var query = @"
                    SELECT DISTINCT 
                        competitor_code as value,
                        competitor_code as label
                    FROM category_brand_competitor_mapping
                    WHERE ss = 1
                    ORDER BY competitor_code";

                var competitors = await connection.QueryAsync<dynamic>(query);
                
                return Ok(competitors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }

    // DTOs
    public class CompetitorBrandMappingDto
    {
        public string CategoryCode { get; set; }
        public string BrandCode { get; set; }
        public string CompetitorCode { get; set; }
        public string CreatedBy { get; set; }
    }

    public class CompetitorBrandUpdateDto
    {
        public string Uid { get; set; }
        public string CompetitorCode { get; set; }
        public string ModifiedBy { get; set; }
    }
}