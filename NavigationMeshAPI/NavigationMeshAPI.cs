using CounterStrikeSharp.API.Modules.Utils;

namespace NavigationMeshAPI;

public interface INavigationMeshAPI
{
    /// <summary>
    /// 返回距离实体最近的point的id
    /// </summary>
    public int GetEntityid(Vector point);
    /// <summary>
    /// 得到1到2的路径
    /// </summary>
    public Vector[]? GetPoint1ToPoint2List(Vector point1,Vector point2);
}