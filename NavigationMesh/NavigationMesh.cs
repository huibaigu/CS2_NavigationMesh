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
    public override string ModuleVersion => "0.0.2";
    public override string ModuleAuthor => "Wangsir";
    public override string ModuleDescription => "Navigation Mesh in cs2";
    public static PluginCapability<INavigationMeshAPI> APICapability{get;} = new("NavigationMesh:core");
    public static string ConfigPath = Path.Combine(Application.RootDirectory, "configs/NavigationMesh/");
    private readonly ILogger<NavigationMesh> _logger = logger;
    public Dictionary<string, MapAttribute> NavigationMeshConfig=new Dictionary<string, MapAttribute>();
    public MapAttributeSet thismap=new MapAttributeSet();
    public INavigationMeshAPI? NM_API{ get; set; }
    public override void Load(bool hotReload)
    {
        AddCommand("css_nm_add", "Add boundary points to the current mesh", addPoint);
        AddCommand("css_nm_delete", "Delete the previous point", deleteStack);
        AddCommand("css_nm_debug", "Generate a route around the mesh", debug);
        AddCommand("css_nm_clear", "clear a route", clear);
        
        AddCommand("css_nm_set1", "set point 1", set1);
        AddCommand("css_nm_set2", "Generate a route from 1 to 2", set2);
        var configPath = Path.Combine(ConfigPath, "Point.jsonc");
        if(!File.Exists(configPath))
        {
            _logger.LogCritical("[NavigationMesh] Couldn't find a Config file!");
            return;
        }
        _logger.LogInformation("[NavigationMesh] Load Config file.");
        NavigationMeshConfig = JsonConvert.DeserializeObject<Dictionary<string, MapAttribute>>(File.ReadAllText(configPath))??new Dictionary<string, MapAttribute>();
        RegisterListener<OnMapStart>(OnMapStart);
        Capabilities.RegisterPluginCapability(APICapability, () => new NavigationMeshInterface());

        NM_API = APICapability.Get()!;
    }
    Vector pointA;
    //[RequiresPermissions("@css/nm_admin")]
    private void set2(CCSPlayerController? player, CommandInfo commandInfo)
    {
        clear(player,commandInfo);
        var list=NM_API.GetPoint1ToPoint2List(pointA,player?.PlayerPawn.Value!.AbsOrigin!)??[];
        for(int i=0;i<list.Length;i++)
        {
            creatTo(list[i],i);
            if(i!=0)creatFrom(list[i-1],i-1,i);
        }
    }
    //[RequiresPermissions("@css/nm_admin")]
    private void set1(CCSPlayerController? player, CommandInfo commandInfo)
    {

        pointA=new Vector(player?.PlayerPawn.Value!.AbsOrigin!.X,player?.PlayerPawn.Value!.AbsOrigin!.Y,player?.PlayerPawn.Value!.AbsOrigin!.Z);
    }

    public void OnMapStart(string mapname)
    {
        if(!NavigationMeshConfig.ContainsKey(mapname))NavigationMeshConfig.Add(mapname, new MapAttribute());
        NavigationMeshConfig[mapname].Name=mapname;
        thismap=new MapAttributeSet(NavigationMeshConfig[mapname]);
        Config.ROOMS=thismap.toVector();
        thismap.getEDGE();
    }
    //[RequiresPermissions("@css/nm_admin")]
    public void addPoint(CCSPlayerController? client, CommandInfo info)
	{
        if(thismap==null)
        {
            _logger.LogCritical("ERROR!");
            return;
        }
        Server.PrintToChatAll("[NavigationMesh]add a point");
        thismap.add(client?.PlayerPawn.Value!.AbsOrigin!);

        var configPath = Path.Combine(ConfigPath, "Point.jsonc");
        NavigationMeshConfig[thismap.Name]=thismap.print();
        JsonConvert.DeserializeObject<Dictionary<string, MapAttribute>>(File.ReadAllText(configPath));
        File.WriteAllText(configPath, JsonConvert.SerializeObject(NavigationMeshConfig, Formatting.Indented));
        Config.ROOMS=thismap.toVector();
        thismap.getEDGE();
        //debug(client,info);
    }
    //[RequiresPermissions("@css/nm_admin")]
    public void deleteStack(CCSPlayerController? client, CommandInfo info)
	{
        if(thismap==null)
        {
            _logger.LogCritical("ERROR!");
            return;
        }
        Server.PrintToChatAll("[NavigationMesh]delete a point");
        thismap.delete();

        var configPath = Path.Combine(ConfigPath, "Point.jsonc");
        NavigationMeshConfig[thismap.Name]=thismap.print();
        JsonConvert.DeserializeObject<Dictionary<string, MapAttribute>>(File.ReadAllText(configPath));
        File.WriteAllText(configPath, JsonConvert.SerializeObject(NavigationMeshConfig, Formatting.Indented));
        Config.ROOMS=thismap.toVector();
        thismap.getEDGE();
        //debug(client,info);
    }
    //[RequiresPermissions("@css/nm_admin")]
    public void debug(CCSPlayerController? client, CommandInfo info)
	{
        clearAll();
        for(int i = 0;i<Config.graph.VertexCount;i++)
        {
            creatTo(new Vector(Config.ROOMS[i].X,Config.ROOMS[i].Y,Config.ROOMS[i].Z+50),i);
            creatFrom(new Vector(Config.ROOMS[i].X,Config.ROOMS[i].Y,Config.ROOMS[i].Z+10),i,i);
        }
    }
    //[RequiresPermissions("@css/nm_admin")]
    private void clear(CCSPlayerController? client, CommandInfo info)
    {
        clearAll();
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
    private void creatTo(Vector location,int id)
    {
        var target = Utilities.CreateEntityByName<CInfoTarget>("info_target");
        if (target == null || !target.IsValid)return;
        target.Entity!.Name="NM_"+id+"TG";
        target.Teleport(new Vector(location.X,location.Y,location.Z+10));
        target.DispatchSpawn();
    }
    private void creatFrom(Vector location,int id,int target)
    {
        var entity = Utilities.CreateEntityByName<CEnvLaser>("env_laser");
        if (entity == null || !entity.IsValid)return;
        entity.Entity!.Name="NM_"+id;
        entity.Render=Color.Red;
        entity.Spawnflags=1;
        entity.Teleport(new Vector(location.X,location.Y,location.Z+10));
        entity.DispatchSpawn();
        entity.LaserTarget="NM_"+target+"TG";
        entity.Target="NM_"+target+"TG";
        entity.SpriteName="NM_"+target+"TG";
        entity.AcceptInput("Width",null,null,"1");
        entity.AcceptInput("Noise",null,null,"1");
        entity.AcceptInput("Alpha",null,null,"255");
        entity.AcceptInput("TurnOn");
    }
}
