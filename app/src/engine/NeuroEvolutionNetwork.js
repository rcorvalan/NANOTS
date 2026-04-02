export class NeuroEvolutionNetwork {
  constructor(brainCompute, agentId, maxNodes = 16) {
    this.gpu = brainCompute;
    this.id = agentId;
    this.maxNodes = maxNodes;
    
    // We point to a section in the GPU buffers
    this.offsetW = this.id * this.maxNodes * this.maxNodes;
    this.offsetB = this.id * this.maxNodes;
    
    this.weightRefs = this.gpu.weightArray;
    this.biasRefs = this.gpu.biasArray;
  }

  // Inicializa con genoma aleatorio básico (directo Inputs a Outputs)
  initializeBasic() {
    this.clear();
    const inputNodes = [0,1,2,3,4]; // Energy, Dist Food, Dir Food, Dist Danger, Dir Danger
    const outputNodes = [13,14,15]; // MoveX, MoveY, Broadcast

    // Random biases
    for(let i=0; i<this.maxNodes; i++) {
        this.biasRefs[this.offsetB + i] = (Math.random() * 2 - 1) * 0.1;
    }

    // Connect randomly
    for (let inNode of inputNodes) {
      for (let outNode of outputNodes) {
        if (Math.random() > 0.5) continue;
        let weight = (Math.random() * 2 - 1) * 2.0;
        this.setWeight(inNode, outNode, weight);
      }
    }
  }

  clear() {
    for(let i=0; i<this.maxNodes * this.maxNodes; i++) this.weightRefs[this.offsetW + i] = 0;
    for(let i=0; i<this.maxNodes; i++) this.biasRefs[this.offsetB + i] = 0;
  }

  setWeight(from, to, value) {
    this.weightRefs[this.offsetW + (from * this.maxNodes + to)] = value;
  }

  getWeight(from, to) {
    return this.weightRefs[this.offsetW + (from * this.maxNodes + to)];
  }

  // Mutación Genética (NEAT topology alter)
  mutate() {
    let rate = 0.1; // 10% chance
    
    // 1. Mutate existing weights
    for(let i=0; i<this.maxNodes * this.maxNodes; i++) {
        if (this.weightRefs[this.offsetW + i] !== 0 && Math.random() < rate) {
            this.weightRefs[this.offsetW + i] += (Math.random() * 2 - 1) * 0.5;
        }
    }

    // 2. Add connection mutation
    if (Math.random() < 0.05) {
        let n1 = Math.floor(Math.random() * this.maxNodes); // From anywhere
        let n2 = Math.floor(Math.random() * (this.maxNodes - 5)) + 5; // To anything but inputs
        
        if (this.getWeight(n1, n2) === 0) {
            this.setWeight(n1, n2, (Math.random() * 2 - 1));
        }
    }
  }

  // Crossover con otro padre
  crossover(partnerBrain) {
    // Generar el hijo con un ID temporario, main.js asignará el correcto mediante assignBrain
    let child = new NeuroEvolutionNetwork(this.gpu, this.id, this.maxNodes);
    child.clear();

    for(let i=0; i<this.maxNodes * this.maxNodes; i++) {
        // Hereda 50/50 
        child.weightRefs[child.offsetW + i] = (Math.random() > 0.5) 
            ? this.weightRefs[this.offsetW + i] 
            : partnerBrain.weightRefs[partnerBrain.offsetW + i];
    }
    for(let i=0; i<this.maxNodes; i++) {
        child.biasRefs[child.offsetB + i] = (Math.random() > 0.5)
            ? this.biasRefs[this.offsetB + i]
            : partnerBrain.biasRefs[partnerBrain.offsetB + i];
    }
    return child;
  }
}
