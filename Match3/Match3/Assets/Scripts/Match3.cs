using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match3 : MonoBehaviour
{
    public ArrayLayout boardLayout;
    public Sprite[] pieces;
    int width = 9;
    int height = 14;
    Node[,] board;

    System.Random random;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void StartGame()
    {
        string seed = getRandomSeed();
        random = new System.Random(seed.GetHashCode());

        InitializeBoard();
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
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Point p = new Point(x,y);
                int val = getValueAtPoint(p);
                if (val <= 0) continue;
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
                    line.Add(p);
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
                    square.Add(p);
                    same++;
                }
            }

            if(same > 2)
            {
                AddPoints(ref connected, square);
            }
        }
    }

    void AddPoints(ref List<Point> points, List<Point> add)
    {

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
        return board[p.x, p.y].value;
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
