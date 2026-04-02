using Godot;

public partial class StigmergicGrid : Sprite2D
{
    private int Width;
    private int Height;
    private float CellSize = 4f;

    // Grid states:
    // 0 = Vacio
    // 1 = Cable (Aislador / Orugas construidas)
    // 2 = Cable Energizado
    private byte[,] GridLogic;
    private byte[,] NextLogic;
    
    // Decay state (Para que la energía se apague gradualmente y regrese al 1)
    private byte[,] SignalLife; 

    private Image Img;
    private ImageTexture Tex;
    
    private int frameSkip = 0;

    public void Initialize(float screenWidth, float screenHeight)
    {
        Width = Mathf.CeilToInt(screenWidth / CellSize);
        Height = Mathf.CeilToInt(screenHeight / CellSize);

        GridLogic = new byte[Width, Height];
        NextLogic = new byte[Width, Height];
        SignalLife = new byte[Width, Height];

        Img = Image.CreateEmpty(Width, Height, false, Image.Format.Rgba8);
        Img.Fill(new Color(0, 0, 0, 0));
        
        Tex = ImageTexture.CreateFromImage(Img);
        Texture = Tex;
        
        // El Grid se dibuja escalado para cubrir la pantalla
        Scale = new Vector2(CellSize, CellSize);
        Position = new Vector2(Width * CellSize / 2f, Height * CellSize / 2f);
        ZIndex = -1; // Debajo de los nanots, encima del fondo oscuro
    }

    public void UpdateAutomata()
    {
        frameSkip++;
        if (frameSkip < 5) return; // Procesar cada 5 frames lógicos
        frameSkip = 0;

        bool changed = false;

        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                NextLogic[x, y] = GridLogic[x, y];
                byte state = GridLogic[x, y];

                if (state == 0) continue;

                if (state == 2) {
                    // Decay the signal
                    if (SignalLife[x, y] > 0) {
                        SignalLife[x, y]--;
                    } else {
                        NextLogic[x, y] = 1; // Return to inert conductive cable
                        changed = true;
                    }
                    
                    // Propagar a vecinos inactivos
                    for(int dy = -1; dy <= 1; dy++) {
                        for(int dx = -1; dx <= 1; dx++) {
                            int nx = x + dx;
                            int ny = y + dy;
                            if (nx >= 0 && nx < Width && ny >= 0 && ny < Height) {
                                if (GridLogic[nx, ny] == 1) { // Vecino conductor
                                    NextLogic[nx, ny] = 2; // Energizar!
                                    SignalLife[nx, ny] = 5; // Lifetime de 5 ticks lógicos
                                    changed = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Swap memory
        byte[,] temp = GridLogic;
        GridLogic = NextLogic;
        NextLogic = temp;

        if (changed) {
            RedrawTexture();
        }
    }

    private void RedrawTexture()
    {
        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                byte state = GridLogic[x, y];
                if (state == 0) {
                    Img.SetPixel(x, y, new Color(0,0,0,0)); // Transparente vacio
                } else if (state == 1) {
                    Img.SetPixel(x, y, new Color(0.3f, 0.3f, 0.3f, 0.4f)); // Cable gris oscuro
                } else if (state == 2) {
                    // Cable Brillante Eléctrico Cyan o Azul
                    Img.SetPixel(x, y, new Color(0f, 1f, 1f, 1f)); 
                }
            }
        }
        Tex.Update(Img);
    }
    
    // --- Interfaces públicas para el Nanot ---

    public byte CheckTile(Vector2 worldPos)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt(worldPos.X / CellSize), 0, Width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(worldPos.Y / CellSize), 0, Height - 1);
        return GridLogic[x, y];
    }

    public bool PlaceTile(Vector2 worldPos)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt(worldPos.X / CellSize), 0, Width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(worldPos.Y / CellSize), 0, Height - 1);
        if (GridLogic[x,y] == 0) {
            GridLogic[x,y] = 1; // Poner Cable inerte
            RedrawTexture();
            return true;
        }
        return false;
    }

    public bool DestroyTile(Vector2 worldPos)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt(worldPos.X / CellSize), 0, Width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(worldPos.Y / CellSize), 0, Height - 1);
        if (GridLogic[x,y] != 0) {
            GridLogic[x,y] = 0;
            RedrawTexture();
            return true; 
        }
        return false;
    }

    public void EnergizeTileFromEntity(Vector2 worldPos)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt(worldPos.X / CellSize), 0, Width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(worldPos.Y / CellSize), 0, Height - 1);
        if (GridLogic[x, y] == 1) { // Si hay estructura inerte y un Nanot la pisa electrificando
            GridLogic[x, y] = 2;
            SignalLife[x, y] = 10; // Carga fuerte
        }
    }
}
