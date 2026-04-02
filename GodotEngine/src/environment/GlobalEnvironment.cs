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
            n.Metabolism.Decay(0.003f, props.heat);
            
            // RADIACIÓN: Zona mutágena real
            if (props.radiation > 0.5f) {
                // Deposita mineral (las zonas radiactivas son ricas en recursos minerales)
                n.Metabolism.Mineral = Mathf.Min(n.Metabolism.MaxMineral, n.Metabolism.Mineral + 0.1f);
                
                // Causa mutación genética real en los pesos neuronales
                if (n.PoolIndex >= 0 && GD.Randf() < 0.01f) { // 1% chance por frame
                    neat.Mutate(n.PoolIndex, 0.3f); // Mutación fuerte
                    n.RadioFrequency += (float)GD.RandRange(-0.05, 0.05); // Deriva tonal
                    if (n.RadioFrequency < 0) n.RadioFrequency += 1f;
                    if (n.RadioFrequency > 1f) n.RadioFrequency -= 1f;
                }
            }
            
            if (n.Metabolism.IsDead())
                n.Die();
        }
    }
}
