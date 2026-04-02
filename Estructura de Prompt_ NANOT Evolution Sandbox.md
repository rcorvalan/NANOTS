### **Título del Proyecto: NANOT Evolution Sandbox (Simulación de Vida Artificial)**

**Contexto:**

Desarrollar una aplicación de escritorio (Windows) que simule una colmena de autómatas NANOT bajo reglas de Inteligencia de Colmena y Biología Sintética. La simulación debe correr en una ventana gráfica interactiva donde el usuario pueda manipular el ecosistema en tiempo real.

**1\. Arquitectura del Agente (NANOT v2.0):**

Cada agente debe heredar las reglas de movimiento del modelo Boids (Separación, Alineación, Cohesión) e integrar un "Sistema Metabólico":

* **Estado Vital:** Energía (0-100), Edad (ciclos), Estado (Buscando, Alimentándose, Comunicando, Reproduciéndose).  
* **Consumo Basal:** La energía disminuye por cada ciclo de reloj.  
* **Costo de Acción:** El movimiento a alta velocidad y el broadcast de mensajes consumen energía adicional.  
* **Reproducción:** Al superar un umbral de energía (![][image1]) y encontrar un par, generan un nuevo NANOT con una mutación aleatoria en sus radios de percepción (![][image2]) o comunicación (![][image3]).

**2\. Lógica del Entorno y Recursos:**

* **Fuentes de Energía:** Nodos de recursos que aparecen de forma estocástica.  
* **Regla de Explotación:** Si demasiados NANOTs consumen del mismo nodo simultáneamente, la tasa de regeneración del nodo disminuye o desaparece (Simulación de Tragedia de los Comunes).  
* **Manejo de Población:** Implementar un límite de "Capacidad de Carga" del ambiente. Si la densidad de población supera un umbral, el ruido en la señal (![][image4] base) aumenta exponencialmente, dificultando la coordinación para hallar comida.

**3\. Interfaz de Usuario (Panel de Control):**

La ventana debe incluir sliders para modificar:

* **Tasa de Natalidad:** Probabilidad de éxito en la reproducción.  
* **Factor de Decaimiento:** Velocidad a la que se agota la energía.  
* **Pesos de Boids:** Influencia de la Cohesión vs. Independencia.  
* **Botón de "Catástrofe":** Elimina el 50% de la población de forma aleatoria para observar la recuperación de la red mesh.

**4\. Flujo Lógico Conceptual (Pseudocódigo):**

PARA CADA ciclo\_simulacion:  
    ACTUALIZAR recursos\_en\_mapa()  
      
    PARA CADA nanot EN colmena:  
        SI nanot.energia \<= 0 O nanot.edad \> limite\_vida:  
            ELIMINAR nanot (Muerte)  
            CONTINUAR  
              
        CALCULAR fuerzas\_boids(nanot, vecinos)  
        CALCULAR atraccion\_recursos(nanot, recursos\_cercanos)  
          
        EJECUTAR movimiento(nanot)  
        nanot.energia \-= costo\_basal \+ costo\_movimiento  
          
        SI nanot.detecta\_recurso():  
            nanot.energia \+= tasa\_ingesta  
            BROADCAST("POI\_RECURSO", posicion)  
              
        SI condicion\_reproduccion\_met(nanot):  
            CREAR nuevo\_nanot(posicion\_actual, mutacion\_ligera)

**5\. Restricciones Técnicas para la Implementación:**

* **Motor:** Usar una librería de dibujo 2D (como Pygame para Python o Raylib para C).  
* **Optimización:** Implementar un Quadtree para la gestión de colisiones y vecindad, asegurando estabilidad con más de 500 agentes.  
* **Estética:** Fondo oscuro, NANOTs representados por triángulos/puntos cuyo color cambie según su energía (Verde=Lleno, Rojo=Crítico).

[image1]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAE0AAAAZCAYAAAB0FqNRAAAEbElEQVR4Xu2XXYiWRRTHnxcVlCzT3Jb9ep93P2DRopIlQkkvpAulEmEDN/aiiy5WRPBCKAIJb0IqiLAoEEJERJQoBBdlEVlK8BOsiwURAhURSrxIWMFg3X7/fea8nh3fr/XNbnz+8OeZOefMzJkzM2fmSZIcOXI8YfT29r5YKpWej+UeAwMDC1pbW5+J5U8l0jRdAY/Dy8Vi8dPOzs5FsY1k6PfBnbGuDKL+WjCqS9nG7f8nzGP8Nrha5Vhp0A7BZk2lndTR0fEC8gsUC7KjfDTUfX8FyeBYS0vLYiefje7u7pSov6fIM+A03BvqM6S+je8xvlN834zbP2l0dXW9zthX4HhYvD/x4x1UBWemyW5Bdwf+gP4q9V9gyQwoDqF7YHXNRX3yvWGkfhveovyS2dWEggVH6XxhrBPoaAB9Wyz3aG9v74plTaLAmN9FsvnIfhRV7unpWUL5HLyIj0vNiPJbPkjM6wD1SatrLgqa1QVstmoXellVaLAw6MdejmxVElZUQevv73/W62Ngfwa783zXJzWOUaPQEaGvn+OJhACcUrK24FgQzSbN8tcdCyTlz3zQkPfAY85+lfy3el1wRF+lwV054OXU9ycuaIlzqgoK2K2kr5/gH5US7RyhXTWtIPX19T0ngeUmW2B9zcY3TLOddF1zUx39BgXX9Bz7TdS/UTns1lE4aPq6wHhYAzPJDpNplRloxNvNBcFp5ZhdmmisbxS0n5Jv8G/8eZ/vIXhCE5U+7LqqQbONEOYzpnbK4+BMGk4S8k+w+z6pvykykIeW03hCAxdd8odXbGs3i3BbDclpHH4lmZ3Ea0I7DF8OptkRnA7cZUe2gaC96+UxsPmVtm+4+ll4F46z2J3etgwdOwwm4b1Iru1fnpytbDNgjPU4eIHvJZVjfSVgd402u+HCUrbTFDQt8MzOaDJo89F/lIR5ctL6rE7bQXh8tnkAg40ERya8nMZfWxmd9nPjSbI65MxqeBH+Hitj6PWOH/v9RZBm+VL+3ldfzQQN/aDbDPLtW5eiZhbEbD3s+tbKKelXBPrtPoiPCT1QlQbOhuNQ94hiN1R89G04D9meEKgRnYg6QdMF9giwL6E7b/VieEH4R636t3IZiiqG1zQoHI71gjpHd47b5uVY1wj0LKD9h/AW+aw/1teCAlIprygQ9DepSfHdGPyf9ca0F4Fytm8rhBx7BG41mQXZB63iLsXoizDgb/6G0+DhhrkOH1Df4NvVA226tXPhjmZ+fHV06OOqco3JKC+j/9Nwnx1byjvhFH5uUV1yjY/strUzpOE9pvl5ueaMfNQHmfresoGtQvrwNqrF8Zr/YQ66GRl8jDYTfDfH+scBfZ2C/8DDaRYc+f2lfwOGnfM58nul7Ejrt++vtMJlk9Z4j+ntBteqrIXC7nRs858DZ0+WGsxXc4DeUcJm+AEL0xobGPQbx0SHsVkX/0UY0H+VVPdPl8FNxjmSZmnr7djgqUS1YBrCr1tbo6crR44cOXLkyFEN/wKoMFyFEAGpAQAAAABJRU5ErkJggg==>

[image2]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABYAAAAaCAYAAACzdqxAAAABwUlEQVR4Xu2UP0tDMRTFW1RQUESwSp9t076ipZNDV8HFRQRxEBx0EJ11Ee0n6BdwEbQgDiLSpUOlQx0cBb+Agw6K4OZQcHBRf7fNk5Dafw8RBA8ckpybe9Lc3NdA4B+/CqXUfDweP2G8jcVij7DI/NCj67pTbAvaeW1BchqzZcZT+MEh67LW2qZosMABw3ZuRyB5X0xsnQOORWc8YNlrx1siFAoNknzVxDirf/V9IpEYt+MtEY1GF3XyjR1De9KxHTvWFiTldHLe1DOZTJ/oUgaZm7G2MMuAwXYkEpnkAZdkjnYHFwJ+ugIDl+Rn+KbqnSFtVtI3qNj7O4ZZXw4Z8XTma6J3XQKBWQa4asYwnkN7h2FT7whGGaq00rQZU/pBu24xQbMy8Hj9aBcSk1uJxjzNw04wluE13NCff6mhXIhH2rggZp5uG2M4gHYOZ6Vr0B8w3WNrkHlYylZLlGsjVLWpyTPPnM0zrF8ZtxjzcFfKomsvxq7s08ZZL883VP0mOW8tpr7ewYaqf+K1Dkomk2Pqm78BX8DoBVb0l1mm7iv2Hl/A7DKVSg05jjPa0A1+obvlq74/Bfkj6hFzszX/Nj4BxNSJPrppSF0AAAAASUVORK5CYII=>

[image3]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABYAAAAZCAYAAAA14t7uAAABi0lEQVR4Xu2UPUsDQRCGE6KgKKhoPJJLcrkLCFYW11qI2KSxstNCsBNsFMkvEH+CoGksRDQWFlrZ2Ip/wMYmCPZCChv1GdyVZcBcEj+qvDDM7DufO+xdKtXHvyIIgmq5XD5GP5RKpSZyiX1oJYqiGcLSOi8RJM9SbAV9grzTZF3OhtsQDmnQYEznJiKbzY6SfCtFtI8GNVP8MQxDT/vbguSIxGfkRfvsTZB74ia0vy2KxeKyTdY+uCfj29G+RJC0Z5LrLh/H8aDwTHogtutLhLOG1+Dz2vIarkyjGx3fMdw1uDvEXhO+60ktgm/WQOEYroXkXL4juM8MWXV9FF6Ce+upsPvMeKNzrs/epOu3KyDxyEzb4IsbsrzYcNfik1sVCoVhuDOzngZ6n7AMdlVu9VVQppMpTVFXTm0MyfOcW+gtdB3ZhU5zbmKHEpPP56douGBzOobv+5Pyz5BJOWY8zxuh6AX2gI79ESiao0lN878CCt/RYBvZZA3n6EUd0ysylUplmqLj2tHH3+MDFMlzat3X69MAAAAASUVORK5CYII=>

[image4]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABoAAAAXCAYAAAAV1F8QAAACLUlEQVR4Xu2VsWtUQRDG77gYDCop5DzCvbt9dwceXiHIAwOiYGEhiIIRTJHGKmlsJJAyjfgPREwhWqQQsUmuOLERFLTQJpU2IqjhLEQwHNgJJr+52w1zc+9BLEU/GGb3m5mdnX2z+3K5fx5xHB90zk2JtjaFQrPZPGLJfYMEM0iXJE/Qm9buka9Wq0vY7zEeGzEiBUuWy+Wjek7wN5JclDF6mgWXU3xWkffwkeb7KBaLhzH+JLCNvo88RN5VKpUr2g/uixybn44xXiHmLforeks00rNxe/CJdoz0coNKtc9L0YGjqoUwDnAZR9aHBLOjxPIapVLpEIu80h+Z+VwY+410wjwV+0kkYKFuOPsoiiZ0DLZFX002JFGtVjuN4yU3+JgryBnrB/dZOophgWObTZLkgOdPSbWpDaDhy5ZFlkno0NeZf5fj0n71ev04/GvkB/JMxXYyG0DDO1/THDu+AXc7pxoiBSN3hvEjZBvZRE4Y/1GwQB3H3+gL1haA/amuRO4Y/mf9NI/9brD1CRxu4TCvSZymkB34y5pXkIWGWpn5Hd0gQ7HhfkC2W63WuHJK4H+pHQ5Bmsd+fDa8ZhINxcoNf0zgSU3KkcG/oQEmNS8QTo7N8nAPMivykGOQx3LNDZ6fj868DAH+GrxoNBrHrE0qxHbT+zF0z61P/wHFcBWZI+H5OOU34I95gwY4Z20B2D8h61TzwZlO/hPk/RM0UmmAXGKqKek38T/+DuwCIEODMCB3KgEAAAAASUVORK5CYII=>