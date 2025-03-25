using CounterStrikeSharp.API.Modules.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuikGraph;

namespace NavigationMesh.Other;
public class NVConvert : JsonConverter<Dictionary<string, MapAttribute>>
{
    public override void WriteJson(JsonWriter writer, Dictionary<string, MapAttribute> value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        foreach(var mp in value)
        {
            writer.WritePropertyName(mp.Key);
            writer.WriteStartObject();
            writer.WritePropertyName("Name");
            writer.WriteValue(mp.Value.m_Name);
            writer.WritePropertyName("Points");
            writer.WriteStartArray();
            foreach(var it in mp.Value.m_points)
            {
                writer.WriteStartArray();
                writer.WriteValue(it.X);
                writer.WriteValue(it.Y);
                writer.WriteValue(it.Z);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        writer.WriteEndObject();
    }

    public override Dictionary<string, MapAttribute> ReadJson(JsonReader reader, Type objectType, Dictionary<string, MapAttribute> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        Dictionary<string, MapAttribute> ans = new Dictionary<string, MapAttribute>();
        foreach (var mname in jsonObject)
        {
            MapAttribute ls= new MapAttribute();
            ls.m_Name= (string)mname.Value["Name"];
            foreach(var it in (JArray)mname.Value["Points"])
            {
                ls.m_points.Add(new Vector((float)it[0],(float)it[1],(float)it[2]));
            }
            ans.Add(mname.Key,ls);
        }
        return ans;
    }
}
public class Config
{
    public static float m_NodeRadius=400;//////////////////////
    public static Dictionary<string, MapAttribute> m_NavigationMeshConfig=new Dictionary<string, MapAttribute>();
    public static string m_name="";
    public static AdjacencyGraph<int, TaggedEdge<int,float>> m_Graph = new AdjacencyGraph<int, TaggedEdge<int,float>>();
    public static Func<TaggedEdge<int,float>,double> m_EdgeCost=edge=>edge.Tag;
}

public class MapAttribute
{
    public string m_Name { get; set; }="";
    public List<Vector>m_points{ get; set; }=new List<Vector>();
    public void getEdge()
    {
        Config.m_Graph=new AdjacencyGraph<int, TaggedEdge<int,float>>();
        for(int i=0;i<m_points.Count;i++)Config.m_Graph.AddVertex(i);
        for(int i=0;i<Config.m_Graph.VertexCount;i++)
        {
            for(int j=i+1;j<Config.m_Graph.VertexCount;j++)
            {
                var spend=(m_points[i]-m_points[j]).Length();
                if(spend<=Config.m_NodeRadius)
                {
                    Config.m_Graph.AddEdge(new TaggedEdge<int,float>(i,j,spend));
                    Config.m_Graph.AddEdge(new TaggedEdge<int,float>(j,i,spend));
                }
            }
        }
    }
    public void add(Vector point)
    {
        m_points.Add(point);
    }
    public void delete(int id)
    {
        m_points.RemoveAt(id);
    }
}