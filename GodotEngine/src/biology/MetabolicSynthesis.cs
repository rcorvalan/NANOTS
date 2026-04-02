using Godot;

public class MetabolicSynthesis
{
    public float Biomass;
    public float Mineral;
    public float MaxBiomass = 200f;
    public float MaxMineral = 100f;

    public MetabolicSynthesis(float initialBiomass = 100f, float initialMineral = 0f)
    {
        Biomass = initialBiomass;
        Mineral = initialMineral;
    }

    public void Decay(float baseDrain, float environmentalHeat)
    {
        // Calor de -1 a 1
        float heatModifier = 1.0f + (environmentalHeat * 0.5f);
        Biomass -= (baseDrain * heatModifier);
    }

    public void Ingest(string type, float amount)
    {
        if (type == "BIOMASS")
            Biomass = Mathf.Min(MaxBiomass, Biomass + amount);
        else if (type == "MINERAL")
            Mineral = Mathf.Min(MaxMineral, Mineral + amount);
    }

    public bool CanReproduce() => Biomass > 100f && Mineral > 30f;

    public void ConsumeForReproduction()
    {
        Biomass -= 60f;
        Mineral -= 30f;
    }

    public bool ConsumeForStigmergy(float cost)
    {
        if (Mineral >= cost) {
            Mineral -= cost;
            return true;
        }
        return false;
    }
    
    public bool IsDead() => Biomass <= 0f || float.IsNaN(Biomass);
}
