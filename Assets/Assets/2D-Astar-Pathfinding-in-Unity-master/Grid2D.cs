using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Grid2D : MonoBehaviour
{
    public Vector3 gridWorldSize;
    public float nodeRadius;
    public Node2D[,] Grid;
    public Tilemap floormap;
    public Tilemap wallmap;
    public Tilemap boxmap;

    Vector3 worldBottomLeft;

    float nodeDiameter;
    public int gridSizeX, gridSizeY;
    public Vector3 offset;
    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    

    void CreateGrid()
    {
        Grid = new Node2D[gridSizeX, gridSizeY];
        worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius) + offset;
                Grid[x, y] = new Node2D(false, worldPoint, x, y);

                if (   wallmap.HasTile(wallmap.WorldToCell(Grid[x, y].worldPosition)) 
                    || boxmap.HasTile(boxmap.WorldToCell(Grid[x, y].worldPosition))
                    || floormap.HasTile(floormap.WorldToCell(Grid[x, y].worldPosition)) == false)
                    Grid[x, y].SetObstacle(true);
                else
                    Grid[x, y].SetObstacle(false);


            }
        }
    }
    void Update() {
        RecalculateGrid();
    }
    public void RecalculateGrid() {
        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                if (wallmap.HasTile(wallmap.WorldToCell(Grid[x, y].worldPosition))
                    || boxmap.HasTile(boxmap.WorldToCell(Grid[x, y].worldPosition))
                    || floormap.HasTile(floormap.WorldToCell(Grid[x, y].worldPosition)) == false)
                    Grid[x, y].SetObstacle(true);
                else
                    Grid[x, y].SetObstacle(false);
            }
        }
    }

    //gets the neighboring nodes in the 4 cardinal directions. If you would like to enable diagonal pathfinding, uncomment out that portion of code
    public List<Node2D> GetNeighbors(Node2D node)
    {
        List<Node2D> neighbors = new List<Node2D>();

        //checks and adds top neighbor
        if (node.GridX >= 0 && node.GridX < gridSizeX && node.GridY + 1 >= 0 && node.GridY + 1 < gridSizeY)
            neighbors.Add(Grid[node.GridX, node.GridY + 1]);

        //checks and adds bottom neighbor
        if (node.GridX >= 0 && node.GridX < gridSizeX && node.GridY - 1 >= 0 && node.GridY - 1 < gridSizeY)
            neighbors.Add(Grid[node.GridX, node.GridY - 1]);

        //checks and adds right neighbor
        if (node.GridX + 1 >= 0 && node.GridX + 1 < gridSizeX && node.GridY >= 0 && node.GridY < gridSizeY)
            neighbors.Add(Grid[node.GridX + 1, node.GridY]);

        //checks and adds left neighbor
        if (node.GridX - 1 >= 0 && node.GridX - 1 < gridSizeX && node.GridY >= 0 && node.GridY < gridSizeY)
            neighbors.Add(Grid[node.GridX - 1, node.GridY]);



        /* Uncomment this code to enable diagonal movement
        
        //checks and adds top right neighbor
        if (node.GridX + 1 >= 0 && node.GridX + 1< gridSizeX && node.GridY + 1 >= 0 && node.GridY + 1 < gridSizeY)
            neighbors.Add(Grid[node.GridX + 1, node.GridY + 1]);

        //checks and adds bottom right neighbor
        if (node.GridX + 1>= 0 && node.GridX + 1 < gridSizeX && node.GridY - 1 >= 0 && node.GridY - 1 < gridSizeY)
            neighbors.Add(Grid[node.GridX + 1, node.GridY - 1]);

        //checks and adds top left neighbor
        if (node.GridX - 1 >= 0 && node.GridX - 1 < gridSizeX && node.GridY + 1>= 0 && node.GridY + 1 < gridSizeY)
            neighbors.Add(Grid[node.GridX - 1, node.GridY + 1]);

        //checks and adds bottom left neighbor
        if (node.GridX - 1 >= 0 && node.GridX - 1 < gridSizeX && node.GridY  - 1>= 0 && node.GridY  - 1 < gridSizeY)
            neighbors.Add(Grid[node.GridX - 1, node.GridY - 1]);
        */



        return neighbors;
    }


    public Node2D NodeFromWorldPoint(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x - offset.x - 1 + (gridSizeX / 2));
        int y = Mathf.RoundToInt(worldPosition.y - offset.y + (gridSizeY / 2));
        return Grid[x, y];
    }


    
    //Draws visual representation of grid
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + offset, new Vector3(gridWorldSize.x, gridWorldSize.y, 1) );

        if (Grid != null)
        {
            foreach (Node2D n in Grid)
            {
                if (n.obstacle)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.white;

                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeRadius));

            }
        }
    }
}
