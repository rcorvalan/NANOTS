using Godot;

public class CellularLink
{
    public Nanot A;
    public Nanot B;
    public string Type; // SYMBIOSIS, PARASITE, COMMUNICATION
    public float Age = 0f;

    public CellularLink(Nanot a, Nanot b, string type)
    {
        A = a;
        B = b;
        Type = type;
    }

    public void Update()
    {
        Age++;
        if (A.IsDead || B.IsDead) return;

        if (Type == "SYMBIOSIS")
        {
            float avgB = (A.Metabolism.Biomass + B.Metabolism.Biomass) / 2f;
            float avgM = (A.Metabolism.Mineral + B.Metabolism.Mineral) / 2f;
            A.Metabolism.Biomass = avgB;
            B.Metabolism.Biomass = avgB;
            A.Metabolism.Mineral = avgM;
            B.Metabolism.Mineral = avgM;
            
            // Fuerza de atracción leve
            float dist = A.Position.DistanceTo(B.Position);
            if (dist > 0.001f) {
                Vector2 diffA = (B.Position - A.Position).Normalized() * 0.01f;
                A.ApplyForce(diffA);
                B.ApplyForce(-diffA);
            }
        }
    }
}
