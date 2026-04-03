using Godot;
using System.Collections.Generic;

public partial class GlobalEnvironment : Node2D
{
    public float EnvWidth;
    public float EnvHeight;
    public TopographyGrid Topography;

    public override void _Ready()
    {
        Topography = new TopographyGrid();
        AddChild(Topography);
    }

    public void Initialize(float w, float h)
    {
        EnvWidth = w;
        EnvHeight = h;
        Topography.Initialize(w, h);
    }

    public void UpdateEnv(List<Nanot> nanots, float maxSpeed, NeuroEvolutionNetwork neat)
    {
        foreach (var n in nanots)
        {
            if (n.IsDead || n.Metabolism == null) continue;
            
            var props = Topography.GetPropsAt(n.Position.X, n.Position.Y);
            
            // CALOR: Drena biomasa más rápido en zonas calientes
            bool isIdle = n.Velocity.LengthSquared() < 0.01f && Mathf.Abs(n.CurrentBroadcastSignal) < 0.5f;
            float erosionRate = 0.003f;
            
            // RADIACIÓN / MINERALES: Zona mutágena y rica en recursos
            if (props.radiation > 0.5f) {
                float radiationEfficiency = 1.0f;
                float mineralHarvest = 0.1f;
                
                // Feature 9.6: Minería Simbiótica (CellularLink Synergy)
                if (n.ActiveLink != null && n.ActiveLink.Type == "SYMBIOSIS") {
                    radiationEfficiency = 0.5f; // El daño se divide / disipa entre ambos
                    mineralHarvest *= 2.0f;     // La extracción se multiplica por resonancia biológica
                }
                
                // Atenuación Radiactiva (30% menos erosión base) + Beneficio Simbiótico
                erosionRate *= (0.7f * radiationEfficiency);
                
                // Deposita mineral
                n.Metabolism.Mineral = Mathf.Min(n.Metabolism.MaxMineral, n.Metabolism.Mineral + mineralHarvest);
                
                // Causa mutación genética real en los pesos neuronales
                if (n.PoolIndex >= 0 && GD.Randf() < 0.01f) { // 1% chance por frame
                    neat.Mutate(n.PoolIndex, 0.3f); // Mutación fuerte
                    n.RadioFrequency += (float)GD.RandRange(-0.05, 0.05); // Deriva tonal
                    if (n.RadioFrequency < 0) n.RadioFrequency += 1f;
                    if (n.RadioFrequency > 1f) n.RadioFrequency -= 1f;
                }
            }
            
            n.Metabolism.Decay(erosionRate, props.heat, isIdle);
            
            if (n.Metabolism.IsDead())
                n.Die();
        }
    }
}
