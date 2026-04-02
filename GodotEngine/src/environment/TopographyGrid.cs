using Godot;
using System;

public partial class TopographyGrid : Node2D
{
    public float[] RadiationMap;
    public float[] HeatMap;
    public int Cols, Rows;
    public float CellSize;
    
    private Image RenderImage;
    private ImageTexture RenderTexture;
    private Sprite2D Visualizer;

    public void Initialize(float width, float height, float cellSize = 20f)
    {
        CellSize = cellSize;
        Cols = Mathf.CeilToInt(width / cellSize);
        Rows = Mathf.CeilToInt(height / cellSize);

        RadiationMap = new float[Cols * Rows];
        HeatMap = new float[Cols * Rows];

        GenerateTerrain();
        CreateVisualizer();
    }

    private void GenerateTerrain()
    {
        float centerX = Cols / 2f;
        float centerY = Rows / 2f;

        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < Cols; x++)
            {
                int idx = x + y * Cols;
                float distToCenterSq = (x - centerX) * (x - centerX) + (y - centerY) * (y - centerY);
                float normalizedDist = distToCenterSq / (centerX * centerX + centerY * centerY + 0.001f);
                
                // Variación de Temperatura y radiación
                float heatVal = Mathf.Cos(normalizedDist * Mathf.Pi * 4f) * 0.5f;
                // Anillos de calor
                heatVal += Mathf.Max(0f, 1.0f - (Mathf.Sqrt(distToCenterSq) / (centerX * 0.5f))); 
                HeatMap[idx] = Mathf.Clamp(heatVal, -1f, 1f);

                float radVal = (Mathf.Sin(normalizedDist * Mathf.Pi * 8f) > 0.8f) ? 1.0f : 0.0f;
                RadiationMap[idx] = radVal;
            }
        }
    }

    private void CreateVisualizer()
    {
        Visualizer = new Sprite2D();
        RenderImage = Image.CreateEmpty(Cols, Rows, false, Image.Format.Rgba8);

        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < Cols; x++)
            {
                float heat = HeatMap[x + y * Cols];
                float rad = RadiationMap[x + y * Cols];
                
                // Mezcla de visualización
                float r = heat > 0 ? 1.0f : 0.0f;
                float b = heat < 0 ? 1.0f : 0.0f;
                float alpha = Mathf.Abs(heat) * 0.15f;

                if (rad > 0.2f) {
                    r = rad; b = rad; // Mutágeno Purpura/Magenta
                    alpha = rad * 0.3f;
                }
                
                Color c = new Color(r, 0f, b, alpha); // Se quitó el canal verde para la UI
                RenderImage.SetPixel(x, y, c);
            }
        }
        
        RenderTexture = ImageTexture.CreateFromImage(RenderImage);
        Visualizer.Texture = RenderTexture;
        Visualizer.Scale = new Vector2(CellSize, CellSize);
        Visualizer.Position = new Vector2(Cols * CellSize * 0.5f, Rows * CellSize * 0.5f); // Centro del Sprite
        AddChild(Visualizer);
    }

    public (float radiation, float heat) GetPropsAt(float x, float y)
    {
        int col = Mathf.FloorToInt(x / CellSize);
        int row = Mathf.FloorToInt(y / CellSize);

        if (col < 0 || col >= Cols || row < 0 || row >= Rows)
            return (0f, 0f); // Default neutro

        int idx = col + row * Cols;
        return (RadiationMap[idx], HeatMap[idx]);
    }
}
