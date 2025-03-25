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
                writer.WriteValue(it.m_point.X);
                writer.WriteValue(it.m_point.Y);
                writer.WriteValue(it.m_point.Z);
                writer.WriteValue(it.m_type);
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
                ls.m_points.Add(new MapAttribute.MeshNode(new Vector((float)it[0],(float)it[1],(float)it[2]),(int)it[3]));
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
    public class MeshNodeConverter : JsonConverter<List<MeshNode>>
    {
        public override void WriteJson(JsonWriter writer, List<MeshNode> value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("return");
            writer.WriteStartArray();
            foreach (var kvp in value)
            {
                writer.WriteStartArray();
                writer.WriteValue(kvp.m_point.X);
                writer.WriteValue(kvp.m_point.Y);            
                writer.WriteValue(kvp.m_point.Z);
                writer.WriteValue(kvp.m_type);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public override List<MeshNode> ReadJson(JsonReader reader, Type objectType, List<MeshNode> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            List<MeshNode> ans = new List<MeshNode>();
            foreach (var mname in jsonObject)
            {
                foreach(var i in (JArray)mname.Value["return"])
                {
                    ans.Add(new MeshNode(new Vector((float)i[0],(float)i[1],(float)i[2]),(int)i[3]));
                }
            }
            return ans;
        }
    }
    public class MeshNode(Vector point,int type)
    {
        public readonly Vector m_point=point;
        //0:地面节点,1:攀爬节点;
        public readonly int m_type=type;
    }
    public string m_Name { get; set; }="";
    public List<MeshNode>m_points{ get; set; }=new List<MeshNode>();
    public void getEdge()
    {
        Config.m_Graph=new AdjacencyGraph<int, TaggedEdge<int,float>>();
        for(int i=0;i<m_points.Count;i++)Config.m_Graph.AddVertex(i);
        for(int i=0;i<Config.m_Graph.VertexCount;i++)
        {
            for(int j=i+1;j<Config.m_Graph.VertexCount;j++)
            {
                var spend=(m_points[i].m_point-m_points[j].m_point).Length();
                if(spend<=Config.m_NodeRadius)
                {
                    Config.m_Graph.AddEdge(new TaggedEdge<int,float>(i,j,spend));
                    Config.m_Graph.AddEdge(new TaggedEdge<int,float>(j,i,spend));
                }
            }
        }
    }
    public void add(Vector point,int? type=0)
    {
        m_points.Add(new MeshNode(point,type??0));
    }
    public void delete(int id)
    {
        m_points.RemoveAt(id);
    }
}