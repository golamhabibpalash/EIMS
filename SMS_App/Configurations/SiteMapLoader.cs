using SMS_App.ViewModels.ModuleSubModuleVM;
using System.Linq;
using System.Xml.Linq;

namespace SMS_App.Configurations;

public static class SiteMapLoader
{
    public static SiteMap Load(string filePath)
    {
        var xdoc = XDocument.Load(filePath);

        var siteMap = new SiteMap
        {
            Modules = xdoc.Descendants("module").Select(m => new Module
            {
                SystemName = m.Attribute("systemName")?.Value,
                DisplayName = m.Attribute("displayName")?.Value,
                Permission = m.Attribute("Permission")?.Value,
                Icon = m.Attribute("Icon")?.Value,
                Submodules = m.Descendants("submodule").Select(s => new Submodule
                {
                    SystemName = s.Attribute("systemName")?.Value,
                    DisplayName = s.Attribute("displayName")?.Value,
                    Icon = s.Attribute("Icon")?.Value,
                    Items = s.Descendants("item").Select(i => new Item
                    {
                        SystemName = i.Attribute("systemName")?.Value,
                        DisplayName = i.Attribute("displayName")?.Value,
                        Controller = i.Attribute("Controller")?.Value,
                        Action = i.Attribute("Action")?.Value,
                        Claim = i.Attribute("Claim")?.Value
                    }).ToList()
                }).ToList()
            }).ToList()
        };

        return siteMap;
    }
}
