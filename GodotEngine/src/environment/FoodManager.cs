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
            RandomNumberGenerator rng = new RandomNumberGenerator();
            rng.Randomize();
            
            // Punto focal del racimo (la comida crece agrupada como en la naturaleza)
            Vector2 focalPoint = new Vector2(rng.RandfRange(80, WorldW - 80), rng.RandfRange(80, WorldH - 80));
            
            for(int i = 0; i < PelletsPerSpawn; i++) {
                Vector2 offset = new Vector2(rng.RandfRange(-60, 60), rng.RandfRange(-60, 60));
                DropFood(focalPoint + offset, rng.RandfRange(60f, 75f));
            }
        }

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
