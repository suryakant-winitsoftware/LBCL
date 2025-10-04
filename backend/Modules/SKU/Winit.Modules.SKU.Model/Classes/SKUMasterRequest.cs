using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SKU.Model.Classes;

public class SKUMasterRequest
{
    public List<string>? OrgUIDs { get; set; }
    public List<string>? DistributionChannelUIDs { get; set; }
    public List<string>? SKUUIDs { get; set; }
    public List<string>? AttributeTypes { get; set; }
}
