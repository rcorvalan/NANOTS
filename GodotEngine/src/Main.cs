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
    private FoodManager FoodMgr;
    private StigmergicGrid StigGrid;
    private List<Predator> Predators = new List<Predator>();
    
    // IA NeuroEvolutiva
    private BrainComputeProvider BCP;
    private NeuroEvolutionNetwork NEAT;
    private float[] FlatInputs;

    private MultiMeshInstance2D MMInst;
    private MultiMesh MM;
    private Camera2D Cam;
    private bool IsDragging = false;
    
    // Inspeccion Individual
    private Nanot SelectedAgent = null;
    private PanelContainer InspectorUI;
    private Label LblInspEnergy, LblInspAge, LblInspFaction, LblInspState;
    private Font defaultFont;

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
        FlatInputs = new float[POP_LIMIT * NEAT.Inputs]; // Dynamic inputs size
        
        SetupUI();
        
        // Cargar Entorno
        Env = new GlobalEnvironment();
        AddChild(Env);
        Env.Initialize(ScreenSize.X, ScreenSize.Y);
        
        StigGrid = new StigmergicGrid();
        AddChild(StigGrid);
        StigGrid.Initialize(ScreenSize.X, ScreenSize.Y);
        
        FoodMgr = new FoodManager();
        AddChild(FoodMgr);
        FoodMgr.SetWorldSize(ScreenSize.X, ScreenSize.Y);
        
        // Cargar MultiMesh +10000 limit
        SetupMultiMesh();
        
        // La población inicial ahora es disparada por el bucle _Process
        // gracias a TargetPopulation = 50;

        Cam = new Camera2D();
        Cam.Position = new Vector2(ScreenSize.X / 2, ScreenSize.Y / 2);
        AddChild(Cam);
        
        defaultFont = ThemeDB.FallbackFont;
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
        float s = 6.0f; // Escala Nanot duplicada (+100%)
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
        sldTime.MinValue = 0.0; // Permite pausa total
        sldTime.MaxValue = 10.0;
        sldTime.Step = 0.1;
        sldTime.Value = TimeScale;
        sldTime.CustomMinimumSize = new Vector2(200, 20);
        sldTime.ValueChanged += (double val) => {
            TimeScale = (float)val;
            Engine.TimeScale = TimeScale;
            LblTime.Text = $"Simulación TimeScale: x{TimeScale:F1}";
        };
        slideBox.AddChild(sldTime); // <- AQUI ESTABA EL ERROR (no se renderizaba el slider de tiempo)
        
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

        // Boton Depredador
        Button btnPred = new Button();
        btnPred.Text = "💀 Desatar Depredador";
        btnPred.CustomMinimumSize = new Vector2(200, 30);
        btnPred.Pressed += () => {
            Predator p = new Predator();
            p.Initialize(new Vector2(ScreenSize.X/2, ScreenSize.Y/2));
            Predators.Add(p);
            AddChild(p);
        };
        slideBox.AddChild(btnPred);
        
        slideBox.AddChild(new HSeparator());
        
        // EXIT Button
        Button btnExit = new Button();
        btnExit.Text = "❌ Salir de Simulación";
        btnExit.AddThemeColorOverride("font_color", new Color(1, 0.4f, 0.4f, 1));
        btnExit.Pressed += () => {
             GetTree().Quit();
        };
        slideBox.AddChild(btnExit);

        canvas.AddChild(panel);

        // --- GLOSARIO DE COLORES (Arriba a la derecha) ---
        PanelContainer legend = new PanelContainer();
        legend.Position = new Vector2(ScreenSize.X - 350, 20);
        legend.AddThemeStyleboxOverride("panel", bg);
        
        VBoxContainer lgBox = new VBoxContainer();
        lgBox.AddThemeConstantOverride("separation", 8);
        legend.AddChild(lgBox);
        
        Label lglT = new Label(); lglT.Text = "GLOSARIO DE ECOSISTEMA";
        lglT.AddThemeColorOverride("font_color", new Color(1, 1, 1, 1));
        lgBox.AddChild(lglT);
        lgBox.AddChild(new HSeparator());
        
        Label lgH = new Label(); lgH.Text = "■ Nubes Rojas / Azules: Calor / Frío";
        lgH.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f, 1));
        lgBox.AddChild(lgH);
        
        Label lgR = new Label(); lgR.Text = "■ Nubes Púrpuras: Alta Radiación (Mutágeno)";
        lgR.AddThemeColorOverride("font_color", new Color(0.8f, 0.3f, 0.8f, 1));
        lgBox.AddChild(lgR);
        
        Label lgW = new Label(); lgW.Text = "■ Pixeles Grises: Estigmergia (Aisladores)";
        lgW.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f, 1));
        lgBox.AddChild(lgW);
        
        Label lgC = new Label(); lgC.Text = "■ Pixeles Celestes: Cable Energizado P2P";
        lgC.AddThemeColorOverride("font_color", new Color(0.2f, 1f, 1f, 1));
        lgBox.AddChild(lgC);
        
        Label lgA = new Label(); lgA.Text = "▲ Triángulos (Color): Escuadrón / Idioma Tonal";
        lgA.AddThemeColorOverride("font_color", new Color(1f, 1f, 0.2f, 1));
        lgBox.AddChild(lgA);
        
        Label lgP = new Label(); lgP.Text = "▲ Triángulo Naranja: Depredador Acústico";
        lgP.AddThemeColorOverride("font_color", new Color(1f, 0.5f, 0.1f, 1));
        lgBox.AddChild(lgP);

        Label lgF = new Label(); lgF.Text = "■ Nodos Dorados: Biomasa Orgánica";
        lgF.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.1f, 1));
        lgBox.AddChild(lgF);

        lgBox.AddChild(new HSeparator());
        
        Label lgST = new Label(); lgST.Text = "SIGNOS FLOTANTES";
        lgST.AddThemeColorOverride("font_color", new Color(1, 1, 1, 1));
        lgBox.AddChild(lgST);
        
        Label lgE1 = new Label(); lgE1.Text = "! Hambre crítica (Biomasa < 15)";
        lgE1.AddThemeColorOverride("font_color", Colors.Red);
        lgBox.AddChild(lgE1);
        
        Label lgE2 = new Label(); lgE2.Text = "♫ Vocalzando (Broadcast P2P fuerte)";
        lgE2.AddThemeColorOverride("font_color", Colors.Cyan);
        lgBox.AddChild(lgE2);

        canvas.AddChild(legend);

        // --- PANEL DE INSPECCION INDIVIDUAL (MICROSCÓPIO) ---
        InspectorUI = new PanelContainer();
        InspectorUI.Position = new Vector2(ScreenSize.X - 350, ScreenSize.Y - 180);
        InspectorUI.Visible = false;
        
        StyleBoxFlat bgInsp = new StyleBoxFlat();
        bgInsp.BgColor = new Color(0.1f, 0.1f, 0.2f, 0.95f);
        bgInsp.SetCornerRadiusAll(12);
        bgInsp.BorderWidthLeft = 2; bgInsp.BorderColor = new Color(0.2f, 1f, 1f, 0.8f);
        bgInsp.ContentMarginLeft = 15; bgInsp.ContentMarginTop = 15;
        InspectorUI.AddThemeStyleboxOverride("panel", bgInsp);

        VBoxContainer inspBox = new VBoxContainer();
        InspectorUI.AddChild(inspBox);
        
        Label inspTitul = new Label(); inspTitul.Text = "🔬 MICROSCOPIO NEURAL";
        inspTitul.AddThemeColorOverride("font_color", new Color(0.7f, 1f, 1f, 1));
        inspBox.AddChild(inspTitul);
        inspBox.AddChild(new HSeparator());

        LblInspState = new Label(); LblInspState.Text = "ESTADO: BÚSQUEDA";
        LblInspEnergy = new Label();
        LblInspAge = new Label();
        LblInspFaction = new Label();
        inspBox.AddChild(LblInspState);
        inspBox.AddChild(LblInspEnergy);
        inspBox.AddChild(LblInspAge);
        inspBox.AddChild(LblInspFaction);

        canvas.AddChild(InspectorUI);

        // Inject
        AddChild(canvas);
    }

    public override void _UnhandledInput(InputEvent @event) {
        if (@event is InputEventMouseButton mb) {
            if (mb.ButtonIndex == MouseButton.WheelUp) { Cam.Zoom *= 1.1f; }
            if (mb.ButtonIndex == MouseButton.WheelDown) { Cam.Zoom *= 0.9f; }
            Cam.Zoom = Cam.Zoom.Clamp(new Vector2(0.3f, 0.3f), new Vector2(5f, 5f));
            if (mb.ButtonIndex == MouseButton.Middle || mb.ButtonIndex == MouseButton.Right) {
                IsDragging = mb.Pressed;
            }
            if (mb.ButtonIndex == MouseButton.Left && mb.Pressed) {
                // Raycast matemático a mano O(n)
                Vector2 gp = GetGlobalMousePosition();
                float closestDist = float.MaxValue;
                Nanot closestAgent = null;
                foreach(var n in Pop) {
                    if(n.IsDead) continue;
                    float d = n.Position.DistanceSquaredTo(gp);
                    if (d < 1500f && d < closestDist) {
                        closestDist = d;
                        closestAgent = n;
                    }
                }
                SelectedAgent = closestAgent;
            }
        }
        else if (@event is InputEventMouseMotion mm && IsDragging) {
            Cam.Position -= mm.Relative / Cam.Zoom;
        }
    }
    
    public override void _Draw() {
        if (SelectedAgent != null && !SelectedAgent.IsDead) {
            var n = SelectedAgent;
            // Dibujar mira de seguimiento holográfica
            DrawArc(n.Position, 30f, 0, Mathf.Tau, 32, Colors.Cyan, 2f);
            DrawLine(n.Position - new Vector2(0, 40), n.Position + new Vector2(0, 40), new Color(0, 1, 1, 0.4f), 1f);
            DrawLine(n.Position - new Vector2(40, 0), n.Position + new Vector2(40, 0), new Color(0, 1, 1, 0.4f), 1f);
        }
        
        // Emotes de Cultura (Solo renderizar si estamos muy cerca con la camara)
        if (Cam != null && Cam.Zoom.X > 1.2f) {
            Rect2 viewBounds = new Rect2(Cam.Position - (ScreenSize / (2 * Cam.Zoom)), ScreenSize / Cam.Zoom);
            foreach(var n in Pop) {
                if (n.IsDead || !viewBounds.HasPoint(n.Position)) continue;
                
                if (n.Metabolism.Biomass < 15f) {
                    DrawString(defaultFont, n.Position + new Vector2(-4, -15), "!", HorizontalAlignment.Center, -1, 14, Colors.Red);
                } 
                else if (Mathf.Abs(n.CurrentBroadcastSignal) > 0.8f) {
                    DrawString(defaultFont, n.Position + new Vector2(-6, -15), "♫", HorizontalAlignment.Center, -1, 16, Colors.Cyan);
                }
                
                // Si está seleccionando objetivo depredador
                if (SelectedAgent != null && !SelectedAgent.IsDead && SelectedAgent == n) {
                    DrawString(defaultFont, n.Position + new Vector2(-20, -35), $"HUE: {(int)(n.RadioFrequency * 360)}", HorizontalAlignment.Left, -1, 14, Colors.White);
                }
            }
        }
    }

    private void SpawnNanot(Vector2 pos)
    {
        if (Pop.Count >= POP_LIMIT) return;
        Nanot n = new Nanot();
        n.Initialize(pos);
        n.PoolIndex = Pop.Count;
        Pop.Add(n);
        AddChild(n);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta * 60f; // Normalizar a 60fps base
        // 0. Auto-Sustento Poblacional (Target Population)
        while(Pop.Count < TargetPopulation && Pop.Count < POP_LIMIT) {
            SpawnNanot(new Vector2((float)GD.RandRange(0, ScreenSize.X), (float)GD.RandRange(0, ScreenSize.Y)));
        }
        
        // Purgar muertos y generar biomasa residual
        for(int i = Pop.Count - 1; i >= 0; i--) {
            var n = Pop[i];
            if (n == null || !GodotObject.IsInstanceValid(n) || n.IsDead) {
                if (n != null && GodotObject.IsInstanceValid(n) && n.Velocity.LengthSquared() > 0) {
                    FoodMgr.DropFood(n.Position, 50f); // Residual biomass
                }
                Pop.RemoveAt(i);
            }
        }
        
        while(Pop.Count > TargetPopulation && Pop.Count > 0) {
            Pop[0].Die(); // Marcar para morir
            Pop.RemoveAt(0);
        }
        
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
            // --- 1.1 Decodificación P2P ---
            float localSignalSum = 0f;
            int validSignals = 0;
            
            foreach(var nb in neighbors) {
                if (nb == agent || nb.IsDead) continue;
                // Si la frecuencia de radio desvía más del 10% (0.1f), es incomprensible.
                if (Mathf.Abs(nb.RadioFrequency - agent.RadioFrequency) < 0.1f) {
                    localSignalSum += nb.CurrentBroadcastSignal;
                    validSignals++;
                }
            }
            float avgSignal = validSignals > 0 ? localSignalSum / validSignals : 0f;
            // -----------------------------
            
            int offset = i * NEAT.Inputs;
            FlatInputs[offset] = agent.Position.X / ScreenSize.X;
            FlatInputs[offset + 1] = agent.Position.Y / ScreenSize.Y;
            FlatInputs[offset + 2] = Mathf.Clamp(neighborCount / 20.0f, 0, 1);
            FlatInputs[offset + 3] = agent.Metabolism.Biomass / agent.Metabolism.MaxBiomass;
            
            // --- Sentido de Comida (Inputs 4-7): Dirección y distancia a la comida más próxima ---
            float closestFoodDist = float.MaxValue;
            Vector2 foodDir = Vector2.Zero;
            foreach(var f in FoodMgr.Pellets) {
                if (f.IsRotten) continue;
                float d = agent.Position.DistanceSquaredTo(f.Position);
                if (d < closestFoodDist) {
                    closestFoodDist = d;
                    foodDir = (f.Position - agent.Position);
                }
            }
            if (closestFoodDist < 40000f) { // Solo percibe comida hasta ~200px
                foodDir = foodDir.Normalized();
                FlatInputs[offset + 4] = foodDir.X; // Dirección X a comida
                FlatInputs[offset + 5] = foodDir.Y; // Dirección Y a comida
                FlatInputs[offset + 6] = 1.0f - Mathf.Clamp(Mathf.Sqrt(closestFoodDist) / 200f, 0, 1); // Proximidad (1=encima, 0=lejos)
            } else {
                FlatInputs[offset + 4] = 0;
                FlatInputs[offset + 5] = 0;
                FlatInputs[offset + 6] = 0;
            }
            // --- Inputs enérgicos y ambientales (7-13) ---
            var envProps = Env.Topography.GetPropsAt(agent.Position.X, agent.Position.Y);
            FlatInputs[offset + 7] = envProps.heat; // Temperatura local [-1, 1]
            FlatInputs[offset + 8] = envProps.radiation; // Radiación local [0, 1]
            FlatInputs[offset + 9] = agent.Metabolism.Mineral / agent.Metabolism.MaxMineral; // Nivel mineral
            FlatInputs[offset + 10] = agent.Age / 5000f; // Edad normalizada
            FlatInputs[offset + 11] = agent.Velocity.Length() / agent.MaxSpeed; // Velocidad normalizada
            FlatInputs[offset + 12] = agent.RadioFrequency; // Su propia frecuencia
            FlatInputs[offset + 13] = Mathf.Clamp((float)validSignals / 10f, 0, 1); // Densidad de compatriotas cercanos
            
            FlatInputs[offset + 14] = StigGrid.CheckTile(agent.Position) > 0 ? 1.0f : 0.0f;
            FlatInputs[offset + 15] = avgSignal;
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
            
            // Outputs P2P y Biológicos
            agent.CurrentBroadcastSignal = outs[4]; // Canal de salida
            agent.RadioFrequency += outs[5] * 0.01f; // Mutación tonal activa (Desplazamiento lento)
            if (agent.RadioFrequency < 0) agent.RadioFrequency += 1f;
            if (agent.RadioFrequency > 1f) agent.RadioFrequency -= 1f;
            
            // Acciones Estigmérgicas (Out 6)
            if (outs[6] > 0.5f && agent.Metabolism.Biomass > 20f) {
                // Instinto de construcción
                if (StigGrid.PlaceTile(agent.Position)) { // True si estaba libre y ahora es conductor
                    agent.Metabolism.Biomass -= 20f;
                }
            } else if (outs[6] < -0.5f) {
                // Instinto de descomposición
                if (StigGrid.DestroyTile(agent.Position)) { // True si había estructura
                    agent.Metabolism.Biomass += 10f; // Reabsorción
                }
            }
            
            // Electrificación Pasiva por Emisión Tonal o Gasto Biológico
            if (agent.CurrentBroadcastSignal > 0.8f) {
                StigGrid.EnergizeTileFromEntity(agent.Position);
            }
            
            agent.AgentUpdate(ScreenSize, dt);
            
            // Consumo de Biomasa (Radio ampliado 400px² = ~20px)
            foreach(var f in FoodMgr.Pellets) {
                if (!f.IsRotten && agent.Position.DistanceSquaredTo(f.Position) < 400.0f) {
                    agent.Metabolism.Biomass += f.Value;
                    f.Value = 0;
                }
            }
            
            // --- FUERZA SOCIAL: Atracción a pares de misma frecuencia ---
            List<Nanot> socialNeighbors = new List<Nanot>();
            qt.Query(new Rect2(agent.Position.X - 80, agent.Position.Y - 80, 160, 160), socialNeighbors);
            Vector2 cohesion = Vector2.Zero;
            int kindred = 0;
            foreach(var sn in socialNeighbors) {
                if (sn == agent || sn.IsDead) continue;
                if (Mathf.Abs(sn.RadioFrequency - agent.RadioFrequency) < 0.1f) {
                    cohesion += (sn.Position - agent.Position);
                    kindred++;
                }
            }
            if (kindred > 0) {
                cohesion /= kindred;
                agent.ApplyForce(cohesion.Normalized() * 0.01f); // Fuerza suave hacia el grupo
            }
            
            // Reglas de Mitosis Biológica
            if (agent.CanReproduce && (Pop.Count + babies.Count < POP_LIMIT)) {
                agent.CanReproduce = false;
                
                Nanot baby = new Nanot();
                Vector2 spawnPos = agent.Position + new Vector2((float)GD.RandRange(-10, 10), (float)GD.RandRange(-10, 10));
                baby.Initialize(spawnPos);
                baby.Rotation = (float)GD.RandRange(0, Mathf.Tau);
                
                int childIndex = Pop.Count + babies.Count;
                NEAT.Inherit(childIndex, i);
                NEAT.Mutate(childIndex, MutationRate);
                
                // Heredar frecuencia (idioma) del padre con pequeña variación
                baby.RadioFrequency = agent.RadioFrequency + (float)GD.RandRange(-0.03, 0.03);
                if (baby.RadioFrequency < 0) baby.RadioFrequency += 1f;
                if (baby.RadioFrequency > 1f) baby.RadioFrequency -= 1f;
                baby.PoolIndex = childIndex;
                
                babies.Add(baby);
            }
        }
        
        // Agregar nuevos a la simulación
        if (babies.Count > 0) {
            foreach(var b in babies) AddChild(b);
            Pop.AddRange(babies);
        }
        
        // 4. Entorno Natural y Decaimiento (con Radiación Mutágena Real)
        Env.UpdateEnv(Pop, 2.0f, NEAT);
        StigGrid.UpdateAutomata();
        FoodMgr.UpdateAndRender();
        
        Predators.RemoveAll(p => p == null || !GodotObject.IsInstanceValid(p) || p.IsDead);
        foreach(var p in Predators) {
            p.Hunt(qt, ScreenSize);
        }
        
        // 5. Instanciar en MultiMesh API
        MM.VisibleInstanceCount = Pop.Count;
        for (int i = 0; i < Pop.Count; i++) {
            var n = Pop[i];
            
            // Si el Agente esta comunicandose con fuerza, "vibra" (Aumenta su escala)
            float talkScale = 1.0f + (Mathf.Abs(n.CurrentBroadcastSignal) * 0.8f);
            
            Transform2D tr = new Transform2D(n.Rotation, n.Position).ScaledLocal(new Vector2(talkScale, talkScale));
            MM.SetInstanceTransform2D(i, tr);
            
            float healthRatio = Mathf.Clamp(n.Metabolism.Biomass / n.Metabolism.MaxBiomass, 0.2f, 1f);
            Color cl = Color.FromHsv(n.RadioFrequency, 1.0f, 1.0f, healthRatio); // Frecuencia = Especie (Hue)
            MM.SetInstanceColor(i, cl);
        }
        
        // Actualizar UI Hilo Principal
        int alive = Pop.FindAll(n => !n.IsDead).Count;
        ulong timeSeconds = Godot.Time.GetTicksMsec() / 1000;
        ulong hours = timeSeconds / 3600;
        ulong mins = (timeSeconds % 3600) / 60;
        ulong secs = timeSeconds % 60;
        
        // Evaluador Cultural Intuitivo (Usa múltiples métricas)
        float avgBiomass = 0;
        int broadcastCount = 0;
        int stigBlocks = 0;
        foreach(var n in Pop) {
            if (n.IsDead) continue;
            avgBiomass += n.Metabolism.Biomass;
            if (Mathf.Abs(n.CurrentBroadcastSignal) > 0.5f) broadcastCount++;
        }
        avgBiomass = alive > 0 ? avgBiomass / alive : 0;
        float commRatio = alive > 0 ? (float)broadcastCount / alive : 0;
        
        string popGrade;
        if (alive < 5) popGrade = "⚠ EXTINCIÓN INMINENTE";
        else if (alive < 30) popGrade = "🔥 Supervivientes Aislados";
        else if (commRatio < 0.1f) popGrade = "🧍 Nómadas Silenciosos";
        else if (commRatio < 0.3f && alive < 200) popGrade = "🏚 Cazadores-Recolectores";
        else if (commRatio >= 0.3f && alive < 500) popGrade = "🏠 Tribus Comunicantes";
        else if (alive >= 500 && commRatio >= 0.3f) popGrade = "🏛 Civilización Emergente";
        else if (alive > 2000) popGrade = "🌍 Sociedad Compleja";
        else popGrade = "🌱 Colonias en Expansión";
        if (alive > 5000) popGrade = "⚠ Sobrepoblación Severa";
        
        LblPop.Text = $"Población: {alive}/{POP_LIMIT} | FPS: {Engine.GetFramesPerSecond()} | T {hours:D2}:{mins:D2}:{secs:D2}";
        LblResource.Text = $"{popGrade} | Comunicando: {(int)(commRatio*100)}% | Biomasa Prom: {avgBiomass:F0}";

        if (SelectedAgent != null) {
            if (SelectedAgent.IsDead) { SelectedAgent = null; InspectorUI.Visible = false; }
            else {
                var ag = SelectedAgent;
                InspectorUI.Visible = true;
                LblInspEnergy.Text = $"Biomasa: {ag.Metabolism.Biomass:F1} / {ag.Metabolism.MaxBiomass:F1}";
                LblInspAge.Text = $"Edad Biol: {ag.Age} ticks";
                LblInspFaction.Text = $"Idioma (HUE): {(int)(ag.RadioFrequency * 360)}°";
                
                string st = "SUPERVIVENCIA";
                if (Mathf.Abs(ag.CurrentBroadcastSignal) > 0.8f) st = "VOCALIZANDO (P2P)";
                if (ag.Metabolism.Biomass < 20f) st = "HAMBRE CRÍTICA";
                if (ag.CanReproduce) st = "PREPARADO PARA MITOSIS";
                LblInspState.Text = $"ESTADO: {st}";
            }
        } else {
            InspectorUI.Visible = false;
        }

        QueueRedraw();
    }
}
