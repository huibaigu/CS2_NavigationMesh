using CounterStrikeSharp.API.Modules.Utils;
using NavigationMeshAPI;

namespace NavigationMesh.Other;
public class NavigationMeshInterface:INavigationMeshAPI
{
    public int GetEntityid(Vector point)
    {
        int ans=0;
        float ls=2147483647;
        for(int i=0;i<Config.nodeCount;i++)
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
            ans.Add(point1);
            ans.Add(Config.ROOMS[from]);
            ans.Add(point2);
            return ans.ToArray();
        }
        ans.Add(point1);
        Dijkstra ls=new Dijkstra();
        ans.AddRange(ls.GetPath(from,to));
        ans.Add(point2);
        return ans.ToArray();
    }
}
