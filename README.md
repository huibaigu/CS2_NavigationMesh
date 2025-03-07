# CS2_NavigationMesh

简单的导航网格

#### 命令：

```C#
css_nm_add          为当前网格增加一个点
css_nm_delete       删除最新的一个点
css_nm_debug        查看当前网格
css_nm_clear        取消查看当前网格
```

#### API

```C#
/// <summary>
/// 返回距离实体最近的point的id
/// </summary>
public int GetEntityid(Vector point);

/// <summary>
/// 得到从1到2的路径
/// </summary>
public Vector[]? GetPoint1ToPoint2List(Vector point1,Vector point2);
```

#### 参考视频
[![Video Label](https://i2.hdslb.com/bfs/archive/80f48ad05b25f585631fe9ea5044725d0da463b6.jpg@308w_174h)](https://www.bilibili.com/video/BV1Er9yYwEnm/)