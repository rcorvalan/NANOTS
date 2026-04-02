using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
    private Vector2 ScreenSize;
    private List<Nanot> Pop = new List<Nanot>();
    private const int POP_LIMIT = 1000;

    public override void _Ready()
    {
        ScreenSize = GetViewportRect().Size;
        
        // Inicializar Agentes visuales
        for(int i = 0; i < 50; i++) {
            SpawnNanot(new Vector2((float)GD.RandRange(0, ScreenSize.X), (float)GD.RandRange(0, ScreenSize.Y)));
        }
    }

    private void SpawnNanot(Vector2 pos)
    {
        if (Pop.Count >= POP_LIMIT) return;
        Nanot n = new Nanot();
        n.Initialize(pos);
        Pop.Add(n);
        AddChild(n);
    }

    public override void _Draw()
    {
        // En Godot 4, podemos dibujar los agentes en el hook _Draw custom, 
        // o dejar implícito que el Renderer de Godot pinte las texturas.
        // Como Node2D soporta dibujo nativo, los renderizamos manualmente por ahora para igualar el CANVAS:
        foreach(var agent in Pop)
        {
            if(agent.IsDead) continue;
            
            // Dibujar un triángulo estilizado
            Vector2[] points = new Vector2[3];
            float s = 3.0f;
            points[0] = agent.Position + new Vector2(0, -s*2).Rotated(agent.Rotation);
            points[1] = agent.Position + new Vector2(-s, s*2).Rotated(agent.Rotation);
            points[2] = agent.Position + new Vector2(s, s*2).Rotated(agent.Rotation);
            
            Color c = new Color(0, 1, 0, 1);
            DrawPolygon(points, new Color[] { c, c, c });
        }
    }

    public override void _Process(double delta)
    {
        // 1. Compute Shaders (Próximamento)
        
        // 2. Físicas
        foreach(var agent in Pop) {
            agent.AgentUpdate(ScreenSize);
        }
        
        // Forzar redibujado de la interfaz visual
        QueueRedraw();
    }
}
