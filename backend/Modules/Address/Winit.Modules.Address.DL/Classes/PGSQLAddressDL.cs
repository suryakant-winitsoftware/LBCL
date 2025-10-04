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
    public class PGSQLAddressDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IAddressDL
    {
        public PGSQLAddressDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Address.Model.Interfaces.IAddress>> SelectAllAddressDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from (SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,
                 modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,type AS Type,name AS Name,
                line1 AS Line1,
                line2 AS Line2,line3 AS Line3,landmark AS Landmark,area AS Area,sub_area AS SubArea,zip_code AS ZipCode,city AS City,
                country_code AS CountryCode,region_code AS RegionCode,phone AS Phone,phone_extension AS PhoneExtension,mobile1 AS Mobile1,mobile2 AS Mobile2,
                email AS Email,fax AS Fax,latitude AS Latitude,longitude AS Longitude,altitude AS Altitude,linked_item_uid AS LinkedItemUID,
                linked_item_type AS LinkedItemType,status AS Status,state_code AS StateCode,territory_code AS TerritoryCode,pan AS PAN,
                aadhar AS AADHAR,ssn AS SSN,is_editable AS IsEditable,is_default AS IsDefault,line4 AS Line4,info AS Info,depot AS Depot ,location_uid as LocationUID
                FROM address) as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,
                 modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,type AS Type,name AS Name,
                line1 AS Line1,
                line2 AS Line2,line3 AS Line3,landmark AS Landmark,area AS Area,sub_area AS SubArea,zip_code AS ZipCode,city AS City,
                country_code AS CountryCode,region_code AS RegionCode,phone AS Phone,phone_extension AS PhoneExtension,mobile1 AS Mobile1,mobile2 AS Mobile2,
                email AS Email,fax AS Fax,latitude AS Latitude,longitude AS Longitude,altitude AS Altitude,linked_item_uid AS LinkedItemUID,
                linked_item_type AS LinkedItemType,status AS Status,state_code AS StateCode,territory_code AS TerritoryCode,pan AS PAN,
                aadhar AS AADHAR,ssn AS SSN,is_editable AS IsEditable,is_default AS IsDefault,line4 AS Line4,info AS Info,depot AS Depot,location_uid as LocationUID 
                FROM address) as SubQuery");
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
                    AppendSortCriteria(sortCriterias, sql, true);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
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

            var sql = @"SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,
                 modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,type AS Type,name AS Name,
                line1 AS Line1,
                line2 AS Line2,line3 AS Line3,landmark AS Landmark,area AS Area,sub_area AS SubArea,zip_code AS ZipCode,city AS City,
                country_code AS CountryCode,region_code AS RegionCode,phone AS Phone,phone_extension AS PhoneExtension,mobile1 AS Mobile1,mobile2 AS Mobile2,
                email AS Email,fax AS Fax,latitude AS Latitude,longitude AS Longitude,altitude AS Altitude,linked_item_uid AS LinkedItemUID,
                linked_item_type AS LinkedItemType,status AS Status,state_code AS StateCode,territory_code AS TerritoryCode,pan AS PAN,
                aadhar AS AADHAR,ssn AS SSN,is_editable AS IsEditable,is_default AS IsDefault,line4 AS Line4,info AS Info,depot AS Depot ,location_uid as LocationUID
                FROM address WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAddress>().GetType();
            Winit.Modules.Address.Model.Interfaces.IAddress AddressDetails = await ExecuteSingleAsync
                <Winit.Modules.Address.Model.Interfaces.IAddress>(sql, parameters, type);
            return AddressDetails;
        }
        public async Task<int> CreateAddressDetails(Winit.Modules.Address.Model.Interfaces.IAddress createAddress)
        {
            var sql = @"INSERT INTO address(
    uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, type, name, 
    line1, line2, line3, landmark, area, sub_area, zip_code, city, country_code, region_code, phone, phone_extension, mobile1, mobile2, email, fax, latitude, 
    longitude, altitude, linked_item_uid, linked_item_type, status, state_code, territory_code, pan, aadhar, ssn, is_editable, is_default, line4, info, depot, 
    location_uid, custom_field1, custom_field2, custom_field3, custom_field4, custom_field5
) VALUES(
    @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @Type, @Name, 
    @Line1, @Line2, @Line3, @Landmark, @Area, @SubArea, @ZipCode, @City, @CountryCode, @RegionCode, @Phone, @PhoneExtension, @Mobile1, @Mobile2, @Email, @Fax, @Latitude, 
    @Longitude, @Altitude, @LinkedItemUID, @LinkedItemType, @Status, @StateCode, @TerritoryCode, @PAN, @AADHAR, @SSN, @IsEditable, @IsDefault, @Line4, @Info, @Depot, 
    @LocationUID, @HusbandName, @FatherName, @GSTNo, @MuncipalRegNo, @PFRegNo
);";

            Dictionary<string, object> parameters = new Dictionary<string, object>
{
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
    {"Line4", createAddress.Line4},
    {"Depot", createAddress.Depot},
    {"LocationUID", createAddress.LocationUID},
    {"HusbandName", createAddress.HusbandName},
    {"FatherName", createAddress.FatherName},
    {"GSTNo", createAddress.GSTNo},
    {"MuncipalRegNo", createAddress.MuncipalRegNo},
    {"ESICRegNo", createAddress.ESICRegNo},
    {"PFRegNo", createAddress.PFRegNo},
            };
            return await ExecuteNonQueryAsync(sql, parameters);

        }

        public async Task<int> UpdateAddressDetails(Winit.Modules.Address.Model.Interfaces.IAddress updateAddress)
        {
            try
            {
                var sql = @"UPDATE address SET
    created_by = @created_by,
    modified_by = @modified_by,
    modified_time = @modified_time,
    server_modified_time = @server_modified_time,
    type = @type,
    name = @name,
    line1 = @line1,
    line2 = @line2,
    line3 = @line3,
    landmark = @landmark,
    area = @area,
    sub_area = @sub_area,
    zip_code = @zip_code,
    city = @city,
    country_code = @country_code,
    region_code = @region_code,
    phone = @phone,
    phone_extension = @phone_extension,
    mobile1 = @mobile1,
    mobile2 = @mobile2,
    email = @email,
    fax = @fax,
    latitude = @latitude,
    longitude = @longitude,
    altitude = @altitude,
    linked_item_uid = @linked_item_uid,
    linked_item_type = @linked_item_type,
    status = @status,
    state_code = @state_code,
    territory_code = @territory_code,
    pan = @pan,
    aadhar = @aadhar,
    ssn = @ssn,
    is_editable = @is_editable,
    is_default = @is_default,
    info = @info,
    location_uid = @LocationUID,
    line4 = @line4,
    depot = @depot,
    custom_field1 = @HusbandName,
    custom_field2 = @FatherName,
    custom_field3 = @GSTNo,
    custom_field4 = @MuncipalRegNo,
    custom_field5 = @PFRegNo
WHERE uid = @uid;";

                Dictionary<string, object> parameters = new Dictionary<string, object>
{
    {"uid", updateAddress.UID},
    {"created_by", updateAddress.CreatedBy},
    {"modified_by", updateAddress.ModifiedBy},
    {"modified_time", updateAddress.ModifiedTime},
    {"server_modified_time", updateAddress.ServerModifiedTime},
    {"type", updateAddress.Type},
    {"name", updateAddress.Name},
    {"line1", updateAddress.Line1},
    {"line2", updateAddress.Line2},
    {"line3", updateAddress.Line3},
    {"landmark", updateAddress.Landmark},
    {"area", updateAddress.Area},
    {"sub_area", updateAddress.SubArea},
    {"zip_code", updateAddress.ZipCode},
    {"city", updateAddress.City},
    {"country_code", updateAddress.CountryCode},
    {"region_code", updateAddress.RegionCode},
    {"phone", updateAddress.Phone},
    {"phone_extension", updateAddress.PhoneExtension},
    {"mobile1", updateAddress.Mobile1},
    {"mobile2", updateAddress.Mobile2},
    {"email", updateAddress.Email},
    {"fax", updateAddress.Fax},
    {"latitude", updateAddress.Latitude},
    {"longitude", updateAddress.Longitude},
    {"altitude", updateAddress.Altitude},
    {"linked_item_uid", updateAddress.LinkedItemUID},
    {"linked_item_type", updateAddress.LinkedItemType},
    {"status", updateAddress.Status},
    {"state_code", updateAddress.StateCode},
    {"territory_code", updateAddress.TerritoryCode},
    {"pan", updateAddress.PAN},
    {"aadhar", updateAddress.AADHAR},
    {"ssn", updateAddress.SSN},
    {"is_editable", updateAddress.IsEditable},
    {"is_default", updateAddress.IsDefault},
    {"info", updateAddress.Info},
    {"line4", updateAddress.Line4},
    {"depot", updateAddress.Depot},
    {"LocationUID", updateAddress.LocationUID},
    {"HusbandName", updateAddress.HusbandName},
    {"FatherName", updateAddress.FatherName},
    {"GSTNo", updateAddress.GSTNo},
    {"MuncipalRegNo", updateAddress.MuncipalRegNo},
    {"PFRegNo", updateAddress.PFRegNo},
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
            var sql = @"delete from address where uid= @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateAddressDetails(string addressUID, string latitude, string longitude)
        {
            throw new NotImplementedException();
        }

        Task<int> IAddressDL.CreateAddressDetailsList(List<IAddress> createAddress)
        {
            throw new NotImplementedException();
        }
    }
}
