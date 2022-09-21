

// https://eskerda.com/bsp-dungeon-generation/


using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Transactions;

Random randGen = new Random();
double _ratio = 0.45;
bool _discard_by_ratio = true;



Container c = new Container(0, 0, 0, 19, 40, 12);

MazeLeaf d = grow_dungeon(c, 4, randGen, _discard_by_ratio, _ratio);

List<MazeLeaf> dd = d.get_level(4);

Room[] maze = populate_dungeon(dd);

foreach(Room rr in maze)
{
    rr.print();
}


Container[] split_Container_random(Container Container, Random gen, bool discard_by_ratio, double ratio)
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

    if (discard_by_ratio && (r0_ratio < ratio || r1_ratio < ratio))
    {
        Containers = split_Container_random(Container, gen, discard_by_ratio, ratio);
    }
    return Containers;
}

MazeLeaf grow_dungeon(Container Container, int iter, Random gen, bool discard_by_ratio, double ratio)
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

    // get the closes walls
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

    // create the corridor(s)
    bool is_diagonal = false;

    // TODO: see how many 90º angles the corridor should have
    // TODO: put the 1-2 angles within the 2nd third of the corridor
    // TODO: a (part of a) corridor is a Room having two of the w/h/d set to 1 and the last variable

    return rst;
}


public class Point3D
{
    public int _x;
    public int _y;
    public int _z;

    public Point3D(int x, int y, int z)
    {
        this._x = x;
        this._y = y;
        this._z = z;
    }
    
    public double euclidianDistanceTo(Point3D dest)
    {
        int x = this._x - dest._x;
        int y = this._y - dest._y;
        int z = this._z - dest._z;

        return Math.Pow(x * x + y * y + z * z, 0.5);
    }
}

public class Container
{
    public int _x;
    public int _y;
    public int _z;
    public int _w;
    public int _h;
    public int _d;
    public Point3D _center;

    public Container(int x, int y, int z, int w, int h, int d)
    {
        this._x = x;
        this._y = y;
        this._z = z;
        this._w = w;
        this._h = h;
        this._d = d;
        this._center = new Point3D(this._w / 2, this._h / 2, this._d / 2);
    }

    public void print()
    {
        Console.WriteLine("x: " + this._x.ToString() + " y: " + this._y.ToString() + " z: " + this._z.ToString());
        Console.WriteLine("\tw: " + this._w.ToString() + " h: " + this._h.ToString() + " d: " + this._d.ToString());
    }
}

public class MazeLeaf
{
    public Container _c;
    public MazeLeaf _r;
    public MazeLeaf _l;
    public MazeLeaf(Container content)
    {
        this._c = content;
        this._r = null;
        this._l = null;
    }

    public List<MazeLeaf> get_level(int level, List<MazeLeaf> arr = null)
    {
        if(arr == null)
        {
            arr = new List<MazeLeaf>();
        }

        if(level == 1)
        {
            arr.Add(this);
        }
        else
        {
            if(this._r != null)
            {
                this._r.get_level(level - 1, arr);
            }
            if(this._l != null)
            {
                this._l.get_level(level - 1, arr);
            }
        }
        return arr;
    }
}

public class Room
{
    public int _x;
    public int _y;
    public int _z;
    public int _w;
    public int _h;
    public int _d;

    Container _cnt;

    public Room(Container content, int padding = 3)
    {
        this._x = content._x + content._w / padding;
        this._y = content._y + content._h / padding;
        this._z = content._z + content._d / padding;

        this._w = content._w - (this._x - content._x) - content._w / padding;
        this._h = content._h - (this._y - content._y) - content._h / padding;
        this._d = content._d - (this._z - content._z) - content._d / padding;

        this._cnt = content;
    }

    public Room(int x, int y, int z)
    {
        _x = x;
        _y = y;
        _z = z;
        _w = 1;
        _h = 1;
        _d = 1;
        _cnt = null;
    }

    public void print()
    {
        Console.WriteLine("[c] x: " + this._cnt._x.ToString() + " y: " + this._cnt._y.ToString() + " z: " + this._cnt._z.ToString());
        Console.WriteLine("[c] \tw: " + this._cnt._w.ToString() + " h: " + this._cnt._h.ToString() + " d: " + this._cnt._d.ToString());

        Console.WriteLine("\t[r] x: " + this._x.ToString() + " y: " + this._y.ToString() + " z: " + this._z.ToString());
        Console.WriteLine("\t[r] \tw: " + this._w.ToString() + " h: " + this._h.ToString() + " d: " + this._d.ToString());
    }

    public Point3D center()
    {
        return new Point3D(this._x, this._y, this._z);
    }

    public Point3D[] face(int n)
    {
        Point3D[] face = new Point3D[4];

        switch (n)
        {
            case 0:
                face[0] = vertex(0);
                face[1] = vertex(1);
                face[2] = vertex(2);
                face[3] = vertex(3);
                break;
            case 1:
                face[0] = vertex(1);
                face[1] = vertex(2);
                face[2] = vertex(5);
                face[3] = vertex(6);
                break;
            case 2:
                face[0] = vertex(4);
                face[1] = vertex(5);
                face[2] = vertex(6);
                face[3] = vertex(7);
                break;
            case 3:
                face[0] = vertex(0);
                face[1] = vertex(3);
                face[2] = vertex(4);
                face[3] = vertex(7);
                break;
            case 4:
                face[0] = vertex(2);
                face[1] = vertex(3);
                face[2] = vertex(4);
                face[3] = vertex(7);
                break;
            case 5:
                face[0] = vertex(0);
                face[1] = vertex(1);
                face[2] = vertex(4);
                face[3] = vertex(5);
                break;
        }

        return face;
    }

    public Point3D face_center(int n)
    {
        Point3D center = null;

        switch (n)
        {
            case 0:
                center = new Point3D(this._x, this._y, this._z - this._d / 2);
                break;
            case 1:
                center = new Point3D(this._x + this._w / 2, this._y, this._z);
                break;
            case 2:
                center = new Point3D(this._x, this._y, this._z - this._d / 2);
                break;
            case 3:
                center = new Point3D(this._x - this._w / 2, this._y, this._z);
                break;
            case 4:
                center = new Point3D(this._x, this._y + this._h / 2, this._z);
                break;
            case 5:
                center = new Point3D(this._x, this._y - this._h / 2, this._z);
                break;
        }

        return center;
    }

    public Point3D vertex(int n)
    {
        Point3D rst = null;
        switch (n)
        {
            case 0:
                rst = new Point3D(this._x - this._w / 2, this._y - this._h / 2, this._z - this._d / 2);
                break;
            case 1:
                rst = new Point3D(this._x + this._w / 2, this._y - this._h / 2, this._z - this._d / 2);
                break;
            case 2:
                rst = new Point3D(this._x - this._w / 2, this._y + this._h / 2, this._z - this._d / 2);
                break;
            case 3:
                rst = new Point3D(this._x + this._w / 2, this._y + this._h / 2, this._z - this._d / 2);
                break;
            case 4:
                rst = new Point3D(this._x - this._w / 2, this._y - this._h / 2, this._z + this._d / 2);
                break;
            case 5:
                rst = new Point3D(this._x + this._w / 2, this._y - this._h / 2, this._z + this._d / 2);
                break;
            case 6:
                rst = new Point3D(this._x - this._w / 2, this._y + this._h / 2, this._z + this._d / 2);
                break;
            case 7:
                rst = new Point3D(this._x + this._w / 2, this._y + this._h / 2, this._z + this._d / 2);
                break;
        }

        return rst;
    }



    
}







