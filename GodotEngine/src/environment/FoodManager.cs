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
        // Pudrir alimento (-0.1f de valor nutricional por frame)
        foreach(var p in Pellets) {
            p.Value -= 0.1f;
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
