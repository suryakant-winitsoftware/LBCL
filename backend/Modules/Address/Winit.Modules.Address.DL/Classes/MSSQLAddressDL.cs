using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Address.DL.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Address.DL.Classes
{
    public class MSSQLAddressDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IAddressDL
    {
        public MSSQLAddressDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Address.Model.Interfaces.IAddress>> SelectAllAddressDetails(List<SortCriteria> sortCriterias, int pageNumber,
   int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT a.id AS Id, a.uid AS UID, a.created_by AS CreatedBy, a.created_time AS CreatedTime,
                                     a.modified_by AS ModifiedBy, a.modified_time AS ModifiedTime, a.server_add_time AS ServerAddTime,
                                     a.server_modified_time AS ServerModifiedTime, type AS Type, a.name AS Name, line1 AS Line1, line2 AS Line2,
                                     line3 AS Line3, landmark AS Landmark, area AS Area, sub_area AS SubArea, zip_code AS ZipCode, city AS City,
                                     country_code AS CountryCode, region_code AS RegionCode, phone AS Phone, phone_extension AS PhoneExtension,
                                     mobile1 AS Mobile1, mobile2 AS Mobile2, email AS Email, fax AS Fax, latitude AS Latitude, longitude AS Longitude,
                                     altitude AS Altitude, linked_item_uid AS LinkedItemUID, linked_item_type AS LinkedItemType, a.status AS Status,
                                     state_code AS StateCode, territory_code AS TerritoryCode, pan AS PAN, aadhar AS AADHAR, ssn AS SSN,
                                     is_editable AS IsEditable, is_default AS IsDefault, line4 AS Line4, info AS Info, a.depot AS Depot,
                                     location_uid AS LocationUID, state as State, locality as Locality, branch_uid as BranchUID, 
									 o.code as OrgUnitUID, sales_office_uid as SalesOfficeUID, a.custom_field3 as SiteNo
									 FROM address a
									 Inner join store_additional_info sai 
									 on a.linked_item_uid = sai.store_uid
                                     left join org o on a.org_unit_uid = o.uid) AS SubQuery");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT a.id AS Id, a.uid AS UID, a.created_by AS CreatedBy, a.created_time AS CreatedTime,
                                     a.modified_by AS ModifiedBy, a.modified_time AS ModifiedTime, a.server_add_time AS ServerAddTime,
                                     a.server_modified_time AS ServerModifiedTime, type AS Type, a.name AS Name, line1 AS Line1, line2 AS Line2,
                                     line3 AS Line3, landmark AS Landmark, area AS Area, sub_area AS SubArea, zip_code AS ZipCode, city AS City,
                                     country_code AS CountryCode, region_code AS RegionCode, phone AS Phone, phone_extension AS PhoneExtension,
                                     mobile1 AS Mobile1, mobile2 AS Mobile2, email AS Email, fax AS Fax, latitude AS Latitude, longitude AS Longitude,
                                     altitude AS Altitude, linked_item_uid AS LinkedItemUID, linked_item_type AS LinkedItemType, a.status AS Status,
                                     state_code AS StateCode, territory_code AS TerritoryCode, pan AS PAN, aadhar AS AADHAR, ssn AS SSN,
                                     is_editable AS IsEditable, is_default AS IsDefault, line4 AS Line4, info AS Info, a.depot AS Depot,
                                     location_uid AS LocationUID, state as State, locality as Locality, branch_uid as BranchUID, 
									 o.code as OrgUnitUID, sales_office_uid as SalesOfficeUID, a.custom_field3 as SiteNo
									 FROM address a
									 Inner join store_additional_info sai 
									 on a.linked_item_uid = sai.store_uid
                                     left join org o on a.org_unit_uid = o.uid) AS SubQuery");
                }

                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Address.Model.Interfaces.IAddress>(filterCriterias, sbFilterCriteria, parameters);
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
                else
                {
                    sql.Append(" ORDER BY Id"); 
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                IEnumerable<Winit.Modules.Address.Model.Interfaces.IAddress> AddressDetails = await ExecuteQueryAsync<Winit.Modules.Address.Model.Interfaces.IAddress>(sql.ToString(), parameters);
                int totalCount = -1;

                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Address.Model.Interfaces.IAddress> pagedResponse = new PagedResponse<Winit.Modules.Address.Model.Interfaces.IAddress>
                {
                    PagedData = AddressDetails,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<Winit.Modules.Address.Model.Interfaces.IAddress> GetAddressDetailsByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };

            var sql = @"SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,
                        type AS Type,name AS Name, line1 AS Line1, line2 AS Line2,line3 AS Line3,landmark AS Landmark,area AS Area,
                        sub_area AS SubArea,zip_code AS ZipCode,city AS City,country_code AS CountryCode,region_code AS RegionCode,
                        phone AS Phone,phone_extension AS PhoneExtension,mobile1 AS Mobile1,mobile2 AS Mobile2, email AS Email,fax 
                        AS Fax,latitude AS Latitude,longitude AS Longitude,altitude AS Altitude,linked_item_uid AS LinkedItemUID,
                        linked_item_type AS LinkedItemType,status AS Status,state_code AS StateCode,territory_code AS TerritoryCode,pan AS PAN,
                        aadhar AS AADHAR,ssn AS SSN,is_editable AS IsEditable,is_default AS IsDefault,line4 AS Line4,info AS Info,depot AS Depot,
                        location_uid as LocationUID FROM address WHERE uid = @UID";
         Winit.Modules.Address.Model.Interfaces.IAddress AddressDetails = await ExecuteSingleAsync<Winit.Modules.Address.Model.Interfaces.IAddress>(sql, parameters);
            return AddressDetails;
        }
        public async Task<int> CreateAddressDetails(Winit.Modules.Address.Model.Interfaces.IAddress createAddress)
        {
                 var sql = @"INSERT INTO address (uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                            server_modified_time, type, name, line1, line2, line3, landmark, area, sub_area, zip_code, city, 
                            country_code, region_code, phone, phone_extension, mobile1, mobile2, email, fax, latitude, longitude, 
                            altitude, linked_item_uid, linked_item_type, status, state_code, territory_code, pan, aadhar, ssn, 
                            is_editable, is_default, line4, info, depot, location_uid, locality, state, branch_uid,  sales_office_uid, org_unit_uid) 
                            VALUES
                            (@UID, @CreatedBy, @CreatedTime,@ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                            @Type, @Name, @Line1, @Line2, @Line3, @Landmark,@Area, @SubArea, @ZipCode, @City, @CountryCode, 
                            @RegionCode, @Phone, @PhoneExtension, @Mobile1, @Mobile2, @Email, @Fax, @Latitude, @Longitude, 
                            @Altitude, @LinkedItemUID, @LinkedItemType, @Status, @StateCode, @TerritoryCode, @PAN, @AADHAR, @SSN, 
                            @IsEditable, @IsDefault, @Line4, @Info, @Depot, @LocationUID, @Locality, @State, @BranchUID,  @SalesOfficeUID, @OrgUnitUID);;";

            return await ExecuteNonQueryAsync(sql, createAddress);
        }
        public async Task<int> CreateAddressDetailsList(List<Winit.Modules.Address.Model.Interfaces.IAddress> createAddress)
        {
                 var sql = @"INSERT INTO address (uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                            server_modified_time, type, name, line1, line2, line3, landmark, area, sub_area, zip_code, city, 
                            country_code, region_code, phone, phone_extension, mobile1, mobile2, email, fax, latitude, longitude, 
                            altitude, linked_item_uid, linked_item_type, status, state_code, territory_code, pan, aadhar, ssn, 
                            is_editable, is_default, line4, info, depot, location_uid, locality, state, branch_uid,  sales_office_uid, org_unit_uid) 
                            VALUES
                            (@UID, @CreatedBy, @CreatedTime,@ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                            @Type, @Name, @Line1, @Line2, @Line3, @Landmark,@Area, @SubArea, @ZipCode, @City, @CountryCode, 
                            @RegionCode, @Phone, @PhoneExtension, @Mobile1, @Mobile2, @Email, @Fax, @Latitude, @Longitude, 
                            @Altitude, @LinkedItemUID, @LinkedItemType, @Status, @StateCode, @TerritoryCode, @PAN, @AADHAR, @SSN, 
                            @IsEditable, @IsDefault, @Line4, @Info, @Depot, @LocationUID, @Locality, @State, @BranchUID,  @SalesOfficeUID, @OrgUnitUID);;";

            return await ExecuteNonQueryAsync(sql, createAddress);
        }

        public async Task<int> UpdateAddressDetails(Winit.Modules.Address.Model.Interfaces.IAddress updateAddress)
        {
            try
            {
                var sql = @"UPDATE address SET modified_by = @ModifiedBy, modified_time = @ModifiedTime, server_modified_time = @ServerModifiedTime,
                            type = @Type, name = @Name, line1 = @Line1, line2 = @Line2, line3 = @Line3, landmark = @Landmark, area = @Area, 
                            sub_area = @SubArea, zip_code = @ZipCode, city = @City, country_code = @CountryCode, region_code = @RegionCode,
                            phone = @Phone, phone_extension = @PhoneExtension, mobile1 = @Mobile1, mobile2 = @Mobile2, email = @Email, fax = @Fax, 
                            latitude = @Latitude, longitude = @Longitude, altitude = @Altitude, linked_item_uid = @LinkedItemUID, 
                            status = @Status, state_code = @StateCode, territory_code = @TerritoryCode, 
                            pan = @PAN, aadhar = @AADHAR, ssn = @SSN, is_editable = @IsEditable, is_default = @IsDefault, line4 = @Line4, info = @Info,
                            depot = @Depot, state=@State, branch_uid =@BranchUID, locality =@Locality,sales_office_uid = @SalesOfficeUID, org_unit_uid = @OrgUnitUID WHERE uid = @UID;";
                
                return await ExecuteNonQueryAsync(sql, updateAddress);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteAddressDetails(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"delete from address where uid= @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateAddressDetails(string addressUID, string latitude, string longitude)
        {
            throw new NotImplementedException();
        }
    }
}
