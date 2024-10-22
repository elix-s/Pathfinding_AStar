using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathfindingController : MonoBehaviour
{
    public GameObject[,] _grid;
    public int _rows = 6;
    public int _cols = 6;
    private Vector2Int _startCell = new Vector2Int(0, 0);
    private List<Vector2Int> _currentPath = new List<Vector2Int>();

    private void Awake()
    {
        _grid = new GameObject[_rows, _cols];
        int index = 0;

        foreach (Transform child in transform)
        {
            int row = index / _cols;
            int col = index % _cols;
            _grid[row, col] = child.gameObject;

            if (!(row == _startCell.x && col == _startCell.y))
            {
                Button button = _grid[row, col].GetComponent<Button>();
                int r = row, c = col;
                button.onClick.AddListener(() => OnClick(r, c));
            }

            index++;
        }

        _grid[_startCell.x, _startCell.y].GetComponent<Image>().color = Color.red;
    }

    public void OnClick(int targetRow, int targetCol)
    {
        ClearPath();
        List<Vector2Int> path = FindPathAStar(_startCell, new Vector2Int(targetRow, targetCol));

        if (path != null)
        {
            foreach (Vector2Int cell in path)
            {
                _grid[cell.x, cell.y].GetComponent<Image>().color = Color.green;
            }

            _currentPath = path;
        }
    }

    private List<Vector2Int> FindPathAStar(Vector2Int start, Vector2Int target)
    {
        var openSet = new List<Node>();
        var closedSet = new HashSet<Node>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        Node startNode = new Node(start, 0, HeuristicFunction(start, target));
        openSet.Add(startNode);

        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0)
        };

        while (openSet.Count > 0)
        {
            //Find the node with the smallest f(x) in the open list
            Node current = GetLowestFScoreNode(openSet);
            
            if (current.Position == target)
            {
                return ReconstructPath(cameFrom, current.Position);
            }

            openSet.Remove(current);
            closedSet.Add(current);  
            
            foreach (var direction in directions)
            {
                Vector2Int neighborPos = current.Position + direction;
                
                if (!IsInBoundsChecking(neighborPos))
                    continue;
                
                if (closedSet.Contains(new Node(neighborPos, 0, 0))) 
                    continue;

                float tentativeGScore = current.g + 1; 
                
                Node neighborNode = openSet.Find(node => node.Position == neighborPos);
                if (neighborNode == null)
                {
                    neighborNode = new Node(neighborPos, tentativeGScore, HeuristicFunction(neighborPos, target));
                    openSet.Add(neighborNode);
                    cameFrom[neighborPos] = current.Position;  // Remember the path
                }
                else if (tentativeGScore < neighborNode.g)
                {
                    neighborNode.g = tentativeGScore;
                    neighborNode.f = tentativeGScore + neighborNode.h;
                    cameFrom[neighborPos] = current.Position;
                }
            }
        }

        return null;  
    }

    private Node GetLowestFScoreNode(List<Node> openSet)
    {
        Node lowest = openSet[0];
        foreach (Node node in openSet)
        {
            if (node.f < lowest.f)
            {
                lowest = node;
            }
        }
        return lowest;
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Reverse();
        return path;
    }

    private bool IsInBoundsChecking(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < _rows && cell.y >= 0 && cell.y < _cols;
    }

    private float HeuristicFunction(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private void ClearPath()
    {
        foreach (Vector2Int cell in _currentPath)
        {
            _grid[cell.x, cell.y].GetComponent<Image>().color = Color.white;
        }

        _currentPath.Clear();
    }
    
    private class Node
    {
        public Vector2Int Position;
        public float g;  
        public float h;  
        public float f;  

        public Node(Vector2Int pos, float g, float h)
        {
            Position = pos;
            this.g = g;
            this.h = h;
            this.f = g + h;
        }

        public override bool Equals(object obj)
        {
            if (obj is Node node)
            {
                return Position.Equals(node.Position);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}
