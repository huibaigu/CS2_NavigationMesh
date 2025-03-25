using CounterStrikeSharp.API.Modules.Utils;

namespace NavigationMeshAPI;

public interface INavigationMeshAPI
{
    /// <summary>
    /// 返回距离point最近的meshid
    /// </summary>
    public int getEntityid(Vector point);
    /// <summary>
    /// 返回point到指定mesh的距离
    /// </summary>
    public float getEntityDistance(Vector point,int id);
    /// <summary>
    /// 得到1到2的最短路径
    /// </summary>
    public string getPoint1ToPoint2List(Vector point1,Vector point2);
}