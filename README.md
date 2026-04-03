# 🧬 NANOT Evolution Sandbox · v9.6 (Estabilización Complex-Systems)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Godot Engine](https://img.shields.io/badge/Godot-4.x-478CBF?logo=godot-engine&logoColor=white)](https://godotengine.org/)
[![Language: C#](https://img.shields.io/badge/Language-C%23-239120?logo=csharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![GPU: Vulkan Compute](https://img.shields.io/badge/GPU-Vulkan%20Compute-AC3220?logo=vulkan)](https://www.vulkan.org/)

> **Proyecto Universitario / De Investigación**  
> *"La verdadera belleza algorítmica reside en observar cómo, ante un obstáculo o recompensa, la colonia se comporta como un fluido inteligente. La información fluye mucho más rápido que la materia misma."*

---

## 🧠 ¿Qué es NANOT Evolution Sandbox?

**NANOT Evolution Sandbox** es una plataforma de simulación de **Vida Artificial** (A-Life) de alta fidelidad basada en **Godot Engine 4** con scripting paralelo en **C# y Vulkan**. 
Simula una colonia masiva de agentes autónomos (*Los N.A.N.O.T.s*) sin comportamientos macro programados; su inteligencia de enjambre y evolución de supervivencia **emerge** naturalmente de química metabólica, cálculos neuronales individuales, y las implacables presiones topográficas.

Esta versión **v9.6** incluye estabilización exhaustiva a largo plazo (resolviendo atascos crónicos en tráfico y bordes limitantes) e implementa mecánicas biológicas y cognitivas extremadamente profundas:

1. **Biología Pura, Metabolismo y Reproducción:** Una ingesta balanceada (3 unidades máximas) evita un colapso en la colmena, permitiendo a los nanots invertir su vitalidad en esparcir señales. Mueren obligadamente de senescencia, ceden cadáveres como recursos reciclables y experimentan un Crossover genético real durante la reproducción.
2. **Sistema P2P y Dinámica Social (Engaño / Empatía):** Comunicación direccional semántica atada a una Frecuencia de Radio tipo Gen. Incluye *Trust Ledgers* locales para castigar la reputación de agentes egoístas (Mentirosos) detectados y un costo físico real al transmitir Gritos a la radio, forzando evolución comunicativa óptima frente a estática generalizada de superpoblación.
3. **Enlaces Celulares Sensoriales (`CellularLinks`):** Mecánicas de adhesión inter-física entre dos sujetos. Fricción Parasitaria (un agente sufre un anclaje forzoso y una sangría de Stats continuos de la que debe huir rápidamente) frente a la hermosa Simbiosis (dos sujetos promedian matemáticamente toda su matriz de recursos de manera perenne, estabilizando sus colores por completo mientras se halan colaborativamente logrando un pseudo multicelularismo físico).
4. **Instinto y Redes Boids Defensivas:** Reconocimiento activo del entorno. Reacciones directas como "Shield Wall", activadas por la percepción de feromonas u obstrucciones donde el miedo grupal a un depredador sobreescribe la propia neuroevolución propiciando cohesión total escapatoria de manera milagrosa.
5. **Entorno Acelerado y UI Modular Profesional:** Panel HUD desacoplado interactivo mediante sistema Drag and Drop de interfaces para control fino científico (Reset instantáneo, Spawn masivo), un estricto log de centralización `/Metricas` donde el habla (lenguaje P2P) e integraciones `lang.csv` caen registradas limpiamente cada 30 segundos mitigando latencia y proveyendo oro puro estadístico a la plataforma de Tesis de Grado.

---

## 🗂️ Documentación Teórica y Arquitectura

Toda la fundamentación conceptual (reglas boids, protocolo mesh, modelos inter-celulares multi-recurso e inferencia neuronal Vulkan SSBO) que rige al agente se ha consolidado en un solo cuerpo central maestro:

👉 **[Consulta la Documentación Maestra Evolutiva Aquí](Documentacion_Maestra_Evolutiva.md)**

---

## 💻 Instalación y Ejecución

### Requisitos Técnicos
- [Godot Engine 4.x](https://godotengine.org/download/) con soporte **.NET / C#** (Obligatorio para la compilación del backend matemático).
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- GPU con firmware Vulkan funcional para soportar el módulo Paralelo Cerebro (`rd.CreateLocalRenderingDevice()`).

### Despliegue Rápido
1. **Clona o descarga el Proyecto:**
   ```bash
   git clone https://github.com/TU-USUARIO/nanot-evolution-sandbox.git
   cd nanot-evolution-sandbox/GodotEngine
   ```
2. **Ejecución vía Engine:**
   - Abre `GodotEngine/project.godot`.
   - Fíjate en los plugins C# de Godot actualizando dependencias (o empuja manualmente con `dotnet restore "Nanot Sandbox.csproj"` en terminal).
   - Presiona `[+ Play]` (F5) o Exporta el proyecto según la Wiki oficial.
3. **Ejecución de Métrica y Análisis offline:** Los datasets resultantes en formato procesable quedan unificadamente expuestos en `/Metricas/` o exportados activando los hooks directos localizados en la interfaz UI.

---

## 🏗️ Arquitectura Básica C#

Un esquema mínimo conceptual de orquestación por inyección Quadtree dentro del bucle de Juego:

```text
GodotEngine/src/
 ├── Main.cs                      (Dios / Dashboard UI Mágnetico / Métricas FileSystem)
 ├── Nanot.cs                     (Estructura atómica individual: Antenas, Sensores, Física Base)
 ├── biology/
 │   ├── MetabolicSynthesis.cs    (Gestión de vida: Biomass / Minerals / Crossover)
 │   └── CellularLink.cs          (Puntos de Simbiosis y Data-links intercelulares Parasitarios)
 ├── engine/
 │   ├── NeuroEvolutionNetwork.cs (Aprendizaje Hebbiano y Mutación intra-vida genética)
 │   └── BrainComputeProvider.cs  (VRAM SSBO Bridge C# -> Render Device Compute Pipeline)
 └── environment/                 (Terreno procedimental de Teseo y Grid Topográfico reactivo)
```

---

## 🛡️ Sandbox Seguro
La simulación aplica reglas inquebrantables de *air-gapped software protection*. Contiene un límite estricto poblacional (*P-Caps*) impidiendo crasheos del ecosistema por multiplicaciones bacterianas que llenen la memoria de PageFaults; aplicando simultáneamente sanitizaciones asépticas constantes para rechazar que cálculos flotantes inválidos (**Not-a-Number**) se suban a internet de las Mallas Vecinales físicas corrompiendo la matriz y la simulacion determinista de Dios.

---

## 📜 Licencia y Autoría

Distribuido bajo la Licencia **MIT**. Para información abierta consulta el archivo `LICENSE`.
Las sugerencias para avanzar nuevas físicas ecosistémicas de Inteligencia Colectiva Universitaria son siempre bienvenidas.
