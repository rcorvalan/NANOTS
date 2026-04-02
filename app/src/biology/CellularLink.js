export class CellularLink {
  /**
   * Gestor de enlaces estructurales e informáticos entre dos Nanots (Multi-celularidad temporal)
   */
  constructor() {
    // Lista de vínculos activos: array de objetos { sourceId, targetId, type }
    this.links = [];
  }

  createLink(nanotA, nanotB, type) {
    // type: "SYMBIOSIS" o "PARASITISM"
    this.links.push({
      idA: nanotA.id,
      idB: nanotB.id,
      agA: nanotA,
      agB: nanotB,
      type: type,
      strength: 1.0 // Se erosiona con el movimiento/distancia
    });
  }

  update() {
    for (let i = this.links.length - 1; i >= 0; i--) {
      let link = this.links[i];
      let A = link.agA;
      let B = link.agB;

      // Si alguno muere, el enlace colapsa
      if (A.dead || B.dead) {
        this.links.splice(i, 1);
        continue;
      }

      let dist = A.position.distance(B.position);

      // Rotura por fuerza física extrema
      if (dist > 30) {
         this.links.splice(i, 1);
         continue;
      }

      // Procesamiento químico del enlace
      if (link.type === 'PARASITISM') {
        // A drena activamente a B (Drenaje vampírico de biomasa)
        if (B.metabolism.biomass > 5) {
            B.metabolism.biomass -= 0.5;
            A.metabolism.ingest('BIOMASS', 0.4); // Pérdida de transformación del 20%
        }
      } 
      else if (link.type === 'SYMBIOSIS') {
        // Comparten promedios de nutrientes para sobrevivir juntos (Homeostasis mutualista)
        let avgB = (A.metabolism.biomass + B.metabolism.biomass) / 2;
        let avgM = (A.metabolism.mineral + B.metabolism.mineral) / 2;
        A.metabolism.biomass = avgB;
        B.metabolism.biomass = avgB;
        A.metabolism.mineral = avgM;
        B.metabolism.mineral = avgM;

        // Atracción elástica para forzarlos a moverse como una membrana celular
        // Un vector tipo "Spring Force"
        let diffA = B.position.clone().sub(A.position).normalize().mult(0.01);
        let diffB = A.position.clone().sub(B.position).normalize().mult(0.01);
        A.applyForce(diffA);
        B.applyForce(diffB);
      }
    }
  }

  draw(ctx) {
    for (let link of this.links) {
      ctx.beginPath();
      ctx.moveTo(link.agA.position.x, link.agA.position.y);
      ctx.lineTo(link.agB.position.x, link.agB.position.y);
      if (link.type === 'SYMBIOSIS') {
          ctx.strokeStyle = "rgba(0, 255, 255, 0.5)"; // Cyan para Mutualismo
          ctx.lineWidth = 2;
      } else {
          ctx.strokeStyle = "rgba(255, 0, 100, 0.5)"; // Rojo Sangre para Parasitismo
          ctx.lineWidth = 1;
      }
      ctx.stroke();
    }
  }
}
