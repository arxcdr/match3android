using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match3 : MonoBehaviour
{
    public ArrayLayout boardLayout;

    [Header("UI Elements")]
    public Sprite[] pieces;
    public RectTransform gameBoard;

    [Header("Prefabs")]
    public GameObject nodePiece;

    int width = 9;
    int height = 14;
    Node[,] board;

    System.Random random;

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        string seed = getRandomSeed();
        random = new System.Random(seed.GetHashCode());

        InitializeBoard();
        VerifyBoard();
        InstantiateBoard();
    }

    void InitializeBoard()
    {
        board = new Node[width, height];
        for(int y=0; y<height; y++)
        {
            for(int x=0; x<width; x++)
            {
                board[x, y] = new Node((boardLayout.rows[y].row[x]) ? -1 : fillPiece(), new Point(x, y));
            }
        }
    }

    void VerifyBoard()
    {
        List<int> remove;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Point p = new Point(x,y);
                int val = getValueAtPoint(p);
                if (val <= 0) continue;

                remove = new List<int>();

                // If we have a match
                while (isConnected(p, true).Count > 0)
                {
                    val = getValueAtPoint(p);
                    if (!remove.Contains(val))
                    {
                        remove.Add(val);
                    }
                    setValueAtPoint(p, newValue(ref remove));
                }
            }
        }
    }

    void InstantiateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int val = board[x, y].value;
                if (val <= 0) continue;
                GameObject p = Instantiate(nodePiece, gameBoard);
                NodePiece node = p.GetComponent<NodePiece>();
                RectTransform rect = p.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(32 + (64 * x), -32 - (64 * y));
                node.Initialize(val, new Point(x, y), pieces[val - 1]);
            }
        }
    }

    List<Point> isConnected(Point p, bool main)
    {
        List<Point> connected = new List<Point>();
        int val = getValueAtPoint(p);

        // Maintain this order for directions to keep loops working
        Point[] directions =
        {
            Point.up,
            Point.right,
            Point.down,
            Point.left
        };

        // Check if there are 2 or more identical shapes
        foreach(Point dir in directions)
        {
            List<Point> line = new List<Point>();

            int same = 0;
            
            for(int i=1; i < 3; i++)
            {
                Point check = Point.add(p, Point.mult(dir, i));
                if(getValueAtPoint(check) == val)
                {
                    line.Add(check);
                    same++;
                }
            }

            // If there's more than 1 of the same shape in the direction, we know it's a match
            if(same > 1)
            {
                // Add these points to the connected list
                AddPoints(ref connected, line);
            }
        }

        // Check if we are in the "middle" of 2 identical shapes
        for (int i = 0; i < 2; i++)
        {
            List<Point> line = new List<Point>();
            int same = 0;

            Point[] check = { Point.add(p, directions[i]), Point.add(p, directions[i + 2]) };

            // Check both sides of the piece & if they are the same value, add them to the list
            foreach(Point next in check) {
                if (getValueAtPoint(next) == val)
                {
                    line.Add(next);
                    same++;
                }
            }

            if(same > 1)
            {
                AddPoints(ref connected, line);
            }
        }

        // Check for 2x2
        for (int i=0; i < 4; i++)
        {
            List<Point> square = new List<Point>();

            int same = 0;
            int next = i + 1;
            if(next >= 4)
            {
                next -= 4;
            }

            Point[] check = { Point.add(p, directions[i]), Point.add(p, directions[next]), Point.add(p, Point.add(directions[i], directions[next])) };

            // Check all sides of the piece & if they are the same value add to the list
            foreach (Point pnt in check)
            {
                if (getValueAtPoint(pnt) == val)
                {
                    square.Add(pnt);
                    same++;
                }
            }

            if(same > 2)
            {
                AddPoints(ref connected, square);
            }
        }

        // Check for other matches on current match
        if (main)
        {
            for(int i=0; i < connected.Count; i++)
            {
                AddPoints(ref connected, isConnected(connected[i], false));
            }
        }

        // If we have connected pieces, add the main piece
        if(connected.Count > 0){
            connected.Add(p);
        }

        return connected;
    }
    
    void AddPoints(ref List<Point> points, List<Point> add)
    {
        foreach(Point p in add)
        {
            bool doAdd = true;
            for(int i = 0; i < points.Count; i++)
            {
                // Don't add points if they're already there
                if (points[i].Equals(p))
                {
                    doAdd = false;
                    break;
                }
            }
            if (doAdd)
            {
                points.Add(p);
            }
        }
    }

    // Return a cube, sphere, cylinder or diamond
    int fillPiece()
    {
        int val = 1;
        val = (random.Next(0, 100) / (100 / pieces.Length))+1;
        return val;
    }

    int getValueAtPoint(Point p)
    {
        // If out of the array, return a hole just in case
        if(p.x < 0 || p.x >= width || p.y < 0 || p.y >= height)
        {
            return -1;
        }
        return board[p.x, p.y].value;
    }

    void setValueAtPoint(Point p, int v)
    {
        board[p.x, p.y].value = v;
    }

    int newValue(ref List<int> remove)
    {
        List<int> available = new List<int>();
        for(int i =0; i < pieces.Length; i++)
        {
            available.Add(i + 1);
        }
        foreach(int i in remove)
        {
            available.Remove(i);
        }
        if (available.Count <= 0) return 0;
        return available[random.Next(0, available.Count)];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Build a 20-char random seed
    string getRandomSeed()
    {
        string seed = "";
        string acceptableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890@#$%^&*()";

        for(int i=0; i<20; i++)
        {
            seed += acceptableChars[Random.Range(0, acceptableChars.Length)];
        }

        return seed;
    }
}

[System.Serializable]

public class Node
{
    // 0 = blank, 1 = cube, 2 = sphere, 3 = cylinder, 4 = pyramid, 5 = diamond, -1 = hole
    public int value;

    public Point index;

    public Node(int v, Point i)
    {
        value = v;
        index = i;
    }
}
