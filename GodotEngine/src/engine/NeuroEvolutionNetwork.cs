using Godot;
using System;

public class NeuroEvolutionNetwork
{
    private int PopSize;
    public int Inputs = 16;
    public int Hidden = 8;
    public int Outputs = 7;

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
}
