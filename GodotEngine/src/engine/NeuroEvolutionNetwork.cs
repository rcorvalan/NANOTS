using Godot;
using System;

public class NeuroEvolutionNetwork
{
    private int PopSize;
    public int Inputs = 21;
    public int Hidden = 8;
    public int Outputs = 12;

    public float[] FlatWeights;
    public float[] FlatBiases;

    private int W_total;
    private int B_total;
    private RandomNumberGenerator rng;

    public NeuroEvolutionNetwork(int popSize)
    {
        PopSize = popSize;
        W_total = (Inputs * Hidden) + (Hidden * Outputs);
        B_total = Hidden + Outputs;

        FlatWeights = new float[PopSize * W_total];
        FlatBiases = new float[PopSize * B_total];

        rng = new RandomNumberGenerator();
        rng.Randomize();

        // Inicialización Xavier/He (Pesos aleatorios)
        for (int i = 0; i < FlatWeights.Length; i++) {
            FlatWeights[i] = rng.RandfRange(-1f, 1f);
        }
        for (int i = 0; i < FlatBiases.Length; i++) {
            FlatBiases[i] = rng.RandfRange(-0.5f, 0.5f);
        }
    }

    public void Mutate(int agentIndex, float rate)
    {
        int wOffset = agentIndex * W_total;
        for (int i = 0; i < W_total; i++) {
            if (rng.Randf() < rate) {
                FlatWeights[wOffset + i] += rng.RandfRange(-0.5f, 0.5f);
                FlatWeights[wOffset + i] = Mathf.Clamp(FlatWeights[wOffset + i], -5f, 5f);
            }
        }

        int bOffset = agentIndex * B_total;
        for (int i = 0; i < B_total; i++) {
            if (rng.Randf() < rate) {
                FlatBiases[bOffset + i] += rng.RandfRange(-0.2f, 0.2f);
                FlatBiases[bOffset + i] = Mathf.Clamp(FlatBiases[bOffset + i], -2f, 2f);
            }
        }
    }

    public void SwapBrains(int targetIndex, int sourceIndex)
    {
        if (targetIndex == sourceIndex) return;
        
        int offsetT_W = targetIndex * W_total;
        int offsetS_W = sourceIndex * W_total;
        for (int i = 0; i < W_total; i++) {
            FlatWeights[offsetT_W + i] = FlatWeights[offsetS_W + i];
        }

        int offsetT_B = targetIndex * B_total;
        int offsetS_B = sourceIndex * B_total;
        for (int i = 0; i < B_total; i++) {
            FlatBiases[offsetT_B + i] = FlatBiases[offsetS_B + i];
        }
    }

    public void Inherit(int childIndex, int parentIndex)
    {
        int pW = parentIndex * W_total;
        int cW = childIndex * W_total;
        for(int i = 0; i < W_total; i++) {
            FlatWeights[cW + i] = FlatWeights[pW + i];
        }

        int pB = parentIndex * B_total;
        int cB = childIndex * B_total;
        for(int i = 0; i < B_total; i++) {
            FlatBiases[cB + i] = FlatBiases[pB + i];
        }
    }
    
    public void Crossover(int childIndex, int parentA, int parentB)
    {
        int aw = parentA * W_total;
        int bw = parentB * W_total;
        int cw = childIndex * W_total;
        
        for (int i = 0; i < W_total; i++) {
            FlatWeights[cw + i] = rng.Randf() > 0.5f ? FlatWeights[aw + i] : FlatWeights[bw + i];
        }
        
        int ab = parentA * B_total;
        int bb = parentB * B_total;
        int cb = childIndex * B_total;
        
        for (int i = 0; i < B_total; i++) {
            FlatBiases[cb + i] = rng.Randf() > 0.5f ? FlatBiases[ab + i] : FlatBiases[bb + i];
        }
    }
    
    /// <summary>
    /// Aprendizaje Hebbiano Simplificado (intra-vida):
    /// reward > 0: Refuerza los pesos de la capa Input→Hidden (lo que hizo fue bueno)
    /// reward < 0: Debilita esos pesos (lo que hizo fue malo)
    /// learningRate controla cuánto cambian por episodio (~0.001f)
    /// </summary>
    public void HebbianUpdate(int agentIndex, float[] lastInputs, float reward, float learningRate = 0.001f)
    {
        if (agentIndex < 0 || Mathf.Abs(reward) < 0.001f) return;
        
        int wOffset = agentIndex * W_total;
        
        // Solo modificar la primera capa (Input→Hidden) para estabilidad
        int inputHiddenWeights = Inputs * Hidden;
        
        for (int h = 0; h < Hidden; h++) {
            for (int inp = 0; inp < Inputs; inp++) {
                int wIdx = wOffset + (h * Inputs + inp);
                if (wIdx >= FlatWeights.Length) break;
                
                // Regla Hebbiana: Δw = η * reward * input_activation
                float inputVal = (inp < lastInputs.Length) ? lastInputs[inp] : 0f;
                float delta = learningRate * reward * inputVal;
                
                FlatWeights[wIdx] += delta;
                FlatWeights[wIdx] = Mathf.Clamp(FlatWeights[wIdx], -5f, 5f);
            }
        }
    }

    /// <summary>
    /// Guarda los pesos y sesgos evolutivos en un archivo binario.
    /// Utiliza Buffer.BlockCopy para una escritura masiva ultra rápida.
    /// </summary>
    public void SaveLearning(string absolutePath)
    {
        try {
            int wBytes = FlatWeights.Length * sizeof(float);
            int bBytes = FlatBiases.Length * sizeof(float);
            byte[] fullData = new byte[wBytes + bBytes];
            
            Buffer.BlockCopy(FlatWeights, 0, fullData, 0, wBytes);
            Buffer.BlockCopy(FlatBiases, 0, fullData, wBytes, bBytes);
            
            System.IO.File.WriteAllBytes(absolutePath, fullData);
        } catch (Exception ex) {
            GD.PrintErr($"[NEAT] Error saving learning: {ex.Message}");
        }
    }

    /// <summary>
    /// Intenta restaurar el aprendizaje de una sesión anterior.
    /// </summary>
    public bool LoadLearning(string absolutePath)
    {
        try {
            if (!System.IO.File.Exists(absolutePath)) return false;
            
            byte[] fullData = System.IO.File.ReadAllBytes(absolutePath);
            int wBytes = FlatWeights.Length * sizeof(float);
            int bBytes = FlatBiases.Length * sizeof(float);
            
            // Validacion de compatibilidad basica
            if (fullData.Length != wBytes + bBytes) {
                GD.PrintErr("[NEAT] Archivo de aprendizaje incompatible. Tamaño de red diferente.");
                return false;
            }
            
            Buffer.BlockCopy(fullData, 0, FlatWeights, 0, wBytes);
            Buffer.BlockCopy(fullData, wBytes, FlatBiases, 0, bBytes);
            return true;
            
        } catch (Exception ex) {
            GD.PrintErr($"[NEAT] Error loading learning: {ex.Message}");
            return false;
        }
    }
}
