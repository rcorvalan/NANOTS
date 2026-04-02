import { Vector2 } from './Vector2.js';
import { NeuroEvolutionNetwork } from './engine/NeuroEvolutionNetwork.js';

export class Nanot {
  constructor(x, y, maxSpeed = 2, maxForce = 0.05, parentOpts = null) {
    this.id = Math.random().toString(36).substr(2, 9);
    this.position = new Vector2(x, y);
    this.velocity = new Vector2(Math.random() * 2 - 1, Math.random() * 2 - 1).setMag(Math.random() * 2 + 1);
    this.acceleration = new Vector2(0, 0);
    this.maxSpeed = maxSpeed;
    this.maxForce = maxForce;
    
    // Sensores Base
    this.perceptionRadius = parentOpts ? parentOpts.perceptionRadius : 50; 
    
    // v5: Genética Avanzada
    this.radioFreq = parentOpts ? parentOpts.radioFreq : Math.random(); // 0.0 a 1.0
    this.deceptionTrait = parentOpts ? parentOpts.deceptionTrait : Math.random() * 0.5; // Mutación lenta
    
    // v5: Cognición y Sociedad
    this.trustLedger = new Map(); // id -> trust_score (0 a 1)

    // Metabolismo
    this.energy = parentOpts ? 50 : 100;
    this.age = 0;
    this.maxAge = 3000 + Math.random() * 2000;
    
    this.size = 3;
    this.dead = false;
    this.color = this.calculateColor(false);

    // NEAT Brain (Asignado dinámicamente por el Environment)
    this.poolIndex = null;
    this.brain = null;
  }

  // Se llama cuando el objeto recicla un slot en la memoria GPU
  assignBrain(brainCompute, poolIndex, parentBrain = null) {
    this.poolIndex = poolIndex;
    this.brain = new NeuroEvolutionNetwork(brainCompute, poolIndex);
    if (parentBrain) {
      // Si nace de un padre, hereda o hace crossover (simplificado copiando e imprimiendo mutación)
      // Nota: Idealmente cruzaríamos con otro, acá hacemos copia + mutacion
      this.brain = parentBrain.crossover(parentBrain);
      this.brain.id = poolIndex;
      this.brain.offsetW = poolIndex * this.brain.maxNodes * this.brain.maxNodes;
      this.brain.offsetB = poolIndex * this.brain.maxNodes;
      this.brain.mutate();
    } else {
      this.brain.initializeBasic();
    }
  }

  // Prepara los inputs vectoriales para enviarlos a la GPU
  prepareInputs(env, boids) {
    if (!this.brain) return;

    // Detectar recurso más cercano (Mock para simplificar)
    let distFood = 1.0; 
    let dirFood = 0.0;

    // Detectar peligro (Mock)
    let distDanger = 1.0;
    let dirDanger = 0.0;

    // Input 0: Energía (Normalizada -1 a 1)
    let eNorm = this.metabolism ? ((this.metabolism.biomass / 200) * 2 - 1) : ((this.energy / 100) * 2 - 1);

    let inputs = [eNorm, distFood, dirFood, distDanger, dirDanger];
    this.brain.gpu.setInputs(this.poolIndex, inputs);
  }

  applyBrainOutputs(brainCompute) {
    if (!this.brain) return;
    let outs = brainCompute.getOutputs(this.poolIndex);
    
    // Antidoto Anti-NaN estricto
    let mx = isNaN(outs.moveX) ? 0 : outs.moveX;
    let my = isNaN(outs.moveY) ? 0 : outs.moveY;
    
    let moveForce = new Vector2(mx, my);
    moveForce.limit(this.maxForce);
    this.applyForce(moveForce);

    // outs.comm puede usarse para activar broadcast Memético
    if (outs.comm > 0.5) {
      this.wantsToCommunicate = true;
    } else {
      this.wantsToCommunicate = false;
    }
  }

  calculateColor(speciationOn) {
    let currentEnergy = this.metabolism ? this.metabolism.biomass : this.energy;

    if (speciationOn) {
      // Color ligado a gen de radio (Diversidad)
      const hue = Math.floor(this.radioFreq * 360);
      const alpha = Math.max(0.2, currentEnergy / 200);
      return `hsla(${hue}, 100%, 50%, ${alpha})`;
    } else {
      // Verde=Lleno, Rojo=Critico
      // Escala visual rápida
      const percent = Math.max(0, Math.min(100, currentEnergy)) / 100;
      const r = Math.round(255 * (1 - percent));
      const g = Math.round(255 * percent);
      return `rgb(${r}, ${g}, 0)`;
    }
  }

  applyForce(force) {
    this.acceleration.add(force);
  }

  // Filtrado P2P por Frecuencia y Confianza
  filterPeers(boids, toggles) {
    let validPeers = [];
    for (let other of boids) {
      if (other === this) continue;
      
      // Especiación: Si difieren mucho en radio, se ignoran (incomunicados)
      if (toggles.speciation && Math.abs(this.radioFreq - other.radioFreq) > 0.15) {
        continue;
      }

      // Teoría de Juegos: Si la confianza es menor a 0.2, bloquear
      if (toggles.gameTheory) {
        let trust = this.trustLedger.get(other.id);
        if (trust !== undefined && trust <= 0.2) {
          continue; // Bloqueado
        }
      }

      validPeers.push(other);
    }
    return validPeers;
  }

  // El sistema Boids ha sido removido a favor de "applyBrainOutputs" que procesa fuerzas calculadas por la Topología Evolutiva en el GPU

  update(env, toggles) {
    if (this.dead) return;
    
    this.velocity.add(this.acceleration);
    this.velocity.limit(this.maxSpeed);
    this.position.add(this.velocity);
    this.acceleration.mult(0); // Reset

    this.checkEdges(env.width, env.height);

    this.age++;

    // En V6 (Metabolism), isDead se evalua desde main.js, eliminamos lógica antigua de energía aquí.
    if ((!this.metabolism && this.energy <= 0) || this.age > this.maxAge) {
      if (!this.dead) {
        let reason = this.age > this.maxAge ? "Vejez" : "Inanición";
        if (Math.random() < 0.05) window.logEvent(`N.A.N.O.T. ha muerto por ${reason}.`, reason === "Vejez" ? "yellow" : "red");
      }
      this.dead = true;
    }
    
    let isSpeciation = toggles ? toggles.speciation : false;
    this.color = this.calculateColor(isSpeciation);
  }

  checkEdges(width, height) {
    if (this.position.x < 0) { this.position.x = 0; this.velocity.x *= -1; }
    if (this.position.x > width) { this.position.x = width; this.velocity.x *= -1; }
    if (this.position.y < 0) { this.position.y = 0; this.velocity.y *= -1; }
    if (this.position.y > height) { this.position.y = height; this.velocity.y *= -1; }
  }

  draw(ctx) {
    if (this.dead) return;
    let theta = Math.atan2(this.velocity.y, this.velocity.x) + Math.PI / 2;
    ctx.save();
    ctx.translate(this.position.x, this.position.y);
    ctx.rotate(theta);
    ctx.beginPath();
    ctx.moveTo(0, -this.size * 2);
    ctx.lineTo(-this.size, this.size * 2);
    ctx.lineTo(this.size, this.size * 2);
    ctx.closePath();
    ctx.fillStyle = this.color;
    ctx.fill();
    ctx.restore();
  }

  clone() {
    // Mutaciones estocásticas clamped
    let newPerception = Math.max(30, Math.min(100, this.perceptionRadius + (Math.random() * 10 - 5)));
    let newRadio = Math.max(0, Math.min(1, this.radioFreq + (Math.random() * 0.06 - 0.03)));
    let newDeception = Math.max(0, Math.min(1, this.deceptionTrait + (Math.random() * 0.1 - 0.05)));

    let childEnvProps = {
      perceptionRadius: newPerception,
      radioFreq: newRadio,
      deceptionTrait: newDeception
    };

    let child = new Nanot(this.position.x, this.position.y, this.maxSpeed, this.maxForce, childEnvProps);
    this.energy -= 20; 
    
    // Sólo lo intermitentemente logea
    if (Math.random() < 0.05) window.logEvent(`Reproducción: Nueva instancia (Gen Mentira: ${(child.deceptionTrait*100).toFixed(0)}%)`, "green");
    return child;
  }
}
