using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Contact.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Contact.DL.Classes
{
    public class MSSQLContactDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IContactDL
    {
        public MSSQLContactDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Contact.Model.Interfaces.IContact>> SelectAllContactDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from
                                                            (SELECT
                                                                id AS Id,
                                                                uid AS UID,
                                                                created_by AS CreatedBy,
                                                                created_time AS CreatedTime,
                                                                modified_by AS ModifiedBy,
                                                                modified_time AS ModifiedTime,
                                                                server_add_time AS ServerAddTime,
                                                                server_modified_time AS ServerModifiedTime,
                                                                title AS Title,
                                                                name AS Name,
                                                                phone AS Phone,
                                                                phone_extension AS PhoneExtension,
                                                                description AS Description,
                                                                designation AS Designation,
                                                                mobile AS Mobile,
                                                                email AS Email,
                                                                email2 AS Email2,
                                                                email3 AS Email3,
                                                                invoice_for_email1 AS InvoiceForEmail1,
                                                                invoice_for_email2 AS InvoiceForEmail2,
                                                                invoice_for_email3 AS InvoiceForEmail3,
                                                                fax AS Fax,
                                                                linked_item_uid AS LinkedItemUID,
                                                                linked_item_type AS LinkedItemType,
                                                                is_default AS IsDefault,
                                                                is_editable AS IsEditable,
                                                                enabled_for_invoice_email AS EnabledForInvoiceEmail,
                                                                enabled_for_docket_email AS EnabledForDocketEmail,
                                                                enabled_for_promo_email AS EnabledForPromoEmail,
                                                                is_email_cc AS IsEmailCC,
                                                                mobile2 AS Mobile2,
                                                                type as Type
                                                            FROM
                                                                contact) as subQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM(select id AS Id,
                                                                        uid AS UID,
                                                                        created_by AS CreatedBy,
                                                                        created_time AS CreatedTime,
                                                                        modified_by AS ModifiedBy,
                                                                        modified_time AS ModifiedTime,
                                                                        server_add_time AS ServerAddTime,
                                                                        server_modified_time AS ServerModifiedTime,
                                                                        title AS Title,
                                                                        name AS Name,
                                                                        phone AS Phone,
                                                                        phone_extension AS PhoneExtension,
                                                                        description AS Description,
                                                                        designation AS Designation,
                                                                        mobile AS Mobile,
                                                                        email AS Email,
                                                                        email2 AS Email2,
                                                                        email3 AS Email3,
                                                                        invoice_for_email1 AS InvoiceForEmail1,
                                                                        invoice_for_email2 AS InvoiceForEmail2,
                                                                        invoice_for_email3 AS InvoiceForEmail3,
                                                                        fax AS Fax,
                                                                        linked_item_uid AS LinkedItemUID,
                                                                        linked_item_type AS LinkedItemType,
                                                                        is_default AS IsDefault,
                                                                        is_editable AS IsEditable,
                                                                        enabled_for_invoice_email AS EnabledForInvoiceEmail,
                                                                        enabled_for_docket_email AS EnabledForDocketEmail,
                                                                        enabled_for_promo_email AS EnabledForPromoEmail,
                                                                        is_email_cc AS IsEmailCC,
                                                                        mobile2 AS Mobile2,type as Type from contact) as SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Contact.Model.Interfaces.IContact>(filterCriterias, sbFilterCriteria, parameters);
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
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }

                }
                IEnumerable<Winit.Modules.Contact.Model.Interfaces.IContact> contactDetails = await ExecuteQueryAsync<Winit.Modules.Contact.Model.Interfaces.IContact>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Contact.Model.Interfaces.IContact> pagedResponse = new PagedResponse<Winit.Modules.Contact.Model.Interfaces.IContact>
                {
                    PagedData = contactDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Contact.Model.Interfaces.IContact> GetContactDetailsByUID(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
                var sql = @"SELECT
                            id AS Id,
                            uid AS UID,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            title AS Title,
                            name AS Name,
                            phone AS Phone,
                            phone_extension AS PhoneExtension,
                            description AS Description,
                            designation AS Designation,
                            mobile AS Mobile,
                            email AS Email,
                            email2 AS Email2,
                            email3 AS Email3,
                            invoice_for_email1 AS InvoiceForEmail1,
                            invoice_for_email2 AS InvoiceForEmail2,
                            invoice_for_email3 AS InvoiceForEmail3,
                            fax AS Fax,
                            linked_item_uid AS LinkedItemUID,
                            linked_item_type AS LinkedItemType,
                            is_default AS IsDefault,
                            is_editable AS IsEditable,
                            enabled_for_invoice_email AS EnabledForInvoiceEmail,
                            enabled_for_docket_email AS EnabledForDocketEmail,
                            enabled_for_promo_email AS EnabledForPromoEmail,
                            is_email_cc AS IsEmailCC,
                            mobile2 AS Mobile2
                        FROM
                            contact
                         WHERE uid = @UID";
                Winit.Modules.Contact.Model.Interfaces.IContact? contact = await ExecuteSingleAsync<Winit.Modules.Contact.Model.Interfaces.IContact>(sql, parameters);
                return contact;
            }
            catch
            {
                throw;
            }

        }
        public async Task<int> CreateContactDetails(Winit.Modules.Contact.Model.Interfaces.IContact createContact)
        {
            try
            {
                var sql = @"
                    INSERT INTO contact (
                        uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time, title, name, phone, phone_extension, description, 
                        designation, mobile, email, email2, email3, invoice_for_email1, 
                        invoice_for_email2, invoice_for_email3, fax, linked_item_uid, 
                        linked_item_type, is_default, is_editable, enabled_for_invoice_email, 
                        enabled_for_docket_email, enabled_for_promo_email, is_email_cc, mobile2,type
                    ) 
                    VALUES (
                        @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                        @ServerModifiedTime, @Title, @Name, @Phone, @PhoneExtension, @Description, 
                        @Designation, @Mobile,  @Email, @Email2, @Email3, @InvoiceForEmail1, 
                        @InvoiceForEmail2, @InvoiceForEmail3, @Fax, @LinkedItemUID, 
                        @LinkedItemType, @IsDefault, @IsEditable, @EnabledForInvoiceEmail, 
                        @EnabledForDocketEmail, @EnabledForPromoEmail, @IsEmailCC, @Mobile2,@Type);";

                return await ExecuteNonQueryAsync(sql, createContact);
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> UpdateContactDetails(Winit.Modules.Contact.Model.Interfaces.IContact updateContact)
        {
            try
            {
                var sql = @"
                UPDATE contact SET
                    modified_by = @ModifiedBy,
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime,
                    title = @Title,
                    name = @Name,
                    phone = @Phone,
                    phone_extension = @PhoneExtension,
                    description = @Description,
                    designation = @Designation,
                    mobile = @Mobile,
                    email = @Email,
                    email2 = @Email2,
                    email3 = @Email3,
                    invoice_for_email1 = @InvoiceForEmail1,
                    invoice_for_email2 = @InvoiceForEmail2,
                    invoice_for_email3 = @InvoiceForEmail3,
                    fax = @Fax,
                    linked_item_uid = @LinkedItemUID,
                    linked_item_type = @LinkedItemType,
                    is_default = @IsDefault,
                    is_editable = @IsEditable,
                    enabled_for_invoice_email = @EnabledForInvoiceEmail,
                    enabled_for_docket_email = @EnabledForDocketEmail,
                    enabled_for_promo_email = @EnabledForPromoEmail,
                    is_email_cc = @IsEmailCC,
                    mobile2 = @Mobile2,
                    type=@Type
                WHERE uid = @UID;";
                return await ExecuteNonQueryAsync(sql, updateContact);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteContactDetails(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"DELETE  FROM contact WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task<PagedResponse<Winit.Modules.Contact.Model.Interfaces.IContact>> ShowAllContactDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * 
                                                        FROM (
                                                            SELECT 
                                                                id AS Id,
                                                                uid AS UID,
                                                                created_by AS CreatedBy,
                                                                created_time AS CreatedTime,
                                                                modified_by AS ModifiedBy,
                                                                modified_time AS ModifiedTime,
                                                                server_add_time AS ServerAddTime,
                                                                server_modified_time AS ServerModifiedTime,
                                                                title AS Title,
                                                                name AS Name,
                                                                phone AS Phone,
                                                                phone_extension AS PhoneExtension,
                                                                description AS Description,
                                                                designation AS Designation,
                                                                mobile AS Mobile,
                                                                email AS Email,
                                                                email2 AS Email2,
                                                                email3 AS Email3,
                                                                invoice_for_email1 AS InvoiceForEmail1,
                                                                invoice_for_email2 AS InvoiceForEmail2,
                                                                invoice_for_email3 AS InvoiceForEmail3,
                                                                fax AS Fax,
                                                                linked_item_uid AS LinkedItemUID,
                                                                linked_item_type AS LinkedItemType,
                                                                is_default AS IsDefault,
                                                                is_editable AS IsEditable,
                                                                enabled_for_invoice_email AS EnabledForInvoiceEmail,
                                                                enabled_for_docket_email AS EnabledForDocketEmail,
                                                                enabled_for_promo_email AS EnabledForPromoEmail,
                                                                is_email_cc AS IsEmailCC,
                                                                mobile2 AS Mobile2
                                                            FROM
                                                                contact 
                                                            WHERE 
                                                                linked_item_type IN ('Showroom', 'Office')
                                                        ) AS Subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT * 
                                                        FROM (
                                                            SELECT 
                                                                id AS Id,
                                                                uid AS UID,
                                                                created_by AS CreatedBy,
                                                                created_time AS CreatedTime,
                                                                modified_by AS ModifiedBy,
                                                                modified_time AS ModifiedTime,
                                                                server_add_time AS ServerAddTime,
                                                                server_modified_time AS ServerModifiedTime,
                                                                title AS Title,
                                                                name AS Name,
                                                                phone AS Phone,
                                                                phone_extension AS PhoneExtension,
                                                                description AS Description,
                                                                designation AS Designation,
                                                                mobile AS Mobile,
                                                                email AS Email,
                                                                email2 AS Email2,
                                                                email3 AS Email3,
                                                                invoice_for_email1 AS InvoiceForEmail1,
                                                                invoice_for_email2 AS InvoiceForEmail2,
                                                                invoice_for_email3 AS InvoiceForEmail3,
                                                                fax AS Fax,
                                                                linked_item_uid AS LinkedItemUID,
                                                                linked_item_type AS LinkedItemType,
                                                                is_default AS IsDefault,
                                                                is_editable AS IsEditable,
                                                                enabled_for_invoice_email AS EnabledForInvoiceEmail,
                                                                enabled_for_docket_email AS EnabledForDocketEmail,
                                                                enabled_for_promo_email AS EnabledForPromoEmail,
                                                                is_email_cc AS IsEmailCC,
                                                                mobile2 AS Mobile2
                                                            FROM
                                                                contact 
                                                            WHERE 
                                                                linked_item_type IN ('Showroom', 'Office')
                                                        ) AS Subquery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Contact.Model.Interfaces.IContact>(filterCriterias, sbFilterCriteria, parameters);
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
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }

                }
                IEnumerable<Winit.Modules.Contact.Model.Interfaces.IContact> contactDetails = await ExecuteQueryAsync<Winit.Modules.Contact.Model.Interfaces.IContact>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Contact.Model.Interfaces.IContact> pagedResponse = new PagedResponse<Winit.Modules.Contact.Model.Interfaces.IContact>
                {
                    PagedData = contactDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
