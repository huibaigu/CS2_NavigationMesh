using CounterStrikeSharp.API.Modules.Utils;
using QuickGraph;

namespace NavigationMesh.Other;
public class Config
{
    public static float m_NodeRadius=400;//步进
    public static List<Vector> ROOMS=new List<Vector>();
    public static UndirectedGraph<int, Edge<int>> graph = new UndirectedGraph<int, Edge<int>>();
    public static Dictionary<Edge<int>,double> weight=new Dictionary<Edge<int>,double>();
    public static Func<Edge<int>,double> edgecost=edge=>weight[edge];
}
public class MapAttribute()
{
    public string Name { get; set; }="";
    public List<List<float>>Rooms{ get; set; }=new List<List<float>>();
    public List<Vector> toVector()
    {
        if(Rooms==null)Rooms=new List<List<float>>();
        return Rooms.Select(x=>new Vector(x[0],x[1],x[2])).ToList();
    }
    private float distance(List<float> A,List<float> B)
    {
        Vector f1=new Vector(A[0],A[1],A[2]);
        Vector f2=new Vector(B[0],B[1],B[2]);
        return (f1-f2).Length();
    }
    public void getEDGE()
    {
        Config.graph=new UndirectedGraph<int, Edge<int>>();
        Config.weight=new Dictionary<Edge<int>,double>();
        for(int i=0;i<Rooms.Count;i++)Config.graph.AddVertex(i);
        for(int i=0;i<Config.graph.VertexCount;i++)
        {
            for(int j=i+1;j<Config.graph.VertexCount;j++)
            {
                var spend=distance(Rooms[i],Rooms[j]);
                if(spend<=Config.m_NodeRadius)
                {
                    var l=new Edge<int>(i,j);
                    Config.graph.AddEdge(l);
                    Config.weight.Add(l,spend);
                }
            }
        }
        Config.edgecost=edge=>Config.weight[edge];
    }
}
public class MapAttributeSet:MapAttribute
{
    public MapAttributeSet()
    {
        
    }
    public MapAttributeSet(MapAttribute s)
    {
        Name = s.Name;
        Rooms = s.Rooms;
    }
    public MapAttribute print()
    {
        MapAttribute a=new MapAttribute();
        a.Name=Name;
        a.Rooms=Rooms;
        return a;
    }
    public void add(Vector point)
    {
        float[] s = [point.X, point.Y, point.Z];
        Rooms.Add(s.ToList());
    }
    public void delete()
    {
        if(Rooms.Count>0)Rooms.RemoveAt(Rooms.Count-1);
    }
}