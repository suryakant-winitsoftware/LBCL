using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CustomSKUField.Model.Classes;

namespace Winit.Modules.SKU.Model.Interfaces;

public interface ISKUMaster
{
    public Winit.Modules.SKU.Model.Interfaces.ISKU SKU { get; set; }
    public List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SKUAttributes { get; set; }
    public List<Winit.Modules.SKU.Model.Interfaces.ISKUUOM> SKUUOMs { get; set; }
    public List<string> ApplicableTaxUIDs { get; set; }
    public List<Winit.Modules.SKU.Model.Interfaces.ISKUConfig> SKUConfigs { get; set; }
    public List<Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUFields> CustomSKUFields { get; set; }
    public List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> FileSysList { get; set; }
    public List<CustomField> DbDataList { get; set; }
}
