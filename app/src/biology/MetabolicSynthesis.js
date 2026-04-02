export class MetabolicSynthesis {
  /**
   * Administra la química y dualidad de recursos:
   * Biomasa = Combustible basal (Energía pura de supervivencia)
   * Mineral = Material constitutivo (Para reproducirse y construir Estigmergia)
   */
  constructor(initialBiomass = 100, initialMineral = 0) {
    this.biomass = initialBiomass;
    this.mineral = initialMineral;

    this.maxBiomass = 200;
    this.maxMineral = 100;
  }

  decay(baseDrain, environmentalHeat) {
    // Heat = -1 to 1. 
    // Calor extra aumenta el gasto metabólico (se mueven más rápido). 
    // Frío lo aletarga (hibernación relativa).
    let heatModifier = 1.0 + (environmentalHeat * 0.5); 
    this.biomass -= (baseDrain * heatModifier);
  }

  ingest(type, amount) {
    if (type === 'BIOMASS') {
        this.biomass = Math.min(this.maxBiomass, this.biomass + amount);
    } else if (type === 'MINERAL') {
        this.mineral = Math.min(this.maxMineral, this.mineral + amount);
    }
  }

  canReproduce() {
    // La reproducción ahora es biológica (requiere biomasa) y sintética (requiere sílice/mineral)
    return this.biomass > 100 && this.mineral > 30;
  }

  consumeForReproduction() {
    this.biomass -= 60;
    this.mineral -= 30; // Costo genético y estructural
  }

  consumeForStigmergy(cost) {
    if (this.mineral >= cost) {
        this.mineral -= cost;
        return true;
    }
    return false;
  }

  isDead() {
    return this.biomass <= 0;
  }
}
