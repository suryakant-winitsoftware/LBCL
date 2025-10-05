using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Address.DL.Interfaces;
using Winit.Modules.Address.Model.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Address.DL.Classes
{
    public class SQLiteAddressDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IAddressDL
    {
        public SQLiteAddressDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Address.Model.Interfaces.IAddress>> SelectAllAddressDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
                        {
                            var sql = new StringBuilder(@"select * from(SELECT 
                id AS Id, uid AS Uid, created_by AS CreatedBy, created_time AS CreatedTime,
                modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime, type AS Type, name AS Name, line1 AS Line1,
                line2 AS Line2, line3 AS Line3, landmark AS Landmark, area AS Area, sub_area AS SubArea,
                zip_code AS ZipCode, city AS City, country_code AS CountryCode, region_code AS RegionCode,
                phone AS Phone, phone_extension AS PhoneExtension, mobile1 AS Mobile1, mobile2 AS Mobile2,
                email AS Email, fax AS Fax, latitude AS Latitude, longitude AS Longitude, altitude AS Altitude,
                linked_item_uid AS LinkedItemUid, linked_item_type AS LinkedItemType, status AS Status,
                state_code AS StateCode, territory_code AS TerritoryCode, pan AS Pan, aadhar AS Aadhar,
                ssn AS Ssn, is_editable AS IsEditable, is_default AS IsDefault, line4 AS Line4, info AS Info,
                depot AS Depot 
            FROM 
                address)as SubQuery");
              var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                id AS Id, uid AS Uid, created_by AS CreatedBy, created_time AS CreatedTime,
                modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime, type AS Type, name AS Name, line1 AS Line1,
                line2 AS Line2, line3 AS Line3, landmark AS Landmark, area AS Area, sub_area AS SubArea,
                zip_code AS ZipCode, city AS City, country_code AS CountryCode, region_code AS RegionCode,
                phone AS Phone, phone_extension AS PhoneExtension, mobile1 AS Mobile1, mobile2 AS Mobile2,
                email AS Email, fax AS Fax, latitude AS Latitude, longitude AS Longitude, altitude AS Altitude,
                linked_item_uid AS LinkedItemUid, linked_item_type AS LinkedItemType, status AS Status,
                state_code AS StateCode, territory_code AS TerritoryCode, pan AS Pan, aadhar AS Aadhar,
                ssn AS Ssn, is_editable AS IsEditable, is_default AS IsDefault, line4 AS Line4, info AS Info,
                depot AS Depot 
            FROM 
                address)as SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
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
                //if (pageNumber > 0 && pageSize > 0)
                //{
                //    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                //}
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAddress>().GetType();
                IEnumerable<Winit.Modules.Address.Model.Interfaces.IAddress> AddressDetails = await ExecuteQueryAsync<Winit.Modules.Address.Model.Interfaces.IAddress>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
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

            var sql = @"SELECT 
                id AS Id, uid AS Uid, created_by AS CreatedBy, created_time AS CreatedTime,
                modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime, type AS Type, name AS Name, line1 AS Line1,
                line2 AS Line2, line3 AS Line3, landmark AS Landmark, area AS Area, sub_area AS SubArea,
                zip_code AS ZipCode, city AS City, country_code AS CountryCode, region_code AS RegionCode,
                phone AS Phone, phone_extension AS PhoneExtension, mobile1 AS Mobile1, mobile2 AS Mobile2,
                email AS Email, fax AS Fax, latitude AS Latitude, longitude AS Longitude, altitude AS Altitude,
                linked_item_uid AS LinkedItemUid, linked_item_type AS LinkedItemType, status AS Status,
                state_code AS StateCode, territory_code AS TerritoryCode, pan AS Pan, aadhar AS Aadhar,
                ssn AS Ssn, is_editable AS IsEditable, is_default AS IsDefault, line4 AS Line4, info AS Info,
                depot AS Depot 
            FROM 
                address WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAddress>().GetType();
            Winit.Modules.Address.Model.Interfaces.IAddress AddressDetails = await ExecuteSingleAsync
                <Winit.Modules.Address.Model.Interfaces.IAddress>(sql, parameters, type);
            return AddressDetails;
        }
        public async Task<int> CreateAddressDetails(Winit.Modules.Address.Model.Interfaces.IAddress createAddress)
        {
            var sql = @"INSERT INTO address(id,uid,created_by,created_time,modified_by,modified_time,server_add_time,
            server_modified_time,type,name,line1,line2,line3,landmark,area,sub_area,zip_code,city,
            country_code,region_code,phone,phone_extension,mobile1,mobile2,email,fax,latitude,longitude,
            altitude,linked_item_uid,linked_item_type,status,state_code,territory_code,pan,aadhar,ssn,is_editable,is_default,
            line4,info, ss) VALUES(@Id,@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@Type,@Name,@Line1,@Line2,@Line3,
              @Landmark,@Area,@SubArea,@ZipCode,@City,@CountryCode,@RegionCode,@Phone,@PhoneExtension,@Mobile1,@Mobile2,@Email,@Fax,@Latitude,@Longitude,
              @Altitude,@LinkedItemUID,@LinkedItemType,@Status,@StateCode,@TerritoryCode,@PAN,@AADHAR,@SSN,@IsEditable,@IsDefault, @Line4, @Info, 1);";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"Id", createAddress.Id},
                {"UID", createAddress.UID},
                {"CreatedBy", createAddress.CreatedBy},
                {"CreatedTime", createAddress.CreatedTime},
                {"ModifiedBy", createAddress.ModifiedBy},
                {"ModifiedTime", createAddress.ModifiedTime},
                {"ServerAddTime", createAddress.ServerAddTime},
                {"ServerModifiedTime", createAddress.ServerModifiedTime},
                {"Type", createAddress.Type},
                {"Name", createAddress.Name},
                {"Line1", createAddress.Line1},
                {"Line2", createAddress.Line2},
                {"Line3", createAddress.Line3},
                {"Landmark", createAddress.Landmark},
                {"Area", createAddress.Area},
                {"SubArea", createAddress.SubArea},
                {"ZipCode", createAddress.ZipCode},
                {"City", createAddress.City},
                {"CountryCode", createAddress.CountryCode},
                {"RegionCode", createAddress.RegionCode},
                {"Phone", createAddress.Phone},
                {"PhoneExtension", createAddress.PhoneExtension},
                {"Mobile1", createAddress.Mobile1},
                {"Mobile2", createAddress.Mobile2},
                {"Email", createAddress.Email},
                {"Fax", createAddress.Fax},
                {"Latitude", createAddress.Latitude},
                {"Longitude", createAddress.Longitude},
                {"Altitude", createAddress.Altitude},
                {"LinkedItemUID", createAddress.LinkedItemUID},
                {"LinkedItemType", createAddress.LinkedItemType},
                {"Status", createAddress.Status},
                {"StateCode", createAddress.StateCode},
                {"TerritoryCode", createAddress.TerritoryCode},
                {"PAN", createAddress.PAN},
                {"AADHAR", createAddress.AADHAR},
                {"SSN", createAddress.SSN},
                {"IsEditable", createAddress.IsEditable},
                {"IsDefault", createAddress.IsDefault},
                {"Info", createAddress.Info},
                {"Line4", createAddress.Line4}
            };
            return await ExecuteNonQueryAsync(sql, parameters);

        }

        public async Task<int> UpdateAddressDetails(Winit.Modules.Address.Model.Interfaces.IAddress updateAddress)
        {
            try
            {
                var sql = @"UPDATE address SET
                    created_by = @CreatedBy,
                    modified_by = @ModifiedBy,
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime,
                    type = @Type,
                    name = @Name,
                    line1 = @Line1,
                    line2 = @Line2,
                    line3 = @Line3,
                    landmark = @Landmark,
                    area = @Area,
                    sub_area = @SubArea,
                    zip_code = @ZipCode,
                    city = @City,
                    country_code = @CountryCode,
                    region_code = @RegionCode,
                    phone = @Phone,
                    phone_extension = @PhoneExtension,
                    mobile1 = @Mobile1,
                    mobile2 = @Mobile2,
                    email = @Email,
                    fax = @Fax,
                    latitude = @Latitude,
                    longitude = @Longitude,
                    altitude = @Altitude,
                    linked_item_uid = @LinkedItemUID,
                    linked_item_type = @LinkedItemType,
                    status = @Status,
                    state_code = @StateCode,
                    territory_code = @TerritoryCode,
                    pan = @PAN,
                    aadhar = @AADHAR,
                    ssn = @SSN,
                    is_editable = @IsEditable,
                    is_default = @IsDefault,
                    info = @Info,
                    line4 = @Line4,
                    ss = 2
                WHERE
                    uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID", updateAddress.UID},
                    {"CreatedBy", updateAddress.CreatedBy },
                    {"ModifiedBy", updateAddress.ModifiedBy},
                    {"ModifiedTime", updateAddress.ModifiedTime},
                    {"ServerModifiedTime", updateAddress.ServerModifiedTime},
                    {"Type", updateAddress.Type},
                    {"Name", updateAddress.Name},
                    {"Line1", updateAddress.Line1},
                    {"Line2", updateAddress.Line2},
                    {"Line3", updateAddress.Line3},
                    {"Landmark", updateAddress.Landmark},
                    {"Area", updateAddress.Area},
                    {"SubArea", updateAddress.SubArea},
                    {"ZipCode", updateAddress.ZipCode},
                    {"City", updateAddress.City},
                    {"CountryCode", updateAddress.CountryCode},
                    {"RegionCode", updateAddress.RegionCode},
                    {"Phone", updateAddress.Phone},
                    {"PhoneExtension", updateAddress.PhoneExtension},
                    {"Mobile1", updateAddress.Mobile1},
                    {"Mobile2", updateAddress.Mobile2},
                    {"Email",updateAddress.Email},
                    {"Fax", updateAddress.Fax},
                    {"Latitude", updateAddress.Latitude},
                    {"Longitude", updateAddress.Longitude},
                    {"Altitude", updateAddress.Altitude},
                    {"LinkedItemUID", updateAddress.LinkedItemUID},
                    {"LinkedItemType", updateAddress.LinkedItemType},
                    {"Status", updateAddress.Status},
                    {"StateCode", updateAddress.StateCode},
                    {"TerritoryCode", updateAddress.TerritoryCode},
                    {"PAN", updateAddress.PAN},
                    {"AADHAR", updateAddress.AADHAR},
                    {"SSN", updateAddress.SSN},
                    {"IsEditable", updateAddress.IsEditable},
                    {"IsDefault", updateAddress.IsDefault},
                    {"Info", updateAddress.Info},
                    {"Line4", updateAddress.Line4}
                 };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateAddressDetails(string addressUID, string latitude, string longitude)
        {
            try
            {
                var sql = @"UPDATE address set   
                    latitude = @Latitude,
                    longitude = @Longitude,
                    modified_time = @ModifiedTime,
                    ss = 2
                    WHERE
                    uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID", addressUID},
                    {"Latitude", latitude},
                    {"Longitude", longitude},
                    {"ModifiedTime", DateTime.Now},
                 };
                return await ExecuteNonQueryAsync(sql, parameters);
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
            var sql = @"DELETE  FROM address WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        Task<int> IAddressDL.CreateAddressDetailsList(List<IAddress> createAddress)
        {
            throw new NotImplementedException();
        }
    }
}
