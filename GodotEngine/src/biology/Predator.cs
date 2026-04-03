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
            // Defender o ser devorado
            if (dist < 6.0f) {
                int defendingSwarm = 0;
                foreach(var swarmMember in prey) {
                    if (!swarmMember.IsDead && Position.DistanceTo(swarmMember.Position) < 25.0f) {
                        defendingSwarm++;
                    }
                }

                if (defendingSwarm >= 4) {
                    // ¡Enjambre Ataca al Depredador!
                    Biomass -= 25f * defendingSwarm; // El depredador sufre daño masivo
                    Velocity = -Velocity * 1.5f; // Es repelido
                    
                    // Recompensa inmediata a todos los atacantes
                    foreach(var swarmMember in prey) {
                         if (!swarmMember.IsDead && Position.DistanceTo(swarmMember.Position) < 25.0f) {
                             swarmMember.RewardSignal += 1.0f; // Exito táctico de grupo
                         }
                    }
                } else {
                    // Depredador tiene éxito
                    Biomass += 100f; // Satiety reward!
                    p.Metabolism.Biomass = 0; // Mata al nanot instantáneamente
                    p.Die();
                }
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
        
        // Interacción con estigmergia (rastros)
        if (grid != null) {
            byte tileClan = grid.GetTileClan(nextPos);
            if (tileClan > 0) {
                // No hay penalización de movimiento (los depredadores no están sujetos a muros)
                
                // Al pasar un depredador por un rastro, los nanots que estén sobre él lo pueden sentir
                foreach(var p in prey) {
                    if (p.IsDead) continue;
                    // Si el nanot está sobre el mismo rastro (misma vibración/feromona)
                    if (grid.GetTileClan(p.Position) == tileClan) {
                        // Siente el peligro a través del rastro
                        p.SignalType = -1.0f; // Peligro
                        p.CurrentBroadcastSignal = 1.0f; // Grito de alerta máxima instantáneo
                        Vector2 avoidDir = (p.Position - Position).Normalized();
                        p.SignalDirX = avoidDir.X;
                        p.SignalDirY = avoidDir.Y;
                        
                        // Respuesta de huida / sobresalto nervioso
                        p.ApplyForce(avoidDir * p.MaxForce * 5.0f);
                    }
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
