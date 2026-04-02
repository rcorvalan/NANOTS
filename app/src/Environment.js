import { TopographyGrid } from './environment/TopographyGrid.js';
import { StigmergyStructures } from './environment/StigmergyStructures.js';
import { CellularLink } from './biology/CellularLink.js';

export class Environment {
  constructor(width, height) {
    this.width = width;
    this.height = height;
    
    // Nodos de biomasa o mineral en el piso
    this.resources = [];
    
    // v6: Subsistemas del entorno
    this.topography = new TopographyGrid(width, height);
    this.stigmergy = new StigmergyStructures();
    this.cellularLinks = new CellularLink();
  }

  spawnResource() {
    if (this.resources.length < 150) {
      // 50% chance de Biomasa o Mineral
      let isBiomass = Math.random() > 0.5;
      this.resources.push({
        x: Math.random() * (this.width - 40) + 20,
        y: Math.random() * (this.height - 40) + 20,
        amount: 50 + Math.random() * 100,
        radius: 4 + Math.random() * 6,
        type: isBiomass ? 'BIOMASS' : 'MINERAL'
      });
    }
  }

  dropBiomass(x, y, amount) {
    this.resources.push({
      x: x,
      y: y,
      amount: amount,
      radius: 6,
      type: 'BIOMASS'
    });
  }

  update(nanots, maxSpeed) {
    // Generación procedimental de recursos
    if (Math.random() < 0.3) {
      this.spawnResource();
    }

    // Gestionar Enlaces Celulares
    this.cellularLinks.update();

    // Comprobar recolección de recursos
    for (let i = this.resources.length - 1; i >= 0; i--) {
      let res = this.resources[i];
      let eaters = [];

      for (let n of nanots) {
        if (n.dead) continue;
        let dx = n.position.x - res.x;
        let dy = n.position.y - res.y;
        let distSq = dx * dx + dy * dy;

        if (distSq < (res.radius + n.size) * (res.radius + n.size) + 100) {
            n.metabolism.ingest(res.type, 5); // NANOT absorbe el tipo específico
            eaters.push(n);
        }
      }

      // Tragedia de los comunes y deterioro
      if (eaters.length > 5) {
        res.amount -= eaters.length * 2;
      } else if (eaters.length > 0) {
        res.amount -= eaters.length;
      }

      if (res.amount <= 0) {
        this.resources.splice(i, 1);
      }
    }
  }

  draw(ctx) {
    // 1. Dibujar Topografía (Fondo)
    this.topography.draw(ctx);
    
    // 2. Dibujar Estigmergia (Estructuras de jugadores)
    this.stigmergy.draw(ctx);
    
    // 3. Enlaces Celulares
    this.cellularLinks.draw(ctx);

    // 4. Recursos Naturales
    for (let res of this.resources) {
      ctx.beginPath();
      ctx.arc(res.x, res.y, res.radius, 0, Math.PI * 2);
      if (res.type === 'BIOMASS') {
        ctx.fillStyle = `rgba(167, 139, 250, 0.8)`; 
        ctx.strokeStyle = '#8b5cf6';
      } else {
        ctx.fillStyle = `rgba(56, 189, 248, 0.8)`; // Cyan para Mineral
        ctx.strokeStyle = '#0284c7';
      }
      ctx.fill();
      ctx.stroke();
      ctx.closePath();
    }
  }
}
