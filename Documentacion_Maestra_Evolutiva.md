# 🧬 Documentación Maestra Evolutiva: NANOT Sandbox v9.5

> Este documento condensa y reemplaza la documentación clásica del proyecto (Documento Maestro, Manual Técnico, Estructura de Prompt y Prompt Arquitectura v5.0). Sirve como la **única fuente de la verdad** teórica, técnica y arquitectónica del Sistema de Experimentación en Inteligencia de Colmena, evolucionando junto con el simulador, reflejando actualmente los avances de la **v9.5** (Empatía Neuro-Cognitiva, UI Interactiva, Federación Cross-Universe UDP y centralización estandarizada de métricas).

---

## 1. Visión General y Filosofía del Proyecto

El **Proyecto NANOT Evolution Sandbox** es una plataforma de simulación interactiva diseñada para explorar la **Inteligencia de Colmena (Swarm Intelligence)**, la **Biología Sintética**, la **Teoría de Juegos** y los **Sistemas Distribuidos**.

A diferencia de los modelos de IA centralizados, la premisa aquí es que el comportamiento complejo, resiliente e inteligente emerge de **reglas simples, ejecutadas por individuos con percepción local y sin un control central**. Al integrar mecánicas de enjambre (Boids), redes dinámicas (Mesh), un metabolismo bio-sintético y neuroevolución acelerada por GPU, el ecosistema se convierte en un laboratorio para estudiar el altruismo algorítmico, límites P2P, y la evolución darwiniana generada por la presión del entorno.

---

## 2. Arquitectura del Agente: El N.A.N.O.T.

El N.A.N.O.T. (*Nodo Autónomo de Navegación y Operación Topológica*) es la unidad atómica de simulación, dotada de procesamiento local multivariable.

### 2.1. Estado Físico y Cinemática
- **Posición y Velocidad:** Limitado estrictamente a la física 2D; sujeto a inercia.
- **Colisiones Físicas:** Las "construcciones" del enjambre actúan como barreras físicas intransitables para los agentes.
- **Frecuencia de Radio (Color/Aspecto):** Determina su especie o facción. Falso reconocimiento si difiere en +/- 10% del emisor.
- **Radios y Sensores:**  
  - *Percepción Visual (`Visual Decision Vectors`):* Alcance estricto para sortear obstáculos inmediatos.  
  - *Comunicación (`CommRadius`):* Mutable y heredable genéticamente, de 20 a 120px.  

### 2.2. Metabolismo Sintético (Dinámica Multi-Recurso)
Cada agente posee una instancia metabólica exigente:
- **Biomasa:** Energía consumible. Consumir 3 unidades (células amarillas) repone la biomasa al máximo temporalmente limitando el tiempo gastado comiendo. Decae perpetuamente a un ritmo basal aumentado por el esfuerzo de moverse rápido o emitir comunicaciones P2P ("el coste biológico de hablar") y modificado por el Calor del entorno (Mapa Topográfico).
- **Mineral:** Obtenido en zonas irradiadas. Indispensable para habilitar reproducción (mediante controles dinámicos de fertilidad) o construir enjambres/redstone estigmérgico.
- **Senescencia:** Muerte forzosa por vejez a los 12.000 *ticks* para asegurar la rotación de generaciones y purgar ineficiencia.
- **Reciclaje Orgánico (Cadáveres):** Al perecer, un 30% de la biomasa máxima se derrama como recurso local, alimentando al ecosistema.

### 2.3. Estado Cognitivo y Genoma (`NeuroEvolutionNetwork`)
Impulsado masivamente en paralelo vía **Vulkan Compute**:
- **Red Neuronal:** Feedforward con 16 entradas → 8 nodos ocultos → 7 salidas.
- **Memoria y Aprendizaje Hebbiano:** Las señales recibidas que desencadenan alimentación exitosa refuerzan los "pesos" (-5f a +5f). Respuestas engañosas sufren "extinción". Un agente se adapta sinápticamente *durante* su propia vida.
- **Reproducción Sexual y Crossover:** Cruce genético estocástico con alineamiento Uniforme de redes neuronales, generando progenie adaptada que toma genes de padre A y madre B.  

### 2.4. Atributos Sociales y Teoría de Juegos
- **Empatía Neuro-Cognitiva:** Mecánicas pro-sociales enfocadas en trabajo en equipo y señalización activa de socorro (distress signaling).
- **Gen del Engaño (`DeceptionTrait`):** Determina estadísticamente qué propensión tiene un NANOT de mentir (ej. avisar "peligro" en vez de "comida" para ser egoísta y no compartir botín).
- **Libro de Confianza (`TrustLedger`):** Cada individuo asocia el Hex-ID de sus pares con un puntaje de reputación. Mentirosos recurrentes son penalizados e irreversiblemente silenciados localmente.

---

## 3. Protocolos de Comunicación P2P (Red Mesh Semántica)

La colmena no usa sistemas mágicos o globables; todo ocurre enviando paquetes a través de la interfaz de radio simulada.

- **Fricción y Latencia:** Hay riesgo probabilístico de pérdida de paquetes y la saturación de ruido empeora si hay un "cuello de botella de población" (Demasiados nanots en un solo radio superponen señales).
- **Semántica Direccional:**
  - `SignalType`: Valores positivos atraen hacia RECURSO, negativos alejan del PELIGRO.
  - `Time-To-Live (TTL)`: Evita bucles infinitos (Broadcast storms) durante puentes comunicativos de múltiples saltos.
- **Aislamiento Social (Especiación):** Cuando dos tribus divergen genéticamente y sus Frecuencias de Radio de mutación no se alinean, se ignoran como "estática", causando estricta ruptura de la reproducción compartida y originando dos especies endémicas.

---

## 4. Estigmergia, Acoplamiento y Ecosistema Físico

- **Grid Topográfico (TopographyGrid):** Capas superpuestas estocásticas procesadas matricialmente que impactan al individuo. Zonas candentes aceleran el metabolismo; zonas radiactivas fomentan hiper-mutación pero erosionan células.
- **Conectividad Tisular (`CellularLink`):** Agentes pueden acoplarse permanentemente:
  - *Simbiosis:* Intercambio equivalente y promediado de Biomasa y Minerales.
  - *Parasitismo:* Drenaje unidireccional.
  - *Data-Link:* Interfaz rígida para cruzar lagunas de comunicación.
- **Estigmergia Computacional:** Los NANOTs excretan nodos estáticos que (al margen de comportarse como barreras físicas) pueden energizarse con gritos, actuando como Autómatas Celulares subyacentes, formando "cables" capaces de llevar poder de red mucho más lejos que una simple antena de señal.
- **Federación Cross-Universe (UDP):** Permite la comunicación inter-simulador emparejando entornos independientes para lograr macro-simulaciones y eventos intergalácticos.

---

## 5. Implementación y Estructura Lógica

El ecosistema está construido en **Godot 4 con C#**, separando el procesamiento por cuellos de botella CPU/VRAM.

### 5.1. El Eje Físico-Cognitivo
La actualización simultánea de 10.000 agentes es imposible sin `QuadTrees`:
1. El Entorno usa un QuadTree indexando al vuelo la posición de la ronda.
2. Cada agente lanza query a este QuadTree descubriendo a sus vecinos.
3. Se aplican Reglas Analógicas (Separación O(log N), Alineación, Cohesión).
4. El paquete P2P de mensajes se entrega de la antena A a la B.
5. El bucle cerebral se delega al `BrainComputeProvider.cs`. Usando arrays paralelos SSBO de Vulkan GLSL se procesan capas neuronales densas (múltiplos de 64 hilos); acelerando astronómicamente Inferencia, Retropropagación Hebbiana y Decadencia Metabólica.

### 5.2. Casos de Intervención y Usabilidad (Dashboard UI)
El usuario (El 'Entorno') altera reglas vía un Dashboard UI flexible, provisto de **menús y paneles de información arrastrables y colapsables (Draggable / Collapsible)**:
- Intervenciones Gravitacionales directas (cambiar pesos de Boids).
- Alterar Tasas Regenerativas de Recursos (Provocar crisis de escasez y especiación por hambre).
- Control Dinámico de Fertilidad Reproductiva.
- **El Laberinto de Teseo:** Generador de pasillos topológicos procedimentales donde los exploradores mueren hasta dejar feromonas visuales para el resto del rebaño.
- **Botón Catástrofe (Extinción):** Purga súbitamente al 50% para rastrear tiempos métricos de reestabilización de red comunitaria.

### 5.3. Sistema Centralizado de Métricas (`/Metricas/`)
La simulación persigue un estándar riguroso de observabilidad y registro persistente expuesto a archivos en ruta unificada:
- Exportación automática a `nanot_metrics.csv` y sus variantes de estampa de tiempo para auditoria genética poblacional en `/Metricas/`.
- Registro paralelo de comunicaciones lingüísticas de colmena (últimos 30 segundos) analizando `lang.csv` (Tipo de mensaje, dirección, radio y % engaño).

### 5.4. Pipeline Seguro 
Garantizar contención local del "Air Gapped A-Life":
- Hard Caps de población inamovibles (evita caída de Windows por PageFault).
- Sanitización absoluta contra floats `NaN` de matemáticas de C# en comportamientos virales en enjambre.

---

## 6. Flujo Lógico Conceptual (Ciclo de Vida)

```python
# Pseudocódigo Core - Bucle de Motor principal
def cycle(swarm, grid, active_modules):
    for agent in swarm:
        # 1. Filtro Social y Comunicación
        messages = filter_by_species(agent.antenna.receive(), agent.radio_freq)
        messages = trust_ledger.validate(messages)
        
        # 2. Computo Paralelo GPU y Toma de Decision
        # Ocurre remotamente delegándolo al VulkanCompute(Brain_glsl)
        response_vector = inference(agent, environment_data, messages)
        
        # 3. Respuesta Mecánica y Consumo
        physics.move(agent, boids_forces + response_vector)
        agent.biomass -= metabolic_cost(agent.speed, grid.heat(agent.pos))
        
        # 4. Extracción de Recursos y Honestidad (Teoría de Juegos)
        if detect_food(agent.pos):
            agent.eat(AMOUNT)
            if rand() < agent.deception_trait:
                broadcast("DANGER", false_coord) # Engaño
            else:
                broadcast("FOOD", real_coord) # Altruismo
                
        # 5. Senescencia y Muerte
        if agent.biomass <= 0 or agent.age > MAX_AGE:
            grid.spawn_biomass_corpse(agent.pos)
            agent.die()
```

Esta consolidación elimina ambigüedades arquitectónicas, integrando el objetivo inicial de robótica de enjambres con la fase actual neuro-evolutiva avanzada de Godot.
