# CS2_NavigationMesh

简单的导航网格

#### 命令：

```C#
css_nm_add          为当前网格增加玩家脚下一个点
css_nm_delete       删除里玩家最近的一个点
css_nm_debug        生成里玩家最近一个点构成的边
css_nm_clear        清除所有激光
css_nm_look         用激光显示所有点的位置
```

#### API

```C#
/// <summary>
/// 返回距离point最近的网格id
/// </summary>
public int GetEntityid(Vector point);
/// <summary>
/// 返回point与网格id的距离
/// </summary>
float getEntityDistance(Vector point,int id)
/// <summary>
/// 得到从1到2的路径
/// </summary>
public Vector[]? GetPoint1ToPoint2List(Vector point1,Vector point2);
```

#### 插件参考视频（旧）
[![Video Label](https://i2.hdslb.com/bfs/archive/80f48ad05b25f585631fe9ea5044725d0da463b6.jpg@308w_174h)](https://www.bilibili.com/video/BV1Er9yYwEnm/)

#### 使用具体效果（旧）
[![Video Label](https://i2.hdslb.com/bfs/archive/80f48ad05b25f585631fe9ea5044725d0da463b6.jpg@308w_174h)](https://www.bilibili.com/video/BV1Er9yYwEnm/)