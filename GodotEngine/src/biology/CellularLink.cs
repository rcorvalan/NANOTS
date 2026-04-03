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
        else if (Type == "PARASITE")
        {
            // A drena activamente a B (B muere de a poco si no se la saca de encima)
            float drainRate = 0.5f;
            if (B.Metabolism.Biomass > drainRate) {
                B.Metabolism.Biomass -= drainRate;
                A.Metabolism.Biomass += drainRate * 0.8f; // 80% eficiencia, 20% friccion
            } else {
                Type = "DEAD_LINK"; // Host ya no tiene como sustentar
            }
            
            // Atraccion forzosa (B intenta huir ralentizado, A se agarra)
            float dist = A.Position.DistanceTo(B.Position);
            if (dist > 0.001f && dist < 100f) {
                Vector2 pull = (B.Position - A.Position).Normalized();
                A.ApplyForce(pull * 0.05f); // Parasite salta al host
                B.ApplyForce(-pull * 0.02f); // Host es mermado fisicamente
            } else if (dist >= 100f) {
                 Type = "DEAD_LINK"; // Host logró escapar
            }
        }
    }
}
