using CounterStrikeSharp.API.Modules.Utils;

namespace NavigationMesh.Other;
public class Config
{
    public static float m_NodeRadius=100;//步进
    public static List<Vector> ROOMS=new List<Vector>();
    public struct EDGE 
    {
        public int next;
        public int to;
        public float spend;
    }
    public static int edgeCount=0;
    public static int nodeCount=0;
    public static EDGE[] edge=new EDGE[100000];
    public static int[] head=new int[1000];
    public static int cnt=0;
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
        Config.nodeCount=Rooms.Count;
        for(int i=0;i<Config.nodeCount;i++)
        {
            for(int j=i+1;j<Config.nodeCount;j++)
            {
                var from=i;
                var to=j;
                var spend=distance(Rooms[i],Rooms[j]);
                if(spend<=Config.m_NodeRadius)
                {
                    Config.edge[++Config.cnt].next=Config.head[from];
                    Config.edge[Config.cnt].to=to;
                    Config.edge[Config.cnt].spend=spend;
                    Config.head[from]=Config.cnt;
                    Config.edgeCount++;

                    Config.edge[++Config.cnt].next=Config.head[to];
                    Config.edge[Config.cnt].to=from;
                    Config.edge[Config.cnt].spend=spend;
                    Config.head[to]=Config.cnt;
                    Config.edgeCount++;
                }
            }
        }
    }
}
public class MapAttributeSet:MapAttribute
{
    public MapAttributeSet()
    {
        
    }
    public MapAttributeSet(MapAttribute s)
    {
        this.Name = s.Name;
        this.Rooms = s.Rooms;
    }
    public MapAttribute print()
    {
        MapAttribute a=new MapAttribute();
        a.Name=this.Name;
        a.Rooms=this.Rooms;
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
public class Dijkstra
{
    private float[] ans;
    private bool[] vis;
    public Dijkstra()
    {
        ans=new float[Config.edgeCount];
        vis=new bool[Config.nodeCount];
        for(int i=0;i<Config.nodeCount;ans[i++]=2147483647);
    }
    public List<Vector> GetPath(int from,int to)
    {
        var pos=from;
        ans[pos]=0;
        List<Vector> ph=new List<Vector>();
        while(!vis[pos])
        {
            ph.Add(Config.ROOMS[pos]);
            if(pos==to)break;
            float minn=2147483647;
            vis[pos]=true;
            for(int i=Config.head[pos];i!=0;i=Config.edge[i].next)
            {
                if(!vis[Config.edge[i].to]&&ans[Config.edge[i].to]>ans[pos]+Config.edge[i].spend)
                {
                    ans[Config.edge[i].to]=ans[pos]+Config.edge[i].spend;
                }
            }
            for(int i=1;i<=Config.nodeCount;i++)
            {
                if(ans[i]<minn&&vis[i]==false)
                {
                    minn=ans[i];
                    pos=i;
                }
            }
        }
        return ph;
    }
}