using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var maze = GetMaze();
        for (int i = 0; i < maze.GetLength(0); i++)
        {
            for (int j = 0; j < maze.GetLength(1); j++)
            {
                for (int h = 0; h < maze.GetLength(2); h++)
                {
                    if (maze[i, j, h])
                    {
                        UnityEngine.Object prefab = Resources.Load("Prefabs/block_brick_brown_1"); // Assets/Resources/Prefabs/prefab1.FBX
                        GameObject t = (GameObject)Instantiate(prefab, new Vector3(i, j, h), Quaternion.identity);
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private bool[,,] GetMaze()
    {
        var maze = new bool[3, 4, 5];
        for (int i = 0; i < maze.GetLength(0); i++)
        {
            for (int j = 0; j < maze.GetLength(1); j++)
            {
                for (int h = 0; h < maze.GetLength(2); h++)
                {
                    maze[i,j,h] = true;
                }
            }
        }
        maze[1,1,0] = false;
        maze[1,1,1] = false;
        maze[1,1,2] = false;
        maze[1,2,2] = false;
        maze[1,2,3] = false;
        maze[1,2,4] = false;
        return maze;
    }
}
