using Godot;
using System;

public partial class Nanot : Node2D
{
    public float MaxSpeed = 2.0f;
    public float MaxForce = 0.05f;
    
    public Vector2 Velocity = Vector2.Zero;
    public Vector2 Acceleration = Vector2.Zero;
    
    public int PoolIndex = -1;
    public bool IsDead = false;

    public void Initialize(Vector2 startPosition)
    {
        Position = startPosition;
        // Random velocity
        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();
        Velocity = new Vector2(rng.RandfRange(-1, 1), rng.RandfRange(-1, 1)).Normalized() * rng.RandfRange(1, 3);
    }
    
    public void ApplyForce(Vector2 force)
    {
        Acceleration += force;
        // Sanitizar aceleración
        if (float.IsNaN(Acceleration.X) || float.IsNaN(Acceleration.Y)) {
            Acceleration = Vector2.Zero;
        }
    }
    
    public void AgentUpdate(Vector2 bounds)
    {
        if (IsDead) return;
        
        Velocity += Acceleration;
        Velocity = Velocity.LimitLength(MaxSpeed);
        Position += Velocity;
        Acceleration = Vector2.Zero;
        
        CheckEdges(bounds);
        Rotation = Velocity.Angle() + (Mathf.Pi / 2.0f);
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
