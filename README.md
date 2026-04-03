# 🧬 NANOT Evolution Sandbox · v9.5

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Godot Engine](https://img.shields.io/badge/Godot-4.x-478CBF?logo=godot-engine&logoColor=white)](https://godotengine.org/)
[![Language: C#](https://img.shields.io/badge/Language-C%23-239120?logo=csharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![GPU: Vulkan Compute](https://img.shields.io/badge/GPU-Vulkan%20Compute-AC3220?logo=vulkan)](https://www.vulkan.org/)

> **Proyecto Universitario / De Investigación**  
> *"La verdadera belleza algorítmica reside en observar cómo, ante un obstáculo o recompensa, la colonia se comporta como un fluido inteligente. La información fluye mucho más rápido que la materia misma."*

---

## 🧠 ¿Qué es NANOT Evolution Sandbox?

**NANOT Evolution Sandbox** es una plataforma de simulación de **Vida Artificial** (A-Life) de alta fidelidad alojada en **Godot Engine 4** con **C#**. 
Simula una colonia de agentes autónomos (*Los N.A.N.O.T.s*) que no tienen un programa prefijado o determinista: su inteligencia colectiva **emerge puramente** de leyes físicas básicas, química neuronal acelerada por GPU y presiones brutales del entorno en el que nacen.

- **Sociedad y Teoría de Juegos:** Sistemas de reputación donde los mentirosos son penalizados. Soporta análisis exhaustivo de evolución lingüística y comportamientos avanzados de empatía (*distress signaling*).
- **Biología Pura:** Senescencia, reproducción sexual con crossover genético guiado, "enlaces celulares parasitarios", estigmergias físicas sólidas y recolección optimizada.
- **Rendimiento Exquisito:** QuadTrees en C# y Shaders GLSL de Vulkan soportando colonias masivas tomando decisiones Hebbianas asincrónicamente a más de 60fps constantes.
- **Avances v9.5:** Arquitectura expandida con paneles interactivos arrastrables (Draggable UI), generador topológico "Laberinto de Teseo", sistemas de federación cruzada UDP, y estandarización centralizada de logs semánticos y biométricos (`Metricas/lang.csv`).

---

## 🗂️ Documentación Teórica y Arquitectura

Toda la fundamentación conceptual (reglas boids, protocolo mesh, modelos multi-recurso, estigmergia algorítmica) que antes se dividía en múltiples archivos ha sido pacificada y consolidada en un solo cuerpo central evolutivo:

👉 **[Consulta la Documentación Maestra Evolutiva Aquí](Documentacion_Maestra_Evolutiva.md)**

---

## 💻 Instalación y Ejecución

### Requisitos Técnicos
- [Godot Engine 4.x](https://godotengine.org/download/) con soporte **.NET / C#** (Opcional, pero vital si decides inspeccionar código base o compilar de fuente).
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
   - Fíjate en los plugins C# de Godot actualizando dependencias (o empuja manualmente con `dotnet restore "Nanot Sandbox.csproj"`).
   - Presiona `[+ Play]` (F5).
3. **Ejecución Compilado:** Utiliza el script `Run_Nanot_Sandbox.bat` presente en la raíz si lo tienes exportado en un entorno Windows nativo y deseas evitar lanzar el Editor entero.

---

## 🏗️ Arquitectura Básica C#

```text
GodotEngine/src/
 ├── Main.cs                      (Dios / Control Maestro de Escena y Caps)
 ├── Nanot.cs                     (Estructura atómica: Antenas, Sensores, Boids forces)
 ├── biology/
 │   ├── MetabolicSynthesis.cs    (Gestión estricta de Biomasa vs Minerales)
 │   └── CellularLink.cs          (Puntos de Simbiosis y Data-links intercelulares)
 ├── engine/
 │   ├── NeuroEvolutionNetwork.cs (Mutaciones, cruce genético, arrays de pesos)
 │   └── BrainComputeProvider.cs  (VRAM SSBO Bridge de C# -> Núcleo Vulkan)
 └── environment/                 (Terreno, Grid topográfico y métricas CSV Export)
```

## 🛡️ Sandbox Seguro
La simulación aplica reglas de air-gapped protection de software: un KillSwitch nativo, contención estricta contra variables Not-a-Number de matemáticas de enjambre y límite estricto poblacional preventivo impidiendo bucles infinitos en Memoria RAM de huésped al simular reproducción exponencial de A-Life.

---

## 📜 Licencia y Autoría

Distribuido bajo la Licencia **MIT**. Para información abierta consulta el archivo `LICENSE`.
Las sugerencias para avanzar nuevas físicas ecosistémicas (fricción termodinámica, federaciones cruzadas, depredación IA) son siempre agradecidas mediante PR/Issues.
