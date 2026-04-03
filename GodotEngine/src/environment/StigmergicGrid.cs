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
    private byte[,] GridClan;
    
    // Decay state (Para que la energía se apague gradualmente y regrese al 1)
    private byte[,] SignalLife; 
    
    // Wall Life (Para que los muros de estigmergia se derrumben por el tiempo)
    private byte[,] WallLife;

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
        WallLife = new byte[Width, Height];
        GridClan = new byte[Width, Height];

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
                
                // Decay of the physical wall itself
                if (WallLife[x, y] > 0) {
                    WallLife[x, y]--;
                } else {
                    NextLogic[x, y] = 0; // Collapsed!
                    GridClan[x, y] = 0;  // Clean Clan
                    changed = true;
                    continue;            // Skip further processing
                }

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
                    // Cable del color del Clan (alto contraste)
                    float hue = (Mathf.Max(0, GridClan[x,y] - 1)) / 100f;
                    Img.SetPixel(x, y, Color.FromHsv(hue, 1f, 0.7f, 1f));
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

    public bool PlaceTile(Vector2 worldPos, float radioFreq)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt(worldPos.X / CellSize), 0, Width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(worldPos.Y / CellSize), 0, Height - 1);
        if (GridLogic[x,y] == 0) {
            GridLogic[x,y] = 1; // Poner Cable inerte
            GridClan[x,y] = (byte)Mathf.Clamp((radioFreq * 254f) + 1, 1, 255);
            WallLife[x,y] = 255; // Vida util del muro (~21 segundos a 60fps)
            RedrawTexture();
            return true;
        } else {
            // Si ya hay muro, pisarlo lo repara (Renueva su vida temporal)
            WallLife[x,y] = 255; 
        }
        return false;
    }

    public bool DestroyTile(Vector2 worldPos)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt(worldPos.X / CellSize), 0, Width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(worldPos.Y / CellSize), 0, Height - 1);
        if (GridLogic[x,y] != 0) {
            GridLogic[x,y] = 0;
            GridClan[x,y] = 0;
            RedrawTexture();
            return true; 
        }
        return false;
    }

    public byte GetTileClan(Vector2 worldPos)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt(worldPos.X / CellSize), 0, Width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(worldPos.Y / CellSize), 0, Height - 1);
        return GridClan[x, y];
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
