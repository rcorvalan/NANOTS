import { Nanot } from './Nanot.js';
import { Environment } from './Environment.js';
import { QuadTree, Rectangle } from './QuadTree.js';
import { Predator } from './Predator.js';
import { Vector2 } from './Vector2.js';
import { BrainCompute } from './engine/BrainCompute.js';
import { MemeticTransmitter } from './engine/MemeticTransmitter.js';
import { MetabolicSynthesis } from './biology/MetabolicSynthesis.js';
import './style.css';

const canvas = document.getElementById('sandboxCanvas');
const ctx = canvas.getContext('2d');

let width, height;
let nanots = [];
let predators = [];
let env;
let brainCompute;
let memeticTransmitter;
let isKilled = false;
let inspectedNanot = null;

// Telemetry state
let simulationCycles = 0;
let csvData = [["Ciclo", "Poblacion", "Predadores", "Biomasa", "Mineral"]];

// UI
const initialPopSlider = document.getElementById('initialPop');
const initialPopVal = document.getElementById('initialPopVal');
const popCount = document.getElementById('popCount');
const fpsCount = document.getElementById('fpsCount');
const avgHealthEl = document.getElementById('avgHealth');
const predCountEl = document.getElementById('predCount');
const resourceCountEl = document.getElementById('resourceCount');
const logContent = document.getElementById('logContent');

const resetBtn = document.getElementById('resetBtn');
const killSwitchBtn = document.getElementById('killSwitchBtn');
const killMsg = document.getElementById('killMsg');
const exportCsvBtn = document.getElementById('exportCsvBtn');

const HARD_CAP = 1000;

window.logEvent = function(msg, colorClass = "") {
  let entry = document.createElement('div');
  entry.className = `log-entry ${colorClass}`;
  entry.textContent = `[Sistema] ${msg}`;
  logContent.appendChild(entry);
  if(logContent.childNodes.length > 50) logContent.removeChild(logContent.firstChild);
  logContent.scrollTop = logContent.scrollHeight;
};

function resize() {
  width = canvas.width = window.innerWidth;
  height = canvas.height = window.innerHeight;
  if (env) {
    env.width = width;
    env.height = height;
  }
}
window.addEventListener('resize', resize);

async function init() {
  resize();
  nanots = [];
  predators = [];
  simulationCycles = 0;
  logContent.innerHTML = "";
  window.logEvent("Generando Entorno Topográfico V6...", "blue");
  
  env = new Environment(width, height);
  memeticTransmitter = new MemeticTransmitter(60);

  // Inicializar Motor WebGPU
  brainCompute = new BrainCompute(HARD_CAP);
  try {
      await brainCompute.init();
      window.logEvent("NEAT WebGPU Pipeline Listo.", "green");
  } catch (e) {
      window.logEvent("ERROR GPU: " + e.message, "red");
      return;
  }

  const initialPop = parseInt(initialPopSlider.value);
  for (let i = 0; i < initialPop; i++) {
    createNanot(Math.random() * width, Math.random() * height);
  }
  
  // Enviar configuración estática inicial a GPU
  brainCompute.updateWeightsAndBiases();

  isKilled = false;
  killMsg.style.display = 'none';
  killSwitchBtn.textContent = "Kill Switch (Stop)";
  killSwitchBtn.classList.add('danger');

  requestAnimationFrame(loop);
}

function createNanot(x, y, parentBrain = null) {
    if (nanots.length >= HARD_CAP) return null;
    let n = new Nanot(x, y);
    // Metabólicamente los nuevos adaptan v6 puro
    n.metabolism = new MetabolicSynthesis(100, 0); 
    
    // Necesitamos buscar un poolIndex libre (simplificado: nanots.length si no reciclamos arrays, 
    // pero para WebGPU estricto buscamos index vacante)
    let poolIndex = nanots.length; // TODO: Implementar un FreeList para índices reutilizables
    
    if(poolIndex < HARD_CAP) {
        n.assignBrain(brainCompute, poolIndex, parentBrain);
        nanots.push(n);
        return n;
    }
    return null;
}


// Listeners UI
initialPopSlider.addEventListener('input', (e) => initialPopVal.textContent = e.target.value);
resetBtn.addEventListener('click', () => { isKilled = true; setTimeout(init, 100); });
killSwitchBtn.addEventListener('click', () => {
  isKilled = !isKilled;
  killMsg.style.display = isKilled ? 'block' : 'none';
  killSwitchBtn.textContent = isKilled ? "Resucitar Motor" : "Kill Switch (Stop)";
  if(isKilled) killSwitchBtn.classList.remove('danger');
  else { killSwitchBtn.classList.add('danger'); requestAnimationFrame(loop); }
});

exportCsvBtn.addEventListener('click', () => {
   let csvContent = "data:text/csv;charset=utf-8," + csvData.map(e => e.join(",")).join("\n");
   let encodedUri = encodeURI(csvContent);
   let link = document.createElement("a");
   link.setAttribute("href", encodedUri);
   link.setAttribute("download", "nanot_v6_telemetry.csv");
   document.body.appendChild(link);
   link.click();
   document.body.removeChild(link);
});

let lastFpsTime = 0;
let frames = 0;

async function loop(timestamp) {
  if (isKilled) return;

  frames++;
  if (timestamp - lastFpsTime >= 1000) {
    fpsCount.textContent = frames;
    frames = 0;
    lastFpsTime = timestamp;
  }

  simulationCycles++;
  ctx.clearRect(0, 0, width, height);

  // 1. Inputs a GPU
  let boundary = new Rectangle(width / 2, height / 2, width / 2, height / 2);
  let qtree = new QuadTree(boundary, 4);

  for (let n of nanots) {
    if (!n.dead) qtree.insert({ position: n.position, userData: n });
    
    // Metabolia
    let props = env.topography.getPropsAt(n.position.x, n.position.y);
    n.metabolism.decay(0.05, props.heat);
    
    // Radiación causa mutaciones aceleradas
    if (props.radiation > 0.5 && Math.random() < 0.01 && n.brain) {
        n.brain.mutate();
        brainCompute.updateWeightsAndBiases(); // Sincronizar GPU
        if(Math.random()<0.05) window.logEvent(`Mutación radioactiva en Agente ${n.id}`, "yellow");
    }

    if (n.metabolism.isDead() && !n.dead) {
       n.dead = true;
       env.dropBiomass(n.position.x, n.position.y, 40);
    }
    
    // Preparar Entradas Sensoriales para el webgpu compute pass
    n.prepareInputs(env, null); 

    if (isNaN(n.position.x) || isNaN(n.metabolism.biomass)) {
        window.logEvent(`DEBUG ERROR: Nanot ${n.id} IS NaN BEFORE GPU. pos.x: ${n.position.x}, biomass: ${n.metabolism.biomass}, heat: ${props.heat}`);
    }
  }

  // 2. Ejecutar Compute Shader en la RTX 5060 de forma asíncrona pero bloqueante para este frame
  await brainCompute.evaluate();

  // 3. Procesar Outputs y Físicas
  let aliveCount = 0;
  let totalHealth = 0;
  let newBorns = [];

  for (let n of nanots) {
      if (n.dead) continue;
      aliveCount++;
      totalHealth += n.metabolism.biomass;

      // Obtiene la fuerza deseada por el cerebro NEAT
      n.applyBrainOutputs(brainCompute);

      // Colisiones estigmérgicas (Los muros repelen)
      env.stigmergy.resolveCollisions(n);

      // Desplazamiento
      n.update(env, {});
      
      // Dibujamos luego del entorno en el pipeline principal

      if (isNaN(n.position.x)) {
          let outs = brainCompute.getOutputs(n.poolIndex);
          window.logEvent(`DEBUG ERROR: Nanot ${n.id} IS NaN AFTER UPDATE. moveX GPU Output: ${outs.moveX}, moveY: ${outs.moveY}`);
      }

      // Reproducción (si su metabolismo lo permite)
      if (n.metabolism.canReproduce() && (aliveCount + newBorns.length) < HARD_CAP) {
          n.metabolism.consumeForReproduction();
          let child = createNanot(n.position.x, n.position.y, n.brain);
          if (child) newBorns.push(child);
      }

      // Evolución Cultural (Memética)
      if (n.wantsToCommunicate) {
          ctx.beginPath();
          ctx.arc(n.position.x, n.position.y, memeticTransmitter.range, 0, Math.PI*2);
          ctx.fillStyle = "rgba(100, 200, 255, 0.1)";
          ctx.fill();

          let range = new Rectangle(n.position.x, n.position.y, memeticTransmitter.range, memeticTransmitter.range);
          let students = qtree.query(range).map(p => p.userData);
          for (let st of students) {
              if (st !== n && st.brain && !st.dead) {
                  if (memeticTransmitter.transmitIdea(n.brain, st.brain)) {
                      brainCompute.updateWeightsAndBiases(); // Sincroniza nueva sinapsis cultural en GPU
                      break; // Educa a 1 por frame
                  }
              }
          }
      }

      // Construir Muros Estigmérgicos (Estocástico si tiene recursos)
      if (n.metabolism.mineral >= 50 && Math.random() < 0.05) {
          if (n.metabolism.consumeForStigmergy(50)) {
              env.stigmergy.buildWall(n.position.x, n.position.y, 50);
              window.logEvent(`Agente ${n.id} construyó Estigmergia (Muro)`, "green");
          }
      }
  }

  // 4. Update Entorno (Físico y Visual Fondo)
  env.update(nanots, 2);
  env.draw(ctx);

  // 5. Draw Nanots (Al Frente para que no sean tapados por el terrain grid)
  for (let n of nanots) {
      if (!n.dead) n.draw(ctx);
  }

  // 6. Limpieza y Métricas
  popCount.textContent = aliveCount;
  avgHealthEl.textContent = aliveCount > 0 ? (totalHealth / aliveCount).toFixed(1) : 0;
  resourceCountEl.textContent = env.resources.length;

  if (Math.random() < 0.05) brainCompute.updateWeightsAndBiases(); // Flush slow syncs

  requestAnimationFrame(loop);
}

// Iniciar aplicación
document.getElementById('resetBtn').textContent = "Iniciar V6 (GPU)";
