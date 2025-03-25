using CounterStrikeSharp.API.Modules.Utils;
using NavigationMeshAPI;
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
            float ll=(point-Config.m_NavigationMeshConfig[Config.m_name].m_points[i]).Length();
            if(ll>ls)continue;
            ls=ll;
            ans=i;
        }
        return ans;
    }
    public float getEntityDistance(Vector point,int id)
    {
        return (point-Config.m_NavigationMeshConfig[Config.m_name].m_points[id]).Length();
    }
    public Vector[] getPoint1ToPoint2List(Vector point1,Vector point2)
    {
        var ls= Config.m_Graph.Clone();
        var from=ls.VertexCount;
        ls.AddVertex(ls.VertexCount);
        var to=ls.VertexCount;
        ls.AddVertex(ls.VertexCount);
        var speed=(point1-point2).Length();
        if(speed<=Config.m_NodeRadius)
        {
            ls.AddEdge(new TaggedEdge<int,float>(from,to,speed));
            ls.AddEdge(new TaggedEdge<int,float>(to,from,speed));
        }
        for(int i=0;i<from;i++)
        {
            speed=(Config.m_NavigationMeshConfig[Config.m_name].m_points[i]-point1).Length();
            if(speed<=Config.m_NodeRadius)
            {
                ls.AddEdge(new TaggedEdge<int,float>(i,from,speed));
                ls.AddEdge(new TaggedEdge<int,float>(from,i,speed));
            }
            speed=(Config.m_NavigationMeshConfig[Config.m_name].m_points[i]-point2).Length();
            if(speed<=Config.m_NodeRadius)
            {
                ls.AddEdge(new TaggedEdge<int,float>(i,to,speed));
                ls.AddEdge(new TaggedEdge<int,float>(to,i,speed));
            }
        }
        List<Vector> ans=new List<Vector>();
        var tryGetPaths = ls.ShortestPathsDijkstra(Config.m_EdgeCost, from);
        if(tryGetPaths(to,out var path))
        {
            foreach(var edge in path)
            {
                if(edge.Target<from)ans.Add(Config.m_NavigationMeshConfig[Config.m_name].m_points[edge.Target]);
                else if (edge.Target==from)ans.Add(point1);
                else if (edge.Target==to)ans.Add(point2);
            }
        }
        return ans.ToArray();
    }
}
