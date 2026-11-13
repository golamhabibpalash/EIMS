using System.Collections.Generic;

namespace SMS_App.ViewModels.ModuleSubModuleVM;

public class SiteMap
{
    public List<Module> Modules { get; set; } = new();
}

public class Module
{
    public string SystemName { get; set; }
    public string DisplayName { get; set; }
    public string Permission { get; set; }
    public string Icon { get; set; }
    public List<Submodule> Submodules { get; set; } = new();
}

public class Submodule
{
    public string SystemName { get; set; }
    public string DisplayName { get; set; }
    public string Icon { get; set; }
    public List<Item> Items { get; set; } = new();
}

public class Item
{
    public string SystemName { get; set; }
    public string DisplayName { get; set; }
    public string Controller { get; set; }
    public string Action { get; set; }
    public string Claim { get; set; }
}
