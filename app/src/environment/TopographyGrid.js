export class TopographyGrid {
  /**
   * Representa el mapa subyacente interactuable (El terreno de la placa de Petri)
   */
  constructor(width, height, cellSize = 20) {
    this.width = width;
    this.height = height;
    this.cellSize = cellSize;
    
    this.cols = Math.ceil(width / cellSize);
    this.rows = Math.ceil(height / cellSize);
    
    // Arrays lineales para alto rendimiento
    this.radiationMap = new Float32Array(this.cols * this.rows); 
    this.heatMap = new Float32Array(this.cols * this.rows); // -1.0 (frío absoluto) a 1.0 (calor extremo)

    this.generateTerrain();
  }

  generateTerrain() {
    // Generación procedimental simplificada de Biomas
    for (let y = 0; y < this.rows; y++) {
      for (let x = 0; x < this.cols; x++) {
        let idx = x + y * this.cols;
        
        // Zonas de Radiación Anulares estocásticas
        if (Math.random() < 0.05) {
            this.radiationMap[idx] = Math.random() * 0.8 + 0.2; 
        }

        // Variaciones de temperatura (Ejemplo: Frío extremo al borde, calor en el centro)
        let dx = (x / this.cols) - 0.5;
        let dy = (y / this.rows) - 0.5;
        let distFromCenter = Math.sqrt(dx*dx + dy*dy);
        
        // Cerca del centro = 1.0 Calor. Bordes = -1.0 Frío
        this.heatMap[idx] = 1.0 - (distFromCenter * 3.0); 
        this.heatMap[idx] = Math.max(-1.0, Math.min(1.0, this.heatMap[idx]));
      }
    }
  }

  getPropsAt(x, y) {
    let col = Math.floor(x / this.cellSize);
    let row = Math.floor(y / this.cellSize);
    
    // Bounds check
    if (col < 0 || col >= this.cols || row < 0 || row >= this.rows) {
        return { radiation: 0, heat: 0 };
    }
    
    let idx = col + row * this.cols;
    return {
        radiation: this.radiationMap[idx],
        heat: this.heatMap[idx]
    };
  }

  draw(ctx) {
    // Dibujo del terreno topográfico (Visualización Debug o UI Map)
    for (let y = 0; y < this.rows; y++) {
      for (let x = 0; x < this.cols; x++) {
         let idx = x + y * this.cols;
         let rad = this.radiationMap[idx];
         let heat = this.heatMap[idx]; // -1 to 1

         if (rad > 0.1 || heat !== 0) {
            let cx = x * this.cellSize;
            let cy = y * this.cellSize;
            
            ctx.fillStyle = `rgba(${heat > 0 ? 255 : 0}, 0, ${heat < 0 ? 255 : 0}, ${Math.abs(heat) * 0.1})`;
            if (rad > 0.2) {
               ctx.fillStyle = `rgba(0, 255, 0, ${rad * 0.2})`; // Verde radiactivo
            }
            
            ctx.fillRect(cx, cy, this.cellSize, this.cellSize);
         }
      }
    }
  }
}
