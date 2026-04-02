using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
    private Vector2 ScreenSize;
    private List<Nanot> Pop = new List<Nanot>();
    private const int POP_LIMIT = 1000;
    
        // Entorno Topográfico
    private GlobalEnvironment Env;
    
    // IA NeuroEvolutiva
    private BrainComputeProvider BCP;
    private NeuroEvolutionNetwork NEAT;
    private float[] FlatInputs;

    public override void _Ready()
    {
        ScreenSize = GetViewportRect().Size;
        
        // IA
        BCP = new BrainComputeProvider((uint)POP_LIMIT);
        NEAT = new NeuroEvolutionNetwork(POP_LIMIT);
        FlatInputs = new float[POP_LIMIT * 15]; // 15 inputs per agent
        
        // Cargar Entorno
        Env = new GlobalEnvironment();
        AddChild(Env);
        Env.Initialize(ScreenSize.X, ScreenSize.Y);
        
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
        // 1. Inputs a IA
        for(int i = 0; i < Pop.Count; i++) {
            // Ejemplo de input base: distancia a bordes (simulado p/ testing)
            FlatInputs[i * 15] = Pop[i].Position.X / ScreenSize.X;
            FlatInputs[i * 15 + 1] = Pop[i].Position.Y / ScreenSize.Y;
        }

        // 2. Compute Shaders (GPU Dispatcher nativo Godot)
        BCP.ForwardProcess(FlatInputs, NEAT.FlatWeights, NEAT.FlatBiases);
        
        // 3. Físicas y aplicación de salidas IA
        for(int i = 0; i < Pop.Count; i++) {
            var agent = Pop[i];
            if(agent.IsDead) continue;
            
            float[] outs = BCP.GetOutputsForAgent(i);
            // Salidas [-1, 1] desde la Tanh de GLSL Compute
            agent.ApplyForce(new Vector2(outs[0], outs[1]) * agent.MaxForce);
            
            agent.AgentUpdate(ScreenSize);
        }
        
        // 4. Entorno Natural y Decaimiento
        Env.UpdateEnv(Pop, 2.0f);
        
        // Forzar redibujado de la interfaz visual
        QueueRedraw();
    }
}
