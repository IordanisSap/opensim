using System;
using System.Collections.Generic;


public class Maze2D
{
    private int _width;
    private int _height;
    private Cell[,] _cells;
    private Random _random = new Random();

    public Maze2D(int width, int height)
    {
        _width = width;
        _height = height;
        _cells = new Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _cells[x, y] = new Cell();
            }
        }
        // Choose a random cell to start from
        int startX = _random.Next(width);
        int endX = _random.Next(width);

        _cells[startX, 0].TopWall = false;
        _cells[endX, height - 1].BottomWall = false;
        GenerateMazeRecursive(startX, _random.Next(height));
    }
    public void GenerateMazeRecursive(int x, int y)
    {
        _cells[x, y].Visited = true;

        List<int[]> neighbors = GetUnvisitedNeighbors(x, y);

        while (neighbors.Count > 0)
        {
            int[] neighbor = neighbors[_random.Next(neighbors.Count)];
            neighbors.Remove(neighbor);

            int nx = neighbor[0];
            int ny = neighbor[1];

            if (!_cells[nx, ny].Visited)
            {
                RemoveWall(x, y, nx, ny);
                GenerateMazeRecursive(nx, ny);
            }
        }
    }
    private List<int[]> GetUnvisitedNeighbors(int x, int y)
    {
        List<int[]> neighbors = new List<int[]>();
        if (x > 0 && !_cells[x - 1, y].Visited)
        {
            neighbors.Add(new int[] { x - 1, y });
        }
        if (x < _width - 1 && !_cells[x + 1, y].Visited)
        {
            neighbors.Add(new int[] { x + 1, y });
        }
        if (y > 0 && !_cells[x, y - 1].Visited)
        {
            neighbors.Add(new int[] { x, y - 1 });
        }
        if (y < _height - 1 && !_cells[x, y + 1].Visited)
        {
            neighbors.Add(new int[] { x, y + 1 });
        }
        return neighbors;
    }
    private void RemoveWall(int x, int y, int nx, int ny)
    {
        if (x == nx)
        {
            // Same column
            if (y < ny)
            {
                // Cell is above neighbor
                _cells[x, y].BottomWall = false;
                _cells[nx, ny].TopWall = false;
            }
            else
            {
                // Cell is below neighbor
                _cells[x, y].TopWall = false;
                _cells[nx, ny].BottomWall = false;
            }
        }
        else
        {
            // Same row
            if (x < nx)
            {
                // Cell is left of neighbor
                _cells[x, y].RightWall = false;
                _cells[nx, ny].LeftWall = false;
            }
            else
            {
                // Cell is right of neighbor
                _cells[x, y].LeftWall = false;
                _cells[nx, ny].RightWall = false;
            }
        }
    }
    public void printMaze()
    {
        for (int y = 0; y < _height; y++)
        {
            // Print the top wall
            for (int x = 0; x < _width; x++)
            {
                Console.Write(_cells[x, y].TopWall ? "+--" : "+  ");
            }
            Console.WriteLine("+");

            for (int x = 0; x < _width; x++)
            {
                Console.Write(_cells[x, y].LeftWall ? "|  " : "   ");
            }

            // Print the bottom wall
            Console.WriteLine("|");
        }
        for (int x = 0; x < _width; x++)
        {
            Console.Write(_cells[x, _height - 1].BottomWall ? "+--" : "+  ");
        }
        Console.WriteLine("+");
    }
    public Cell[,] getCells()
    {
        return _cells;
    }
}


public class BinaryMaze2D
{
    private int _width;
    private int _height;
    private int[,] _cells;

    public BinaryMaze2D(Cell[,] cells)
    {
        _width = cells.GetLength(0);
        _height = cells.GetLength(1);

        _cells = new int[_width * 2 + 1, _height * 2 + 1];

        {
            for (int y = 0; y < _height * 2 + 1; y++)

            {
                for (int x = 0; x < _width * 2 + 1; x++)
                {
                    _cells[x, y] = 1;
                }
            }

            Console.WriteLine("Maze");
            for (int y = 0; y < _height; y++)

            {
                for (int x = 0; x < _width; x++)
                {
                    _cells[x * 2, y * 2 + 1] = cells[x, y].LeftWall ? 1 : 0;
                    _cells[x * 2 + 1, y * 2] = cells[x, y].TopWall ? 1 : 0;
                    _cells[x * 2 + 1, y * 2 + 1] = 0;
                }
                Console.WriteLine();
            }
            for (int x = 0; x < _width; x++)
            {
                _cells[x * 2 + 1, _height * 2] = cells[x, _height - 1].BottomWall ? 1 : 0;
            }
        }
    }

    public void print()
    {

        for (int y = 0; y < _height * 2 + 1; y++)

        {
            for (int x = 0; x < _width * 2 + 1; x++)
            {
                Console.Write(_cells[x, y]);
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }
    public int[,] getCells()
    {
        return _cells;
    }
}

public class MazeSolver
{
    int[,] maze;

    bool[,] visited;

    int[] start;

    int[] end;

    private Random _random = new Random();

    private List<int[]> _path = new List<int[]>();

    public MazeSolver(int[,] maze)
    {
        start = new int[2];
        end = new int[2];
        this.maze = maze;
        visited = new bool[maze.GetLength(0), maze.GetLength(1)];

        for (int x = 0; x < maze.GetLength(0); x++)
        {
            if (maze[x, 0] == 0)
            {
                start = new int[] { x, 0};
            }
        }
        for (int x = 0; x < maze.GetLength(0); x++)
        {
            if (maze[x, maze.GetLength(1) - 1] == 0)
            {
                end = new int[] { x, maze.GetLength(1) - 1 };
            }
        }
        Console.WriteLine("Start: " + start[0] + ", " + start[1]);
        Console.WriteLine("End: " + end[0] + ", " + end[1]);
    }
    public void Solve()
    {
        SolvePath(start[0], start[1]);
        _path.Reverse();
    }

    private bool SolvePath(int x, int y)
    {
        bool flag = false;
        visited[x, y] = true;
        if(maze[x,y] == 1) return false;
        if(x == end[0] && y == end[1] || x == start[0] && y == start[1])
        {
            flag = true;
        }
        List<int[]> neighbors = GetUnvisitedNeighbors(x, y);
        while (neighbors.Count > 0)
        {
            int[] neighbor = neighbors[_random.Next(neighbors.Count)];
            neighbors.Remove(neighbor);

            int nx = neighbor[0];
            int ny = neighbor[1];

            if (!visited[nx, ny])
            {
                if(SolvePath(nx, ny)) flag = true;
            }
        }
        if (flag) _path.Add(new int[] { x, y });
        return flag;
    }

    private List<int[]> GetUnvisitedNeighbors(int x, int y)
    {
        List<int[]> neighbors = new List<int[]>();
        if (x > 0 && !visited[x - 1, y])
        {
            neighbors.Add(new int[] { x - 1, y });
        }
        if (x < maze.GetLength(0) - 1 && !visited[x + 1, y])
        {
            neighbors.Add(new int[] { x + 1, y });
        }
        if (y > 0 && !visited[x, y - 1])
        {
            neighbors.Add(new int[] { x, y - 1 });
        }
        if (y < maze.GetLength(1) - 1 && !visited[x, y + 1])
        {
            neighbors.Add(new int[] { x, y + 1 });
        }
        return neighbors;
    }

    public void printPath()
    {
        foreach(int[] i in _path)
        {
            Console.WriteLine(i[0] + ", " + i[1]);
        }
    }

    public List<int[]> getPath()
    {
        if (_path.Count == 0) Solve();
        return _path;
    }
}


public class MazeLandmark
{
    private int[] startPoint;

    public MazeLandmark(int[] startPoint)
    {
        this.startPoint = startPoint;
    }

    public void Print()
    {
        Console.WriteLine("Landmark: [" + startPoint[0] + "," + startPoint[1] + "]");
    }
}

public class LandmarkCreator
{
    private List<int[]> path;
    private Random _random = new Random();
    private int size = 0;
    private List<List<int[]>> lines = new List<List<int[]>>();
    private List<List<int[]>> sigmas = new List<List<int[]>>();

    private List<List<int[]>> pis = new List<List<int[]>>();


    private List<MazeLandmark> landmarks = new List<MazeLandmark>();

    public LandmarkCreator(List<int[]> path, int size = 0)
    {
        this.path = path;
        this.size = size;
        getLines();
        getShapes();
        //lines.Sort((line1, line2) => Math.Abs(line2.Count).CompareTo(Math.Abs(line1.Count)));
        createLandmarks();
    }

    private void getLines()
    {
        lines.Add(new List<int[]> { path[0] });
        int current = 0;
        bool vertical = true;
        for (int i = 1; i < path.Count; i++)
        {
            if (path[i][0] == path[i - 1][0] && vertical || path[i][1] == path[i - 1][1] && !vertical)
            {
                lines[current].Add(path[i]);
            }
            else
            {
                current++;
                lines.Add(new List<int[]> { path[i] });
                vertical = !vertical;
            }
        }
    }

    private void getShapes()
    {
        const int PI_SIZE = 3;
        const int FAIL = -1;
        bool OVERLAP = false;
        int getNextHorizontalPoint(int start)
        {
            if (start + PI_SIZE >= path.Count) return FAIL;
            for (int i = start + 1; i < start + PI_SIZE; i++)
            {
                if (path[i][0] != path[i - 1][0]) return FAIL;
            }
            return start + PI_SIZE - 1;
        }

        int getNextVerticalPoint(int start)
        {
            if (start + PI_SIZE >= path.Count) return FAIL;
            for (int i = start + 1; i < start + PI_SIZE; i++)
            {
                if (path[i][1] != path[i - 1][1]) return FAIL;
            }
            return start + PI_SIZE - 1;
        }

        for (int i = 0; i < path.Count; i++)
        {
            Console.Write("[" + path[i][0] + ", " + path[i][1] + "] ");
            int firstHorizontalPoint = getNextHorizontalPoint(i);
            if (firstHorizontalPoint != FAIL)
            {
                //Console.Write("firstHorizontalPoint");
                int secondVerticalPoint = getNextVerticalPoint(firstHorizontalPoint);
                if (secondVerticalPoint != FAIL)
                {
                    //Console.Write("secondVerticalPoint");
                    int thirdHorizontalPoint = getNextHorizontalPoint(secondVerticalPoint);
                    if (thirdHorizontalPoint != FAIL)
                    {
                        //Console.Write("thirdHorizontalPoint");
                        if (path[i][0] == path[i + 6][0] || path[i][1] == path[i + 6][1]) pis.Add(new List<int[]> { path[i], path[i + 1], path[i + 2], path[i + 3], path[i + 4], path[i + 5], path[i + 6] });
                        else sigmas.Add(new List<int[]> { path[i], path[i + 1], path[i + 2], path[i + 3], path[i + 4], path[i + 5], path[i + 6] });
                        if (!OVERLAP) i += 6;
                    }
                }
            }
            int firstVerticalPoint = getNextVerticalPoint(i);
            if (firstVerticalPoint != FAIL)
            {
                int secondHorizontalPoint = getNextHorizontalPoint(firstVerticalPoint);
                if (secondHorizontalPoint != FAIL)
                {
                    int thirdVerticalPoint = getNextVerticalPoint(secondHorizontalPoint);
                    if (thirdVerticalPoint != FAIL)
                    {
                        if (path[i][0] == path[i + 6][0] || path[i][1] == path[i + 6][1]) pis.Add(new List<int[]> { path[i], path[i + 1], path[i + 2], path[i + 3], path[i + 4], path[i + 5], path[i + 6] });
                        else sigmas.Add(new List<int[]> { path[i], path[i + 1], path[i + 2], path[i + 3], path[i + 4], path[i + 5], path[i + 6] });
                        if (!OVERLAP) i += 6;
                    }
                }
            }
        }
    }

    private void createLandmarks()
    {
        const int FACTOR = 3;
        // List<List<int[]>> landMarkLines = lines.GetRange(0, lines.Count / 2);
        // foreach (List<int[]> line in landMarkLines)
        // {
        //     if (line.Count > 1)
        //     {
        //         int[] midPoint = line[line.Count / 2];
        //         landmarks.Add(new Landmark(midPoint));
        //     }
        // }
        int totalLandMarks = lines.Count / FACTOR;
        landmarks.Add(new MazeLandmark(path[0]));
        for (int i = 0; i < totalLandMarks; i += 1)
        {
            List<List<int[]>> landMarkLines = lines.GetRange(i * FACTOR, FACTOR);
            List<int[]> maxLine = landMarkLines[0];
            foreach (List<int[]> line in landMarkLines)
            {
                if (line.Count > maxLine.Count)
                {
                    maxLine = line;
                }
            }
            if (maxLine.Count > 1 && maxLine.Count > 2)
            {
                int[] midPoint = maxLine[maxLine.Count / 2];
                if (midPoint[0] == path[0][0]) continue;
                landmarks.Add(new MazeLandmark(midPoint));
            }
        }

        if (landmarks.Count == 0) landmarks.Add(new MazeLandmark(path[path.Count / 2]));
        landmarks.Add(new MazeLandmark(path[path.Count - 1]));
    }

    public void printLandmarks()
    {
        foreach (MazeLandmark landmark in landmarks)
            landmark.Print();
    }
    public void printLines()
    {
        Console.WriteLine("Lines:");
        foreach (List<int[]> line in lines)
        {
            foreach (int[] point in line)
            {
                Console.Write("[" + point[0] + "," + point[1] + "] ");
            }
            Console.WriteLine();
        }
    }
    public void printShapes()
    {
        Console.WriteLine();
        Console.WriteLine("Pis:");
        foreach (List<int[]> pi in pis)
        {
            foreach (int[] point in pi)
            {
                Console.Write("[" + point[0] + "," + point[1] + "] ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("Sigmas:");
        foreach (List<int[]> sigma in sigmas)
        {
            foreach (int[] point in sigma)
            {
                Console.Write("[" + point[0] + "," + point[1] + "] ");
            }
            Console.WriteLine();
        }
    }
}