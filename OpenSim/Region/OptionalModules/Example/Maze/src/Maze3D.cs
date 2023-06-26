using System;
using System.Collections.Generic;



public class Maze3D
{
    private int _width;
    private int _height;
    private int _depth;
    private Cell[,,] _cells;
    private Random _random = new Random();

    public Maze3D(int width, int height, int depth)
    {
        _width = width;
        _height = height;
        _depth = depth;
        _cells = new Cell[width, height, depth];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    _cells[x, y, z] = new Cell();
                }
            }
        }
        // Choose a random cell to start from
        int startX = 0;
        int startY = 0;
        int startZ = 0;

        int endX = width - 1;
        int endY = height - 1;
        int endZ = depth - 1;

        _cells[startX, startY, startZ].TopWall = false;
        _cells[endX, endY, endZ].LeftWall = false;
        _cells[endX, endY, endZ].BackWall = false;
        GenerateMazeRecursive(startX, startY, startZ);
    }
    public void GenerateMazeRecursive(int x, int y, int z)
    {
        _cells[x, y, z].Visited = true;

        List<int[]> neighbors = GetUnvisitedNeighbors(x, y, z);

        while (neighbors.Count > 0)
        {
            int[] neighbor = neighbors[_random.Next(neighbors.Count)];
            neighbors.Remove(neighbor);

            int nx = neighbor[0];
            int ny = neighbor[1];
            int nz = neighbor[2];

            if (!_cells[nx, ny, nz].Visited)
            {
                RemoveWall(x, y, z, nx, ny, nz);
                GenerateMazeRecursive(nx, ny, nz);
            }
        }
    }
    private List<int[]> GetUnvisitedNeighbors(int x, int y, int z)
    {
        List<int[]> neighbors = new List<int[]>();
        if (x > 0 && !_cells[x - 1, y, z].Visited)
        {
            neighbors.Add(new int[] { x - 1, y, z });
        }
        if (x < _width - 1 && !_cells[x + 1, y, z].Visited)
        {
            neighbors.Add(new int[] { x + 1, y, z });
        }
        if (y > 0 && !_cells[x, y - 1, z].Visited)
        {
            neighbors.Add(new int[] { x, y - 1, z });
        }
        if (y < _height - 1 && !_cells[x, y + 1, z].Visited)
        {
            neighbors.Add(new int[] { x, y + 1, z });
        }
        if (z > 0 && !_cells[x, y, z - 1].Visited)
        {
            neighbors.Add(new int[] { x, y, z - 1 });
        }
        if (z < _depth - 1 && !_cells[x, y, z + 1].Visited)
        {
            neighbors.Add(new int[] { x, y, z + 1 });
        }
        return neighbors;
    }
    private void RemoveWall(int x, int y, int z, int nx, int ny, int nz)
    {
        if (x == nx && z == nz)
        {
            // Same column
            if (y < ny)
            {
                // Cell is above neighbor
                _cells[x, y, z].BottomWall = false;
                _cells[nx, ny, nz].TopWall = false;
            }
            else
            {
                // Cell is below neighbor
                _cells[x, y, z].TopWall = false;
                _cells[nx, ny, nz].BottomWall = false;
            }
        }
        else if (y == ny && z == nz)
        {
            // Same row
            if (x < nx)
            {
                // Cell is left of neighbor
                _cells[x, y, z].RightWall = false;
                _cells[nx, ny, nz].LeftWall = false;
            }
            else
            {
                // Cell is right of neighbor
                _cells[x, y, z].LeftWall = false;
                _cells[nx, ny, nz].RightWall = false;
            }
        }
        else if (z == nz)
        {
            // Same row
            if (z < nz)
            {
                // Cell is left of neighbor
                _cells[x, y, z].FrontWall = false;
                _cells[nx, ny, nz].BackWall = false;
            }
            else
            {
                // Cell is right of neighbor
                _cells[x, y, z].BackWall = false;
                _cells[nx, ny, nz].FrontWall = false;
            }
        }
    }
    public void printMaze()
    {
        for (int z = 0; z < _depth; z++)
        {
            for (int y = 0; y < _height; y++)

            {
                // Print the top wall
                for (int x = 0; x < _width; x++)
                {
                    Console.Write(_cells[x, y, z].TopWall ? "+--" : "+  ");
                }
                Console.WriteLine("+");

                for (int x = 0; x < _width; x++)
                {
                    Console.Write(_cells[x, y, z].LeftWall ? "|  " : "   ");
                }

                // Print the bottom wall
                Console.WriteLine("|");
            }
            for (int x = 0; x < _width; x++)
            {
                Console.Write(_cells[x, _height - 1, z].BottomWall ? "+--" : "+  ");
            }
            Console.WriteLine("+");
            Console.WriteLine();
        }
    }
    public Cell[,,] getCells()
    {
        return _cells;
    }
}




public class BinaryMaze3D
{
    private int _width;
    private int _height;
    private int _depth;

    private int[,,] _cells;

    public BinaryMaze3D(Cell[,,] cells)
    {
        _width = cells.GetLength(0);
        _height = cells.GetLength(1);
        _depth = cells.GetLength(2) + 1;

        _cells = new int[_width * 2 + 1, _height * 2 + 1, _depth];

        for (int z = 0; z < _depth; z++)
        {
            for (int y = 0; y < _height * 2 + 1; y++)

            {
                for (int x = 0; x < _width * 2 + 1; x++)
                {
                    _cells[x, y, z] = 1;
                }
            }
        }


        Console.WriteLine("Maze");
        for (int z = 1; z < _depth; z++)
        {
            for (int y = 0; y < _height; y++)

            {
                for (int x = 0; x < _width; x++)
                {
                    _cells[x * 2, y * 2 + 1, z - 1] = cells[x, y, z - 1].LeftWall ? 1 : 0;
                    _cells[x * 2 + 1, y * 2, z - 1] = cells[x, y, z - 1].TopWall ? 1 : 0;
                    _cells[x * 2 + 1, y * 2 + 1, z - 1] = 0;
                }
                Console.WriteLine();
            }
        }
    }

    public void print()
    {
        for (int z = 0; z < _depth; z++)
        {
            for (int y = 0; y < _height * 2 + 1; y++)

            {
                for (int x = 0; x < _width * 2 + 1; x++)
                {
                    Console.Write(_cells[x, y, z]);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
