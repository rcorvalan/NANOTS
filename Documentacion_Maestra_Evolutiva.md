# 🧬 Documentación Maestra Evolutiva: NANOT Sandbox v9.6 (Estabilización y Sistemas Complejos)

> Este documento actúa como la **única fuente de la verdad** teórica, técnica y arquitectónica del Sistema de Experimentación en Inteligencia de Colmena, evolucionando junto con el simulador. Esta iteración (v9.6) refleja las integraciones masivas de estabilización metabólica, defensa colectiva antidepredador, enlaces biológicos profundos y arquitectura UI robusta.

---

## 1. Visión General y Filosofía del Proyecto

El **Proyecto NANOT Evolution Sandbox** es una plataforma de simulación interactiva diseñada para explorar la **Inteligencia de Colmena (Swarm Intelligence)**, la **Biología Sintética**, la **Teoría de Juegos** y los **Sistemas Distribuidos**.

A diferencia de los modelos de IA centralizados, la premisa aquí es que el comportamiento complejo y resiliente emerge de **reglas simples, ejecutadas por individuos con percepción local y sin un control central**. El simulador integra fuerzas de enjambre (Boids), redes dinámicas (Mesh), metabolismo bio-sintético y neuroevolución acelerada por GPU, creando un laboratorio paramétrico para estudiar evolución darwiniana, altruismo algorítmico, y dinámicas de sistemas complejos.

---

## 2. Arquitectura del Agente: El N.A.N.O.T.

El N.A.N.O.T. (*Nodo Autónomo de Navegación y Operación Topológica*) es la unidad atómica de simulación. Carece de visión omnisciente y debe valerse de percepciones limitadas para sobrevivir.

### 2.1. Estado Físico y Cinemática
- **Posición, Velocidad y Atracción Boids:** Sujetos a física 2D estricta (Cohesión, Alineación, Separación).
- **Manejo de Tráfico y Navegación Dinámica:** Las construcciones y barreras dejaron de ser muros impenetrables que colapsan el tráfico; ahora aplican **mecánicas de fricción (slow-down)**. Esto permite sortear mapas sin atascos masivos.
- **Prevención de Clustering en Bordes:** El motor físico cuenta con mecanismos de repulsión de bordes y control de steering que evita que las poblaciones queden atrapadas en las esquinas de manera permanente, forzando una distribución espacial útil.
- **Frecuencia de Radio (Color/Facción):** Determina su especie. Falso reconocimiento o incompatibilidad de cruce genético si difiere del estándar. Diferencias genéticas originan especiación (aislamiento comunicativo).

### 2.2. Metabolismo Sintético Rebalanceado
El metabolismo rige la vida y motivación de los agentes:
- **Biomasa (Energía Cero = Muerte):** Consumible basal. Para la v9.6, el consumo está rebalanceado: bastan **3 células amarillas** para rellenar al máximo la biomasa de un nanot, evitando un colapso sistémico. Esto libera su tiempo computacional de la persecución de comida para dedicarlo a mecánicas socio-cognitivas.
- **Costo Metabólico de Transmisión:** Cada Grito/Broadcast por radio descuenta reservas, obligando a priorizar comunicación útil sobre el ruido.
- **Minerales:** Obtenidos de la tierra, requeridos para la reproducción o edificaciones.
- **Senescencia:** Muerte ineludible por límite de ticks, para fomentar la evolución generacional. Otorga reciclaje orgánico pasivo del cadáver al suelo.

### 2.3. Supervivencia y Protocolos Anti-Depredador
- **Conciencia del Rastro:** Los nanots son capaces de detectar estigmergia cruzada, incluyendo el rastro de Depredadores en el ecosistema.
- **Defensa Cooperativa ("Shield Wall"):** Cuando se detecta inminente peligro o un depredador, los instintos de los nanots **sobreescriben temporalmente** las redes neuronales estándar, activando un comportamiento de extrema cohesión ("Formación Escudo" o *Shield Wall*) y vuelo guiado de la manada entera para priorizar la supervivencia comunitaria.

### 2.4. Estado Cognitivo, Neuro-Evolución neuronal y Socialización
El cerebro opera en **Vulkan Compute (SSBO)**:
- **Red Neuronal y Aprendizaje Hebbiano:** Arquitectura *Feedforward* que recompensa sinápticamente decisiones en vida (recompensa intra-vida) alterando pesos cuando se logra ingesta biológica a pesar del caos. 
- **Conformación Genética y Reproducción:** Mecánicas de cruce genético (crossover algorítmico) para heredar variables como **Mutabilidad de Radio (CommRadius)** y predisposiciones al Engaño.
- **El Gen del Engaño (*DeceptionTrait*) y Reputación (*Trust Ledger*):** Cada agente califica matemáticamente a sus pares. Emitir llamadas de "Comida" intencionalmente falsas fomenta una actitud egoísta (guardar recursos) pero hace que las entidades mientan con el tiempo. El perdón existe a tasas basales lentas (*Forgiveness Rate*).

---

## 3. Vínculos Biológicos, Estigmergia y Entorno

- **Grid Estigmérgico Topográfico:** El motor computa mapas espaciales donde feromonas decaen y el terreno influye en la velocidad/calor metabólico del agente de manera pasiva.
- **Enlaces Celulares Interactivos (`CellularLink.cs`):** 
  En ocasiones, colisiones a bajas velocidades producen mutaciones biológicas intercelulares uniendo dos nanots visual y físicamente mediante una línea:
  1. **Simbiosis (Línea de color / Cyan):** Promedio de Stats Permanente. Ambos agentes amalgaman matemáticamente su Biomasa y Minerales cuadro a cuadro. Esto les confiere un brillo y color idéntico sincronizado de forma perpetua. Ambos se halan pasivamente mediante fuerza motriz ayudándose en trayecto.
  2. **Parasitismo (Línea Roja):** Vínculo unilateral forzado. Un agente (Parásito) succiona pasivamente biomasa mientras ancla al Hueped (Host). Genera una física de persecución: el parásito tira hacia el huésped para no soltarse, mientras que el huésped experimenta inercia opuesta huyendo hasta que la distancia exceda la ruptura elástica o alguien muera.
- **Federación Cross-Universe (UDP):** Transferencia intergaláctica de cerebros en binario a otros motores remotos.
- **Laberinto de Teseo:** Generación procedimental de dificultades topológicas con barreras "slow-down", testando el potencial de marcaje estigmérgico para que las inteligencias encuentren la salida.

---

## 4. UI Interactiva, Dashboard y Datos Centralizados

La usabilidad de la herramienta virtual para el experimentador humano ha sido estandarizada en su formato de Escritorio:
- **UI Profesional:** Menús modulares (Control General, Inspector de Entidad, Leyenda del Ecosistema) que son **arrastrables** y **colapsables**. Incorporan controles de tamaño mínimo (*CustomMinimumSize*) y anclas robustas para evitar redimensiones compulsivas o bugs visuales o recálculos (*jittering*) durante simulaciones frenéticas de más de 60fps.
- **Intervenciones Administrativas:** Capacidad nativa en la UI para aplicar "Reinicio Maestro" del Ecosistema en un clic de forma limpia, alterar variables biológicas, o invocar extinciones cataclísmicas controladas.
- **Centralización Científica (\Metricas):** Todos los archivos de salida caen estandarizados dentro del directorio de trabajo de métricas. 
  - `nanot_metrics_{fecha}.csv` registra demografía poblacional estática.
  - `lang.csv` provee un búfer histórico perpetuo de los últimos 30 segundos (en logs en vivo de Godot y disco) conteniendo ID agente, facción, intenciones de engaño, porcentaje transmitido direccional y fuerza de transmisión, sentando bases documentales directas para tesis investigativas de lingüística generada procedimentalmente.

---

## 5. Implementación y Tecnologías Fundacionales

La eficiencia no sería posible sin un enfoque riguroso de algoritmos paralelos en memoria y hardware:
- Eje Físico atado a un árbol espacial O(log n) continuo `QuadTree` de reconstrucción frame a frame, lo que habilita la gestión de más de 3.000+ uniones de mensajería (mallas mesh) en simultáneo.
- Las Rutinas Cerebrales actúan como un proveedor *Compute Shader* despachando sub-cargas al DispatchGroup general de Vulkan `[Brain_glsl]`, trayendo arrays estáticos reconstruyéndolos sin golpear el Heap nativo.
- Prevención de desastres con una purga de datos rigurosa evitando que cálculos nan (*Not a Number*) se viralicen en inercia boids destruyendo la malla de nodos.
