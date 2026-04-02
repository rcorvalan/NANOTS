export class StigmergyStructures {
  /**
   * Administra construcciones y rastros que alteran permanentemente o semi-permanentemente el entorno.
   */
  constructor() {
    this.walls = [];
    this.pheromones = []; // Opcionales para rastros
  }

  // Los NANOTs depositan pedazos de muro ("Ladrillos de mineral") para construir fortalezas
  buildWall(x, y, mineralCost) {
    this.walls.push({
      x: x,
      y: y,
      hp: mineralCost * 2, // Resistencia basada en la cantidad invertida
      radius: 10
    });
  }

  // Verifica si un agente está colisionando con una macro-estructura y lo repele
  resolveCollisions(agent) {
    for (let i = this.walls.length - 1; i >= 0; i--) {
      let wall = this.walls[i];
      
      let dx = agent.position.x - wall.x;
      let dy = agent.position.y - wall.y;
      let dist = Math.sqrt(dx*dx + dy*dy);
      
      if (dist < wall.radius + agent.size) {
          // Prevenir División por Cero que inyecta NaN permanentemente a los agentes
          if (dist === 0) {
              dx = Math.random() - 0.5;
              dy = Math.random() - 0.5;
              dist = Math.sqrt(dx*dx + dy*dy);
          }
          
          // Normalizar el vector
          let nx = dx / dist;
          let ny = dy / dist;

          // Repulsión puramente física (Fuerza inquebrantable)
          let pushBack = (wall.radius + agent.size) - dist;
          agent.position.x += nx * pushBack;
          agent.position.y += ny * pushBack;

          // Si el agente es un depredador tratando de entrar, daña el muro
          if (agent.isPredator) {
              wall.hp -= 5;
              if (wall.hp <= 0) {
                  this.walls.splice(i, 1);
              }
          }
      }
    }
  }

  draw(ctx) {
    for (let wall of this.walls) {
      ctx.beginPath();
      ctx.arc(wall.x, wall.y, wall.radius, 0, Math.PI * 2);
      ctx.fillStyle = `rgba(150, 150, 150, ${wall.hp / 100})`; // Más daño = más transparente
      ctx.fill();
      ctx.strokeStyle = "white";
      ctx.lineWidth = 1;
      ctx.stroke();
    }
  }
}
