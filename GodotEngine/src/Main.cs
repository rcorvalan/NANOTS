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
    private List<CellularLink> Links = new List<CellularLink>();
    
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
    private Label LblInspCommRadius, LblInspDeception, LblInspNeighbors, LblInspReward;
    private Font defaultFont;
    
    // CSV Export
    private FileDialog CsvFileDialog;
    private FileDialog CommFileDialog;
    private Label LblStatus;
    private float StatusTimer = 0f;

    // Telemetría de Lenguaje
    public struct CommEvent {
        public ulong Timestamp;
        public string Data;
    }
    private Queue<CommEvent> CommLogBuffer = new Queue<CommEvent>();
    private const ulong MAX_COMM_HISTORY_MS = 30000; // 30 segundos

    // Constantes/Controles de Simulacion
    private float MutationRate = 0.1f;
    private float TimeScale = 1.0f;
    private int TargetPopulation = 50;

    // Cross-Universe
    private PacketPeerUdp udpPeer;
    private bool crossUniverseEnabled = false;

    public override void _Ready()
    {
        ScreenSize = GetViewportRect().Size;
        
        // IA
        BCP = new BrainComputeProvider((uint)POP_LIMIT);
        NEAT = new NeuroEvolutionNetwork(POP_LIMIT);
        FlatInputs = new float[POP_LIMIT * NEAT.Inputs]; // Dynamic inputs size
        
        udpPeer = new PacketPeerUdp();
        udpPeer.Bind(9876);
        udpPeer.SetDestAddress("255.255.255.255", 9876);
        udpPeer.SetBroadcastEnabled(true);
        
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
        
        // Generación de población inicial (sin generación espontánea en _Process)
        for(int i = 0; i < TargetPopulation; i++) {
            SpawnNanot(new Vector2((float)GD.RandRange(0, ScreenSize.X), (float)GD.RandRange(0, ScreenSize.Y)));
        }

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

        VBoxContainer mainContainer = new VBoxContainer();
        panel.AddChild(mainContainer);

        HBoxContainer header = new HBoxContainer();
        header.MouseFilter = Control.MouseFilterEnum.Stop;
        mainContainer.AddChild(header);

        // --- Titulo ---
        Label lblTitle = new Label();
        lblTitle.Text = "NANOT EVOLUTION SANDBOX v9.5";
        lblTitle.AddThemeColorOverride("font_color", new Color(1, 0.8f, 0.2f, 1));
        lblTitle.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        header.AddChild(lblTitle);

        Button btnCollapse = new Button();
        btnCollapse.Text = "-";
        header.AddChild(btnCollapse);

        VBoxContainer vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 15);
        mainContainer.AddChild(vbox);

        btnCollapse.Pressed += () => {
            vbox.Visible = !vbox.Visible;
            btnCollapse.Text = vbox.Visible ? "-" : "+";
        };

        bool isDragging1 = false;
        header.GuiInput += (InputEvent @event) => {
            if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left) {
                if (mb.Pressed) { isDragging1 = true; } 
                else { isDragging1 = false; }
            } else if (@event is InputEventMouseMotion mm && isDragging1) {
                panel.Position += mm.Relative;
            }
        };

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

        // Fertilidad
        Label LblFertility = new Label();
        LblFertility.Text = $"Exigencia Fertilidad (Biomasa): {MetabolicSynthesis.FertilityThreshold}";
        LblFertility.AddThemeColorOverride("font_color", new Color(1f, 0.4f, 0.6f, 1));
        slideBox.AddChild(LblFertility);

        HSlider sldFertility = new HSlider();
        sldFertility.MinValue = 50.0;
        sldFertility.MaxValue = 200.0;
        sldFertility.Step = 5.0;
        sldFertility.Value = MetabolicSynthesis.FertilityThreshold;
        sldFertility.CustomMinimumSize = new Vector2(200, 20);
        sldFertility.ValueChanged += (double val) => {
            MetabolicSynthesis.FertilityThreshold = (float)val;
            LblFertility.Text = $"Exigencia Fertilidad (Biomasa): {val}";
        };
        slideBox.AddChild(sldFertility);

        // Abundancia de Recursos
        Label LblAbundance = new Label();
        LblAbundance.Text = "Abundancia de Oasis: 1.0x";
        LblAbundance.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.1f, 1));
        slideBox.AddChild(LblAbundance);

        HSlider sldAbundance = new HSlider();
        sldAbundance.MinValue = 0.0; // Hambruna
        sldAbundance.MaxValue = 3.0; // Sobreabundancia
        sldAbundance.Step = 0.1;
        sldAbundance.Value = 1.0;
        sldAbundance.CustomMinimumSize = new Vector2(200, 20);
        sldAbundance.ValueChanged += (double val) => {
            if (FoodMgr != null) FoodMgr.AbundanceMultiplier = (float)val;
            LblAbundance.Text = $"Abundancia de Oasis: {val:F1}x";
        };
        slideBox.AddChild(sldAbundance);

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
        
        // Feature 8: Botón Catástrofe
        Button btnCatastrophe = new Button();
        btnCatastrophe.Text = "☄ Catástrofe (Extinción 50%)";
        btnCatastrophe.CustomMinimumSize = new Vector2(200, 30);
        btnCatastrophe.AddThemeColorOverride("font_color", new Color(1f, 0.6f, 0.1f, 1));
        btnCatastrophe.Pressed += () => {
            int killCount = Pop.Count / 2;
            for(int k = 0; k < killCount && Pop.Count > 0; k++) {
                int idx = (int)(GD.Randi() % Pop.Count);
                Pop[idx].Die();
                Pop.RemoveAt(idx);
            }
        };
        slideBox.AddChild(btnCatastrophe);
        
        // --- DESAFÍO A: Pastor Mentiroso ---
        Button btnLiar = new Button();
        btnLiar.Text = "🎭 Desafío: Invasión de Mentirosos";
        btnLiar.CustomMinimumSize = new Vector2(200, 30);
        btnLiar.AddThemeColorOverride("font_color", new Color(1f, 0.4f, 0.8f, 1));
        btnLiar.Pressed += () => {
            // Generar 50 Nanots de una facción específica altamente mentirosa
            for(int i = 0; i < 50; i++) {
                if (Pop.Count >= POP_LIMIT) break;
                Nanot n = new Nanot();
                n.Initialize(new Vector2(ScreenSize.X/2 + (float)GD.RandRange(-200, 200), ScreenSize.Y/2 + (float)GD.RandRange(-200, 200)));
                n.RadioFrequency = 0.85f; // Faccion Purpura
                n.DeceptionTrait = 1.0f; // 100% Mentirosos
                n.PoolIndex = Pop.Count;
                Pop.Add(n);
                AddChild(n);
            }
            ShowStatus("🎭 50 Pastores Mentirosos han aparecido (Facción Púrpura).");
        };
        slideBox.AddChild(btnLiar);

        // --- DESAFÍO B: Pista de Pávlov ---
        Button btnPavlov = new Button();
        btnPavlov.Text = "🧪 Desafío: Laberinto de Pávlov";
        btnPavlov.CustomMinimumSize = new Vector2(200, 30);
        btnPavlov.AddThemeColorOverride("font_color", new Color(0.4f, 1f, 0.4f, 1));
        btnPavlov.Pressed += () => {
             // Dibujar pista estigmérgica con "trampas térmicas" marcadas por un color previo
             Vector2 start = new Vector2(ScreenSize.X * 0.2f, ScreenSize.Y * 0.5f);
             for(float x = 0; x < ScreenSize.X * 0.6f; x += 15f) {
                 Vector2 currentTrace = start + new Vector2(x, 0);
                 
                 // Crear TRAMPAS térmicas (Peligro) pero precedidas por un "Color Estigmérgico" (Aroma)
                 if (x % 300 > 150 && x % 300 < 200) {
                     // El "Aroma" avisa del peligro (radioFreq 0.3 = Verde)
                     StigGrid.PlaceTile(currentTrace, 0.3f);
                     StigGrid.PlaceTile(currentTrace + new Vector2(0, 15), 0.3f);
                     StigGrid.PlaceTile(currentTrace + new Vector2(0, -15), 0.3f);
                 } else if (x % 300 >= 200 && x % 300 < 250) {
                     // La Trampa en sí (no necesita estigmergia, solo les drena la vida por posicion, pero lo haremos con biomasa para forzar RewardSignal)
                     // Mejor: Hacemos que si pisan el radioFreq 0.9 (Rojo) pierden vida. Esto lo haremos en UpdateEnv o _Process.
                     StigGrid.PlaceTile(currentTrace, 0.9f); // Rojo peligroso!
                     StigGrid.PlaceTile(currentTrace + new Vector2(0, 15), 0.9f);
                     StigGrid.PlaceTile(currentTrace + new Vector2(0, -15), 0.9f);
                 } else {
                     // Camino pacífico (RadioFreq 0.6 = Azul)
                     StigGrid.PlaceTile(currentTrace, 0.6f);
                 }
             }
             ShowStatus("🧪 Pista de Pávlov dibujada. Vigila cómo reaccionan al verde y rojo.");
        };
        slideBox.AddChild(btnPavlov);
        
        // --- DESAFÍO C: Laberinto de Teseo ---
        Button btnTeseo = new Button();
        btnTeseo.Text = "🧱 Desafío: Laberinto de Teseo";
        btnTeseo.CustomMinimumSize = new Vector2(200, 30);
        btnTeseo.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f, 1));
        btnTeseo.Pressed += () => {
             BuildTheseusMaze();
             ShowStatus("🧱 Laberinto de Teseo dibujado. Muros inflexibles.");
        };
        slideBox.AddChild(btnTeseo);

        // --- FEDERACIÓN CROSS-UNIVERSE ---
        CheckButton chkUniverse = new CheckButton();
        chkUniverse.Text = "🌐 Federación Cross-Universe (UDP)";
        chkUniverse.ButtonPressed = crossUniverseEnabled;
        chkUniverse.Toggled += (bool toggledOn) => {
            crossUniverseEnabled = toggledOn;
            ShowStatus(toggledOn ? "🌐 Cross-Universe Activado (Puerto 9876)" : "🚫 Cross-Universe Desactivado");
        };
        slideBox.AddChild(chkUniverse);
        
        // Feature 9: Botón exportar CSV
        Button btnCSV = new Button();
        btnCSV.Text = "📊 Exportar Métricas CSV";
        btnCSV.CustomMinimumSize = new Vector2(200, 30);
        btnCSV.Pressed += () => {
            CsvFileDialog.Popup();
        };
        slideBox.AddChild(btnCSV);

        // Language Log Button
        Button btnCommLog = new Button();
        btnCommLog.Text = "🗣 Exportar Log Lenguaje (30s)";
        btnCommLog.CustomMinimumSize = new Vector2(200, 30);
        btnCommLog.Pressed += () => {
            CommFileDialog.Popup();
        };
        slideBox.AddChild(btnCommLog);
        
        slideBox.AddChild(new HSeparator());
        
        // RESTART Button
        Button btnRestart = new Button();
        btnRestart.Text = "🔄 Reiniciar Simulación";
        btnRestart.AddThemeColorOverride("font_color", new Color(1, 0.8f, 0.2f, 1));
        btnRestart.Pressed += () => {
             GetTree().ReloadCurrentScene();
        };
        slideBox.AddChild(btnRestart);
        
        // EXIT Button
        Button btnExit = new Button();
        btnExit.Text = "❌ Salir de Simulación";
        btnExit.AddThemeColorOverride("font_color", new Color(1, 0.4f, 0.4f, 1));
        btnExit.Pressed += () => {
             GetTree().Quit();
        };
        slideBox.AddChild(btnExit);
        
        // --- Status Label (Toast de feedback) ---
        LblStatus = new Label();
        LblStatus.Position = new Vector2(20, ScreenSize.Y - 50);
        LblStatus.AddThemeColorOverride("font_color", new Color(0.3f, 1f, 0.3f, 1));
        LblStatus.Visible = false;
        canvas.AddChild(LblStatus);

        canvas.AddChild(panel);
        
        // --- FileDialog para CSV ---
        CsvFileDialog = new FileDialog();
        CsvFileDialog.FileMode = FileDialog.FileModeEnum.SaveFile;
        CsvFileDialog.Title = "Guardar Métricas CSV";
        CsvFileDialog.Access = FileDialog.AccessEnum.Filesystem;
        CsvFileDialog.CurrentDir = "c:/Users/rcorv/OneDrive - Adecua SpA/Escritorio/Nanots/Metricas";
        CsvFileDialog.Filters = new string[] { "*.csv ; Archivos CSV" };
        CsvFileDialog.CurrentFile = $"nanot_metrics_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        CsvFileDialog.Size = new Vector2I(700, 500);
        CsvFileDialog.FileSelected += (string selectedPath) => {
            ExportMetricsCSV(selectedPath);
        };
        canvas.AddChild(CsvFileDialog);

        // --- FileDialog para Comm Log ---
        CommFileDialog = new FileDialog();
        CommFileDialog.FileMode = FileDialog.FileModeEnum.SaveFile;
        CommFileDialog.Title = "Guardar Log de Lenguaje CSV";
        CommFileDialog.Access = FileDialog.AccessEnum.Filesystem;
        CommFileDialog.CurrentDir = "c:/Users/rcorv/OneDrive - Adecua SpA/Escritorio/Nanots/Metricas";
        CommFileDialog.Filters = new string[] { "*.csv ; Archivos CSV" };
        CommFileDialog.CurrentFile = $"nanot_language_log_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        CommFileDialog.Size = new Vector2I(700, 500);
        CommFileDialog.FileSelected += (string selectedPath) => {
            ExportCommLogCSV(selectedPath);
        };
        canvas.AddChild(CommFileDialog);

        // --- GLOSARIO DE COLORES (Arriba a la derecha) ---
        PanelContainer legend = new PanelContainer();
        legend.Position = new Vector2(ScreenSize.X - 350, 20);
        legend.AddThemeStyleboxOverride("panel", bg);
        
        VBoxContainer legendMain = new VBoxContainer();
        legend.AddChild(legendMain);

        HBoxContainer legendHeader = new HBoxContainer();
        legendHeader.MouseFilter = Control.MouseFilterEnum.Stop;
        legendMain.AddChild(legendHeader);

        Label lglT = new Label(); lglT.Text = "GLOSARIO DE ECOSISTEMA";
        lglT.AddThemeColorOverride("font_color", new Color(1, 1, 1, 1));
        lglT.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        legendHeader.AddChild(lglT);

        Button btnLegCol = new Button(); btnLegCol.Text = "-";
        legendHeader.AddChild(btnLegCol);

        VBoxContainer lgBox = new VBoxContainer();
        lgBox.AddThemeConstantOverride("separation", 8);
        legendMain.AddChild(lgBox);

        btnLegCol.Pressed += () => {
            lgBox.Visible = !lgBox.Visible;
            btnLegCol.Text = lgBox.Visible ? "-" : "+";
        };

        bool isDragging2 = false;
        legendHeader.GuiInput += (InputEvent @event) => {
            if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left) {
                if (mb.Pressed) { isDragging2 = true; } 
                else { isDragging2 = false; }
            } else if (@event is InputEventMouseMotion mm && isDragging2) {
                legend.Position += mm.Relative;
            }
        };

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
        InspectorUI.Position = new Vector2(ScreenSize.X - 350, ScreenSize.Y - 620);
        InspectorUI.Visible = false;
        
        StyleBoxFlat bgInsp = new StyleBoxFlat();
        bgInsp.BgColor = new Color(0.1f, 0.1f, 0.2f, 0.95f);
        bgInsp.SetCornerRadiusAll(12);
        bgInsp.BorderWidthLeft = 2; bgInsp.BorderWidthRight = 2;
        bgInsp.BorderColor = new Color(0.2f, 1f, 1f, 0.8f);
        bgInsp.ContentMarginLeft = 15; bgInsp.ContentMarginTop = 12;
        bgInsp.ContentMarginRight = 15; bgInsp.ContentMarginBottom = 12;
        InspectorUI.AddThemeStyleboxOverride("panel", bgInsp);

        VBoxContainer inspMain = new VBoxContainer();
        InspectorUI.AddChild(inspMain);

        HBoxContainer inspHeader = new HBoxContainer();
        inspHeader.MouseFilter = Control.MouseFilterEnum.Stop;
        inspMain.AddChild(inspHeader);

        Label inspTitul = new Label(); inspTitul.Text = "🔬 MICROSCOPIO NEURAL";
        inspTitul.AddThemeColorOverride("font_color", new Color(0.7f, 1f, 1f, 1));
        inspTitul.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        inspHeader.AddChild(inspTitul);

        Button btnInspCol = new Button(); btnInspCol.Text = "-";
        inspHeader.AddChild(btnInspCol);

        VBoxContainer inspBox = new VBoxContainer();
        inspMain.AddChild(inspBox);

        btnInspCol.Pressed += () => {
            inspBox.Visible = !inspBox.Visible;
            btnInspCol.Text = inspBox.Visible ? "-" : "+";
        };

        bool isDragging3 = false;
        inspHeader.GuiInput += (InputEvent @event) => {
            if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left) {
                if (mb.Pressed) { isDragging3 = true; } 
                else { isDragging3 = false; }
            } else if (@event is InputEventMouseMotion mm && isDragging3) {
                InspectorUI.Position += mm.Relative;
            }
        };

        inspBox.AddChild(new HSeparator());

        LblInspState = new Label(); LblInspState.Text = "ESTADO: BÚSQUEDA";
        LblInspEnergy = new Label();
        LblInspAge = new Label();
        LblInspFaction = new Label();
        LblInspCommRadius = new Label();
        LblInspDeception = new Label();
        LblInspNeighbors = new Label();
        LblInspReward = new Label();
        Label LblInspLink = new Label(); LblInspLink.Name = "LblInspLink";
        inspBox.AddChild(LblInspState);
        inspBox.AddChild(LblInspEnergy);
        inspBox.AddChild(LblInspAge);
        inspBox.AddChild(LblInspFaction);
        inspBox.AddChild(LblInspCommRadius);
        inspBox.AddChild(LblInspDeception);
        inspBox.AddChild(LblInspNeighbors);
        inspBox.AddChild(LblInspReward);
        inspBox.AddChild(LblInspLink);

        inspBox.AddChild(new HSeparator());
        Label inspVecTitle = new Label(); inspVecTitle.Text = "VECTORES DE DECISIÓN:";
        inspVecTitle.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 1f, 1));
        inspBox.AddChild(inspVecTitle);
        
        Label lgVel = new Label(); lgVel.Text = "→ Verde: Velocidad actual";
        lgVel.AddThemeColorOverride("font_color", Colors.Green); lgVel.AddThemeConstantOverride("line_spacing", 0);
        inspBox.AddChild(lgVel);
        Label lgFood = new Label(); lgFood.Text = "→ Amarillo: Dir. a comida";
        lgFood.AddThemeColorOverride("font_color", Colors.Yellow); lgFood.AddThemeConstantOverride("line_spacing", 0);
        inspBox.AddChild(lgFood);
        Label lgSocF = new Label(); lgSocF.Text = "→ Naranja: Comida social";
        lgSocF.AddThemeColorOverride("font_color", new Color(1f, 0.6f, 0.1f)); lgSocF.AddThemeConstantOverride("line_spacing", 0);
        inspBox.AddChild(lgSocF);
        Label lgDang = new Label(); lgDang.Text = "→ Rojo: Peligro social";
        lgDang.AddThemeColorOverride("font_color", Colors.Red); lgDang.AddThemeConstantOverride("line_spacing", 0);
        inspBox.AddChild(lgDang);
        Label lgCoh = new Label(); lgCoh.Text = "→ Celeste: Cohesión Boids";
        lgCoh.AddThemeColorOverride("font_color", Colors.Cyan); lgCoh.AddThemeConstantOverride("line_spacing", 0);
        inspBox.AddChild(lgCoh);
        Label lgSep = new Label(); lgSep.Text = "→ Magenta: Separación Boids";
        lgSep.AddThemeColorOverride("font_color", Colors.Magenta); lgSep.AddThemeConstantOverride("line_spacing", 0);
        inspBox.AddChild(lgSep);
        Label lgAli = new Label(); lgAli.Text = "→ Aqua: Alineación Boids";
        lgAli.AddThemeColorOverride("font_color", new Color(0.3f, 1f, 0.8f)); lgAli.AddThemeConstantOverride("line_spacing", 0);
        inspBox.AddChild(lgAli);
        Label lgNeuro = new Label(); lgNeuro.Text = "→ Blanco: Salida neuronal";
        lgNeuro.AddThemeColorOverride("font_color", Colors.White); lgNeuro.AddThemeConstantOverride("line_spacing", 0);
        inspBox.AddChild(lgNeuro);
        Label lgComm = new Label(); lgComm.Text = "○ Anillo cyan: Radio comunicación";
        lgComm.AddThemeColorOverride("font_color", new Color(0, 1, 1, 0.5f)); lgComm.AddThemeConstantOverride("line_spacing", 0);
        inspBox.AddChild(lgComm);

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
    
    // Feature 11: Helper — Dibuja una flecha con cabeza triangular
    private void DrawArrowVector(Vector2 from, Vector2 dir, float length, Color color, float width = 2f) {
        if (dir.LengthSquared() < 0.001f) return;
        Vector2 to = from + dir.Normalized() * length;
        DrawLine(from, to, color, width);
        // Cabeza de flecha
        Vector2 arrowDir = (to - from).Normalized();
        Vector2 perp = new Vector2(-arrowDir.Y, arrowDir.X);
        float headSize = Mathf.Max(5f, length * 0.25f);
        Vector2 h1 = to - arrowDir * headSize + perp * headSize * 0.5f;
        Vector2 h2 = to - arrowDir * headSize - perp * headSize * 0.5f;
        DrawLine(to, h1, color, width);
        DrawLine(to, h2, color, width);
    }
    
    // Feature 12: Helper — Dibuja anillo punteado del CommRadius
    private void DrawDashedCircle(Vector2 center, float radius, Color color, int segments = 32, float width = 1.5f) {
        for (int s = 0; s < segments; s += 2) {
            float a1 = (float)s / segments * Mathf.Tau;
            float a2 = (float)(s + 1) / segments * Mathf.Tau;
            Vector2 p1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
            Vector2 p2 = center + new Vector2(Mathf.Cos(a2), Mathf.Sin(a2)) * radius;
            DrawLine(p1, p2, color, width);
        }
    }
    
    public override void _Draw() {
        if (SelectedAgent != null && !SelectedAgent.IsDead) {
            var n = SelectedAgent;
            // Dibujar mira de seguimiento holográfica
            DrawArc(n.Position, 30f, 0, Mathf.Tau, 32, Colors.Cyan, 2f);
            DrawLine(n.Position - new Vector2(0, 40), n.Position + new Vector2(0, 40), new Color(0, 1, 1, 0.4f), 1f);
            DrawLine(n.Position - new Vector2(40, 0), n.Position + new Vector2(40, 0), new Color(0, 1, 1, 0.4f), 1f);
            
            // Feature 12: Visualizar Radio de Comunicación mutable
            DrawDashedCircle(n.Position, n.CommRadius, new Color(0, 1, 1, 0.3f), 48, 1.5f);
            // Etiqueta del radio
            DrawString(defaultFont, n.Position + new Vector2(n.CommRadius + 4, -8),
                $"R:{(int)n.CommRadius}px", HorizontalAlignment.Left, -1, 10, new Color(0, 1, 1, 0.6f));
            
            // ===== Feature 11: VECTORES DE DECISIÓN COMPLETOS =====
            float vecScale = 35f; // Escala visual base
            
            // 1. Velocidad actual (Verde)
            DrawArrowVector(n.Position, n.Velocity, vecScale, Colors.Green, 2.5f);
            
            // 2. Dirección a comida más cercana (Amarillo) — intensidad proporcional a proximidad
            if (n.DbgFoodProximity > 0.01f) {
                Color foodCol = new Color(1f, 1f, 0.1f, 0.4f + n.DbgFoodProximity * 0.6f);
                DrawArrowVector(n.Position, n.DbgFoodDir, vecScale * (0.5f + n.DbgFoodProximity), foodCol, 2f);
            }
            
            // 3. Dirección social a comida reportada (Naranja)
            if (n.DbgSocialFoodDir.LengthSquared() > 0.01f) {
                DrawArrowVector(n.Position, n.DbgSocialFoodDir, vecScale * 0.8f, new Color(1f, 0.6f, 0.1f, 0.8f), 2f);
            }
            
            // 4. Dirección social de peligro (Rojo)
            if (n.DbgSocialDangerDir.LengthSquared() > 0.01f) {
                DrawArrowVector(n.Position, n.DbgSocialDangerDir, vecScale * 0.8f, new Color(1f, 0.2f, 0.2f, 0.8f), 2f);
            }
            
            // 5. Cohesión Boids (Celeste)
            if (n.DbgCohesion.LengthSquared() > 0.0001f) {
                DrawArrowVector(n.Position, n.DbgCohesion, vecScale * 0.6f, new Color(0.3f, 0.9f, 1f, 0.7f), 1.5f);
            }
            
            // 6. Separación Boids (Magenta)
            if (n.DbgSeparation.LengthSquared() > 0.0001f) {
                DrawArrowVector(n.Position, n.DbgSeparation, vecScale * 0.7f, Colors.Magenta, 1.5f);
            }
            
            // 7. Alineación Boids (Aqua verde)
            if (n.DbgAlignment.LengthSquared() > 0.0001f) {
                DrawArrowVector(n.Position, n.DbgAlignment, vecScale * 0.5f, new Color(0.3f, 1f, 0.8f, 0.7f), 1.5f);
            }
            
            // 8. Salida directa neuronal (Blanco pulsante)
            if (n.DbgNeuralOutput.LengthSquared() > 0.01f) {
                float pulse = 0.5f + Mathf.Sin((float)Godot.Time.GetTicksMsec() / 200f) * 0.3f;
                DrawArrowVector(n.Position, n.DbgNeuralOutput, vecScale * 0.9f, new Color(1, 1, 1, pulse), 3f);
            }
            
            DrawString(defaultFont, n.Position + new Vector2(-50, -50), 
                $"HUE:{(int)(n.RadioFrequency*360)}° | Dec:{(int)(n.DeceptionTrait*100)}% | Vecinos:{n.DbgNeighborCount}({n.DbgKindredCount})", 
                HorizontalAlignment.Left, -1, 11, new Color(1, 1, 1, 0.9f));
        }
        
        // Feature 9.6: Visualizar Enlaces de Simbiosis Activos
        foreach(var link in Links) {
            if (link.Type == "SYMBIOSIS") {
                Color linkCol = new Color(0.2f, 1.0f, 1.0f, 0.4f);
                DrawLine(link.A.Position, link.B.Position, link.Type == "SYMBIOSIS" ? linkCol : Colors.Red, 2.0f);
            }
        }
        
        // Emotes de Cultura (Solo renderizar si estamos muy cerca con la camara)
        if (Cam != null && Cam.Zoom.X > 1.2f) {
            Rect2 viewBounds = new Rect2(Cam.Position - (ScreenSize / (2 * Cam.Zoom)), ScreenSize / Cam.Zoom);
            foreach(var n in Pop) {
                if (n.IsDead || !viewBounds.HasPoint(n.Position)) continue;
                
                if (n.Metabolism.Biomass < 15f) {
                    DrawString(defaultFont, n.Position + new Vector2(-4, -15), "!", HorizontalAlignment.Center, -1, 14, Colors.Red);
                } 
                else if (n.DeceptionTrait > 0.5f && Mathf.Abs(n.CurrentBroadcastSignal) > 0.5f) {
                    DrawString(defaultFont, n.Position + new Vector2(-6, -15), "🎭", HorizontalAlignment.Center, -1, 14, Colors.Yellow);
                }
                else if (Mathf.Abs(n.CurrentBroadcastSignal) > 0.8f) {
                    DrawString(defaultFont, n.Position + new Vector2(-6, -15), "♫", HorizontalAlignment.Center, -1, 16, Colors.Cyan);
                }
                else if (n.CanReproduce) {
                    DrawString(defaultFont, n.Position + new Vector2(-6, -15), "♥", HorizontalAlignment.Center, -1, 14, Colors.HotPink);
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
    
    // Telemetría Continua (Feature 9 extendida)
    private Queue<string> TelemetryBuffer = new Queue<string>();
    private float TelemetryTimer = 0f;
    private const float TELEMETRY_INTERVAL = 0.1f; // 10 capturas por segundo
    private const int MAX_TELEMETRY_SAMPLES = 150; // 15 segundos de historia a 10 capturas/s
    
    private void CaptureTelemetrySnapshot() {
        int alive = 0; float totalBio = 0; int broadcasting = 0; float totalDeception = 0;
        float totalCommRadius = 0; float totalReward = 0; float totalAge = 0;
        System.Collections.Generic.HashSet<int> factions = new System.Collections.Generic.HashSet<int>();
        
        foreach(var n in Pop) {
            if (n.IsDead) continue;
            alive++;
            totalBio += n.Metabolism.Biomass;
            if (Mathf.Abs(n.CurrentBroadcastSignal) > 0.5f) broadcasting++;
            totalDeception += n.DeceptionTrait;
            totalCommRadius += n.CommRadius;
            totalReward += n.RewardSignal;
            totalAge += n.Age;
            factions.Add((int)(n.RadioFrequency * 10));
        }
        
        if (alive == 0) return;
        
        float avgBio = totalBio / alive;
        float commR = (float)broadcasting / alive;
        float avgDec = totalDeception / alive;
        float avgComm = totalCommRadius / alive;
        float avgRew = totalReward / alive;
        float avgAge = totalAge / alive;
        
        // Usar Godot.Time.GetTicksMsec() como milisegundos absolutos para el timeline
        ulong ts = Godot.Time.GetTicksMsec();
        
        string record = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2:F2},{3:F3},{4:F4},{5},{6:F1},{7:F5},{8:F0}", ts, alive, avgBio, commR, avgDec, factions.Count, avgComm, avgRew, avgAge);
        TelemetryBuffer.Enqueue(record);
        
        // Mantener solo los últimos 15 segundos
        while(TelemetryBuffer.Count > MAX_TELEMETRY_SAMPLES) {
            TelemetryBuffer.Dequeue();
        }
    }

    private void CaptureCommSnapshot() {
        ulong ts = Godot.Time.GetTicksMsec();
        foreach(var n in Pop) {
            if (n.IsDead) continue;
            // Si la señal es mayor a un umbral bajo o hay una clara señal semántica directiva O grita por auxilio
            if (Mathf.Abs(n.CurrentBroadcastSignal) > 0.1f || Mathf.Abs(n.SignalType) > 0.1f || n.CurrentHelpSignal > 0.5f) {
                string msgType = "vocalizing";
                if (n.CurrentHelpSignal > 0.5f) msgType = "help_me";
                else if (n.SignalType > 0.5f) msgType = "food_location";
                else if (n.SignalType < -0.5f) msgType = "danger_warning";
                
                string faction = ((int)(n.RadioFrequency * 10)).ToString();
                int fraud = (int)(n.DeceptionTrait * 100);
                
                string record = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4:F2},{5:F2},{6},{7:F2},{8:F0}", ts, n.PoolIndex, faction, msgType, n.SignalDirX, n.SignalDirY, fraud, n.CurrentBroadcastSignal, n.CommRadius);
                CommLogBuffer.Enqueue(new CommEvent { Timestamp = ts, Data = record });
            }
        }
        
        while(CommLogBuffer.Count > 0 && ts - CommLogBuffer.Peek().Timestamp > MAX_COMM_HISTORY_MS) {
            CommLogBuffer.Dequeue();
        }
    }

    // Feature 9: Exportar historial de últimos 15 segundos a CSV
    private void ExportMetricsCSV(string absolutePath) {
        try {
            // Asegurar que capturemos el ultimísimo frame
            CaptureTelemetrySnapshot();
            
            using (var writer = new System.IO.StreamWriter(absolutePath, append: false)) {
                writer.WriteLine("timestamp_ms,population,avg_biomass,comm_ratio,avg_deception,factions_estimate,avg_comm_radius,avg_hebbian_reward,avg_age");
                foreach(string record in TelemetryBuffer) {
                    writer.WriteLine(record);
                }
            }
            
            ShowStatus($"✅ CSV (15s) histórico exportado: {absolutePath}");
            GD.Print($"Métricas de los últimos 15 segundos exportadas a {absolutePath} ({TelemetryBuffer.Count} instantes capturados)");
            
            // Opcional: Limpiar el buffer si quisieras que no haya overlap entre exportaciones
            // TelemetryBuffer.Clear(); 
        } catch (System.Exception ex) {
            ShowStatus($"❌ Error al exportar CSV: {ex.Message}");
            GD.PrintErr($"Error exportando CSV: {ex.Message}");
        }
    }

    private void ExportCommLogCSV(string absolutePath) {
        try {
            using (var writer = new System.IO.StreamWriter(absolutePath, append: false)) {
                writer.WriteLine("timestamp_ms,agent_id,faction,message_type,signal_dir_x,signal_dir_y,deception_pct,broadcast_strength,comm_radius");
                foreach(var evt in CommLogBuffer) {
                    writer.WriteLine(evt.Data);
                }
            }
            ShowStatus($"✅ Log Lenguaje (30s) exportado: {absolutePath}");
            GD.Print($"Log de comunicaciones exportado a {absolutePath} ({CommLogBuffer.Count} eventos en los ultimos 30s)");
        } catch (System.Exception ex) {
            ShowStatus($"❌ Error al exportar Log: {ex.Message}");
            GD.PrintErr($"Error exportando Log: {ex.Message}");
        }
    }
    
    private void ShowStatus(string msg) {
        LblStatus.Text = msg;
        LblStatus.Visible = true;
        StatusTimer = 4.0f; // Mostrar por 4 segundos
    }
    
    private void BuildTheseusMaze() {
        // Limpiamos o seteamos las paredes duras en clan=255 (Bloqueo Total)
        float cx = ScreenSize.X / 2f;
        float cy = ScreenSize.Y / 2f;
        // Borde exterior
        for(float x = cx - 300; x <= cx + 300; x += 15) {
            StigGrid.PlaceTile(new Vector2(x, cy - 300), 1.0f); // Clan 255 (Blanco/Neutro)
            StigGrid.PlaceTile(new Vector2(x, cy + 300), 1.0f);
        }
        for(float y = cy - 300; y <= cy + 300; y += 15) {
            StigGrid.PlaceTile(new Vector2(cx - 300, y), 1.0f);
            StigGrid.PlaceTile(new Vector2(cx + 300, y), 1.0f);
        }
        // Paredes interiores
        for(float y = cy - 200; y < cy + 200; y += 15) { StigGrid.PlaceTile(new Vector2(cx - 200, y), 1.0f); }
        for(float x = cx - 200; x < cx + 100; x += 15) { StigGrid.PlaceTile(new Vector2(x, cy - 200), 1.0f); }
        for(float y = cy - 200; y < cy; y += 15) { StigGrid.PlaceTile(new Vector2(cx + 100, y), 1.0f); }
        for(float x = cx - 100; x < cx + 200; x += 15) { StigGrid.PlaceTile(new Vector2(x, cy), 1.0f); }
        for(float y = cy; y < cy + 200; y += 15) { StigGrid.PlaceTile(new Vector2(cx - 100, y), 1.0f); }
        // Salidas (Borramos algunos tiles)
        StigGrid.DestroyTile(new Vector2(cx - 300, cy));
        StigGrid.DestroyTile(new Vector2(cx + 300, cy));
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta * 60f; // Normalizar a 60fps base
        // Status Toast Timer
        if (StatusTimer > 0) {
            StatusTimer -= (float)delta;
            if (StatusTimer <= 0) { LblStatus.Visible = false; }
        }
        
        // Telemetría Continua
        TelemetryTimer += (float)delta;
        if (TelemetryTimer >= TELEMETRY_INTERVAL) {
            TelemetryTimer = 0f;
            CaptureTelemetrySnapshot();
            CaptureCommSnapshot();
        }
        
        // --- FEDERACION CROSS-UNIVERSE LECTURA ---
        if (crossUniverseEnabled && udpPeer != null && udpPeer.GetAvailablePacketCount() > 0) {
            while(udpPeer.GetAvailablePacketCount() > 0) {
                byte[] data = udpPeer.GetPacket();
                if (data != null && data.Length > 0 && Pop.Count < POP_LIMIT) {
                    Nanot incoming = new Nanot();
                    // Aparecen en borde aleatorio para simular viaje
                    float spawnX = GD.Randf() > 0.5f ? 10f : ScreenSize.X - 10f;
                    float spawnY = (float)GD.RandRange(10, ScreenSize.Y - 10);
                    incoming.Initialize(new Vector2(spawnX, spawnY));
                    try { incoming.ImportGenome(data); } catch { incoming.QueueFree(); continue; }
                    incoming.PoolIndex = Pop.Count;
                    Pop.Add(incoming);
                    AddChild(incoming);
                    ShowStatus("🌐 Un Nanot extranjero acaba de migrar a nuestro universo!");
                }
            }
        }
        
        // El auto-sustento por generación espontánea fue removido (Feature: Reproducción Nativa)
        // Los Nanots ahora deben reproducirse metabólicamente para alcanzar TargetPopulation.
        
        // Purgar muertos y generar biomasa residual
        for(int i = Pop.Count - 1; i >= 0; i--) {
            var n = Pop[i];
            if (n == null || !GodotObject.IsInstanceValid(n) || n.IsDead) {
                if (n != null && GodotObject.IsInstanceValid(n)) {
                    // Feature 2: Reciclaje orgánico proporcional (30%)
                    FoodMgr.DropFood(n.Position, n.Metabolism.MaxBiomass * 0.3f);
                }
                Pop.RemoveAt(i);
            }
        }
        
        while(Pop.Count > TargetPopulation && Pop.Count > 0) {
            Pop[0].Die(); // Marcar para morir
            Pop.RemoveAt(0);
        }
        
        // Actualizar Enlaces (Symbiosis/Parasitism)
        for (int i = Links.Count - 1; i >= 0; i--) {
            Links[i].Update();
            if (Links[i].A.IsDead || Links[i].B.IsDead || Links[i].Type == "DEAD_LINK") {
                Links.RemoveAt(i);
            }
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
            // --- 1.1 Decodificación P2P Semántica ---
            float localSignalSum = 0f;
            int validSignals = 0;
            float socialFoodDirX = 0f, socialFoodDirY = 0f; // Dirección social a comida reportada
            float socialDangerDirX = 0f, socialDangerDirY = 0f;
            float socialHelpDirX = 0f, socialHelpDirY = 0f; // Dirección hacia alguien pidiendo ayuda
            int foodReports = 0, dangerReports = 0, helpReports = 0;
            
            foreach(var nb in neighbors) {
                if (nb == agent || nb.IsDead) continue;
                if (Mathf.Abs(nb.RadioFrequency - agent.RadioFrequency) < 0.1f) {
                    // Filtrar por confianza (Feature 6)
                    float trust = agent.GetTrust(nb.PoolIndex);
                    // Eje 3: Umbral de Silenciamiento Dinámico (Señales de auxilio ignoran el baneo)
                    if (trust < 0.2f && nb.CurrentHelpSignal < 0.5f) continue; 
                    
                    localSignalSum += nb.CurrentBroadcastSignal;
                    validSignals++;
                    
                    // Decodificar señales semánticas
                    if (nb.CurrentHelpSignal > 0.5f) {
                        Vector2 diff = nb.Position - agent.Position;
                        float dist = diff.Length();
                        if (dist > 0.1f) {
                            socialHelpDirX += (diff.X / dist) * trust;
                            socialHelpDirY += (diff.Y / dist) * trust;
                        }
                        helpReports++;
                        
                        // Mecánica de Rescate (Donación de Biomasa y Recompensa Empática)
                        if (dist < 15f && agent.Metabolism.Biomass > 30f && nb.Metabolism.Biomass < nb.Metabolism.MaxBiomass * 0.5f) {
                            agent.Metabolism.Biomass -= 5.0f; // Ceder energía
                            nb.Metabolism.Biomass += 5.0f;    // Curar herido
                            agent.RewardSignal += 1.5f;       // Masivo placer por empatía
                        }
                    } else if (nb.SignalType > 0.5f) { // Reporte de comida
                        socialFoodDirX += nb.SignalDirX * trust;
                        socialFoodDirY += nb.SignalDirY * trust;
                        foodReports++;
                    } else if (nb.SignalType < -0.5f) { // Reporte de peligro
                        socialDangerDirX += nb.SignalDirX * trust;
                        socialDangerDirY += nb.SignalDirY * trust;
                        dangerReports++;
                    }
                }
            }
            float avgSignal = validSignals > 0 ? localSignalSum / validSignals : 0f;
            if (foodReports > 0) { socialFoodDirX /= foodReports; socialFoodDirY /= foodReports; }
            if (dangerReports > 0) { socialDangerDirX /= dangerReports; socialDangerDirY /= dangerReports; }
            if (helpReports > 0) { socialHelpDirX /= helpReports; socialHelpDirY /= helpReports; }
            
            // Feature 11: Almacenar vectores sociales para debug visual
            agent.DbgSocialFoodDir = new Vector2(socialFoodDirX, socialFoodDirY);
            agent.DbgSocialDangerDir = new Vector2(socialDangerDirX, socialDangerDirY);
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
                // Feature 11: Almacenar vector de comida para debug visual
                agent.DbgFoodDir = foodDir;
                agent.DbgFoodProximity = FlatInputs[offset + 6];
            } else {
                FlatInputs[offset + 4] = 0;
                FlatInputs[offset + 5] = 0;
                FlatInputs[offset + 6] = 0;
                agent.DbgFoodDir = Vector2.Zero;
                agent.DbgFoodProximity = 0f;
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
            
            FlatInputs[offset + 14] = socialFoodDirX; // Dirección social: "los vecinos dicen comida por aquí"
            FlatInputs[offset + 15] = socialFoodDirY;
            FlatInputs[offset + 16] = socialDangerDirX; // Dirección social de peligro "evade por acá"
            FlatInputs[offset + 17] = socialDangerDirY;
            
            // Estímulo Neutro (Condicionamiento Clásico): Aroma/Color del suelo bajo sus pies
            byte floorTile = StigGrid.GetTileClan(agent.Position);
            FlatInputs[offset + 18] = floorTile / 255.0f; // Normalizado
            
            // Percepción de Socorro (Empatía)
            FlatInputs[offset + 19] = socialHelpDirX;
            FlatInputs[offset + 20] = socialHelpDirY;
            
            agent.DbgSocialHelpDir = new Vector2(socialHelpDirX, socialHelpDirY);
            agent.DbgSocialDangerDir = new Vector2(socialDangerDirX, socialDangerDirY);
            
            // --- Eje 4.2 / Highway Comm Boost ---
            byte tileState = StigGrid.CheckTile(agent.Position);
            if (tileState > 0) {
                float boost = (tileState == 2) ? 1.5f : 1.2f;
                agent.CommRadius *= boost; // Boost de radio por estigmergia Conductiva
            }
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
            
            // Feature 11: Almacenar salida neuronal para debug visual
            agent.DbgNeuralOutput = new Vector2(outs[0], outs[1]);
            
            // --- APRENDIZAJE HEBBIANO (intra-vida / v9.6 Refuerzo Asimétrico) ---
            // Calcular recompensa: ¿ganó o perdió biomasa desde el frame anterior?
            float biomassDelta = agent.Metabolism.Biomass - agent.PreviousBiomass;
            
            // Highway Comm Boost: Costo cero si está energizado (usa energía de la red)
            byte gridState = StigGrid.CheckTile(agent.Position);
            bool freeBroadcast = gridState == 2;
            
            // Eje 2: Asimetría de Recompensa/Castigo
            float finalReward = 0f;
            if (biomassDelta > 0.001f) {
                finalReward = Mathf.Clamp(biomassDelta * 5.0f, 0f, 5.0f); // Alta gratificación por comer
            } else if (biomassDelta < -0.001f) {
                finalReward = Mathf.Clamp(biomassDelta * 1.0f, -1.0f, 0f); // Castigo mitigado
            }
            
            agent.RewardSignal = finalReward;
            agent.PreviousBiomass = agent.Metabolism.Biomass;
            
            // Extraer inputs actuales para Hebbian
            int inputOffset = i * NEAT.Inputs;
            float[] agentInputs = new float[NEAT.Inputs];
            for(int inp = 0; inp < NEAT.Inputs && (inputOffset + inp) < FlatInputs.Length; inp++) {
                agentInputs[inp] = FlatInputs[inputOffset + inp];
            }
            // Aplicar aprendizaje: refuerza conexiones que llevaron a comer, debilita las que llevaron a hambre
            NEAT.HebbianUpdate(agent.PoolIndex, agentInputs, agent.RewardSignal, 0.0005f);
            
            // Eje 3: Decadencia de Castigo (Recuperación de confianza)
            agent.UpdateTrustLedger();
            
            // --- EMISIÓN DE SEÑALES SEMÁNTICAS (v9.6 Rate Limiting) ---
            int currentTick = (int)(Godot.Time.GetTicksMsec() / 16);
            if (currentTick > agent.lastBroadcastTick + Nanot.BROADCAST_COOLDOWN) {
                agent.CurrentBroadcastSignal = outs[4];
                agent.lastBroadcastTick = currentTick;
                
                // Si el mensaje es real y el costo es cero (energizado), no descontar biomasa en Nanot.cs
                if (freeBroadcast && Mathf.Abs(agent.CurrentBroadcastSignal) > 0.5f) {
                    agent.Metabolism.Biomass += Nanot.METABOLIC_RADIO_COST; // Reintegro instantáneo
                }
            } else {
                agent.CurrentBroadcastSignal *= 0.8f; // Atenuación si hay cooldown
            }
            
            // Si encontró comida cercana, emitir señal "COMIDA + dirección"
            bool hasNearbyFood = false;
            Vector2 nearFoodDir = Vector2.Zero;
            foreach(var f in FoodMgr.Pellets) {
                if (!f.IsRotten && agent.Position.DistanceSquaredTo(f.Position) < 2500f) {
                    hasNearbyFood = true;
                    nearFoodDir = (f.Position - agent.Position).Normalized();
                    break;
                }
            }
            
            if (hasNearbyFood) {
                // Engaño: mentirosos invierten la dirección
                float deceptionFlip = (GD.Randf() < agent.DeceptionTrait) ? -1f : 1f;
                agent.SignalType = 1.0f; // COMIDA
                agent.SignalDirX = nearFoodDir.X * deceptionFlip;
                agent.SignalDirY = nearFoodDir.Y * deceptionFlip;
                agent.CurrentBroadcastSignal = 1.0f; // Grito fuerte
            } else {
                agent.SignalType = outs[4]; // La red decide qué decir
                agent.SignalDirX = outs[0]; // Comparte su dirección de movimiento
                agent.SignalDirY = outs[1];
                agent.CurrentHelpSignal = outs[11]; // TAREA REQ: Emitir grito de auxilio
            }
            
            // Aplicar fuerzas sociales multiplicadas por Matriz Hebbiana
            if (Mathf.Abs(agentInputs[14]) > 0.01f || Mathf.Abs(agentInputs[15]) > 0.01f) {
                Vector2 socialFoodPull = new Vector2(agentInputs[14], agentInputs[15]).Normalized();
                agent.ApplyForce(socialFoodPull * 0.015f * outs[9]); // Multiplicado por matriz Hebbiana de Confianza en Reportes
            }
            if (Mathf.Abs(agent.DbgSocialHelpDir.X) > 0.01f || Mathf.Abs(agent.DbgSocialHelpDir.Y) > 0.01f) {
                Vector2 socialHelpPull = agent.DbgSocialHelpDir.Normalized();
                agent.ApplyForce(socialHelpPull * 0.02f * outs[10]); // Fuerza hacia el rescate, guiada por Empatía (outs[10])
            }
            
            // Reducir significativamente la deriva de frecuencia para que las facciones (clanes) sean estables
            agent.RadioFrequency += outs[5] * 0.0005f;
            if (agent.RadioFrequency < 0) agent.RadioFrequency += 1f;
            if (agent.RadioFrequency > 1f) agent.RadioFrequency -= 1f;
            
            // Posición de interacción estigmérgica: Detrás del agente (para dejar una estela de construcción)
            Vector2 forwardDir = new Vector2(Mathf.Cos(agent.Rotation - Mathf.Pi / 2.0f), Mathf.Sin(agent.Rotation - Mathf.Pi / 2.0f));
            Vector2 interactPos = agent.Position - forwardDir * 16f;
            
            // Acciones Estigmérgicas (Out 6)
            if (outs[6] > 0.5f && agent.Metabolism.Biomass > 2f) {
                // Instinto de construcción
                if (StigGrid.PlaceTile(interactPos, agent.RadioFrequency)) { // True si estaba libre y ahora es conductor
                    agent.Metabolism.Biomass -= 2f;
                }
            } else if (outs[6] < -0.5f) {
                // Instinto de descomposición
                if (StigGrid.DestroyTile(interactPos)) { // True si había estructura
                    agent.Metabolism.Ingest("BIOMASS", 1f); // Reabsorción balanceada
                }
            }
            
            // Electrificación Pasiva por Emisión Tonal o Gasto Biológico
            if (agent.CurrentBroadcastSignal > 0.8f) {
                StigGrid.EnergizeTileFromEntity(agent.Position);
            }
            
            // --- DESAFÍO C / PÁVLOV: Efecto del suelo en la biomasa ---
            byte tileAroma = StigGrid.GetTileClan(agent.Position);
            if (tileAroma > 0) {
                // Mapear el clan a frecuencia (aprox) -> TileClan = (Freq * 100) + 1
                // Rojo (Peligro) era 0.9f => 91
                if (Mathf.Abs(tileAroma - 91) < 5) {
                    // Trampa termal! Drena biomasa rápido
                    agent.Metabolism.Biomass -= 1.5f; // Dolor
                }
            }
            
            // --- INSTINTO ANIMAL BÁSICO (Prioridad Supervivencia) ---
            // Evaluar hambre: si tienen hambre y perciben comida, priorizan comer por instinto
            float hungerRatio = agent.Metabolism.Biomass / agent.Metabolism.MaxBiomass;
            if (hungerRatio < 0.6f && agent.DbgFoodDir != Vector2.Zero) {
                float desperacion = 1.0f - (hungerRatio / 0.6f); // 0 a 1 dependiendo del hambre
                // Condicionamiento Clásico: El instinto ahora es ponderado por outs[2] (Atracción o Evasión de la comida percibida)
                // outs[2] en [-1, 1], lo escalamos a [-2.5, 2.5] aprox
                Vector2 instintoComida = agent.DbgFoodDir * agent.MaxForce * (2.5f + desperacion * 7.0f) * outs[2]; 
                agent.ApplyForce(instintoComida);
                
                // Si están muy hambrientos, instintivamente frenan los gritos y construcciones para no morir
                if (desperacion > 0.5f) {
                    agent.CurrentBroadcastSignal *= 0.1f;
                }
            }
            
            int crossFlag = agent.AgentUpdate(ScreenSize, StigGrid, dt);
            
            // --- FEDERACIÓN CROSS-UNIVERSE (EXPORTAR GENOMA) ---
            if (crossUniverseEnabled && crossFlag != 0 && udpPeer != null) {
                byte[] genome = agent.ExportGenome();
                udpPeer.PutPacket(genome);
                agent.Die();
                continue;
            }
            
            // Consumo de Biomasa (Radio ampliado 400px² = ~20px)
            foreach(var f in FoodMgr.Pellets) {
                if (!f.IsRotten && agent.Position.DistanceSquaredTo(f.Position) < 400.0f) {
                    agent.Metabolism.Ingest("BIOMASS", f.Value);
                    f.Value = 0;
                }
            }
            
            // --- BOIDS COMPLETO: Cohesión + Separación + Alineación ---
            List<Nanot> socialNeighbors = new List<Nanot>();
            float qr = agent.CommRadius;
            qt.Query(new Rect2(agent.Position.X - qr, agent.Position.Y - qr, qr*2, qr*2), socialNeighbors);
            
            Vector2 cohesion = Vector2.Zero;
            Vector2 separation = Vector2.Zero; // Feature 5
            Vector2 alignment = Vector2.Zero;  // Feature 10
            int kindred = 0;
            
            foreach(var sn in socialNeighbors) {
                if (sn == agent || sn.IsDead) continue;
                float dist = agent.Position.DistanceTo(sn.Position);
                bool sameSpecies = Mathf.Abs(sn.RadioFrequency - agent.RadioFrequency) < 0.1f;
                
                // Feature 5: Separación (todos los vecinos, no solo misma especie)
                if (dist < 18f && dist > 0.001f) {
                    separation += (agent.Position - sn.Position).Normalized() / dist;
                }
                
                if (sameSpecies) {
                    cohesion += (sn.Position - agent.Position);
                    alignment += sn.Velocity; // Feature 10
                    kindred++;
                    
                    // Feature 6: Actualización de confianza
                    float trust = agent.GetTrust(sn.PoolIndex);
                    if (trust > 0.2f && Mathf.Abs(sn.CurrentBroadcastSignal) > 0.5f) {
                        // Si el vecino grita "comida" pero no hay comida cerca de él, reducir confianza
                        bool foodNearSpeaker = false;
                        foreach(var f in FoodMgr.Pellets) {
                            if (!f.IsRotten && sn.Position.DistanceSquaredTo(f.Position) < 2500f) {
                                foodNearSpeaker = true; break;
                            }
                        }
                        if (!foodNearSpeaker && sn.CurrentBroadcastSignal > 0.5f) {
                            agent.UpdateTrust(sn.PoolIndex, -0.05f); // Penalización más fuerte
                            
                            // Eje 2: Validación Sensorial antes de penalización Hebbiana
                            // Si el agente siguió un reporte falso y perdió energía buscando, penalizar neuronas
                            if (agent.DbgFoodProximity < 0.1f && agent.RewardSignal < 0) {
                                NEAT.HebbianUpdate(agent.PoolIndex, agentInputs, -2.0f, 0.001f); // Castigo por dejarse engañar
                            }
                        } else if (foodNearSpeaker) {
                            agent.UpdateTrust(sn.PoolIndex, 0.02f); // Gana reputación rápido
                        }
                        
                        // Eje 4: Crear Vínculo Simbiótico (Solo misma especie y alta confianza)
                        if (dist < 12f && trust > 0.8f && Links.Count < 500) {
                             bool alreadyLinked = false;
                             foreach(var l in Links) if((l.A == agent && l.B == sn) || (l.A == sn && l.B == agent)) { alreadyLinked = true; break; }
                             if(!alreadyLinked) {
                                 Links.Add(new CellularLink(agent, sn, "SYMBIOSIS"));
                             }
                        }
                    }
                }
            }
            // Feature 11: Contar vecinos para debug visual
            agent.DbgNeighborCount = socialNeighbors.Count - 1; // Excluir a sí mismo
            agent.DbgKindredCount = kindred;
            
            if (kindred > 0) {
                cohesion /= kindred;
                alignment /= kindred;
                
                // Aplicar Condicionamiento Operante: La red neuronal (HebbianMatrix) dicta la fuerza de Cohesión (outs[3]) y Alineación (outs[8])
                float cohWeight = 0.008f * outs[3];
                float aliWeight = 0.005f * outs[8];
                agent.ApplyForce(cohesion.Normalized() * cohWeight); // Cohesión
                agent.ApplyForce(alignment.Normalized() * aliWeight); // Alineación
                // Feature 11: Almacenar Boids para debug visual
                agent.DbgCohesion = cohesion.Normalized() * Mathf.Abs(outs[3]);
                agent.DbgAlignment = alignment.Normalized() * Mathf.Abs(outs[8]);
            } else {
                agent.DbgCohesion = Vector2.Zero;
                agent.DbgAlignment = Vector2.Zero;
            }
            // Multiplicador Hebbiano de separación (outs[7])
            float sepWeight = 0.03f * (1.0f + Mathf.Max(0, outs[7])); // Separación suele ser positiva obligatoria, pero el cerebro puede iterarla
            agent.ApplyForce(separation * sepWeight); // Separación
            agent.DbgSeparation = separation * (1.0f + Mathf.Max(0, outs[7])); // Feature 11
            
            // Feature 7: Engaño - El agente puede mentir sobre recursos
            if (agent.CurrentBroadcastSignal > 0.5f && GD.Randf() < agent.DeceptionTrait) {
                agent.CurrentBroadcastSignal *= -1; // Invierte la señal (engaño)
            }
            
            // Feature 3: Reproducción SEXUAL (2 padres + crossover)
            // Solo pueden nacer nuevos nanots si la población actual permite acercarse a la objetivo
            if (agent.CanReproduce && (Pop.Count + babies.Count < TargetPopulation)) {
                agent.CanReproduce = false;
                
                // Buscar pareja compatible cercana (misma especie, también fértil)
                Nanot mate = null;
                foreach(var sn in socialNeighbors) {
                    if (sn == agent || sn.IsDead || !sn.CanReproduce) continue;
                    if (Mathf.Abs(sn.RadioFrequency - agent.RadioFrequency) < 0.1f) {
                        mate = sn; break;
                    }
                }
                
                int parentB = mate != null ? Pop.IndexOf(mate) : i; // Si no hay pareja, mitosis asexual
                if (mate != null) mate.CanReproduce = false;
                
                // Pagar costo reproductivo
                agent.Metabolism.ConsumeForReproduction();
                
                Nanot baby = new Nanot();
                Vector2 spawnPos = agent.Position + new Vector2((float)GD.RandRange(-10, 10), (float)GD.RandRange(-10, 10));
                baby.Initialize(spawnPos);
                baby.Rotation = (float)GD.RandRange(0, Mathf.Tau);
                
                int childIndex = Pop.Count + babies.Count;
                if (parentB >= 0 && parentB != i) {
                    NEAT.Crossover(childIndex, i, parentB); // Crossover sexual real
                } else {
                    NEAT.Inherit(childIndex, i); // Mitosis fallback
                }
                NEAT.Mutate(childIndex, MutationRate);
                
                // Heredar rasgos del padre (con variación)
                baby.RadioFrequency = agent.RadioFrequency + (float)GD.RandRange(-0.03, 0.03);
                if (baby.RadioFrequency < 0) baby.RadioFrequency += 1f;
                if (baby.RadioFrequency > 1f) baby.RadioFrequency -= 1f;
                baby.PoolIndex = childIndex;
                
                // Heredar rasgos sociales
                float mateDeception = mate != null ? mate.DeceptionTrait : agent.DeceptionTrait;
                baby.DeceptionTrait = Mathf.Clamp(
                    (agent.DeceptionTrait + mateDeception) / 2f + (float)GD.RandRange(-0.05, 0.05),
                    0f, 1f
                );
                // Feature 12: CommRadius mutable con crossover sexual real
                float mateComm = mate != null ? mate.CommRadius : agent.CommRadius;
                baby.CommRadius = Mathf.Clamp(
                    (agent.CommRadius + mateComm) / 2f + (float)GD.RandRange(-8, 8),
                    20f, 120f
                );
                
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
            p.Hunt(qt, ScreenSize, StigGrid);
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
                LblInspCommRadius.Text = $"Radio Comunic: {(int)ag.CommRadius}px [20-120]";
                LblInspCommRadius.AddThemeColorOverride("font_color", new Color(0, 1, 1, 1));
                LblInspDeception.Text = $"Engaño: {(int)(ag.DeceptionTrait * 100)}%";
                LblInspDeception.AddThemeColorOverride("font_color", ag.DeceptionTrait > 0.5f ? Colors.Red : Colors.Green);
                LblInspNeighbors.Text = $"Vecinos: {ag.DbgNeighborCount} total | {ag.DbgKindredCount} especie";
                LblInspReward.Text = $"Recompensa Hebbiana: {ag.RewardSignal:F3}";
                LblInspReward.AddThemeColorOverride("font_color", ag.RewardSignal > 0 ? Colors.Green : Colors.Red);
                
                var lblLink = InspectorUI.FindChild("LblInspLink", true, false) as Label;
                if (lblLink != null) {
                    if (ag.ActiveLink != null) {
                        lblLink.Text = $"Vínculo: {ag.ActiveLink.Type} (Age:{ag.ActiveLink.Age:F0})";
                        lblLink.AddThemeColorOverride("font_color", ag.ActiveLink.Type == "SYMBIOSIS" ? Colors.Cyan : Colors.Red);
                    } else {
                        lblLink.Text = "Vínculo: Ninguno";
                        lblLink.AddThemeColorOverride("font_color", Colors.Gray);
                    }
                }
                
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
