using CounterStrikeSharp.API.Modules.Utils;
using NavigationMeshAPI;
using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;

namespace NavigationMesh.Other;
public class NavigationMeshInterface:INavigationMeshAPI
{
    public int GetEntityid(Vector point)
    {
        int ans=0;
        float ls=2147483647;
        for(int i=0;i<Config.graph.VertexCount;i++)
        {
            float ll=(point-Config.ROOMS[i]).Length();
            if(ll>ls)continue;
            ls=ll;
            ans=i;
        }
        return ans;
    }
    public Vector[]? GetPoint1ToPoint2List(Vector point1,Vector point2)
    {
        var from=GetEntityid(point1);
        var to=GetEntityid(point2);
        List<Vector> ans=new List<Vector>();
        if(from==to)
        {
            ans.Add(point2);
            return ans.ToArray();
        }
        var pathFinder = new UndirectedDijkstraShortestPathAlgorithm<int, Edge<int>>(Config.graph, Config.edgecost);
        var predecessors=new UndirectedVertexPredecessorRecorderObserver<int, Edge<int>>();
        using (predecessors.Attach(pathFinder))
        {
            pathFinder.Compute(from);
        }
        if(predecessors.TryGetPath(to,out var path))
        {
            int ls=-1;
            foreach(var edge in path)
            {
                if(ls==-1)
                {
                    ans.Add(Config.ROOMS[edge.Source]);
                    ls=0;
                }
                ans.Add(Config.ROOMS[edge.Target]);
            }
            ans.Add(point2);
            return ans.ToArray();
        }
        ans.Add(point2);
        return ans.ToArray();
    }
}
