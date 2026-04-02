# **Documento Maestro: Proyecto NANOT Evolution Sandbox**

## **1. Visión General y Argumentación del Proyecto**

El **Proyecto NANOT Evolution Sandbox** es una plataforma de simulación interactiva diseñada para explorar la intersección entre la **Inteligencia de Colmena (Swarm Intelligence)**, la **Biología Sintética** y los **Sistemas Distribuidos**. 

**¿Por qué es relevante este proyecto?**
Tradicionalmente, la inteligencia artificial se concibe de forma centralizada (un gran modelo o un servidor centralizado tomando decisiones). En contraste, la naturaleza nos enseña que el comportamiento complejo y resiliente a menudo surge de **reglas simples, ejecutadas por individuos con percepción local y sin un control central** (como las colonias de hormigas, las bandadas de aves o las redes neuronales biológicas). 

Este proyecto propone crear un ecosistema digital donde la inteligencia no se programa directamente, sino que **emerge**. Al integrar reglas de navegación biomimética (modelo Boids), protocolos de red paramétricos (redes Mesh) y un sistema metabólico evolutivo, el NANOT Sandbox se convierte en un laboratorio poderoso para:
1. **Modelar comportamientos sociológicos y biológicos**, como el altruismo algorítmico, la capacidad de carga de un entorno, y la "Tragedia de los Comunes".
2. **Estudiar la optimización de redes P2P y robótica de enjambre**, analizando cómo se propaga la información crítica en entornos con ruido o fallas.
3. **Observar la evolución no guiada**, viendo cómo las presiones ambientales (escasez de energía) moldean la eficiencia (mutaciones) de la colonia a lo largo de las generaciones.

---

## **2. Definición del Agente: El NANOT v2.0**

El **N.A.N.O.T.** (acrónimo de ***N**odo **A**utónomo de **N**avegación y **O**peración **T**opológica*) es la unidad anatómica y lógica de este universo. No tiene un "cerebro" global, sino que procesa información puramente local basándose en su entorno inmediato y la retroalimentación de sus vecinos.

### **2.1. Atributos Físicos y Locomotores**
* **Posición (![][image1]):** Coordenadas (x, y) en el espacio bidimensional.
* **Velocidad (![][image3]) e Inercia:** Vector de movimiento limitado por una velocidad máxima (![][image4]). El agente no rompe las leyes de la física local; la inercia impide giros instantáneos.
* **Sensores (Radios):**
  * **Radio de Percepción (![][image5]):** Alcance visual pasivo para detectar recursos inmediatos o muros.
  * **Radio de Comunicación (![][image6]):** Alcance de su antena para transmitir y recibir estados de la red.

### **2.2. Sistema Metabólico (Biología Sintética)**
Los recursos no son infinitos. Para mantener el realismo y forzar decisiones, cada agente posee un metabolismo:
* **Energía:** Valor (ej. 0-100). Es el combustible vital.
* **Consumo Basal:** La energía decae inevitablemente en cada ciclo de simulación.
* **Costo de Acción:** Acciones complejas (moverse a máxima velocidad o hacer "broadcast" de mensajes urgentes) tienen un sobreprecio energético. Los agentes deben decidir entre silencio/ahorro o comunicación/gasto.
* **Edad:** Ciclos vividos. Al superar la esperanza de vida, el agente muere por senescencia.

---

## **3. Protocolos de Comunicación y Carga Cognitiva**

La colmena no depende de la liberación de feromonas (que persisten pasivamente en el entorno), sino de una **Red Mesh Dinámica P2P**. 

### **3.1. Reglas de Propagación P2P**
1. **Heartbeat (Estado Base):** Todo NANOT emite su rumbo y necesidad energética periódicamente.
2. **Mensajes de Interés (POI):** Al hallar un botín o amenaza, el descubridor emite una alerta. Para evitar bucles infinitos en la red (broadcast storm), los mensajes incluyen un "Time-To-Live" (TTL) o límite de saltos cognitivos.
3. **Prioridad de Señal:** Las alertas de peligro (ej. un depredador) suprimen temporalmente los mensajes exploratorios para usar todo el ancho de banda.
4. **Ruido por Saturación:** Si hay demasiados NANOTs comunicándose en el mismo espacio, el ruido electromagnético y la superposición de datos causan una caída en la asimilación de la información (![][image4] base aumenta), emulando los límites termodinámicos de la comunicación masiva.

---

## **4. Lógica de Comportamiento Colectivo**

La danza de la colmena se basa en la integración de vectores de fuerza. Es una versión ampliada del clásico algoritmo de *Boids* de Craig Reynolds:

* **Separación:** Repulsión instintiva de vecinos muy cercanos para evitar la colisión y la saturación de recursos.
* **Alineación:** Seguir la corriente armónica de los pares para mantener un flujo de tráfico ordenado.
* **Cohesión:** Fuerza que mantiene al enjambre junto, evitando que los exploradores pierdan cobertura de la red.
* **Atracción al Objetivo:** Vector direccional hacia un recurso comunicado por un vecino.

---

## **5. Simulación Ecológica: Evolución y el Entorno**

El entorno del Sandbox no es inerte; reacciona a los NANOTs.

### **5.1. Recursos y "La Tragedia de los Comunes"**
El mapa genera nodos de energía esporádicamente. Si un grupo moderado de agentes los consume, el nodo se regenera. Sin embargo, si la densidad de población local explota y demasiados NANOTs intentan extraer energía al mismo tiempo, el nodo se erosiona y muere. Esto fuerza a la colmena a aprender (evolutivamente) a no sobreexplotar agresivamente o perecer.

### **5.2. Ciclo Reproductivo y Genética**
Si un agente se alimenta con creces (![][image1]) y coincide geoespacialmente con otro individuo próspero, se reproducen genéticamente. 
El descendiente hereda promedios de los "radios" de percepción y motores de los padres, pero sufre **mutaciones estocásticas**. Las generaciones futuras que nazcan en tiempos de escasez serán aquellas que hayan mutado para tener un metabolismo más lento o sensores más amplios, mostrando Evolución Darwiniana en base al motor físico.

---

## **6. Uso de la Herramienta (Sandbox e Interfaz del Investigador)**

La ventana de interacción le entrega atributos de "Dios" al investigador. El sistema ofrece herramientas en tiempo real para modificar parámetros ecosistémicos:

* **Manipulación Gravitacional:** Alterar dinámicamente los pesos del modelo (separación, cohesión, costo de comunicación).
* **Estrés Físico (Escenarios Predefinidos):** 
  * *Laberintos de Teseo* para estudiar la latencia en guiar a la masa hacia la salida.
  * *Evasión de Depredadores* para calcular tasas de sacrificio.
* **Eventos de Extinción (Botón Catástrofe):** Permite exterminar instantáneamente al 50% aleatorio de la población para observar cuánto tarda la red mesh fractal en sanarse y re-coordinar sus exploradores.

---

## **7. Protocolos de Seguridad y Contención (Sandbox Seguro)**

Entendiendo la naturaleza impredecible de los comportamientos emergentes y las mutaciones estocásticas, es crítico establecer **límites duros ("Kill Switches")**. El entorno garantiza que la simulación es un *sandbox* puro y no se saldrá de control, protegiendo tanto la conceptualización como el rendimiento de la máquina anfitriona:

* **Tope Poblacional Estricto (Hard Cap):** Independientemente de la riqueza de recursos en el mapa, el motor impone un techo inquebrantable de entidades simultáneas (ej. Max 1000 NANOTs). Evita bucles infinitos de reproducción y fugas de memoria (RAM overflows).
* **Entorno Aislado (Air Gapped):** La "Red Mesh" es completamente emulada. Los agentes no tienen acceso a interfaces de red reales, almacenamiento del disco duro ni pueden inyectar código en el sistema operativo.
* **Bordes del Mundo Finitos:** El canvas de simulación es un plano cerrado sin generación procedural infinita. Si un agente choca con el borde de la simulación, su inercia se invierte o se detiene.
* **Acotamiento Genético (Clamping):** Las mutaciones estocásticas tienen rangos predefinidos inmutables en el código subyacente. Un NANOT jamás podrá mutar para generar "energía infinita", ni extender su radio de comunicación más allá de lo físicamente creíble en su escala.
* **Botón de Aniquilación Total (Kill Switch):** Un botón maestro de "Reset" en la interfaz gráfica que destruye las instancias de los objetos en memoria y detiene el ciclo de actualización (`update()`) de inmediato.

---

## **8. Arquitectura Técnica Recomendada**

Para sostener una simulación de alta entropía (![][image7] 500+ agentes) a 60 FPS:

* **Motor de Renderizado:** Librerías veloces en 2D (Raylib en C/C++, o p5.js/CanvasAPI WebGL optimizado).
* **Partición Espacial:** Uso mandatorio de estructuras **Quadtree** o *Spatial Hashing* para el cálculo de distancias (Boids en cuadrática O(n²) colapsaría el CPU). 
* **Lenguaje Visual:** Representación minimalista. Agentes como geometrías direccionales (triángulos) que cambian cromáticamente (Verdad/Energía alta a Rojo/Energía baja) para que el estado general de la colmena sea legible heurísticamente con un solo vistazo.

---
*Nota Resumen: La verdadera belleza algorítmica del NANOT Evolution Sandbox reside en observar cómo, ante la caída de un alimento o la aparición de un "muro", la colonia se comporta como un líquido dinámico. La información -viajando telepáticamente a la velocidad de las ondas de radio de agente en agente- se mueve mucho más rápido que la materia misma. El sistema aprende, se adapta y fluye, demostrando que el todo es, irrevocablemente, mucho más que la simple suma de sus partes matemáticas.*

[image1]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA8AAAAZCAYAAADuWXTMAAABQklEQVR4Xu1SPUvEQBDdQzu1sIhB8r2RC1cJliLY2AnHIXb+ABt7Wxv/wHVWam9rZyFYCraCIIiFVl5xraD3JkyO5ZFExOIaHzw2O2/e7uxkjPklwjBcw9LhuItOFEV5HYMg6GIdsGGKOI53kiR5aWKapmfs+RE41ILnHJ8xfN9fwJt2Udp+HfHWgTSNfSWstV2YH8A38FsI02tF7EcSwyHb7J0CCQdqvmXN87xFPeCEtRIQh2oesoYKltV8yZopimIJ4p2a91hHrKfmQ9Yq8UPZq9GPwEdMWsiaQTf7euu9lOhqMteIPckEuvES+qtu1DxyuvyM9QLrJtLm2FcCCRYJ7+AnvrdYb4WUA+OX3JRlmc96K2A61ZKvsJ1nvRUwXYsZFRyz1gidmlUtWd68kef5imlqTgXqsMsx3r3O+f/4AyZwKGVs12+PrAAAAABJRU5ErkJggg==>
[image2]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAC0AAAAZCAYAAACl8achAAADGklEQVR4Xu1WPWhTURR+IQqKiorG0Py8m5eIMSg4REWLIEIFHXRoB4WIs0MmCwoK4uLgJAjiUhEHKQiCooUODgFHF4cWQShUUTpIKYotVTHx+/Luaw6H99Lnkiz54PDuPd+593733L/nOAP0F0lrfUG1Wt1YLBa3a38k2MAY8wDFhOZ6BQqGhqdOHA0U7LruQzR4pLleI5vN7oKOcWc94Qgag83lcrm9musHkMDPnucd1f41QGwdtpTP5w9qrl8oFApnoOk3v5prA2QDNoWATZrrFzKZzG5omoVNorpB8xS9DLuh/QLJUqm0R0xK12MjlUpt5Z7V/hAkeCBh89iyWU1SdAt2VvstktxfNMSsQOhdfN/DvsAWYaO6QQQogtvwK+ynvtbgm4Nvn/RhzOvw/8H3hPS3AWIBRFH74TsA7lVQp0BOEMUExD9h2fjLty4Q95pnBlnbjH6nUT8uuAqswVWQbRA3An+T4qW/DRCfYEMh/mEMdF7U71jRLA/BTlFEp0U0EFvndoKAKsrLsh3qNdh9GU8EsWGik1GiJZgFZgO2qLn/AYTfDiZukYCoxzI5AbqJjsy0hPGXkHv4rebiAoPvRPt3cuLBLRG2PTGRI+BWokT/wEV+SPsde0uwYPwlbBmxjMjaSWajE94dHINjyYkH2dT72XLn7JhXNUdBJGrSJ7ZDy/UP5IKNG7VP/i2aY59a+Ccs38JkLsq+AoDzjH/rNFhHJg+j/A3WVKFtGP8McXUrmiPJjsIOwmXXv+o+wIZd/99kCTYDu0nxIvY0fN8pIHQ5LcBdQswqvi9tv79g8zouSBripkMPO8hJ2Cz3l+bYWIpDFnd0e1TQT62baILt+cDYiTY5vo4xnTNU11wbtvFqIeqdjw/eBPdgI5qwW4EPy3NmjonAeM9Q/8vxdTzFGn8HeJpbAw4JeDODji5oLg7sf/BU2NVFoN8r3Gr4HkOWc8a/yq7JVRTg6/lGv5qh4K8gOvqo/XGQTqe3QNB+J/ofmO/BGPp/QUGmyxXLOCZR+yOBBpVyubxN+3sFTKoIDRPaP8AAMfAPTdDWq/UyFoMAAAAASUVORK5CYII=>
[image3]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAXCAYAAAAC9s/ZAAABTUlEQVR4Xu2SPy8EURTFZ5ctJNsIIzH/J1OKQiYikdhGpVgS8Qk01BrUvoFGrZRoFHpKsd+BbZQKQUHB723eyN2bN6xC5yQnM3PO/Tf3Pc8bEWma3vEY07pGM47jwsUkSa6zLDvVCRpNQdOtem8xwT1FVmXwb1AV/CMEQTDNeFuSURSFMobveenzS8vIjYHJcjYQnuGH5SOLW5EF0I7hWxVDzoH0TcCaNV/oUA6ZFnib8IHkJe0Zcxb2bfUd7aPtwrOyLFvaG4CukyTfmgK872sfvYc+p/Uv+L7fJujKTjB0YUxXtEOvWlodTKLdw6XUWeh6GIZTUnOCxD1boG+lhunM6CcyrhYEdm2BJ/PNcwHthiKZCnWDURdJeoXvXJwJki/M9nVcLVJxlLDnPO/vIE8CHnk/bd2BcRLPTYGiKGa0ORLyPO9QYFvr/6jHJ689U/KS+RgiAAAAAElFTkSuQmCC>
[image4]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACcAAAAZCAYAAACy0zfoAAACXElEQVR4Xu2WO2hUQRSG77IKvsXHurivuy9YQ5AgiwQkFgEtJKiQSrAzoBaSIkViYiNIwEqDCEIKJV3SmyaIioJNAmkUQbCwshdMp/H7b87oMGyjkrvN/nA4M/85c8+ZOfO4UdRDD+kjU61Wb8dxPO8L3LTvBHfNt1cqlXHfvl3IEOwM8gXZRN6R2JVSqXTSd1Lf7OvIGNLn27cVBJu14Msktyu0w1WxnQr5VEDgCUvubavV2h+YM5RxTjrg0wHBL1pyKu9x31Yul8/CvfS5VEECp0lgA/lWq9UGHF+v1w9S0RWSP+/7pwqtlq3aplZRHLqf/hqHoRn6pwrtM+03S25KHCu2hNwMfVOHTiiJLdu+eyRO/Vwuty/07QpIcEHJSReLxRJ6MPTpGlROS+4N+mnUraujE0jogpX1B6f3Ugf7HRI/gf6KzENlmdAi7ffu0MTeVaSTjn2SZoYbIA//Uby2Cvx9qnNEbfjHyAjyUzn8ieiBAW2M39HP2u32ztCObViDkc8kU8TvEO1V5JbnsypebR0m+akNV1fixuu1GVa7UCgcpT3abDYP4DPUKW4CPrRbjhoQ2hzirWcueeLQfUpUd6SZd8R2mGxFXke2NQh8Tr7m9xt2v/7/O53P5/fyoRfIjProq8gHNxkF0exVYks8WSmzJZPS3Uk3qzFaJSZ5wz2XGqcFcmP+Cpo98smVimCvkDHfTv8BAS+bfQRuPN76xXqIft5oNI6Zn8bew/cu/evIE90Q7lv/As14j+vYjLOePSmn39emj6y0OiCOl5/zZbKHnU8PPXQTvwB66ZQol3v90AAAAABJRU5ErkJggg==>
[image5]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABYAAAAaCAYAAACzdqxAAAABwUlEQVR4Xu2UP0tDMRTFW1RQUESwSp9t076ipZNDV8HFRQRxEBx0EJ11Ee0n6BdwEbQgDiLSpUOlQx0cBb+Agw6K4OZQcHBRf7fNk5Dafw8RBA8ckpybe9Lc3NdA4B+/CqXUfDweP2G8jcVij7DI/NCj67pTbAvaeW1BchqzZcZT+MEh67LW2qZosMABw3ZuRyB5X0xsnQOORWc8YNlrx1siFAoNknzVxDirf/V9IpEYt+MtEY1GF3XyjR1De9KxHTvWFiTldHLe1DOZTJ/oUgaZm7G2MMuAwXYkEpnkAZdkjnYHFwJ+ugIDl+Rn+KbqnSFtVtI3qNj7O4ZZXw4Z8XTma6J3XQKBWQa4asYwnkN7h2FT7whGGaq00rQZU/pBu24xQbMy8Hj9aBcSk1uJxjzNw04wluE13NCff6mhXIhH2rggZp5uG2M4gHYOZ6Vr0B8w3WNrkHlYylZLlGsjVLWpyTPPnM0zrF8ZtxjzcFfKomsvxq7s08ZZL883VP0mOW8tpr7ewYaqf+K1Dkomk2Pqm78BX8DoBVb0l1mm7iv2Hl/A7DKVSg05jjPa0A1+obvlq74/Bfkj6hFzszX/Nj4BxNSJPrppSF0AAAAASUVORK5CYII=>
[image6]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABYAAAAZCAYAAAA14t7uAAABi0lEQVR4Xu2UPUsDQRCGE6KgKKhoPJJLcrkLCFYW11qI2KSxstNCsBNsFMkvEH+CoGksRDQWFlrZ2Ip/wMYmCPZCChv1GdyVZcBcEj+qvDDM7DufO+xdKtXHvyIIgmq5XD5GP5RKpSZyiX1oJYqiGcLSOi8RJM9SbAV9grzTZF3OhtsQDmnQYEznJiKbzY6SfCtFtI8GNVP8MQxDT/vbguSIxGfkRfvsTZB74ia0vy2KxeKyTdY+uCfj29G+RJC0Z5LrLh/H8aDwTHogtutLhLOG1+Dz2vIarkyjGx3fMdw1uDvEXhO+60ktgm/WQOEYroXkXL4juM8MWXV9FF6Ce+upsPvMeKNzrs/epOu3KyDxyEzb4IsbsrzYcNfik1sVCoVhuDOzngZ6n7AMdlVu9VVQppMpTVFXTm0MyfOcW+gtdB3ZhU5zbmKHEpPP56douGBzOobv+5Pyz5BJOWY8zxuh6AX2gI79ESiao0lN878CCt/RYBvZZA3n6EUd0ysylUplmqLj2tHH3+MDFMlzat3X69MAAAAASUVORK5CYII=>
[image7]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEkAAAAZCAYAAAB9/QMrAAADvklEQVR4Xu1XTUhUURQeGYOiP6Vk1Pm5MzY1CBXBQJFEi2gRSC1CKHDXojaW0KZFGbVo0aZCWoUgBdEfJFFK1EBSUPazCsQIhQoTXMhA4KIi7fvmnat3rm/0jY+BoPfBx333nO/c9+6Ze8+9EwoFCBAgwD+KZDK5QymVSyQS39B+QnsS5ipTA/tZ6G6ivaHJOFNTaaTT6XWRSGS1bY/FYqtsW11d3Zp4PL6VtH0mMIcaxG/OZrMrbF8RIIoiMW1gJyY/C+atwatga4H/FNpptOfRtrp9cCWB957h9+kfC+09tN/xrZtMHWzd4A/6hZftRMJWD81DcBLjPkI7BXbAFTZ1C8DBEXxBEjWSSqUipr+xsXEj7M2mrRSampq2RKPRDbbdDyRJ7yVBF/mO0MIVvxu8ZNmYtJ/0sY/YozLHdq2BbSX6/eD4fKSFTCazFoInXEFo8zIIMzsHfOQesNa0lQJeegD8iERvD1kTWS6YJK4i225CEjI3eQIxJ2Q+heRJMn5zPqZOYmdNWxHgbAYH8FiNtkcGHTI1fJnZ94AwxngDfgBb2bcF5WCpJLEO4T2D0B007ezLfHIsEWi/KqdsZC1dYTubtiLA2Q5RrzxzyXJ5zhiSanzgfaPvGVhNGeXs/wk/dUwnCeN0Kefw6EIJiGs/+g1MwCJJGpaSMU2WShK3nmnXzlo4B5RRb/B8GJxRso+h2Y/gZ/NRywOTlHAOiCn9o3iFfFNO93kiod+HEnFI/EsliSuImkWTxBVp2rUzC+dzJkvbUBTXwzYEXke3SgYoKoh+wAmygDLxfuoWvwt8ytOrokmSwtZt22HrACekmPdxNdkan2DN+sJEgTW20wuUs7rGcZLGKpkk3oFu82W2A7YUgxB8F+0r3qdszXIgv/oxcIL1yva7ATHboB9WxnYjJAGFCXso3IOiKa9wSyF7oUrcf2Rwst+1oJUJjHMOnASvgA22vxT0BMA/ph390+CYvtPhucc+hUXD2MJuQZsDZ+ydwVjqTBsRxoAv4RjCL5UOudQF+O4olzuFV7DecDsh/jNrkO33CqmR/KGOaBvG3glbHq3SNlkpY9AltY198LG+dcvWHOHcOS5teN6lnNV1TccxUB/zswb75wSGDoFvueJs3xJgvdmnnHtSC/u2oFxwcpjMO+Uc/w/AX+BrW0cbOIrvPk7iOYe4elOTdP6vjsp4XGl5aHv9XFHKBl54FS++Zdv9Qo79dq5K5ZQIt+SHWW+0LuSyQwiOhVW4F9o2874VIECAAAECBPiv8RfQzjzFaILwrwAAAABJRU5ErkJggg==>
