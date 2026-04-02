# **Manual Técnico y Conceptual: Proyecto NANOT Sandbox**

## **Sistema de Experimentación en Inteligencia de Colmena**

Este documento define las bases operativas, las reglas de comunicación y los desafíos de diseño para la sandbox de autómatas **NANOT**. El objetivo es crear un entorno donde la inteligencia emerja de la interacción simple entre agentes individuales mediante comunicación directa.

## **1\. Definición del Agente: El NANOT**

Cada NANOT es una unidad autónoma mínima con capacidad de procesamiento local. No existe un "cerebro central"; la inteligencia es distribuida.

### **1.1. Atributos Físicos y Lógicos**

* **Posición (![][image1]):** Coordenadas ![][image2] en el espacio bidimensional.  
* **Velocidad (![][image3]):** Vector de movimiento limitado por una velocidad máxima (![][image4]).  
* **Radio de Percepción (![][image5]):** Alcance de los sensores pasivos (detección de muros).  
* **Radio de Comunicación (![][image6]):** Alcance de la antena para intercambio de datos P2P.  
* **ID Único:** Identificador hexadecimal para trazabilidad en el log.

## **2\. Protocolo de Comunicación Directa**

A diferencia de los modelos biológicos basados en feromonas, los NANOT utilizan un protocolo de **Red Mesh Dinámica**.

### **2.1. Reglas de Intercambio**

1. **Broadcast de Estado:** Cada agente emite un "latido" (heartbeat) con su posición y rumbo cada determinado ciclo de reloj.  
2. **Transmisión de Mensajes de Objetivo:** Cuando un NANOT detecta un punto de interés (POI), el mensaje se propaga con un contador de "saltos" (hops) para evitar bucles infinitos.  
3. **Prioridad de Ancho de Banda:** Los mensajes de "Alerta de Obstáculo" tienen prioridad absoluta sobre los de "Exploración".

## **3\. Lógica de Comportamiento (Reglas de Navegación)**

El movimiento se basa en la integración de fuerzas vectoriales inspiradas en el modelo de *Boids* de Craig Reynolds, adaptado para resolución de problemas:

1. **Separación:** Evitar el hacinamiento con NANOTs vecinos.  
2. **Alineación:** Coincidir con el rumbo promedio de la colmena cercana para mantener flujos de tráfico eficientes.  
3. **Cohesión:** Permanecer cerca de la masa crítica para no perder el enlace de comunicación.  
4. **Atracción al Objetivo:** Una fuerza vectorial que empuja al agente hacia la meta comunicada por sus pares.

## **4\. Escenarios de Desafío (Sandboxing)**

| Escenario | Descripción | Variable de Éxito |
| :---- | :---- | :---- |
| **El Laberinto de Teseo** | Navegación por pasillos estrechos con múltiples callejones sin salida. | Tiempo de salida del 90% de la colmena. |
| **El Vacío de Conexión** | Un abismo separa dos plataformas; los NANOTs deben formar una cadena estática para guiar a otros. | Estabilidad del enlace de datos. |
| **Recolección Crítica** | Múltiples recursos dispersos que deben ser llevados a una zona de carga central. | Recursos/Segundo (R/s). |
| **Evasión de Depredador** | Un obstáculo móvil que "caza" NANOTs dentro de su radio de acción. | Tasa de supervivencia. |

## **5\. Implementación Técnica Sugerida**

Para el desarrollo de la ventana gráfica y el motor de física, se sugieren los siguientes enfoques:

* **Motor Gráfico:** p5.js o Canvas API (HTML5) para una visualización fluida en navegador.  
* **Estructura de Datos:** Uso de un *Quadtree* para optimizar la detección de vecinos y colisiones (esencial cuando ![][image7] agentes).  
* **Hardware (Opcional):** La lógica de estos agentes es compatible con microcontroladores como **ESP32** utilizando protocolos como *ESP-NOW* para pruebas de colmena en el mundo físico.

## **6\. Reglas de Limitación (Para mayor realismo)**

Para evitar que la simulación sea trivial, se deben aplicar "fricciones":

* **Ruido en la señal:** 5% de probabilidad de pérdida de paquetes.  
* **Inercia:** Los NANOTs no pueden cambiar de dirección instantáneamente.  
* **Energía:** La comunicación constante agota la batería virtual; los agentes deben equilibrar silencio y cooperación.

**Nota de Diseño:** La belleza de este sistema radica en observar cómo, ante un obstáculo inesperado, la colmena "fluye" como un líquido gracias a que la información viaja más rápido que los propios agentes.

[image1]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA8AAAAZCAYAAADuWXTMAAABQklEQVR4Xu1SPUvEQBDdQzu1sIhB8r2RC1cJliLY2AnHIXb+ABt7Wxv/wHVWam9rZyFYCraCIIiFVl5xraD3JkyO5ZFExOIaHzw2O2/e7uxkjPklwjBcw9LhuItOFEV5HYMg6GIdsGGKOI53kiR5aWKapmfs+RE41ILnHJ8xfN9fwJt2Udp+HfHWgTSNfSWstV2YH8A38FsI02tF7EcSwyHb7J0CCQdqvmXN87xFPeCEtRIQh2oesoYKltV8yZopimIJ4p2a91hHrKfmQ9Yq8UPZq9GPwEdMWsiaQTf7euu9lOhqMteIPckEuvES+qtu1DxyuvyM9QLrJtLm2FcCCRYJ7+AnvrdYb4WUA+OX3JRlmc96K2A61ZKvsJ1nvRUwXYsZFRyz1gidmlUtWd68kef5imlqTgXqsMsx3r3O+f/4AyZwKGVs12+PrAAAAABJRU5ErkJggg==>

[image2]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAC0AAAAZCAYAAACl8achAAADGklEQVR4Xu1WPWhTURR+IQqKiorG0Py8m5eIMSg4REWLIEIFHXRoB4WIs0MmCwoK4uLgJAjiUhEHKQiCooUODgFHF4cWQShUUTpIKYotVTHx+/Luaw6H99Lnkiz54PDuPd+593733L/nOAP0F0lrfUG1Wt1YLBa3a38k2MAY8wDFhOZ6BQqGhqdOHA0U7LruQzR4pLleI5vN7oKOcWc94Qgag83lcrm9musHkMDPnucd1f41QGwdtpTP5w9qrl8oFApnoOk3v5prA2QDNoWATZrrFzKZzG5omoVNorpB8xS9DLuh/QLJUqm0R0xK12MjlUpt5Z7V/hAkeCBh89iyWU1SdAt2VvstktxfNMSsQOhdfN/DvsAWYaO6QQQogtvwK+ynvtbgm4Nvn/RhzOvw/8H3hPS3AWIBRFH74TsA7lVQp0BOEMUExD9h2fjLty4Q95pnBlnbjH6nUT8uuAqswVWQbRA3An+T4qW/DRCfYEMh/mEMdF7U71jRLA/BTlFEp0U0EFvndoKAKsrLsh3qNdh9GU8EsWGik1GiJZgFZgO2qLn/AYTfDiZukYCoxzI5AbqJjsy0hPGXkHv4rebiAoPvRPt3cuLBLRG2PTGRI+BWokT/wEV+SPsde0uwYPwlbBmxjMjaSWajE94dHINjyYkH2dT72XLn7JhXNUdBJGrSJ7ZDy/UP5IKNG7VP/i2aY59a+Ccs38JkLsq+AoDzjH/rNFhHJg+j/A3WVKFtGP8McXUrmiPJjsIOwmXXv+o+wIZd/99kCTYDu0nxIvY0fN8pIHQ5LcBdQswqvi9tv79g8zouSBripkMPO8hJ2Cz3l+bYWIpDFnd0e1TQT62baILt+cDYiTY5vo4xnTNU11wbtvFqIeqdjw/eBPdgI5qwW4EPy3NmjonAeM9Q/8vxdTzFGn8HeJpbAw4JeDODji5oLg7sf/BU2NVFoN8r3Gr4HkOWc8a/yq7JVRTg6/lGv5qh4K8gOvqo/XGQTqe3QNB+J/ofmO/BGPp/QUGmyxXLOCZR+yOBBpVyubxN+3sFTKoIDRPaP8AAMfAPTdDWq/UyFoMAAAAASUVORK5CYII=>

[image3]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAXCAYAAAAC9s/ZAAABTUlEQVR4Xu2SPy8EURTFZ5ctJNsIIzH/J1OKQiYikdhGpVgS8Qk01BrUvoFGrZRoFHpKsd+BbZQKQUHB723eyN2bN6xC5yQnM3PO/Tf3Pc8bEWma3vEY07pGM47jwsUkSa6zLDvVCRpNQdOtem8xwT1FVmXwb1AV/CMEQTDNeFuSURSFMobveenzS8vIjYHJcjYQnuGH5SOLW5EF0I7hWxVDzoH0TcCaNV/oUA6ZFnib8IHkJe0Zcxb2bfUd7aPtwrOyLFvaG4CukyTfmgK872sfvYc+p/Uv+L7fJujKTjB0YUxXtEOvWlodTKLdw6XUWeh6GIZTUnOCxD1boG+lhunM6CcyrhYEdm2BJ/PNcwHthiKZCnWDURdJeoXvXJwJki/M9nVcLVJxlLDnPO/vIE8CHnk/bd2BcRLPTYGiKGa0ORLyPO9QYFvr/6jHJ689U/KS+RgiAAAAAElFTkSuQmCC>

[image4]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACcAAAAZCAYAAACy0zfoAAACXElEQVR4Xu2WO2hUQRSG77IKvsXHurivuy9YQ5AgiwQkFgEtJKiQSrAzoBaSIkViYiNIwEqDCEIKJV3SmyaIioJNAmkUQbCwshdMp/H7b87oMGyjkrvN/nA4M/85c8+ZOfO4UdRDD+kjU61Wb8dxPO8L3LTvBHfNt1cqlXHfvl3IEOwM8gXZRN6R2JVSqXTSd1Lf7OvIGNLn27cVBJu14Msktyu0w1WxnQr5VEDgCUvubavV2h+YM5RxTjrg0wHBL1pyKu9x31Yul8/CvfS5VEECp0lgA/lWq9UGHF+v1w9S0RWSP+/7pwqtlq3aplZRHLqf/hqHoRn6pwrtM+03S25KHCu2hNwMfVOHTiiJLdu+eyRO/Vwuty/07QpIcEHJSReLxRJ6MPTpGlROS+4N+mnUraujE0jogpX1B6f3Ugf7HRI/gf6KzENlmdAi7ffu0MTeVaSTjn2SZoYbIA//Uby2Cvx9qnNEbfjHyAjyUzn8ieiBAW2M39HP2u32ztCObViDkc8kU8TvEO1V5JbnsypebR0m+akNV1fixuu1GVa7UCgcpT3abDYP4DPUKW4CPrRbjhoQ2hzirWcueeLQfUpUd6SZd8R2mGxFXke2NQh8Tr7m9xt2v/7/O53P5/fyoRfIjProq8gHNxkF0exVYks8WSmzJZPS3Uk3qzFaJSZ5wz2XGqcFcmP+Cpo98smVimCvkDHfTv8BAS+bfQRuPN76xXqIft5oNI6Zn8bew/cu/evIE90Q7lv/As14j+vYjLOePSmn39emj6y0OiCOl5/zZbKHnU8PPXQTvwB66ZQol3v90AAAAABJRU5ErkJggg==>

[image5]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABYAAAAaCAYAAACzdqxAAAABwUlEQVR4Xu2UP0tDMRTFW1RQUESwSp9t076ipZNDV8HFRQRxEBx0EJ11Ee0n6BdwEbQgDiLSpUOlQx0cBb+Agw6K4OZQcHBRf7fNk5Dafw8RBA8ckpybe9Lc3NdA4B+/CqXUfDweP2G8jcVij7DI/NCj67pTbAvaeW1BchqzZcZT+MEh67LW2qZosMABw3ZuRyB5X0xsnQOORWc8YNlrx1siFAoNknzVxDirf/V9IpEYt+MtEY1GF3XyjR1De9KxHTvWFiTldHLe1DOZTJ/oUgaZm7G2MMuAwXYkEpnkAZdkjnYHFwJ+ugIDl+Rn+KbqnSFtVtI3qNj7O4ZZXw4Z8XTma6J3XQKBWQa4asYwnkN7h2FT7whGGaq00rQZU/pBu24xQbMy8Hj9aBcSk1uJxjzNw04wluE13NCff6mhXIhH2rggZp5uG2M4gHYOZ6Vr0B8w3WNrkHlYylZLlGsjVLWpyTPPnM0zrF8ZtxjzcFfKomsvxq7s08ZZL883VP0mOW8tpr7ewYaqf+K1Dkomk2Pqm78BX8DoBVb0l1mm7iv2Hl/A7DKVSg05jjPa0A1+obvlq74/Bfkj6hFzszX/Nj4BxNSJPrppSF0AAAAASUVORK5CYII=>

[image6]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABYAAAAZCAYAAAA14t7uAAABi0lEQVR4Xu2UPUsDQRCGE6KgKKhoPJJLcrkLCFYW11qI2KSxstNCsBNsFMkvEH+CoGksRDQWFlrZ2Ip/wMYmCPZCChv1GdyVZcBcEj+qvDDM7DufO+xdKtXHvyIIgmq5XD5GP5RKpSZyiX1oJYqiGcLSOi8RJM9SbAV9grzTZF3OhtsQDmnQYEznJiKbzY6SfCtFtI8GNVP8MQxDT/vbguSIxGfkRfvsTZB74ia0vy2KxeKyTdY+uCfj29G+RJC0Z5LrLh/H8aDwTHogtutLhLOG1+Dz2vIarkyjGx3fMdw1uDvEXhO+60ktgm/WQOEYroXkXL4juM8MWXV9FF6Ce+upsPvMeKNzrs/epOu3KyDxyEzb4IsbsrzYcNfik1sVCoVhuDOzngZ6n7AMdlVu9VVQppMpTVFXTm0MyfOcW+gtdB3ZhU5zbmKHEpPP56douGBzOobv+5Pyz5BJOWY8zxuh6AX2gI79ESiao0lN878CCt/RYBvZZA3n6EUd0ysylUplmqLj2tHH3+MDFMlzat3X69MAAAAASUVORK5CYII=>

[image7]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEkAAAAZCAYAAAB9/QMrAAADvklEQVR4Xu1XTUhUURQeGYOiP6Vk1Pm5MzY1CBXBQJFEi2gRSC1CKHDXojaW0KZFGbVo0aZCWoUgBdEfJFFK1EBSUPazCsQIhQoTXMhA4KIi7fvmnat3rm/0jY+BoPfBx333nO/c9+6Ze8+9EwoFCBAgwD+KZDK5QymVSyQS39B+QnsS5ipTA/tZ6G6ivaHJOFNTaaTT6XWRSGS1bY/FYqtsW11d3Zp4PL6VtH0mMIcaxG/OZrMrbF8RIIoiMW1gJyY/C+atwatga4H/FNpptOfRtrp9cCWB957h9+kfC+09tN/xrZtMHWzd4A/6hZftRMJWD81DcBLjPkI7BXbAFTZ1C8DBEXxBEjWSSqUipr+xsXEj7M2mrRSampq2RKPRDbbdDyRJ7yVBF/mO0MIVvxu8ZNmYtJ/0sY/YozLHdq2BbSX6/eD4fKSFTCazFoInXEFo8zIIMzsHfOQesNa0lQJeegD8iERvD1kTWS6YJK4i225CEjI3eQIxJ2Q+heRJMn5zPqZOYmdNWxHgbAYH8FiNtkcGHTI1fJnZ94AwxngDfgBb2bcF5WCpJLEO4T2D0B007ezLfHIsEWi/KqdsZC1dYTubtiLA2Q5RrzxzyXJ5zhiSanzgfaPvGVhNGeXs/wk/dUwnCeN0Kefw6EIJiGs/+g1MwCJJGpaSMU2WShK3nmnXzlo4B5RRb/B8GJxRso+h2Y/gZ/NRywOTlHAOiCn9o3iFfFNO93kiod+HEnFI/EsliSuImkWTxBVp2rUzC+dzJkvbUBTXwzYEXke3SgYoKoh+wAmygDLxfuoWvwt8ytOrokmSwtZt22HrACekmPdxNdkan2DN+sJEgTW20wuUs7rGcZLGKpkk3oFu82W2A7YUgxB8F+0r3qdszXIgv/oxcIL1yva7ATHboB9WxnYjJAGFCXso3IOiKa9wSyF7oUrcf2Rwst+1oJUJjHMOnASvgA22vxT0BMA/ph390+CYvtPhucc+hUXD2MJuQZsDZ+ydwVjqTBsRxoAv4RjCL5UOudQF+O4olzuFV7DecDsh/jNrkO33CqmR/KGOaBvG3glbHq3SNlkpY9AltY198LG+dcvWHOHcOS5teN6lnNV1TccxUB/zswb75wSGDoFvueJs3xJgvdmnnHtSC/u2oFxwcpjMO+Uc/w/AX+BrW0cbOIrvPk7iOYe4elOTdP6vjsp4XGl5aHv9XFHKBl54FS++Zdv9Qo79dq5K5ZQIt+SHWW+0LuSyQwiOhVW4F9o2874VIECAAAECBPiv8RfQzjzFaILwrwAAAABJRU5ErkJggg==>