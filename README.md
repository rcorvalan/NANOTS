# 🧬 NANOT Evolution Sandbox · v9.0 — Sociedad Trascendental

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Godot Engine](https://img.shields.io/badge/Godot-4.x-478CBF?logo=godot-engine&logoColor=white)](https://godotengine.org/)
[![Language: C#](https://img.shields.io/badge/Language-C%23-239120?logo=csharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![GPU: Vulkan Compute](https://img.shields.io/badge/GPU-Vulkan%20Compute-AC3220?logo=vulkan)](https://www.vulkan.org/)

> _"La verdadera belleza algorítmica reside en observar cómo, ante un obstáculo o recompensa, la colonia se comporta como un fluido inteligente. La información fluye, demostrando que el todo es, irrevocablemente, mucho más que la suma de sus partes."_

---

## 🧠 ¿Qué es NANOT Evolution Sandbox?

**NANOT Evolution Sandbox** es una plataforma de simulación de **Vida Artificial** (A-Life) de alta fidelidad construida sobre **Godot Engine 4** con **C#**. Su núcleo simula una colonia de agentes autónomos —los **N.A.N.O.T.s** (*Nodos Autónomos de Navegación y Operación Topológica*)— cuya inteligencia colectiva no se programa, sino que **emerge** a través de:

- **Neuroevolución Continua** (arquitectura de pesos dinámicos y Crossover/Mitosis) calculada sin cuellos de botella en la **GPU** mediante shaders de cómputo Vulkan puro (GLSL).
- Un **metabolismo multi-recurso** que diferencia entre *Biomasa* y *Minerales* como vectores energéticos independientes.
- **Interacciones espaciales densas** gestionadas por un `QuadTree` nativo sobre C# capaz de alojar hasta **10.000 agentes** sin latencia de renderizado gracias al enjambre de `MultiMeshInstance2D`.
- Un **entorno topográfico activo** con mapas térmicos que modulan el comportamiento y aceleran el decaimiento.

---

## ✨ Características Principales — v9.0

### 🤖 Agente NANOT (Clase `Nanot.cs`)
Cada entidad hereda de `Node2D` de Godot e integra:
- **Cinemática física** con velocidad, aceleración e inercia.
- **Metaclase metabólica** (`MetabolicSynthesis`) con recursos diferenciados (*Biomass* y *Mineral*).
- **Senescencia**: Los agentes mueren de vejez después de `12.000 ticks` (~3.3 min), forzando rotación generacional.
- **Boids Completo**: Separación + Cohesión + Alineación entre pares de la misma especie.
- **DeceptionTrait**: Gen heredable que determina la probabilidad de mentir al transmitir señales P2P.
- **TrustLedger**: Diccionario de reputación per-agente. Los mentirosos pierden credibilidad y son ignorados.
- **CommRadius**: Radio de comunicación heredable y mutable (20-120px).

### 🧪 Metabolismo Multi-Recurso (`MetabolicSynthesis.cs`)
El metabolismo **v9.0** implementa una biología sintética completa:

| Recurso | Función |
|---------|---------|
| **Biomasa** | Combustible vital. Decae con el tiempo (+modificador ambiental de calor). Hablar por P2P cuesta energía. Llegar a 0 = muerte. |
| **Mineral** | Se gana en zonas radiactivas. Necesario para reproducción (`>30f`) y construcción estigmérgica. |
| **Reproducción sexual** | Requiere 2 padres compatibles (misma especie). Crossover genético real entre redes neuronales. |
| **Reciclaje Orgánico** | Al morir, un Nanot deja el 30% de su biomasa máxima como alimento para otros. |

### 🧬 Neuroevolución en GPU (`NeuroEvolutionNetwork.cs` + `BrainCompute.glsl`)

La toma de decisiones de cada agente está impulsada por una **red neuronal feedforward** con:
- **16 entradas** · **8 nodos ocultos** · **7 salidas**.
- **Inicialización Xavier/He** aleatoria para diversidad poblacional inicial.
- **Mutación estocástica** por gen con clamp de pesos en `[-5f, 5f]`.
- **Crossover genético** (recombinación uniforme): los hijos heredan genes aleatoriamente de padre A o padre B.
- **Aprendizaje Hebbiano (intra-vida)**: Los pesos neuronales se refuerzan cuando el agente gana biomasa (+reward) y se debilitan cuando pierde biomasa (-punishment). Cada individuo **mejora durante su vida**, no solo entre generaciones.

### 📡 Comunicación Semántica P2P

Los Nanots no emiten un simple número — transmiten **señales estructuradas** con contenido:

| Campo | Significado |
|-------|-------------|
| `SignalType` | >0.5 = "Hay comida" · <-0.5 = "Hay peligro" |
| `SignalDirX/Y` | Dirección normalizada hacia el recurso/amenaza |

**Flujo de información social:**
1. Un Nanot detecta comida cercana (<50px) → emite `COMIDA + dirección`
2. Vecinos de la misma especie reciben la señal → la filtran por `TrustLedger`
3. Si confían en el emisor → aplican fuerza hacia la dirección indicada
4. Si llegan y encuentran comida → aumentan confianza del emisor
5. Si el emisor era mentiroso (`DeceptionTrait`) → invierte la dirección y pierde reputación

**El paso de inferencia biológica procesa en la GPU vía Vulkan Compute:**
- El shader `BrainCompute.glsl` evalúa **64 agentes en paralelo** por invocación del procesador, operando asíncronamente con C#.
- `BrainComputeProvider.cs` es el puente de hiper-eficiencia de VRAM: pre-aloca gigantescos Data Buffers SSBO en frío (`StorageBufferCreate`) y sólo desliza inyecciones per-frame a través de actualizaciones dinámicas (`BufferUpdate`) limitando drásticamente los calls al Kernel de gráfico.
- Activaciones: **tanh** ultra veloz procesado en hardware nativo.

```
Input (15)  →  [ReLU]  →  Hidden (8)  →  [tanh]  →  Output (5)
          GPU Vulkan Compute · 64 agentes / workgroup
```

### 🔗 Biología Intercelular (`CellularLink.cs`)
Los NANOTs pueden establecer **vínculos biológicos** persistentes con otros individuos:

| Tipo de Vínculo | Efecto |
|----------------|--------|
| `SYMBIOSIS` | Equilibra Biomasa y Mineral entre ambos agentes. Aplica una leve fuerza de atracción mutua. |
| `PARASITE` | (Implementación pendiente) Un agente drena recursos del otro. |
| `COMMUNICATION` | Canal de señalización directa entre dos nodos de la red mesh. |

### 🔗 Sociedad Trascendental (Redstone & P2P)
**V8.2 introduce las mecánicas fundacionales de las macro-sociedades artificiales:**
- **Mesh Lingüística (P2P):** Los agentes transcriben `RadioFrequencies` dinámicas que actúan como idiomas. Los nodos interceptan las señales (`CurrentBroadcastSignal`) de sus pares circundantes, validando inputs sólo si la diferencia tonal es < 10% (Tolerancia de especie).
- **Estigmergia Computacional:** Los Nanots pueden excretar polímeros estáticos sobre el grid del mapa creando circuitos artificiales (`StigmergicGrid.cs`).
- **Energización Natural (Redstone):** Los bloques estigmérgicos operan como Autómatas Celulares. Si un líder Nanot vocaliza con fuerza sobre un cable inerte, lo *"energiza"*, permitiendo transportar chispa azulada de un rincón del mapa al otro de forma ultrarrápida para computar lógica física o defenderse de depredadores.

### 🗺️ Entorno Topográfico Activo (`TopographyGrid.cs`)
El mundo no es un lienzo inerte. Un **grid topográfico procedural** genera:
- **HeatMap**: Anillos de temperatura que irradian desde el centro. El calor modifica el **coeficiente de decaimiento metabólico** via `Decay(baseDrain, environmentalHeat)`.
- **RadiationMap**: Zonas de radiación puntual que penalizan o potencian características específicas del agente.
- **Visualizador dinámico**: Renderizado en tiempo real mediante `ImageTexture` de Godot (rojo = calor, verde = radiación).

---

## 🏗️ Arquitectura del Proyecto

```
Nanots/
├── GodotEngine/                  # Motor principal (Godot 4 + C#)
│   ├── project.godot
│   ├── Main.tscn
│   └── src/
│       ├── Main.cs               # Orquestador principal de la simulación
│       ├── Nanot.cs              # Definición del agente autónomo
│       ├── biology/
│       │   ├── MetabolicSynthesis.cs   # Sistema metabólico multi-recurso
│       │   └── CellularLink.cs         # Vínculos inter-agente (simbiosis, parasitismo)
│       ├── engine/
│       │   ├── NeuroEvolutionNetwork.cs  # Genoma neuronal + crossover/mutación
│       │   ├── BrainComputeProvider.cs   # Puente optimizado C#-Vulkan SSBO Update
│       │   └── BrainCompute.glsl         # Raw Vulkan Compute de inferencia paralela
│       └── environment/
│           ├── TopographyGrid.cs         # Mapa procedural de Calor radiativo
│           ├── QuadTree.cs               # Particionador espacial O(n log n)
│           └── GlobalEnvironment.cs      # Parámetros del cosmos topográfico
```

---

## 💻 Instalación y Ejecución

### Requisitos
- [Godot Engine 4.x](https://godotengine.org/download/) con soporte **.NET / C#**
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- GPU con soporte **Vulkan** (requerido para el pipeline de cómputo GPU)

### Pasos

1. **Clona el repositorio:**
   ```bash
   git clone https://github.com/TU-USUARIO/nanot-evolution-sandbox.git
   cd nanot-evolution-sandbox/GodotEngine
   ```

2. **Abre el proyecto en Godot:**
   - Lanza Godot Engine 4.
   - Selecciona `Import → Buscar project.godot` dentro de `GodotEngine/`.

3. **Importa y restaura dependencias C#:**
   ```bash
   dotnet restore "Nanot Sandbox.csproj"
   ```

4. **Ejecuta la simulación:**
   - Presiona **F5** en Godot o el botón ▶ `Play`.

> **Nota GPU**: El sistema detecta `RenderingServer.CreateLocalRenderingDevice()` automáticamente. Si tu GPU no soporta Vulkan Compute, el shader GLSL no compilará. Verifica que tus drivers gráficos están actualizados.

---

## 🛡️ Protocolos de Contención — Sandbox Seguro

El diseño garantiza un entorno computacionalmente seguro y estable:

| Protocolo | Implementación |
|-----------|---------------|
| **Tope Poblacional (Hard Cap)** | Controlado desde `Main.cs`. Previene explosiones de memoria RAM. |
| **Liberación Explícita de GPU** | `BrainComputeProvider` llama a `rd.FreeRid()` en cada buffer tras el readback. Sin leaks de VRAM. |
| **Limpieza Automática de Agentes** | `Nanot.Die()` utiliza purga paramétrica y `QueueFree()`, mientras el gestor vacía proactivamente del `Pop` arrays subyacentes. |
| **Acotamiento Genético (Clamping)** | Pesos en `[-5f, 5f]`, biases en `[-2f, 2f]`. Las mutaciones no pueden escalar al infinito. |
| **Topografía Finita** | El `TopographyGrid` tiene dimensiones fijas. Los agentes rebotan en los bordes (`CheckEdges`). |
| **NaN Sanitization** | `ApplyForce()` en `Nanot.cs` detecta y neutraliza vectores `NaN` antes de que corrupcionen la simulación. |

---

## 🗺️ Hoja de Ruta

- [x] Sistema de reputación (Trust Ledger) y engaño estratégico (DeceptionTrait heredable)
- [x] Construcción de Grid Estigmérgico para Autómata Celular ("Redstone Orgánico")
- [x] Especiación por divergencia de frecuencia de radio (facciones visuales cromáticas)
- [x] Panel de inspección individual (Microscopio Neural) con vectores de decisión
- [x] Exportación de métricas a CSV (población, facciones, engaño, biomasa)
- [x] Botón Catástrofe (Extinción del 50% aleatoria)
- [x] Reproducción sexual (2 padres + crossover genético real)
- [x] Senescencia (muerte por vejez, rotación generacional)
- [x] Boids completo (Separación + Cohesión + Alineación)
- [x] Costo energético de comunicación P2P
- [x] Cámara libre con Zoom + arrastre
- [ ] Escenarios predefinidos: Laberinto de Teseo
- [ ] Federación Cross-Universe (migración inter-IP)

---

## 🤝 Contribuciones

¿Te interesa la neuroevolución, la vida artificial o la computación GPU paralela? Las sugerencias, *Issues* y *Pull Requests* son bienvenidos.

Por favor, abre un *Issue* antes de implementar cambios estructurales grandes para alinear el diseño con la visión del proyecto.

---

## 📄 Licencia

Distribuido bajo la licencia **MIT**. Consulta el archivo `LICENSE` para más detalles.
