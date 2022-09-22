

// https://eskerda.com/bsp-dungeon-generation/


using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Transactions;


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

    public bool isLinkingSegmentDiagonal(Point3D dest)
    {
        return !((this._x == dest._x && this._y == dest._y) || (this._x == dest._x && this._z == dest._z) || (this._y == dest._y && this._z == dest._z));
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

    int _padding;

    Container _cnt;

    public Room(Container content, int padding = 3)
    {
        this._x = content._x;
        this._y = content._y;
        this._z = content._z;

        this._w = 2 * content._w / padding;
        this._h = 2 * content._h / padding;
        this._d = 2 * content._d / padding;

        this._padding = padding;
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
        return new Point3D(
            this._cnt._x + this._cnt._w / this._padding, 
            this._cnt._y + this._cnt._h / this._padding, 
            this._cnt._z + this._cnt._d / this._padding)
        ;
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







