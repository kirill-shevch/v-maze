using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldScript : MonoBehaviour
{
    GameObject user;
    public static int maze_width = 8;
    public static int maze_height = 6;
    public static int maze_depth = 8;

    void Start()
    {
        var Rand = new System.Random();
        user = GameObject.Find("XR Origin");
        var entrance = GetEntrance();
        var exit = this.GetExit();
        var maze = GetMaze();
        var middleCoordinates = new Tuple<float, float, float>((float)maze.GetLength(0) / 2,
            (float)maze.GetLength(1) / 2, (float)maze.GetLength(2) / 2);
        var cubePrefab0 = Resources.Load("Prefabs/dungeon/dungeon_block_0");
        var cubePrefab1 = Resources.Load("Prefabs/dungeon/dungeon_block_1");
        var mazeObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mazeObject.transform.position =
            new Vector3(middleCoordinates.Item1, middleCoordinates.Item2, middleCoordinates.Item3);
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
                        int randVal = Rand.Next(2);
                        if (randVal == 1)
                            SpawnCube(i, j, h, mazeObject, cubePrefab0);
                        else
                            SpawnCube(i, j, h, mazeObject, cubePrefab1);
                    }
                }
            }
        }

        SpawnCube(entrance.X, entrance.Y, entrance.Z, mazeObject, cubePrefab0);
        SpawnCube(exit.X, exit.Y, exit.Z, mazeObject, cubePrefab1);
        SpawnExit(exit, mazeObject);
        user.transform.position = new Vector3(entrance.X, entrance.Y + 1, entrance.Z);
    }

    private void SpawnExit(Point3D exit, GameObject mazeObject)
    {
        var exitArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
        exitArea.name = "End";
        exitArea.transform.position = new Vector3(exit.X, exit.Y + 0.5f, exit.Z);
        exitArea.transform.localScale = new Vector3(1, 0.5f, 1);
        exitArea.transform.parent = mazeObject.transform;
        var render = exitArea.GetComponent<Renderer>();
        render.material = (Material)Resources.Load("Materials/Effect_03");
        var collider = exitArea.GetComponent<Collider>();
        collider.isTrigger = true;
        collider.tag = "End";
        var audioSource = exitArea.AddComponent<AudioSource>();
        audioSource.clip = Resources.Load<AudioClip>("Sounds/Teleport-exit");
    }

    // Update is called once per frame
    void Update()
    {
        if (user.transform.position.y < -150)
        {
            SceneManager.LoadScene("MovementScene");
        }
    }

    private bool[,,] GetMaze()
    {
        var maze = new bool[maze_width, maze_height, maze_depth];
        for (int i = 0; i < maze.GetLength(0); i++)
        {
            for (int j = 0; j < maze.GetLength(1); j++)
            {
                for (int h = 0; h < maze.GetLength(2); h++)
                {
                    maze[i, j, h] = true;
                }
            }
        }

        //maze[1, 1, 0] = false;
        //maze[1, 1, 1] = false;
        //maze[1, 1, 2] = false;
        //maze[1, 2, 2] = false;
        //maze[1, 2, 3] = false;
        //maze[1, 2, 4] = false;

        // Optionally close the entrance, exist to simulate lightning in closed env.
        // if (!isClosed)
        // {
        //     maze[1, 1, 0] = false;
        //     maze[1, 2, 4] = false;
        // }

        // return maze


        SpawnCoridors(maze);
        SpawnRooms(maze);
        return maze;
    }

    private void SpawnRooms(bool[,,] maze)
    {
        System.Random randGen = new System.Random();
        var counter = 0;
        while (true)
        {
            if (counter > (maze_width * maze_height * maze_depth) / 6)
            {
                return;
            }
            var size = randGen.Next(3, 5);
            var x = randGen.Next(0, maze_width);
            var y = randGen.Next(0, maze_height);
            var z = randGen.Next(0, maze_depth);
            for (int i = x; i < x + size; i++)
            {
                for (int j = y; j < y + size; j++)
                {
                    for (int k = z; k < z + size; k++)
                    {
                        if (i < maze_width && j < maze_height && k < maze_depth)
                        {
                            maze[i, j, k] = false;
                            counter++;
                        }
                    }
                }
            }

        }
    }

    private void SpawnCoridors(bool[,,] maze)
    {
        var currentPoint = GetEntrance();
        var endpoint = GetExit();
        currentPoint.Y++;
        currentPoint.Z++;
        endpoint.Y++;
        endpoint.Z--;
        System.Random randGen = new System.Random();
        maze[currentPoint.X, currentPoint.Y, currentPoint.Z] = false;
        while (true)
        {
            var direction = randGen.Next(0, 3);
            var length = randGen.Next(1, 4);
            for (int i = 0; i < length; i++)
            {
                switch (direction)
                {
                    case 0:
                        {
                            if (currentPoint.X < endpoint.X)
                            {
                                currentPoint.X++;
                            }
                            break;
                        }
                    case 1:
                        {
                            if (currentPoint.Y < endpoint.Y)
                            {
                                currentPoint.Y++;
                            }
                            break;
                        }
                    case 2:
                        {
                            if (currentPoint.Z < endpoint.Z)
                            {
                                currentPoint.Z++;
                            }
                            break;
                        }
                    default:
                        break;
                }
                maze[currentPoint.X, currentPoint.Y, currentPoint.Z] = false;
                if (currentPoint.X == endpoint.X && currentPoint.Y == endpoint.Y && currentPoint.Z == endpoint.Z)
                {
                    return;
                }
            }
        }
    }

    private GameObject SpawnCube(int x, int y, int z, GameObject mazeObject, UnityEngine.Object cubePrefab)
    {
        var cubeObject = (GameObject)Instantiate(cubePrefab, new Vector3(x, y, z), Quaternion.identity);
        cubeObject.transform.parent = mazeObject.transform;
        var rigidbody = cubeObject.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = false;
        rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        cubeObject.AddComponent<BoxCollider>();
        return cubeObject;
    }

    private Point3D GetEntrance()
    {
        return new Point3D(1, 0, -1);
    }

    private Point3D GetExit()
    {
        return new Point3D(maze_width - 2, maze_height - 2, maze_depth);
    }

    private class Point3D
    {
        public int X;
        public int Y;
        public int Z;

        public Point3D(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}