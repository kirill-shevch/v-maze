using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldScript : MonoBehaviour
{
    GameObject user;

    [SerializeField] private Boolean isClosed = true;


    // Start is called before the first frame update
    void Start()
    {
        user = GameObject.Find("XR Origin");
        var entrance = GetEntrance();
        var exit = GetExit();
        var maze = GetMaze();
        var middleCoordinates = new Tuple<float, float, float>((float)maze.GetLength(0) / 2,
            (float)maze.GetLength(1) / 2, (float)maze.GetLength(2) / 2);
        var cubePrefab = Resources.Load("Prefabs/block_brick_brown_1");
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
                        SpawnCube(i, j, h, mazeObject, cubePrefab);
                    }
                }
            }
        }

        SpawnCube(entrance.Item1, entrance.Item2, entrance.Item3, mazeObject, cubePrefab);
        SpawnCube(exit.Item1, exit.Item2, exit.Item3, mazeObject, cubePrefab);

        var exitArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
        exitArea.name = "End";
        exitArea.transform.position = new Vector3(exit.Item1, exit.Item2 + 0.5f, exit.Item3);
        exitArea.transform.localScale = new Vector3(1, 0.5f, 1);
        exitArea.transform.parent = mazeObject.transform;
        var render = exitArea.GetComponent<Renderer>();
        render.material = (Material)Resources.Load("Materials/transparentBlack");
        var collider = exitArea.GetComponent<Collider>();
        collider.isTrigger = true;
        collider.tag = "End";

        user.transform.position = new Vector3(entrance.Item1, entrance.Item2 + 1, entrance.Item3);
        // user.transform.position = new Vector3(entrance.Item1, entrance.Item2, entrance.Item3);
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
        var maze = new bool[3, 4, 5];
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

        maze[1, 1, 0] = false;
        maze[1, 1, 1] = false;
        maze[1, 1, 2] = false;
        maze[1, 2, 2] = false;
        maze[1, 2, 3] = false;
        maze[1, 2, 4] = false;
        return maze;
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

    private Tuple<int, int, int> GetEntrance()
    {
        return new Tuple<int, int, int>(1, 0, -1);
    }

    private Tuple<int, int, int> GetExit()
    {
        return new Tuple<int, int, int>(1, 1, 5);
    }
}