using CounterStrikeSharp.API.Modules.Utils;
using NavigationMeshAPI;

namespace NavigationMesh.Other;
public class NavigationMeshInterface:INavigationMeshAPI
{
    private float isLeft(Vector P0, Vector P1,Vector P2)
    {
        return (P1.X-P0.X)*(P2.Y-P0.Y)-(P2.X-P0.X)*(P1.Y-P0.Y);
    }
    public int CheckEntityInWhere(Vector point)
    {
        for(int id=0;id<Config.ROOMS.Count;id++)
        {
            if(point.Z>50+Config.ROOMS[id].Max(a=>a.Z))continue;
            if(point.Z<Config.ROOMS[id].Min(a=>a.Z)-50)continue;
            int wn=0,j=0;
            for (int i=0;i<Config.ROOMS[id].Count;i++)
            {
                if (i==Config.ROOMS[id].Count-1)j=0;
                else j++;
                if (Config.ROOMS[id][i].Y<point.Y)
                {
                    if (Config.ROOMS[id][j].Y>point.Y&&isLeft(Config.ROOMS[id][i],Config.ROOMS[id][j],point)>0)wn++;
                }
                else
                {
                    if (Config.ROOMS[id][j].Y<point.Y&&isLeft(Config.ROOMS[id][i],Config.ROOMS[id][j],point)<0)wn--;
                }
            }
            if(wn!=0)return id;
        }
        return -1;
    }
    public bool CheckEntityInRoom(Vector point,int meshid)
    {
        return Config.CheckEntityInRoom(point,meshid);
    }
    public Vector[]? GetPoint1ToPoint2List(Vector point1,Vector point2)
    {
        var roomid1=CheckEntityInWhere(point1);
        if(roomid1==-1)return null;
        else if(roomid1!=CheckEntityInWhere(point2))return null;
        A.FindPath ls=new A.FindPath(roomid1,point1,point2);
        ls.Update();
        return ls.m_Path.ToList().Select(item =>item.m_WorldPos).ToArray();
    }
}
