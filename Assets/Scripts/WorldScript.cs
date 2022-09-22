using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldScript : MonoBehaviour
{
    GameObject user;
    int maze_width = 8;
    int maze_height = 8;
    int maze_depth = 8;

    [SerializeField] private Boolean isClosed = true;

    // Start is called before the first frame update
    void Start()
    {
        user = GameObject.Find("XR Origin");
        var entrance = GetEntrance();
        var exit = this.GetExit();
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

        SpawnCube(entrance.X, entrance.Y, entrance.Z, mazeObject, cubePrefab);
        SpawnCube(exit.X, exit.Y, exit.Z, mazeObject, cubePrefab);

        var exitArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
        exitArea.name = "End";
        exitArea.transform.position = new Vector3(exit.X, exit.Y + 0.5f, exit.Z);
        exitArea.transform.localScale = new Vector3(1, 0.5f, 1);
        exitArea.transform.parent = mazeObject.transform;
        var render = exitArea.GetComponent<Renderer>();
        render.material = (Material)Resources.Load("Materials/transparentBlack");
        var collider = exitArea.GetComponent<Collider>();
        collider.isTrigger = true;
        collider.tag = "End";

        user.transform.position = new Vector3(entrance.X, entrance.Y + 1, entrance.Z);
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

    Container[] split_Container_random(Container Container, System.Random gen, bool discard_by_ratio, double ratio)
    {
        Container[] Containers = new Container[2];
        int side = gen.Next(3);
        double r0_ratio = 0;
        double r1_ratio = 0;
        if (side == 0) // x-axis
        {
            Containers[0] = new Container(Container._x, Container._y, Container._z, gen.Next(Container._w), Container._h, Container._d);
            Containers[1] = new Container(Container._x + Containers[0]._w, Container._y, Container._z, Container._w - Containers[0]._w, Container._h, Container._d);
            r0_ratio = 1.0 * Containers[0]._w / Containers[0]._h;
            r1_ratio = 1.0 * Containers[1]._w / Containers[1]._h;

        }
        else if (side == 1) // y-axis
        {
            Containers[0] = new Container(Container._x, Container._y, Container._z, Container._w, gen.Next(Container._h), Container._d);
            Containers[1] = new Container(Container._x, Container._y + Containers[0]._h, Container._z, Container._w, Container._h - Containers[0]._h, Container._d);
            r0_ratio = 1.0 * Containers[0]._w / Containers[0]._h;
            r1_ratio = 1.0 * Containers[1]._w / Containers[1]._h;
        }
        else // z-axis
        {
            Containers[0] = new Container(Container._x, Container._y, Container._z, Container._w, Container._h, gen.Next(Container._d));
            Containers[1] = new Container(Container._x, Container._y, Container._z + Containers[0]._d, Container._w, Container._h, Container._d - Containers[0]._d);
            r0_ratio = 1.0 * Containers[0]._w / Containers[0]._h;
            r1_ratio = 1.0 * Containers[1]._w / Containers[1]._h;
        }

        if (discard_by_ratio && (r0_ratio < ratio || r1_ratio < ratio) && Container._x < 4 && Container._y < 4 && Container._z < 4)
        {
            Containers = split_Container_random(Container, gen, discard_by_ratio, ratio);
        }
        return Containers;
    }

    MazeLeaf grow_dungeon(Container Container, int iter, System.Random gen, bool discard_by_ratio, double ratio)
    {
        MazeLeaf root = new MazeLeaf(Container);
        if(iter > 0)
        {
            Container[] split = split_Container_random(Container, gen, discard_by_ratio, ratio);
            root._r = grow_dungeon(split[0], iter - 1, gen, discard_by_ratio, ratio);
            root._l = grow_dungeon(split[1], iter - 1, gen, discard_by_ratio, ratio);
        }
        return root;
    }

    Room[] populate_dungeon(List<MazeLeaf> maze)
    {
        Room[] rooms = new Room[maze.Count];
        for(int ii = 0; ii < maze.Count; ii++)
        {
            rooms[ii] = new Room(maze[ii]._c, 5);
        }

        Room[] corridors = create_corridors(rooms);

        return rooms.Concat(corridors).ToArray();
    }

    Room[] create_corridors(Room[] rooms)
    {
        List<Room> cpy = rooms.ToList();
        List<Room> cor = new List<Room>();

        Room ori = cpy.First();
        while (cpy.Count > 0)
        {
            double[] d = new double[cpy.Count];
            for (int ii = 0; ii < cpy.Count; ii++)
            {

                d[ii] = ori.center().euclidianDistanceTo(cpy[ii].center());
            }

            int dst_idx = Array.IndexOf(d, d.Min());
            Room dst = cpy.ElementAt(dst_idx);
            cpy.RemoveAt(dst_idx);

            cor.Concat(create_corridors_between_rooms(ori, dst)).ToList();
        }

        return cor.ToArray();
    }

    // TODO: this function is not finished
    List<Room> create_corridors_between_rooms(Room ori, Room dst)
    {
        List<Room> rst = new List<Room>();

        // get the closest walls
        int[] face_1 = new int[6];
        for(int ii = 0; ii < 6; ii++)
        {
            double[] dist_2 = new double[6];
            for (int jj = 0; jj < 6; jj++)
            {
                dist_2[jj] = ori.face_center(ii).euclidianDistanceTo(dst.face_center(jj));
            }
            face_1[ii] = Array.IndexOf(dist_2, dist_2.Min());
        }
        double[] dist_1 = new double[6];
        for(int ii = 0; ii < 6; ii++)
        {
            dist_1[ii] = ori.face_center(ii).euclidianDistanceTo(dst.face_center(face_1[ii]));
        }
        int ori_wall = Array.IndexOf(dist_1, dist_1.Min());
        int dst_wall = face_1[ori_wall];

        Point3D ori_face_central_point = ori.face_center(ori_wall);
        Point3D dst_face_central_point = dst.face_center(dst_wall);

        // create the corridor(s)
        bool is_diagonal = ori_face_central_point.isLinkingSegmentDiagonal(dst_face_central_point);

        //if (!is_diagonal)
        //{
        //    rst.Concat(Room())
        //}
        // TODO: see how many 90º angles the corridor should have
        // TODO: put the 1-2 angles within the 2nd third of the corridor
        // TODO: a (part of a) corridor is a Room having two of the w/h/d set to 1 and the last variable

        return rst;
    }

    private bool[,,] MazeToBool(Room[] maze, int width, int height, int depth)
    {
        var bool_maze = new bool[width, height, depth];
        for (int i = 0; i < bool_maze.GetLength(0); i++)
        {
            for (int j = 0; j < bool_maze.GetLength(1); j++)
            {
                for (int h = 0; h < bool_maze.GetLength(2); h++)
                {
                    bool_maze[i,j,h] = true;
                }
            }
        }

        foreach (Room room in maze)
        {
            for(int x = room._x; x < room._x + room._w; x++)
            {
                for(int y = room._y; y < room._y + room._h; y++)
                {
                    for(int z = room._z; z < room._z + room._d; z++)
                    {
                        bool_maze[x, y, z] = false;
                    }
                }
            }
        }
        return bool_maze;
    }


    private bool[,,] GetMaze()
    {
        //var maze = new bool[3, 4, 5];
        //for (int i = 0; i < maze.GetLength(0); i++)
        //{
        //    for (int j = 0; j < maze.GetLength(1); j++)
        //    {
        //        for (int h = 0; h < maze.GetLength(2); h++)
        //        {
        //            maze[i, j, h] = true;
        //        }
        //    }
        //}

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

        System.Random randGen = new System.Random();
        double _ratio = 0.45;
        bool _discard_by_ratio = true;

        int division_level = 4;

        Container c = new Container(0, 0, 0, maze_width, maze_height, maze_depth);

        MazeLeaf d = grow_dungeon(c, division_level, randGen, _discard_by_ratio, _ratio);

        List<MazeLeaf> dd = d.get_level(division_level);

        Room[] maze = populate_dungeon(dd);

        //foreach(Room rr in maze)
        //{
        //    rr.print();
        //}

        var boolMaze = MazeToBool(maze, maze_width, maze_height, maze_depth);
        SpawnCoridors(boolMaze);
        return boolMaze;
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
}