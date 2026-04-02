# **Prompt de Desarrollo: NANOT Evolution Sandbox v5.0 (Ultimate A-Life Architecture)**

**Objetivo Central:** Desarrollar un entorno de simulación modular en 2D (Sugerido: Python/Pygame o C/Raylib) que emule la evolución de agentes autónomos (NANOTs). El sistema debe integrar mecánicas de enjambre (Boids), metabolismo biológico, aprendizaje asociativo y dinámicas sociales complejas (Teoría de Juegos y Especiación).

### **1\. Arquitectura del Agente (Clase NANOT Ampliada)**

Cada entidad contiene un estado multidimensional:

* **Estado Físico:** Posición, Velocidad, Aceleración, Frecuencia de Radio (Color de Facción).  
* **Estado Metabólico:** Energía (0-100), Tasa de Decaimiento, Edad.  
* **Estado Cognitivo (Memoria):**  
  * SpatialHeatmap: Rejilla de memoria de rutas (Éxito vs. Trauma).  
  * ConditioningTable: Pesos sinápticos de estímulo-respuesta.  
* **Estado Social:**  
  * TrustLedger: Diccionario de IDs de otros NANOTs mapeados a un "Puntaje de Confianza" (0.0 a 1.0).  
  * DeceptionTrait: Gen que determina la probabilidad (0-100%) de emitir una señal falsa cuando encuentra un recurso.

### **2\. Módulos de Simulación (Toggles de Interfaz)**

El motor principal debe operar mediante el patrón *Strategy/Feature Toggle*, permitiendo al usuario encender/apagar dinámicamente:

#### **A. Bio-Metabolismo & Reciclaje**

* Consumo basal de energía. Muerte al llegar a 0\.  
* **Reciclaje Orgánico:** Al morir, un NANOT deja un 30% de su energía residual como un "Nodo de Biomasa" que otros pueden consumir.

#### **B. Cognición y Aprendizaje**

* Aprendizaje Hebbiano simplificado: Refuerzo positivo al encontrar comida tras una señal; Refuerzo negativo (Extinción) si la señal era falsa.

#### **C. Teoría de Juegos (Engaño y Reputación)**

* **El Engaño (Fake Broadcast):** Un NANOT egoísta encuentra comida, pero transmite a la red mesh: *"¡Depredador en mis coordenadas\!"* para alejar a la competencia.  
* **La Reputación:** Si el NANOT "B" viaja a las coordenadas dadas por el NANOT "A" y no encuentra lo prometido, reduce el puntaje de confianza del ID "A" en su TrustLedger. Si el puntaje cae por debajo de un umbral, "B" silencia permanentemente las transmisiones de "A".

#### **D. Especiación y Divergencia**

* Al reproducirse, la "Frecuencia de Radio" muta ligeramente. Si la diferencia de frecuencia entre dos NANOTs supera un ![][image1] máximo, sus mensajes se consideran "Ruido Ininteligible" y no pueden reproducirse entre sí (creación de especies/facciones por color).

#### **E. Depredación Dinámica**

* Agentes cazadores que se sienten atraídos por el "ruido de radio" (sectores con mucha comunicación P2P).

### **3\. Sistema de Inspección Profunda (UI/UX)**

Al pausar y hacer clic en un NANOT, se despliega el **Panel Analítico**:

1. **Red de Confianza (Social Graph):** Nodos conectados por líneas verdes (Aliados de alta confianza) y líneas rojas cortadas (Mentirosos silenciados).  
2. **Mapa Mental (Heatmap):** Visualización de la memoria espacial (Verde \= Zonas seguras, Rojo \= Zonas de peligro).  
3. **Árbol Genético:** Indicador de su Facción (Frecuencia) y porcentaje de tendencia al engaño.  
4. **Vectores de Decisión:** En el mapa principal, flechas superpuestas mostrando la suma de fuerzas: Mecánica (Boids) \+ Cognitiva (Memoria) \+ Social (Seguir al líder de confianza).

### **4\. Flujo Lógico Conceptual (Pseudocódigo Core)**

FUNCION actualizar\_nanot(agente, entorno, modulos\_activos):  
    // 1\. Procesar Red Mesh y Reputación  
    SI modulos\_activos.teoria\_juegos:  
        mensajes\_validos \= FILTRAR\_POR\_CONFIANZA(agente.buzon, agente.trust\_ledger)  
        mensajes\_entendibles \= FILTRAR\_POR\_ESPECIE(mensajes\_validos, agente.frecuencia)  
    SINO:  
        mensajes\_entendibles \= agente.buzon

    // 2\. Procesamiento Cognitivo  
    SI modulos\_activos.aprendizaje:  
        accion\_planeada \= CONSULTAR\_MEMORIA\_Y\_CONDICIONAMIENTO(mensajes\_entendibles)  
    SINO:  
        accion\_planeada \= CALCULAR\_BOIDS\_MECANICO(agente, entorno)

    // 3\. Ejecución y Consumo  
    APLICAR\_MOVIMIENTO(agente, accion\_planeada)  
    agente.energia \-= CALCULAR\_COSTO\_METABOLICO(agente.velocidad)

    // 4\. Interacción con el Entorno (Engaño)  
    SI DETECTA\_RECURSO(agente, entorno):  
        agente.energia \+= INGESTA  
        SI modulos\_activos.teoria\_juegos Y (RANDOM() \< agente.deception\_trait):  
            BROADCAST(tipo="PELIGRO", coords=agente.posicion) // Miente  
        SINO:  
            BROADCAST(tipo="COMIDA", coords=agente.posicion)  // Dice la verdad  
              
    // 5\. Ciclo de Vida  
    SI agente.energia \<= 0:   
        GENERAR\_BIOMASA(agente.posicion, agente.energia\_max \* 0.3)  
        ELIMINAR(agente)

### **5\. Restricciones de Implementación**

* **Rendimiento:** Obligatorio implementar *Spatial Partitioning* (Quadtree o Hash Grid) para las físicas, y una matriz de adyacencia optimizada para el sistema de confianza.  
* **Gráficos:** El color del NANOT debe mapearse a su Frecuencia de Radio (Especiación). Su opacidad (Alpha) debe representar su Energía actual.  
* **Datos Abiertos:** El sistema debe exportar un archivo CSV continuo con métricas de población, cantidad de facciones y porcentaje de "mentirosos" para análisis externo.

[image1]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAXCAYAAAAC9s/ZAAAA8ElEQVR4Xu1SsQrCMBSMoF/gZNu0Fdx0y6Q/5iQI/oCDo3t/RnASwQ9w6dDJWe9qAskDU50E8eAgfZe7vLxUqZ9GryiKBcm1FDuR5/kU5prkWuqdgHEH3kkEbKXeCZguMB9sSC31KGA2WZZNsOzDvGeI1nom971Ca1J2cDTiu+GVXC0Kno7NJ6/E1+A8mne6aE8vy3LtF20XnAU76/taAGyYg2dwLCR2wYAbOxTaE9Z8TJJES43gU0ZDIFQQlrLu4P1Y7iohYqdb8Bob10WgGGMGKK7AEZmm6TDYYMF/A/qVIcobJidf2eRPWPkhf3wTD9vMTm5hFOg3AAAAAElFTkSuQmCC>