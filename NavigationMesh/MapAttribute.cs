using CounterStrikeSharp.API.Modules.Utils;

namespace NavigationMesh.Other;
public class Config
{
    public static List<List<Vector>> ROOMS=new List<List<Vector>>();
    public static List<A.GridBase> GRIDS=new List<A.GridBase>();
    private static float isLeft(Vector P0, Vector P1,Vector P2)
    {
        return (P1.X-P0.X)*(P2.Y-P0.Y)-(P2.X-P0.X)*(P1.Y-P0.Y);
    }
    public static bool CheckEntityInRoom(Vector point,int meshid)
    {
        if(point.Z>50+Config.ROOMS[meshid].Max(a=>a.Z))return false;
        if(point.Z<Config.ROOMS[meshid].Min(a=>a.Z)-50)return false;
        int wn=0,j=0;
        for (int i=0;i<Config.ROOMS[meshid].Count;i++)
        {
            if (i==Config.ROOMS[meshid].Count-1)j=0;
            else j++;
            if (Config.ROOMS[meshid][i].Y<point.Y)
            {
                if (Config.ROOMS[meshid][j].Y>point.Y&&isLeft(Config.ROOMS[meshid][i],Config.ROOMS[meshid][j],point)>0)wn++;
            }
            else
            {
                if (Config.ROOMS[meshid][j].Y<point.Y&&isLeft(Config.ROOMS[meshid][i],Config.ROOMS[meshid][j],point)<0)wn--;
            }
        }
        return (wn!=0);
    }
}
public class MapAttribute()
{
    public string Name { get; set; }="";
    public List<List<List<float>>>Rooms{ get; set; }=new List<List<List<float>>>();
    public List<List<Vector>> toVector()
    {
        if(Rooms==null)Rooms=new List<List<List<float>>>();
        return Rooms.Select(items => items.Select(x=>new Vector(x[0],x[1],x[2])).ToList()).ToList();
    }
    public List<A.GridBase> getGRIDS()
    {
        var ans=new List<A.GridBase>();
        for(int i=0;i<Rooms.Count;i++)
        {
            var m_Grid = new A.GridBase(i,Config.ROOMS[i].Max(a=>a.X)-Config.ROOMS[i].Min(a=>a.X),Config.ROOMS[i].Max(a=>a.Y)-Config.ROOMS[i].Min(a=>a.Y));
            ans.Add(m_Grid);
        }
        return ans;
    }
}
public class MapAttributeSet:MapAttribute
{
    List<List<float>>cellList{get;set;}=new List<List<float>>();
    public MapAttributeSet()
    {
        
    }
    public MapAttributeSet(MapAttribute s)
    {
        this.Name = s.Name;
        this.Rooms = s.Rooms;
    }
    public MapAttribute print()
    {
        MapAttribute a=new MapAttribute();
        a.Name=this.Name;
        a.Rooms=this.Rooms;
        return a;
    }
    public Vector[] printcell()
    {
        if(cellList==null)cellList=new List<List<float>>();
        return cellList.Select(item=>new Vector(item[0],item[1],item[2])).ToArray();
    }
    public void add(Vector point)
    {
        float[] s = [point.X, point.Y, point.Z];
        cellList.Add(s.ToList());
    }
    public void clear()
    {
        cellList.Clear();
    }
    public void push()
    {
        Rooms.Add(cellList.ToList());
        clear();
    }
}
public class A
{
    public class Node
    {
        //是否可以通过
        public bool m_CanWalk;
        //节点空间位置
        public Vector m_WorldPos;
        //节点在数组的位置
        public int m_GridX;
        public int m_GridY;
        //开始节点到当前节点的距离估值
        public int m_gCost;
        //当前节点到目标节点的距离估值
        public int m_hCost;

        public int FCost
        {
            get { return m_gCost + m_hCost; }
        }
        //当前节点的父节点
        public Node m_Parent;

        public Node(bool canWalk, Vector position, int gridX, int gridY)
        {
            m_CanWalk = canWalk;
            m_WorldPos = position;
            m_GridX = gridX;
            m_GridY = gridY;
            m_Parent=this;
        }
        public Node(Node ls)
        {
            m_CanWalk = ls.m_CanWalk;
            m_WorldPos = ls.m_WorldPos;
            m_GridX = ls.m_GridX;
            m_GridY = ls.m_GridY;
            m_Parent=this;
        }
    }
    public class GridBase
    {
        public Node[,] m_Grid;
        public Vector m_GridSize=new Vector();
        public float m_NodeRadius=25;//========================================步进,格子半径
        private float m_NodeDiameter;
        public int m_GridCountX;
        public int m_GridCountY;
        public int roomid;
        public GridBase(int roomid,float x,float y)
        {
            this.roomid=roomid;
            this.m_GridSize.X=x;
            this.m_GridSize.Y=y;
            m_NodeDiameter = m_NodeRadius * 2;
            m_GridCountX = (int)Math.Round(m_GridSize.X / m_NodeDiameter);
            m_GridCountY = (int)Math.Round(m_GridSize.Y / m_NodeDiameter);
            m_Grid = new Node[m_GridCountX, m_GridCountY];
            CreateGrid();
        }
        /// <summary>
        /// 创建格子
        /// </summary>
        private void CreateGrid()
        {
            Vector GridstartPos = new Vector(Config.ROOMS[roomid].Min(a=>a.X),Config.ROOMS[roomid].Min(a=>a.Y),Config.ROOMS[roomid].Max(a=>a.Z));
            for (int i = 0; i < m_GridCountX; i++)
            {
                for (int j = 0; j < m_GridCountY; j++)
                {
                    Vector worldPos = new Vector(GridstartPos.X, GridstartPos.Y,GridstartPos.Z);
                    worldPos.X = worldPos.X + i * m_NodeDiameter + m_NodeRadius;
                    worldPos.Y = worldPos.Y + j * m_NodeDiameter + m_NodeRadius;
                    bool canWalk = Config.CheckEntityInRoom(worldPos,roomid);
                    m_Grid[i, j] = new Node(canWalk, worldPos, i, j);
                }
            }
        }
        /// <summary>
        /// 通过空间位置获得对应的节点
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Node GetFromPosition(Vector pos)/////////////////////////////////////////////////////////////
        {
            float percentX = (pos.X-Config.ROOMS[roomid].Min(a=>a.X))/m_GridSize.X;
            float percentY = (pos.Y-Config.ROOMS[roomid].Min(a=>a.Y))/m_GridSize.Y;
            percentX = percentX>1?1:percentX<0?0:percentX;
            percentY = percentY>1?1:percentY<0?0:percentY;
            int x = (int)Math.Round((m_GridCountX - 1) * percentX);
            int y = (int)Math.Round((m_GridCountY - 1) * percentY);
            return m_Grid[x, y];
        }

        /// <summary>
        /// 获得当前节点的相邻节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<Node> GetNeighor(Node node)
        {
            List<Node> neighborList = new List<Node>();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }
                    int tempX = node.m_GridX + i;
                    int tempY = node.m_GridY + j;
                    if (tempX < m_GridCountX && tempX > 0 && tempY > 0 && tempY < m_GridCountY)
                    {
                        neighborList.Add(m_Grid[tempX, tempY]);
                    }
                }
            }
            return neighborList;
        }
    }
    public class FindPath
    {
        private Vector m_StartNode;
        private Vector m_EndNode;
        private List<Node> openList = new List<Node>();
        private HashSet<Node> closeSet = new HashSet<Node>();
        public Stack<Node> m_Path = new Stack<Node>();
        public int roomid;
        public FindPath(int roomid,Vector start,Vector end)
        {
            this.roomid=roomid;
            m_StartNode=start;
            m_EndNode=end;
        }
        public void Update()
        {
            FindingPath(m_StartNode, m_EndNode);
        }
        /// <summary>
        /// A*算法，寻找最短路径
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void FindingPath(Vector start, Vector end)
        {
            Node startNode = Config.GRIDS[roomid].GetFromPosition(start);
            Node endNode = Config.GRIDS[roomid].GetFromPosition(end);
            openList.Clear();
            closeSet.Clear();
            openList.Add(startNode);
            while (openList.Count > 0)
            {
                Node currentNode = openList[0];
                for (int i = 0; i < openList.Count; i++)
                {
                    Node node = openList[i];
                    if (node.FCost < currentNode.FCost || node.FCost == currentNode.FCost && node.m_hCost < currentNode.m_hCost)
                    {
                        currentNode = node;
                    }
                }
                openList.Remove(currentNode);
                closeSet.Add(currentNode);
                if (currentNode == endNode)
                {
                    GeneratePath(startNode, endNode);
                    return ;
                }
                foreach (var node in Config.GRIDS[roomid].GetNeighor(currentNode))
                {
                    if (!node.m_CanWalk || closeSet.Contains(node))continue;
                    int gCost = (int)(currentNode.m_gCost + GetDistanceNodes(currentNode, node));
                    if (gCost < node.m_gCost || !openList.Contains(node))
                    {
                        node.m_gCost = gCost;
                        node.m_hCost = (int)GetDistanceNodes(node, endNode);
                        node.m_Parent = currentNode;
                        if (!openList.Contains(node))openList.Add(node);
                    }
                }
            }
        }
        /// <summary>
        /// 生成路径
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        private void GeneratePath(Node startNode, Node endNode)
        {
            Node node = endNode;
            if(startNode==endNode)return;
            while (node.m_Parent != startNode)
            {
                m_Path.Push(new Node(node));
                node = node.m_Parent;
            }
        }

        /// <summary>
        /// 获得两个节点的距离
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        private float GetDistanceNodes(Node node1, Node node2)
        {
            var deltaX = MathF.Abs(node1.m_GridX - node2.m_GridX);
            var deltaY = MathF.Abs(node1.m_GridY - node2.m_GridY);
            if (deltaX > deltaY)
            {
                return deltaY * 14 + 10 * (deltaX - deltaY);
            }
            else
            {
                return deltaX * 14 + 10 * (deltaY - deltaX);
            }
        }
    }
}