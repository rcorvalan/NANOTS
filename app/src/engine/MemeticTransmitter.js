export class MemeticTransmitter {
  /**
   * Maneja la transmisión de ideas (pesos neuronales) entre NANOTs.
   * Esto permite la evolución cultural P2P sin necesidad de reproducción estricta.
   */
  constructor(transmissionRange) {
    this.range = transmissionRange;
  }

  /**
   * Intenta transmitir un subconjunto neuronal útil de un "Maestro" a un "Estudiante"
   * @param {NeuroEvolutionNetwork} teacherBrain Cerebro del agente veterano
   * @param {NeuroEvolutionNetwork} studentBrain Cerebro del agente joven o inexperto
   */
  transmitIdea(teacherBrain, studentBrain) {
    const successRate = 0.5; // No todas las ideas son comprendidas

    if (Math.random() > successRate) return false;

    // Seleccionar una conexión sináptica activa aleatoria del maestro que no sea 0
    let maxNodes = teacherBrain.maxNodes;
    let attempts = 10;
    while(attempts > 0) {
        let from = Math.floor(Math.random() * maxNodes);
        let to = Math.floor(Math.random() * maxNodes);
        
        let teacherWeight = teacherBrain.getWeight(from, to);
        if (Math.abs(teacherWeight) > 0.1) {
            // El estudiante asimila la idea: fusiona el peso con el propio 
            // (Plasticidad memética)
            let curr = studentBrain.getWeight(from, to);
            studentBrain.setWeight(from, to, curr + (teacherWeight * 0.2)); 
            return true;
        }
        attempts--;
    }
    return false;
  }
}
