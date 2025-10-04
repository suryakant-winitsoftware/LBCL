using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class SchemeOrgBL : ISchemeOrgBL
    {
        ISchemeOrgDL _schemeOrgDL { get; }
        public SchemeOrgBL(ISchemeOrgDL schemeOrgDL)
        {
            _schemeOrgDL = schemeOrgDL;
        }


        public async Task<List<ISchemeOrg>> GetSchemeOrgByLinkedItemUID(string linkedItemUID)
        {
            return await _schemeOrgDL.GetSchemeOrgByLinkedItemUID(linkedItemUID);
        }
        public async Task<int> CreateSchemeOrgs(List<ISchemeOrg> schemeOrgs)
        {

            return await _schemeOrgDL.CreateSchemeOrgs(schemeOrgs);

        }
        public async Task<int> CDOrgs(List<ISchemeOrg> schemeOrgs, string linkedItemUID)
        {
            int cnt = 0;
            var exstOrgs = await _schemeOrgDL.GetSchemeOrgByLinkedItemUID(linkedItemUID);
            if (exstOrgs != null && exstOrgs.Count > 0)
            {
                List<ISchemeOrg> insertList = [];
                List<string> deleteList = new List<string>();


                if (schemeOrgs != null && schemeOrgs.Count > 0)
                {
                    schemeOrgs.ForEach(p =>
                    {
                        if (!exstOrgs.Any(q => q.LinkedItemUID.Equals(p.LinkedItemUID) && q.OrgUID.Equals(p.OrgUID)))
                        {
                            insertList.Add(p);
                        }
                    });
                    exstOrgs.ForEach(p =>
                    {
                        if (!schemeOrgs.Any(q => q.LinkedItemUID.Equals(p.LinkedItemUID) && q.OrgUID.Equals(p.OrgUID)))
                        {
                            deleteList.Add(p.UID);
                        }
                    });



                    //insertList.AddRange(sellInDTO.SchemeOrgs.Except(exstOrgs));

                    if (insertList.Count > 0)
                        cnt += await _schemeOrgDL.CreateSchemeOrgs(insertList);
                    if (deleteList.Count > 0)
                        cnt += await _schemeOrgDL.DeleteSchemeOrgs(deleteList);
                }
                else
                {
                    deleteList.AddRange(exstOrgs.Select(p => p.UID));

                    if (deleteList.Count > 0)
                        cnt += await _schemeOrgDL.DeleteSchemeOrgs(deleteList);
                }

            }
            else if (schemeOrgs != null && schemeOrgs.Count > 0)
            {
                cnt += await _schemeOrgDL.CreateSchemeOrgs(schemeOrgs);
            }
            return cnt;
        }
    }
}
