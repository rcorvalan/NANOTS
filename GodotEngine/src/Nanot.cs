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
    public bool CanReproduce = false;
    public int Age = 0;
    
    public float RadioFrequency = 0f;
    public float CurrentBroadcastSignal = 0f;
    
    public MetabolicSynthesis Metabolism;

    public void Initialize(Vector2 startPosition)
    {
        Position = startPosition;
        Metabolism = new MetabolicSynthesis(150f, 10f); // Más vida inicial
        
        // Random velocity
        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();
        Velocity = new Vector2(rng.RandfRange(-1, 1), rng.RandfRange(-1, 1)).Normalized() * rng.RandfRange(1, 3);
        RadioFrequency = rng.RandfRange(0f, 1f); // Semilla de facción aleatoria
    }
    
    public void ApplyForce(Vector2 force)
    {
        Acceleration += force;
        // Sanitizar aceleración
        if (float.IsNaN(Acceleration.X) || float.IsNaN(Acceleration.Y)) {
            Acceleration = Vector2.Zero;
        }
    }
    
    public void AgentUpdate(Vector2 bounds, float dt = 1.0f)
    {
        if (IsDead) return;
        
        Age++;
        
        Velocity += Acceleration;
        Velocity = Velocity.LimitLength(MaxSpeed);
        Position += Velocity * dt;
        Acceleration = Vector2.Zero;
        
        CheckEdges(bounds);
        Rotation = Velocity.Angle() + (Mathf.Pi / 2.0f);
        
        // Lógica Bioma/Mitosis
        if (Metabolism.Biomass >= Metabolism.MaxBiomass * 0.9f && Age > 100) {
            CanReproduce = true;
            Metabolism.ConsumeForReproduction();
        }

        if (Metabolism.Biomass <= 0) {
            Die();
        }
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
    
    public void Die() {
        IsDead = true;
        QueueFree(); // Desvincular limpieza de memoria automática de Godot
    }
}
