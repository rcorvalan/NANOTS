using Godot;
using System;
using System.Collections.Generic;

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
    public const int MAX_AGE = 12000; // Senescencia: ~3.3 minutos a 60fps
    
    // Comunicación P2P Estructurada
    public float RadioFrequency = 0f;
    public float CurrentBroadcastSignal = 0f;
    public float CommRadius = 60f;
    
    // Señal semántica: (tipo, dirX, dirY) — permite comunicar ubicaciones
    // SignalType: 0=nada, >0.5=comida, <-0.5=peligro
    public float SignalType = 0f;
    public float SignalDirX = 0f;
    public float SignalDirY = 0f;
    
    // Sociología: Engaño y Reputación
    public float DeceptionTrait = 0f;
    public Dictionary<int, float> TrustLedger = new Dictionary<int, float>();
    
    // Aprendizaje Hebbiano: tracking para recompensa intra-vida
    public float PreviousBiomass = 150f; // Para calcular delta de recompensa
    public float RewardSignal = 0f; // +1 si ganó biomasa, -1 si perdió
    
    public MetabolicSynthesis Metabolism;

    public void Initialize(Vector2 startPosition)
    {
        Position = startPosition;
        Metabolism = new MetabolicSynthesis(150f, 10f);
        
        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();
        Velocity = new Vector2(rng.RandfRange(-1, 1), rng.RandfRange(-1, 1)).Normalized() * rng.RandfRange(1, 3);
        RadioFrequency = rng.RandfRange(0f, 1f);
        DeceptionTrait = rng.RandfRange(0f, 0.15f); // Mayormente honestos al inicio
        CommRadius = rng.RandfRange(40f, 80f);
    }
    
    public void ApplyForce(Vector2 force)
    {
        Acceleration += force;
        if (float.IsNaN(Acceleration.X) || float.IsNaN(Acceleration.Y)) {
            Acceleration = Vector2.Zero;
        }
    }
    
    public float GetTrust(int otherId) {
        if (TrustLedger.ContainsKey(otherId)) return TrustLedger[otherId];
        return 0.5f; // Confianza neutra por defecto
    }
    
    public void UpdateTrust(int otherId, float delta) {
        float current = GetTrust(otherId);
        TrustLedger[otherId] = Mathf.Clamp(current + delta, 0f, 1f);
    }
    
    public void AgentUpdate(Vector2 bounds, float dt = 1.0f)
    {
        if (IsDead) return;
        
        Age++;
        
        // Costo energético de comunicación (Feature 1)
        if (Mathf.Abs(CurrentBroadcastSignal) > 0.5f) {
            Metabolism.Biomass -= 0.05f; // Hablar cuesta energía
        }
        
        Velocity += Acceleration;
        Velocity = Velocity.LimitLength(MaxSpeed);
        Position += Velocity * dt;
        Acceleration = Vector2.Zero;
        
        CheckEdges(bounds);
        Rotation = Velocity.Angle() + (Mathf.Pi / 2.0f);
        
        // Senescencia (Feature 4): Muerte por vejez
        if (Age > MAX_AGE) {
            Die();
            return;
        }
        
        // Lógica de fertilidad (requiere 2 padres, se chequea en Main.cs)
        if (Metabolism.CanReproduce() && Age > 200) {
            CanReproduce = true;
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
        QueueFree();
    }
}
