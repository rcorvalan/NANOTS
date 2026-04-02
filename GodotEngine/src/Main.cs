using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
    private Vector2 ScreenSize;
    private List<Nanot> Pop = new List<Nanot>();
    private const int POP_LIMIT = 10000;
    
        // Entorno Topográfico
    private GlobalEnvironment Env;
    
    // IA NeuroEvolutiva
    private BrainComputeProvider BCP;
    private NeuroEvolutionNetwork NEAT;
    private float[] FlatInputs;

    private MultiMeshInstance2D MMInst;
    private MultiMesh MM;

    // Constantes/Controles de Simulacion
    private float MutationRate = 0.1f;
    private float TimeScale = 1.0f;
    private int TargetPopulation = 50;

    public override void _Ready()
    {
        ScreenSize = GetViewportRect().Size;
        
        // IA
        BCP = new BrainComputeProvider((uint)POP_LIMIT);
        NEAT = new NeuroEvolutionNetwork(POP_LIMIT);
        FlatInputs = new float[POP_LIMIT * 15]; // 15 inputs per agent
        
        SetupUI();
        
        // Cargar Entorno
        Env = new GlobalEnvironment();
        AddChild(Env);
        Env.Initialize(ScreenSize.X, ScreenSize.Y);
        
        // Cargar MultiMesh +10000 limit
        SetupMultiMesh();
        
        // La población inicial ahora es disparada por el bucle _Process
        // gracias a TargetPopulation = 50;
    }

    private void SetupMultiMesh() {
        MMInst = new MultiMeshInstance2D();
        MM = new MultiMesh();
        MM.TransformFormat = MultiMesh.TransformFormatEnum.Transform2D;
        MM.UseColors = true;
        MM.InstanceCount = POP_LIMIT;
        MM.VisibleInstanceCount = 0;
        
        var arr = new Godot.Collections.Array();
        arr.Resize((int)Mesh.ArrayType.Max);
        var verts = new Vector2[3];
        float s = 3.0f;
        verts[0] = new Vector2(0, -s*2);
        verts[1] = new Vector2(-s, s*2);
        verts[2] = new Vector2(s, s*2);
        arr[(int)Mesh.ArrayType.Vertex] = verts;
        var colors = new Color[3] { Colors.White, Colors.White, Colors.White };
        arr[(int)Mesh.ArrayType.Color] = colors;
        
        var arrayMesh = new ArrayMesh();
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arr);
        
        MM.Mesh = arrayMesh;
        MMInst.Multimesh = MM;
        AddChild(MMInst);
    }

    private Label LblPop;
    private Label LblResource;
    private Label LblMutate;
    private Label LblTime;
    private Label LblTargetPop;

    private void SetupUI()
    {
        CanvasLayer canvas = new CanvasLayer();
        canvas.Layer = 100; // Siempre arriba

        // Panel Principal
        PanelContainer panel = new PanelContainer();
        panel.Position = new Vector2(20, 20);
        
        // Estilo Base oscuro y translúcido
        StyleBoxFlat bg = new StyleBoxFlat();
        bg.BgColor = new Color(0.1f, 0.1f, 0.12f, 0.9f);
        bg.ContentMarginLeft = 20; bg.ContentMarginRight = 20;
        bg.ContentMarginTop = 15; bg.ContentMarginBottom = 15;
        bg.CornerRadiusTopLeft = 8; bg.CornerRadiusTopRight = 8;
        bg.CornerRadiusBottomLeft = 8; bg.CornerRadiusBottomRight = 8;
        panel.AddThemeStyleboxOverride("panel", bg);

        VBoxContainer vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 15);
        panel.AddChild(vbox);

        // --- Titulo ---
        Label lblTitle = new Label();
        lblTitle.Text = "NANOT EVOLUTION SANDBOX v6.0";
        lblTitle.AddThemeColorOverride("font_color", new Color(1, 0.8f, 0.2f, 1));
        vbox.AddChild(lblTitle);

        vbox.AddChild(new HSeparator());

        // --- Estadisticas Base ---
        LblPop = new Label();
        LblPop.AddThemeColorOverride("font_color", new Color(0.4f, 1f, 0.4f, 1));
        
        LblResource = new Label();
        LblResource.AddThemeColorOverride("font_color", new Color(0.4f, 1f, 1f, 1));

        vbox.AddChild(LblPop);
        vbox.AddChild(LblResource);
        
        vbox.AddChild(new HSeparator());

        // --- Sliders Globales ---
        VBoxContainer slideBox = new VBoxContainer();
        vbox.AddChild(slideBox);

        // Mutacion
        LblMutate = new Label();
        LblMutate.Text = $"Tasa de Mutación: {MutationRate * 100}%";
        slideBox.AddChild(LblMutate);

        HSlider sldMutate = new HSlider();
        sldMutate.MinValue = 0.01;
        sldMutate.MaxValue = 1.0;
        sldMutate.Step = 0.01;
        sldMutate.Value = MutationRate;
        sldMutate.CustomMinimumSize = new Vector2(200, 20);
        sldMutate.ValueChanged += (double val) => {
            MutationRate = (float)val;
            LblMutate.Text = $"Tasa de Mutación: {(int)(MutationRate * 100)}%";
        };
        slideBox.AddChild(sldMutate);

        // Velocidad Tiempo
        LblTime = new Label();
        LblTime.Text = $"Simulación TimeScale: x{TimeScale}";
        slideBox.AddChild(LblTime);

        HSlider sldTime = new HSlider();
        sldTime.MinValue = 0.1;
        sldTime.MaxValue = 10.0;
        sldTime.Step = 0.1;
        sldTime.Value = TimeScale;
        sldTime.CustomMinimumSize = new Vector2(200, 20);
        sldTime.ValueChanged += (double val) => {
            TimeScale = (float)val;
            Engine.TimeScale = TimeScale;
            LblTime.Text = $"Simulación TimeScale: x{TimeScale:F1}";
        };
        // Población Objetivo
        LblTargetPop = new Label();
        LblTargetPop.Text = $"Población Objetivo: {TargetPopulation}";
        slideBox.AddChild(LblTargetPop);

        HSlider sldTargetPop = new HSlider();
        sldTargetPop.MinValue = 10;
        sldTargetPop.MaxValue = POP_LIMIT;
        sldTargetPop.Step = 10;
        sldTargetPop.Value = TargetPopulation;
        sldTargetPop.CustomMinimumSize = new Vector2(200, 20);
        sldTargetPop.ValueChanged += (double val) => {
            TargetPopulation = (int)val;
            LblTargetPop.Text = $"Población Objetivo: {TargetPopulation}";
        };
        slideBox.AddChild(sldTargetPop);

        // Inject
        canvas.AddChild(panel);
        AddChild(canvas);
    }

    private void SpawnNanot(Vector2 pos)
    {
        if (Pop.Count >= POP_LIMIT) return;
        Nanot n = new Nanot();
        n.Initialize(pos);
        Pop.Add(n);
        AddChild(n);
    }

    public override void _Process(double delta)
    {
        // 0. Auto-Sustento Poblacional (Target Population)
        while(Pop.Count < TargetPopulation && Pop.Count < POP_LIMIT) {
            SpawnNanot(new Vector2((float)GD.RandRange(0, ScreenSize.X), (float)GD.RandRange(0, ScreenSize.Y)));
        }
        while(Pop.Count > TargetPopulation && Pop.Count > 0) {
            Pop[0].Die(); // Firing die detaches the Nanot instance safely
            Pop.RemoveAt(0);
        }

        // Purgar muertos antes de procesar
        Pop.RemoveAll(n => n == null || !GodotObject.IsInstanceValid(n) || n.IsDead);
        
        // 0. Construir QuadTree Espacial
        QuadTree qt = new QuadTree(new Rect2(0, 0, ScreenSize.X, ScreenSize.Y), 16);
        foreach(var agent in Pop) {
            qt.Insert(agent);
        }

        // 1. Inputs a IA
        for(int i = 0; i < Pop.Count; i++) {
            var agent = Pop[i];
            
            // Consultar visión (radio 50px)
            List<Nanot> neighbors = new List<Nanot>();
            qt.Query(new Rect2(agent.Position.X - 50, agent.Position.Y - 50, 100, 100), neighbors);
            
            int neighborCount = Mathf.Max(0, neighbors.Count - 1); // Excluir a sí mismo
            
            FlatInputs[i * 15] = agent.Position.X / ScreenSize.X;
            FlatInputs[i * 15 + 1] = agent.Position.Y / ScreenSize.Y;
            FlatInputs[i * 15 + 2] = Mathf.Clamp(neighborCount / 20.0f, 0, 1); // Densidad local (normalizada)
            FlatInputs[i * 15 + 3] = agent.Metabolism.Biomass / agent.Metabolism.MaxBiomass;
        }

        // 2. Compute Shaders (GPU Dispatcher nativo Godot)
        if (BCP != null) {
            BCP.ForwardProcess(FlatInputs, NEAT.FlatWeights, NEAT.FlatBiases);
        } else {
            GD.PrintErr("BCP ES NULO EN PROCESS");
        }
        
        // 3. Físicas y aplicación de salidas IA
        List<Nanot> babies = new List<Nanot>();
        
        for(int i = 0; i < Pop.Count; i++) {
            var agent = Pop[i];
            
            float[] outs = BCP.GetOutputsForAgent(i);
            // Salidas [-1, 1] desde la Tanh de GLSL Compute
            agent.ApplyForce(new Vector2(outs[0], outs[1]) * agent.MaxForce);
            
            agent.AgentUpdate(ScreenSize);
            
            // Reglas de Mitosis Biológica
            if (agent.CanReproduce && (Pop.Count + babies.Count < POP_LIMIT)) {
                agent.CanReproduce = false;
                
                Nanot baby = new Nanot();
                Vector2 spawnPos = agent.Position + new Vector2((float)GD.RandRange(-10, 10), (float)GD.RandRange(-10, 10));
                baby.Initialize(spawnPos);
                baby.Rotation = (float)GD.RandRange(0, Mathf.Tau);
                
                int childIndex = Pop.Count + babies.Count;
                NEAT.Inherit(childIndex, i);
                NEAT.Mutate(childIndex, MutationRate); // Mutación enlazada al Dashboard
                
                babies.Add(baby);
            }
        }
        
        // Agregar nuevos a la simulación
        if (babies.Count > 0) {
            foreach(var b in babies) AddChild(b);
            Pop.AddRange(babies);
        }
        
        // 4. Entorno Natural y Decaimiento
        Env.UpdateEnv(Pop, 2.0f);
        
        // 5. Instanciar en MultiMesh API
        MM.VisibleInstanceCount = Pop.Count;
        for (int i = 0; i < Pop.Count; i++) {
            var n = Pop[i];
            Transform2D tr = new Transform2D(n.Rotation, n.Position);
            MM.SetInstanceTransform2D(i, tr);
            
            float healthRatio = Mathf.Clamp(n.Metabolism.Biomass / n.Metabolism.MaxBiomass, 0, 1);
            Color cl = new Color(1-healthRatio, healthRatio, 0, 1); // Verde=Saludable Rojo=Muriendo
            MM.SetInstanceColor(i, cl);
        }
        
        // Actualizar UI Hilo Principal
        int alive = Pop.FindAll(n => !n.IsDead).Count;
        LblPop.Text = $"Nanots: {alive}/{POP_LIMIT} | FPS: {Engine.GetFramesPerSecond()} GPU: Godot.Render";
        LblResource.Text = $"Decay Base: Activo | Topography Override: Activo";
    }
}
