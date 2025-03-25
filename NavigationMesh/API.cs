using CounterStrikeSharp.API.Modules.Utils;
using NavigationMeshAPI;
using Newtonsoft.Json;
using QuikGraph;
using QuikGraph.Algorithms;

namespace NavigationMesh.Other;
public class NavigationMeshInterface:INavigationMeshAPI
{
    public int getEntityid(Vector point)
    {
        int ans=0;
        float ls=0x7f7f7f7f;
        for(int i=0;i<Config.m_Graph.VertexCount;i++)
        {
            float ll=(point-Config.m_NavigationMeshConfig[Config.m_name].m_points[i].m_point).Length();
            if(ll>ls)continue;
            ls=ll;
            ans=i;
        }
        return ans;
    }
    public float getEntityDistance(Vector point,int id)
    {
        return (point-Config.m_NavigationMeshConfig[Config.m_name].m_points[id].m_point).Length();
    }
    public string getPoint1ToPoint2List(Vector point1,Vector point2)
    {
        var ls= Config.m_Graph.Clone();
        var from=ls.VertexCount;
        ls.AddVertex(ls.VertexCount);
        var to=ls.VertexCount;
        ls.AddVertex(ls.VertexCount);
        var spend=(point1-point2).Length();
        if(spend<=Config.m_NodeRadius)
        {
            ls.AddEdge(new TaggedEdge<int,float>(from,to,spend));
            ls.AddEdge(new TaggedEdge<int,float>(to,from,spend));
        }
        for(int i=0;i<from;i++)
        {
            spend=(Config.m_NavigationMeshConfig[Config.m_name].m_points[i].m_point-point1).Length();
            if(spend<=Config.m_NodeRadius)
            {
                ls.AddEdge(new TaggedEdge<int,float>(i,from,spend));
                ls.AddEdge(new TaggedEdge<int,float>(from,i,spend));
            }
            spend=(Config.m_NavigationMeshConfig[Config.m_name].m_points[i].m_point-point2).Length();
            if(spend<=Config.m_NodeRadius)
            {
                ls.AddEdge(new TaggedEdge<int,float>(i,to,spend));
                ls.AddEdge(new TaggedEdge<int,float>(to,i,spend));
            }
        }
        List<MapAttribute.MeshNode> ans=new List<MapAttribute.MeshNode>();
        var tryGetPaths = ls.ShortestPathsDijkstra(Config.m_EdgeCost, from);
        if(tryGetPaths(to,out var path))
        {
            foreach(var edge in path)
            {
                if(edge.Target<from)ans.Add(Config.m_NavigationMeshConfig[Config.m_name].m_points[edge.Target]);
                else if (edge.Target==from)ans.Add(new MapAttribute.MeshNode(point1,0));
                else if (edge.Target==to)ans.Add(new MapAttribute.MeshNode(point2,0));
            }
        }
        return JsonConvert.SerializeObject(ans,Formatting.Indented,new MapAttribute.MeshNodeConverter());
    }
}
