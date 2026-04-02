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

    public void UpdateEnv(List<Nanot> nanots, float maxSpeed)
    {
        // En un motor nativo, los componentes Node2D manejan sus propios _Process.
        // Aquí centralizaremos las lógicas de Recursos Naturales y Decaimiento Metabólico
        
        foreach (var n in nanots)
        {
            if (n.IsDead || n.Metabolism == null) continue;
            
            var props = Topography.GetPropsAt(n.Position.X, n.Position.Y);
            n.Metabolism.Decay(0.05f, props.heat);
            
            if (n.Metabolism.IsDead())
                n.Die();
        }
    }
}
