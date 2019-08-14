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

    List<NodePiece> update;
    List<FlippedPieces> flipped;

    System.Random random;

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    void Update()
    {
        List<NodePiece> finishedUpdating = new List<NodePiece>();
        for (int i = 0; i < update.Count; i++)
        {
            NodePiece piece = update[i];
            if (!piece.UpdatePiece()) finishedUpdating.Add(piece);
        }
        for (int i = 0; i < finishedUpdating.Count; i++)
        {
            NodePiece piece = finishedUpdating[i];
            FlippedPieces flip = getFlipped(piece);
            NodePiece flippedPiece = null;
            List<Point> connected = isConnected(piece.index, true);
            bool wasFlipped = (flip != null);

            // If we flipped to make this update
            if (wasFlipped)
            {
                flippedPiece = flip.getOtherPiece(piece);
                AddPoints(ref connected, isConnected(flippedPiece.index, true));
            }               

            // If we didn't make a match
            if(connected.Count == 0)
            {
                // If we flipped
                if(wasFlipped)
                {
                    // Flip back
                    FlipPieces(piece.index, flippedPiece.index, false);
                }
            }

            // If we made a match
            else
            {
                // Remove the node pieces connected
                foreach(Point pnt in connected)
                {
                    Node node = getNodeAtPoint(pnt);
                    NodePiece nodePiece = node.getPiece();
                    if(nodePiece != null)
                    {
                        nodePiece.gameObject.SetActive(false);
                    }
                }
            }
            // Remove the flip after update
            flipped.Remove(flip);
            update.Remove(piece);
        }
    }

    FlippedPieces getFlipped(NodePiece p)
    {
        FlippedPieces flip = null;
        for(int i = 0; i < flipped.Count; i++)
        {
            if (flipped[i].getOtherPiece(p) != null)
            {
                flip = flipped[i];
                break;
            }
                
        }
        return flip;
    }

    void StartGame()
    {
        string seed = getRandomSeed();
        random = new System.Random(seed.GetHashCode());
        update = new List<NodePiece>();
        flipped = new List<FlippedPieces>();

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
                Node node = getNodeAtPoint(new Point(x, y));
                int val = node.value;
                if (val <= 0) continue;
                GameObject p = Instantiate(nodePiece, gameBoard);
                NodePiece piece = p.GetComponent<NodePiece>();
                RectTransform rect = p.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(32 + (64 * x), -32 - (64 * y));
                piece.Initialize(val, new Point(x, y), pieces[val - 1]);
                node.SetPiece(piece);
            }
        }
    }

    public void ResetPiece(NodePiece piece)
    {
        piece.ResetPosition();
        update.Add(piece);
    }

    public void FlipPieces(Point one, Point two, bool main)
    {
        // If it's a hole, do nothing
        if (getValueAtPoint(one) < 0) return;

        Node nodeOne = getNodeAtPoint(one);
        NodePiece pieceOne = nodeOne.getPiece();
        if (getValueAtPoint(two) > 0)
        {
            Node nodeTwo = getNodeAtPoint(two);
            NodePiece pieceTwo = nodeTwo.getPiece();

            // Flip the pieces
            nodeOne.SetPiece(pieceTwo);
            nodeTwo.SetPiece(pieceOne);

            if(main)
                flipped.Add(new FlippedPieces(pieceOne, pieceTwo));

            update.Add(pieceOne);
            update.Add(pieceTwo);
        }
        else
            ResetPiece(pieceOne);
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

    Node getNodeAtPoint(Point p)
    {
        return board[p.x, p.y];
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

    public Vector2 getPositionFromPoint(Point p)
    {
        return new Vector2(32 + (64 * p.x), -32 - (64 * p.y));
    }
}

[System.Serializable]

public class Node
{
    // 0 = blank, 1 = cube, 2 = sphere, 3 = cylinder, 4 = pyramid, 5 = diamond, -1 = hole
    public int value;
    public Point index;
    NodePiece piece;

    public Node(int v, Point i)
    {
        value = v;
        index = i;
    }

    public void SetPiece(NodePiece p)
    {
        piece = p;
        value = (piece == null) ? 0 : piece.value;
        if (piece == null) return;
        piece.SetIndex(index);
    }
    public NodePiece getPiece()
    {
        return piece;
    }
}

[System.Serializable]
public class FlippedPieces
{
    public NodePiece one;
    public NodePiece two;

    public FlippedPieces(NodePiece o, NodePiece t)
    {
        one = o;
        two = t;
    }

    public NodePiece getOtherPiece(NodePiece p)
    {
        if (p == one)
            return two;
        else if (p == two)
            return one;
        else
            return null;
    }
}