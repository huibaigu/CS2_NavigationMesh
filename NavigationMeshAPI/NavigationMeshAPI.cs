using CounterStrikeSharp.API.Modules.Utils;

namespace NavigationMeshAPI;

public interface INavigationMeshAPI
{
    /// <summary>
    /// 返回实体是否在room内，简化计算，高度轴为上下50个高度
    /// </summary>
    public bool CheckEntityInRoom(Vector point,int meshid);
    /// <summary>
    /// 返回实体在map中哪个room内，简化计算，高度轴为上下50个高度，失败返回-1
    /// </summary>
    public int CheckEntityInWhere(Vector point);
    /// <summary>
    /// 得到1到2的路径,1和2需要在同一个room中
    /// </summary>
    public Vector[]? GetPoint1ToPoint2List(Vector point1,Vector point2);
}
