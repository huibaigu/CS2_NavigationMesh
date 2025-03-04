using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using NavigationMeshAPI;
using NavigationMesh.Other;
using Newtonsoft.Json;
using static CounterStrikeSharp.API.Core.Listeners;
using CounterStrikeSharp.API;
using System.Drawing;
namespace NavigationMesh;
public class NavigationMesh(ILogger<NavigationMesh> logger) : BasePlugin
{
    public override string ModuleName => "NavigationMesh";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "Wangsir";
    public override string ModuleDescription => "Navigation Mesh in cs2";
    public static PluginCapability<INavigationMeshAPI> APICapability{get;} = new("NavigationMesh:core");
    public static string ConfigPath = Path.Combine(Application.RootDirectory, "configs/NavigationMesh/");
    private readonly ILogger<NavigationMesh> _logger = logger;
    public Dictionary<string, MapAttribute> NavigationMeshConfig=new Dictionary<string, MapAttribute>();
    public MapAttributeSet thismap=new MapAttributeSet();
    public override void Load(bool hotReload)
    {
        AddCommand("css_nm_add", "Add boundary points to the current mesh", addPoint);
        AddCommand("css_nm_clear", "Clear queue", clearStack);
        AddCommand("css_nm_push", "Form a new mesh from the current queue", pushStack);
        AddCommand("css_nm_look", "Generate a route around the mesh", look);
        var configPath = Path.Combine(ConfigPath, "Point.jsonc");
        if(!File.Exists(configPath))
        {
            _logger.LogCritical("[Monster] Couldn't find a Config file!");
            return;
        }
        _logger.LogInformation("[Monster] Load Config file.");
        NavigationMeshConfig = JsonConvert.DeserializeObject<Dictionary<string, MapAttribute>>(File.ReadAllText(configPath))??new Dictionary<string, MapAttribute>();
        RegisterListener<OnMapStart>(OnMapStart);
        Capabilities.RegisterPluginCapability(APICapability, () => new NavigationMeshInterface());
    }
    public void OnMapStart(string mapname)
    {
        if(!NavigationMeshConfig.ContainsKey(mapname))NavigationMeshConfig.Add(mapname, new MapAttribute());
        NavigationMeshConfig[mapname].Name=mapname;
        thismap=new MapAttributeSet(NavigationMeshConfig[mapname]);
        Config.ROOMS=thismap.toVector();
        Config.GRIDS=thismap.getGRIDS();
    }
    //[RequiresPermissions("@css/nm_admin")]
    public void addPoint(CCSPlayerController? client, CommandInfo info)
	{
        Server.PrintToChatAll("[Monster]add one point");
        thismap.add(client?.PlayerPawn.Value!.AbsOrigin!);
        look(client,info);
    }
    //[RequiresPermissions("@css/nm_admin")]
    public void clearStack(CCSPlayerController? client, CommandInfo info)
	{
        Server.PrintToChatAll("[Monster]clear Stack");
        thismap.clear();
        look(client,info);
    }
    //[RequiresPermissions("@css/nm_admin")]
    public void pushStack(CCSPlayerController? client, CommandInfo info)
	{
        if(thismap==null)
        {
            _logger.LogCritical("ERROR!");
            return;
        }
        Server.PrintToChatAll("[Monster]push a Stack");
        var configPath = Path.Combine(ConfigPath, "Point.jsonc");
        thismap.push();
        NavigationMeshConfig[thismap.Name]=thismap.print();
        JsonConvert.DeserializeObject<Dictionary<string, MapAttribute>>(File.ReadAllText(configPath));
        File.WriteAllText(configPath, JsonConvert.SerializeObject(NavigationMeshConfig, Formatting.Indented));
        Config.ROOMS=thismap.toVector();
        Config.GRIDS=thismap.getGRIDS();
        look(client,info);
    }
    //[RequiresPermissions("@css/nm_admin")]
    public void look(CCSPlayerController? client, CommandInfo info)
	{
        clearAll();
        int flash=0;
        for(int i = 0;i<Config.ROOMS.Count;i++)
        {
            int startflash=flash;
            for(int j = 0;j<Config.ROOMS[i].Count;j++)
            {
                creatOne(Config.ROOMS[i][j],flash);
                if(j>0)creatLine(flash-1,flash);
                flash++;
            }
            creatLine(flash-1,startflash);
        }
        var ii=0;
        foreach(var pt in thismap.printcell())
        {
            creatOne(pt,flash);
            if(ii>0)creatLine(flash-1,flash);
            flash++;
            ii++;
        }
    }
    private void clearAll()
    {
        var entities=Utilities.FindAllEntitiesByDesignerName<CEnvLaser>("env_laser");
        foreach (var i in entities)
        {
            if (i == null || !i.IsValid)continue;
            if(i.Entity!.Name.StartsWith("NM_"))
            {
                i.Remove();
            }
        }
        var entities2 = Utilities.FindAllEntitiesByDesignerName<CInfoTarget>("info_target");
        foreach (var i in entities2)
        {
            if (i == null || !i.IsValid)continue;
            if(i.Entity!.Name.StartsWith("NM_"))
            {
                i.Remove();
            }
        }
    }
    private void creatOne(Vector location,int id)
    {
        var entity = Utilities.CreateEntityByName<CEnvLaser>("env_laser");
        if (entity == null || !entity.IsValid)return;
        entity.Entity!.Name="NM_"+id;
        entity.Render=Color.Red;
        entity.Spawnflags=1;
        entity.Teleport(new Vector(location.X,location.Y,location.Z+10));
        entity.DispatchSpawn();
    }
    private void creatLine(int from,int to)
    {
        var entities=Utilities.FindAllEntitiesByDesignerName<CEnvLaser>("env_laser");
        foreach (var i in entities)
        {
            if (i == null || !i.IsValid)continue;
            if(i.Entity!.Name!="NM_"+from)continue;
            foreach (var j in entities)
            {
                if (j == null || !j.IsValid)continue;
                if(j.Entity!.Name!="NM_"+to)continue;
                var target = Utilities.CreateEntityByName<CInfoTarget>("info_target");
                if (target == null || !target.IsValid)continue;
                target.Entity!.Name=i.Entity.Name+"TG";
                i.LaserTarget=target.Entity.Name;
                i.Target=target.Entity.Name;
                i.SpriteName=target.Entity.Name;
                target.Teleport(j.AbsOrigin);
                target.DispatchSpawn();
                i.AcceptInput("Width",null,null,"3");
                i.AcceptInput("Noise",null,null,"3");
                i.AcceptInput("Alpha",null,null,"255");
                i.AcceptInput("TurnOn");
            }
        }
    }
}
