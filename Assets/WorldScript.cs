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
        var middleCoordinates = new Tuple<float, float, float>((float)maze.GetLength(0) / 2, (float)maze.GetLength(1) / 2, (float)maze.GetLength(2) / 2);
        var cubePrefab = Resources.Load("Prefabs/block_brick_brown_1"); // Assets/Resources/Prefabs/prefab1.FBX
        var mazeObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mazeObject.transform.position = new Vector3(middleCoordinates.Item1, middleCoordinates.Item2, middleCoordinates.Item3);
        mazeObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        mazeObject.name = "maze";

        for (int i = 0; i < maze.GetLength(0); i++)
        {
            for (int j = 0; j < maze.GetLength(1); j++)
            {
                for (int h = 0; h < maze.GetLength(2); h++)
                {
                    if (maze[i, j, h])
                    {
                        var cubeObject = (GameObject)Instantiate(cubePrefab, new Vector3(i, j, h), Quaternion.identity);
                        cubeObject.transform.parent = mazeObject.transform;
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
