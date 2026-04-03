using Godot;
using System.Collections.Generic;

public class FoodPellet
{
    public Vector2 Position;
    public float Value;
    public float MaxValue;
    
    public bool IsRotten => Value <= 0;
}

public partial class FoodManager : MultiMeshInstance2D
{
    public List<FoodPellet> Pellets = new List<FoodPellet>();
    private const int MAX_PELLETS = 10000;
    private float SpawnTimer = 0f;
    private float SpawnInterval = 0.2f; // Cada 0.2 segundos genera un lote
    private int PelletsPerSpawn = 5;
    private float WorldW, WorldH;
    
    // Feature: Nodos Estratégicos (Oasis)
    public List<Vector2> ResourceNodes = new List<Vector2>();
    public float AbundanceMultiplier = 1.0f;
    
    public override void _Ready()
    {
        Multimesh = new MultiMesh();
        Multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform2D;
        Multimesh.UseColors = true;
        Multimesh.InstanceCount = MAX_PELLETS;
        Multimesh.VisibleInstanceCount = 0;
        
        // Simple Square for food
        var rectMesh = new QuadMesh();
        rectMesh.Size = new Vector2(10, 10);
        Multimesh.Mesh = rectMesh;
        
        // El color estandar sera manejado por instancia
    }

    public void SetWorldSize(float w, float h) {
        WorldW = w;
        WorldH = h;
        
        // Generar 3 Oasis permanentes
        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();
        for(int i = 0; i < 3; i++) {
            ResourceNodes.Add(new Vector2(rng.RandfRange(150, WorldW - 150), rng.RandfRange(150, WorldH - 150)));
        }
    }
    
    public override void _Draw() {
        // Dibujar tenues campos de energía dorada bajo los oasis
        foreach (var node in ResourceNodes) {
            DrawCircle(node, 120f, new Color(1f, 0.9f, 0.1f, 0.05f)); 
            DrawCircle(node, 60f, new Color(1f, 0.9f, 0.1f, 0.1f)); 
        }
    }
    
    public void DropFood(Vector2 pos, float value)
    {
        if (Pellets.Count >= MAX_PELLETS) return;
        
        Pellets.Add(new FoodPellet {
            Position = pos,
            Value = value,
            MaxValue = value
        });
    }
    
    public void UpdateAndRender()
    {
        // Generación ambiental periódica de comida
        SpawnTimer += 0.016f; // ~60fps
        if (SpawnTimer >= SpawnInterval && Pellets.Count < MAX_PELLETS - 100) {
            SpawnTimer = 0f;
            int spawnAmount = Mathf.RoundToInt(PelletsPerSpawn * AbundanceMultiplier);
            
            if (spawnAmount > 0 && ResourceNodes.Count > 0) {
                RandomNumberGenerator rng = new RandomNumberGenerator();
                rng.Randomize();
                
                // Elegir aleatoriamente uno de los nodos (Oasis) ya definidos
                Vector2 focalPoint = ResourceNodes[rng.RandiRange(0, ResourceNodes.Count - 1)];
                
                for(int i = 0; i < spawnAmount; i++) {
                    // Concentrado cerca del centro con leve desvío (radio de ~60px)
                    Vector2 offset = new Vector2(rng.RandfRange(-60, 60), rng.RandfRange(-60, 60));
                    DropFood(focalPoint + offset, rng.RandfRange(60f, 75f));
                }
            }
        }
        
        // Solicitar repintado de halos de oasis si es necesario
        QueueRedraw();

        // Pudrir alimento (-0.02f en vez de -0.1f para que dure más)
        foreach(var p in Pellets) {
            p.Value -= 0.02f;
        }
        
        // Purgar la biomasa podrida o comida
        Pellets.RemoveAll(p => p.IsRotten);
        
        Multimesh.VisibleInstanceCount = Pellets.Count;
        for(int i = 0; i < Pellets.Count; i++) {
            var f = Pellets[i];
            Multimesh.SetInstanceTransform2D(i, new Transform2D(0, f.Position));
            
            // Color de comida: Amarillo Dorado (Biomasa/Cristal)
            float alpha = Mathf.Clamp(f.Value / f.MaxValue, 0.4f, 1f);
            Multimesh.SetInstanceColor(i, new Color(1f, 0.9f, 0.1f, alpha));
        }
    }
}
