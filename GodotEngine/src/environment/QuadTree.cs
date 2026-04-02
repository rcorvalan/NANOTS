using Godot;
using System.Collections.Generic;

public class QuadTree
{
    public Rect2 Boundary;
    public int Capacity;
    public List<Nanot> Points;
    public bool Divided;

    public QuadTree NorthEast;
    public QuadTree NorthWest;
    public QuadTree SouthEast;
    public QuadTree SouthWest;

    public QuadTree(Rect2 boundary, int capacity)
    {
        Boundary = boundary;
        Capacity = capacity;
        Points = new List<Nanot>();
        Divided = false;
    }

    public void Subdivide()
    {
        float x = Boundary.Position.X;
        float y = Boundary.Position.Y;
        float w = Boundary.Size.X / 2;
        float h = Boundary.Size.Y / 2;

        Rect2 ne = new Rect2(x + w, y, w, h);
        NorthEast = new QuadTree(ne, Capacity);

        Rect2 nw = new Rect2(x, y, w, h);
        NorthWest = new QuadTree(nw, Capacity);

        Rect2 se = new Rect2(x + w, y + h, w, h);
        SouthEast = new QuadTree(se, Capacity);

        Rect2 sw = new Rect2(x, y + h, w, h);
        SouthWest = new QuadTree(sw, Capacity);

        Divided = true;
    }

    public bool Insert(Nanot nanot)
    {
        if (!Boundary.HasPoint(nanot.Position))
        {
            return false;
        }

        if (Points.Count < Capacity)
        {
            Points.Add(nanot);
            return true;
        }
        else
        {
            if (!Divided)
            {
                Subdivide();
            }

            if (NorthEast.Insert(nanot)) return true;
            if (NorthWest.Insert(nanot)) return true;
            if (SouthEast.Insert(nanot)) return true;
            if (SouthWest.Insert(nanot)) return true;
        }
        return false;
    }

    public void Query(Rect2 range, List<Nanot> found)
    {
        if (!Boundary.Intersects(range) && !Boundary.Encloses(range) && !range.Encloses(Boundary))
        {
            return;
        }

        foreach (var p in Points)
        {
            if (range.HasPoint(p.Position))
            {
                found.Add(p);
            }
        }

        if (Divided)
        {
            NorthWest.Query(range, found);
            NorthEast.Query(range, found);
            SouthWest.Query(range, found);
            SouthEast.Query(range, found);
        }
    }
}
