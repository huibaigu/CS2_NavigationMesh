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
using static NavigationMesh.Other.Config;

namespace NavigationMesh;
public class NavigationMesh(ILogger<NavigationMesh> logger) : BasePlugin
{
    public override string ModuleName => "NavigationMesh";
    public override string ModuleVersion => "0.0.4";
    public override string ModuleAuthor => "Wangsir";
    public override string ModuleDescription => "Navigation Mesh in cs2";
    public static PluginCapability<INavigationMeshAPI> m_APICapability{get;} = new("NavigationMesh:core");
    public static string m_ConfigPath = Path.Combine(Application.RootDirectory, "configs/plugins/NavigationMesh/");
    private readonly ILogger<NavigationMesh> _logger = logger;
    public INavigationMeshAPI m_API{ get; set; }=new NavigationMeshInterface();
    public override void Load(bool hotReload)
    {
        AddCommand("css_nm_add", "将脚下点加入网格", addPoint);
        AddCommand("css_nm_delete", "删除离你最近的点", deletePoint);
        AddCommand("css_nm_debug", "生成最近一个点的网格", debug);
        AddCommand("css_nm_look", "查看所有点", look);
        AddCommand("css_nm_clear", "清除所有激光", clear);
        var configPath = Path.Combine(m_ConfigPath, "Point.jsonc");
        if(!File.Exists(configPath))
        {
            _logger.LogCritical("[NavigationMesh] Couldn't find a Config file!");
            return;
        }
        _logger.LogInformation("[NavigationMesh] Load Config file.");
        m_NavigationMeshConfig = JsonConvert.DeserializeObject<Dictionary<string, MapAttribute>>(File.ReadAllText(configPath),new NVConvert())??new Dictionary<string, MapAttribute>();
        RegisterListener<OnMapStart>(OnMapStart);
        m_API=new NavigationMeshInterface();
        Capabilities.RegisterPluginCapability(m_APICapability, () => m_API);
    }
    
    public void OnMapStart(string mapname)
    {
        m_name=mapname;
        if(!m_NavigationMeshConfig.ContainsKey(m_name))m_NavigationMeshConfig.Add(mapname, new MapAttribute());
        m_NavigationMeshConfig[m_name].getEdge();
    }
    //[RequiresPermissions("@css/nm_admin")]
    public void addPoint(CCSPlayerController? client, CommandInfo info)
	{
        if(!m_NavigationMeshConfig.ContainsKey(m_name)||client == null)
        {
            _logger.LogCritical("ERROR!");
            return;
        }
        Server.PrintToChatAll("[NavigationMesh]add a point");
        m_NavigationMeshConfig[m_name].add(new Vector(client?.PlayerPawn.Value!.AbsOrigin!.X,client?.PlayerPawn.Value!.AbsOrigin!.Y,client?.PlayerPawn.Value!.AbsOrigin!.Z+10));

        var configPath = Path.Combine(m_ConfigPath, "Point.jsonc");
        File.WriteAllText(configPath, JsonConvert.SerializeObject(m_NavigationMeshConfig,Formatting.Indented,new NVConvert()));
        m_NavigationMeshConfig[m_name].getEdge();
        debug(client,info);
    }
    //[RequiresPermissions("@css/nm_admin")]
    public void deletePoint(CCSPlayerController? client, CommandInfo info)
	{
        if(!m_NavigationMeshConfig.ContainsKey(m_name)||client == null)
        {
            _logger.LogCritical("ERROR!");
            return;
        }
        Server.PrintToChatAll("[NavigationMesh]delete a point");
        m_NavigationMeshConfig[m_name].delete(m_API.getEntityid(client?.PlayerPawn.Value!.AbsOrigin!));
        var configPath = Path.Combine(m_ConfigPath, "Point.jsonc");
        File.WriteAllText(configPath, JsonConvert.SerializeObject(m_NavigationMeshConfig,Formatting.Indented,new NVConvert()));
        m_NavigationMeshConfig[m_name].getEdge();
        debug(client,info);
    }
    //[RequiresPermissions("@css/nm_admin")]
    public void debug(CCSPlayerController? client, CommandInfo info)
	{
        if(!m_NavigationMeshConfig.ContainsKey(m_name)||client == null)
        {
            _logger.LogCritical("ERROR!");
            return;
        }
        clear(client,info);
        int id=m_API.getEntityid(client?.PlayerPawn.Value!.AbsOrigin!);
        creatTo(m_NavigationMeshConfig[m_name].m_points[id].m_point,id);
        foreach(var edge in m_Graph.Edges)
        {
            if(edge.Source!=id&&edge.Target!=id)continue;
            if(edge.Source==id)
            {
                creatFrom(m_NavigationMeshConfig[m_name].m_points[edge.Target].m_point,edge.Target,id);

            }
            else if (edge.Target==id)
            {
                creatFrom(m_NavigationMeshConfig[m_name].m_points[edge.Source].m_point,edge.Source,id);
            }
        }
    }
    //[RequiresPermissions("@css/nm_admin")]
    private void look(CCSPlayerController? client, CommandInfo info)
    {
        if(!m_NavigationMeshConfig.ContainsKey(m_name))
        {
            _logger.LogCritical("ERROR!");
            return;
        }
        clear(client,info);
        for(int i = 0;i<Config.m_Graph.VertexCount;i++)
        {
            creatTo(m_NavigationMeshConfig[m_name].m_points[i].m_point,i);
            creatFrom(new Vector(m_NavigationMeshConfig[m_name].m_points[i].m_point.X,m_NavigationMeshConfig[m_name].m_points[i].m_point.Y,m_NavigationMeshConfig[m_name].m_points[i].m_point.Z-10),i,i);
        }
    }
    //[RequiresPermissions("@css/nm_admin")]
    private void clear(CCSPlayerController? client, CommandInfo info)
    {
        if(!m_NavigationMeshConfig.ContainsKey(m_name))
        {
            _logger.LogCritical("ERROR!");
            return;
        }
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
        target.Teleport(new Vector(location.X,location.Y,location.Z));
        target.DispatchSpawn();
    }
    private void creatFrom(Vector location,int id,int target)
    {
        var entity = Utilities.CreateEntityByName<CEnvLaser>("env_laser");
        if (entity == null || !entity.IsValid)return;
        entity.Entity!.Name="NM_"+id;
        entity.Render=Color.Red;
        entity.Spawnflags=1;
        entity.Teleport(new Vector(location.X,location.Y,location.Z));
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
