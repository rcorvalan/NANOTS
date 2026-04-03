using Godot;
using System.Collections.Generic;

public partial class Predator : Node2D
{
    public float MaxSpeed = 3.5f; // Faster than normal nanots
    public float MaxForce = 0.1f;
    
    public Vector2 Velocity = Vector2.Zero;
    public Vector2 Acceleration = Vector2.Zero;
    
    public int Age = 0;
    public float Biomass = 200f; // 200 base, dies if hits 0
    public bool IsDead => Biomass <= 0;

    public void Initialize(Vector2 startPosition)
    {
        Position = startPosition;
        Velocity = new Vector2((float)GD.RandRange(-1, 1), (float)GD.RandRange(-1, 1)).Normalized() * MaxSpeed;
    }
    
    public void ApplyForce(Vector2 force)
    {
        Acceleration += force;
        if (float.IsNaN(Acceleration.X) || float.IsNaN(Acceleration.Y)) {
            Acceleration = Vector2.Zero;
        }
    }

    public void Hunt(QuadTree qt, Vector2 bounds, StigmergicGrid grid = null)
    {
        if (IsDead) return;
        
        Age++;
        Biomass -= 0.5f; // High metabolism! Much higher drain than Nanots

        // Buscar presa cercana en QuadTree (Radio 150)
        List<Nanot> prey = new List<Nanot>();
        qt.Query(new Rect2(Position.X - 150, Position.Y - 150, 300, 300), prey);
        
        Vector2 centerOfMass = Vector2.Zero;
        int count = 0;
        
        Nanot target = null;
        float closestDist = float.MaxValue;
        
        foreach(var p in prey)
        {
            if (p.IsDead) continue;
            float dist = Position.DistanceTo(p.Position);
            
            // Si lo toca (Distancia < 5), lo devora
            if (dist < 5.0f) {
                Biomass += 100f; // Satiety reward!
                p.Metabolism.Biomass = 0; // Mata al nanot instantáneamente
                p.Die();
            }
            
            if (dist < closestDist) {
                closestDist = dist;
                target = p;
            }
            
            centerOfMass += p.Position;
            count++;
        }
        
        // Comportamiento Orientado a Objetivos
        if (target != null) {
            Vector2 desired = (target.Position - Position).Normalized() * MaxSpeed;
            Vector2 steer = (desired - Velocity).LimitLength(MaxForce);
            ApplyForce(steer);
        } else if (count > 0) {
            // Ir hacia el centro de masa del grupo más cercano
            centerOfMass /= count;
            Vector2 desired = (centerOfMass - Position).Normalized() * MaxSpeed;
            Vector2 steer = (desired - Velocity).LimitLength(MaxForce);
            ApplyForce(steer);
        }
        
        Velocity += Acceleration;
        Velocity = Velocity.LimitLength(MaxSpeed);
        
        Vector2 nextPos = Position + Velocity;
        
        // Colisión con estigmergia (barreras físicas)
        if (grid != null) {
            if (grid.CheckTile(nextPos) > 0) {
                // Hay obstáculo
                Velocity = -Velocity * 0.5f; 
                nextPos = Position + Velocity;
                if (grid.CheckTile(nextPos) > 0) {
                    nextPos = Position;
                    Velocity = Vector2.Zero;
                }
            }
        }
        
        Position = nextPos;
        Acceleration = Vector2.Zero;
        
        CheckEdges(bounds);
        Rotation = Velocity.Angle() + (Mathf.Pi / 2.0f);
        
        if (IsDead) {
            QueueFree();
        } else {
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        Vector2[] points = new Vector2[4];
        float s = 6.0f;
        points[0] = new Vector2(0, -s * 2);
        points[1] = new Vector2(s, 0);
        points[2] = new Vector2(0, s * 2);
        points[3] = new Vector2(-s, 0);
        
        DrawPolygon(points, new Color[] { Colors.Orange, Colors.Orange, Colors.Orange, Colors.Orange });
    }

    private void CheckEdges(Vector2 bounds)
    {
        Vector2 pos = Position;
        if (pos.X < 0) { pos.X = 0; Velocity.X *= -1; }
        if (pos.X > bounds.X) { pos.X = bounds.X; Velocity.X *= -1; }
        if (pos.Y < 0) { pos.Y = 0; Velocity.Y *= -1; }
        if (pos.Y > bounds.Y) { pos.Y = bounds.Y; Velocity.Y *= -1; }
        Position = pos;
    }
}
