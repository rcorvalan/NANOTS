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
    public const int MAX_AGE = 216000; // Senescencia: ~60 minutos a 60fps
    
    // Comunicación P2P Estructurada
    public float RadioFrequency = 0f;
    public float CurrentBroadcastSignal = 0f;
    public float CommRadius = 60f;
    
    // Señal semántica: (tipo, dirX, dirY) — permite comunicar ubicaciones
    // SignalType: 0=nada, >0.5=comida, <-0.5=peligro
    public float SignalType = 0f;
    public float SignalDirX = 0f;
    public float SignalDirY = 0f;
    public float CurrentHelpSignal = 0f;
    
    // Sociología: Engaño y Reputación
    public float DeceptionTrait = 0f;
    public Dictionary<int, float> TrustLedger = new Dictionary<int, float>();
    
    // Aprendizaje Hebbiano: tracking para recompensa intra-vida
    public float PreviousBiomass = 150f; // Para calcular delta de recompensa
    public float RewardSignal = 0f; // +1 si ganó biomasa, -1 si perdió
    
    // Feature 11: Vectores de decisión (almacenados en _Process, renderizados en _Draw)
    public Vector2 DbgFoodDir = Vector2.Zero;        // Dirección a comida más cercana
    public float DbgFoodProximity = 0f;               // Proximidad a comida [0-1]
    public Vector2 DbgSocialFoodDir = Vector2.Zero;   // Dirección social a comida reportada
    public Vector2 DbgSocialDangerDir = Vector2.Zero;  // Dirección social de peligro reportado
    public Vector2 DbgSocialHelpDir = Vector2.Zero;    // Dirección social de auxilio reportado
    public Vector2 DbgCohesion = Vector2.Zero;         // Fuerza Boids: cohesión
    public Vector2 DbgSeparation = Vector2.Zero;       // Fuerza Boids: separación
    public Vector2 DbgAlignment = Vector2.Zero;        // Fuerza Boids: alineación
    public Vector2 DbgNeuralOutput = Vector2.Zero;     // Salida directa de la red neuronal
    public int DbgNeighborCount = 0;                   // Vecinos en CommRadius
    public int DbgKindredCount = 0;                    // Vecinos de misma especie
    
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
    
    public int AgentUpdate(Vector2 bounds, StigmergicGrid grid = null, float dt = 1.0f)
    {
        if (IsDead) return 0;
        
        Age++;
        
        // Costo energético de comunicación (Feature 1)
        if (Mathf.Abs(CurrentBroadcastSignal) > 0.5f) {
            Metabolism.Biomass -= 0.05f; // Hablar cuesta energía
        }
        
        Velocity += Acceleration;
        Velocity = Velocity.LimitLength(MaxSpeed);
        
        if (Velocity.LengthSquared() > 0.001f) {
            Rotation = Velocity.Angle() + (Mathf.Pi / 2.0f);
        }
        
        Vector2 nextPos = Position + Velocity * dt;
        
        // Colisión con estigmergia (Facciones e Infraestructura)
        if (grid != null) {
            byte tileClan = grid.GetTileClan(nextPos);
            if (tileClan != 0) {
                byte myClan = (byte)Mathf.Clamp((RadioFrequency * 100f) + 1, 1, 255);
                int diff = Mathf.Abs((int)tileClan - (int)myClan);
                if (diff > 50) diff = 100 - diff; // Corrección circular del Hue (0 y 100 son el mismo color/clan)
                
                bool isAlly = diff <= 12; // 12% de tolerancia genética para pertenecer a la Tribu
                
                if (isAlly) {
                    // Camino del propio clan: Autopista rápida (boost de velocidad inercial)
                    Velocity *= 1.05f; 
                    Velocity = Velocity.LimitLength(MaxSpeed * 1.5f); // Permiso super-velocidad sobre carreteras
                    nextPos = Position + Velocity * dt;
                } else {
                    // Infraestructura enemiga: Actúa como pantano/telaraña.
                    // Ralentiza drásticamente el movimiento pero no bloquea el mapa permanentemente.
                    Velocity = Velocity * 0.25f; // Pierde 75% de inercia al cruzar territorio hostil
                    nextPos = Position + Velocity * dt;
                }
            }
        }
        
        Position = nextPos;
        Acceleration = Vector2.Zero;
        
        
        int crossState = CheckEdges(bounds);
        
        // Senescencia (Feature 4): Muerte por vejez
        if (Age > MAX_AGE) {
            Die();
            return 0;
        }
        
        // Lógica de fertilidad (requiere 2 padres, se chequea en Main.cs)
        if (Metabolism.CanReproduce() && Age > 200) {
            CanReproduce = true;
        }

        if (Metabolism.Biomass <= 0) {
            Die();
        }
        
        return crossState;
    }

    private int CheckEdges(Vector2 bounds)
    {
        int cross = 0;
        Vector2 pos = Position;
        if (pos.X < 0) { pos.X = 0; Velocity.X *= -1; cross = -1; }
        else if (pos.X > bounds.X) { pos.X = bounds.X; Velocity.X *= -1; cross = 1; }
        else if (pos.Y < 0) { pos.Y = 0; Velocity.Y *= -1; cross = -2; }
        else if (pos.Y > bounds.Y) { pos.Y = bounds.Y; Velocity.Y *= -1; cross = 2; }
        Position = pos;
        return cross;
    }
    
    // Feature: Cross-Universe Serialization
    public byte[] ExportGenome() {
        using (var ms = new System.IO.MemoryStream())
        using (var writer = new System.IO.BinaryWriter(ms)) {
            writer.Write(Metabolism.Biomass);
            writer.Write(Metabolism.Mineral);
            writer.Write(RadioFrequency);
            writer.Write(DeceptionTrait);
            writer.Write(CommRadius);
            writer.Write(Age);
            return ms.ToArray();
        }
    }
    
    public void ImportGenome(byte[] data) {
        using (var ms = new System.IO.MemoryStream(data))
        using (var reader = new System.IO.BinaryReader(ms)) {
            Metabolism.Biomass = reader.ReadSingle();
            Metabolism.Mineral = reader.ReadSingle();
            RadioFrequency = reader.ReadSingle();
            DeceptionTrait = reader.ReadSingle();
            CommRadius = reader.ReadSingle();
            Age = reader.ReadInt32();
        }
    }
    
    public void Die() {
        IsDead = true;
        QueueFree();
    }
}
