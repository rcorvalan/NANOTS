# 🧬 NANOT Evolution Sandbox

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Vite](https://img.shields.io/badge/Vite-B73BFE?style=flat&logo=vite&logoColor=FFD62E)](https://vitejs.dev/)

El **Proyecto NANOT Evolution Sandbox** es una plataforma de simulación interactiva web diseñada para explorar la intersección entre la **Inteligencia de Colmena (Swarm Intelligence)**, la **Biología Sintética** y los **Sistemas Distribuidos**.

En lugar de programar una inteligencia artificial centralizada, este ecosistema digital permite que la inteligencia **emerja** a través de reglas locales simples ejecutadas por cientos de agentes autónomos, inspirándose en las colonias de insectos y bancos de peces.

## 🚀 Características Principales

- 🤖 **Agentes Autónomos (N.A.N.O.T.s):** Cada entidad (*Nodo Autónomo de Navegación y Operación Topológica*) opera con percepción local, sin un "cerebro" centralizado.
- 📡 **Red Mesh Dinámica P2P:** Los agentes se comunican mediante una topología paramétrica, emitiendo *heartbeats* y alertas informativas limitadas por tiempo de vida (TTL) para evitar tormentas de broadcast.
- 🌀 **Comportamiento Colectivo (Boids Avanzado):** Navegación biomimética basada en separación, alineación, cohesión y atracción comunicada hacia fuentes de energía.
- ⚡ **Metabolismo Finito:** Sistema riguroso de energía con consumo basal, desgaste por acciones intensas y senescencia metabólica.
- 🧬 **Evolución Estocástica:** Mecánicas reproductivas con herencia paramétrica. Sobreviven los individuos más adaptados a las condiciones de abundancia o escasez del entorno dinámico.
- 🕵️ **Herramientas de Investigador:** Interfaz para manipular la matriz de comportamientos ("God Mode"), induciendo estrés físico, introduciendo depredadores naturales o eventos de extinción repentinos.

## 🛠️ Patrones de Diseño & Arquitectura

* **Motor:** Simulación construida en Vanilla JavaScript utilizando Canvas API para alto rendimiento gráfico.
* **Empaquetado:** Motorizado a través de Vite.
* **Optimización Matemática:** Utiliza algoritmos como *Quadtrees* o *Spatial Hashing* (Partición Espacial) para mitigar el costo computacional a magnitudes `O(n log n)`, permitiendo +500 agentes colisionando a 60 FPS sostenidos.

## 💻 Instalación y Ejecución

**Requisitos Previos:** Asegúrate de tener instalado [Node.js](https://nodejs.org/) (versión 18 o superior).

1. **Clona el repositorio:**
   ```bash
   git clone https://github.com/TU-USUARIO/nanot-evolution-sandbox.git
   ```

2. **Entra al directorio de la aplicación web:**
   ```bash
   cd nanot-evolution-sandbox/app
   ```

3. **Instala las dependencias y corre el servidor en modo desarrollo:**
   ```bash
   npm install
   npm run dev
   ```

4. Abre tu navegador en la dirección indicada en la terminal (usualmente `http://localhost:5173/`).

## 🛡️ Protocolos de Contención (Sandbox Seguro)

Este proyecto incorpora *Kill Switches* garantizando un uso computacionalmente seguro:
- **Tope Poblacional Rígido (Hard Cap):** Prevención nativa de derbordamientos de memoria al fijar máximos de población.
- **Confinamiento (Air Gapped):** Los NANOTs y la red Mesh están estrictamente emulados dentro del Sandbox del navegador.
- **Acotamiento Genético (Clamping):** Las mutaciones no pueden generar energía de la nada o adquirir habilidades ilógicas, respetando la física local diseñada.

## 🤝 Contribuciones

Si te interesa el comportamiento emergente complejo, la neuroevolución topológica u optimizaciones de robótica de enjambre, ¡las sugerencias y *Pull Requests* son bienvenidos! Si encuentras algún comportamiento anómalo o quieres implementar nuevas métricas, por favor abre un nuevo *Issue*.

## 📄 Licencia

Este proyecto se distribuye bajo la licencia **MIT**. Consulta el archivo `LICENSE` para más detalles.

---

> _"La verdadera belleza algorítmica reside en observar cómo, ante un obstáculo o recompensa, la colonia se comporta como un líquido dinámico. La información fluye, demostrando que el todo es, irrevocablemente, mucho más que la simple suma de sus partes matemáticas."_
