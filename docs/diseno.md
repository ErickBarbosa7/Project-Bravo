# Especificación de Diseño UI/UX: Panel de Gestión de Eventos

Este documento detalla la estructura, los tokens visuales y la composición de los componentes basados en la referencia visual proporcionada (`image_86170f.jpg`). El diseño adopta un enfoque minimalista, destacando el uso de **tarjetas flotantes (floating cards)** sobre un fondo con gradientes suaves, ideal para implementarse con herramientas como React, Tailwind CSS y Framer Motion para las transiciones.

---

## 1. Estilo Visual y Temática (Tokens Core)

El sistema visual transmite limpieza y modernidad, separando claramente la navegación del contenido accionable mediante el uso de sombras suaves (soft shadows) y esquinas redondeadas.

*   **Fondo (Background):** Un gradiente muy sutil y cálido (tonos lavanda/rosado pálido) que se desvanece hacia blanco o gris muy claro. Esto resalta las tarjetas blancas.
*   **Tarjetas (Cards):** Fondo blanco sólido (`bg-white`), bordes sutiles o sin borde, con sombras amplias y difuminadas (`shadow-lg` o `shadow-[0_8px_30px_rgb(0,0,0,0.04)]`) y radios de borde pronunciados (`rounded-2xl` o `rounded-3xl`).
*   **Tipografía:** Familia Sans-serif limpia (estilo Inter, Roboto o similar). Fuerte contraste en los pesos: `font-bold` para títulos e identificadores, y `font-normal` o `text-gray-500` para metadatos y subtítulos.
*   **Modo / Tema:** El diseño actual es predominantemente claro (Light Mode), con acentos oscuros (`bg-black` o `bg-gray-900`) para elementos activos y botones primarios.

---

## 2. Estructura del Layout General

### A. Cabecera y Navegación Principal (Top Nav)
*   **Logo/Branding:** Icono circular oscuro (ej. "TIX") en la esquina superior izquierda.
*   **Contexto del Evento:** Botón de retroceso (`<- BFF Demo`), seguido de metadatos con iconos pequeños: Fecha/Hora, Ubicación (ej. "New York", "Jardín Botánico Nacional").
*   **Navegación Global (Pills):** Una barra central con botones estilo "píldora" (pill-shaped). 
    *   *Estado Activo:* Fondo negro, texto blanco (ej. "Dashboard", "Marketing", "Check-in").
    *   *Estado Inactivo:* Fondo transparente o gris muy claro, texto negro, borde sutil.
*   **Acciones Rápidas:** Enlaces al evento (`tix.do/myevent`), visibilidad ("Public") y menú de usuario/perfil.

### B. Navegación Secundaria (Sub-Tabs)
*   Una fila de pestañas horizontales justo debajo del contexto del evento.
*   Opciones: *Ventas, Participantes, Check-in, Cortesías, Descuentos, Promotores, Marketing*.
*   *Estilo:* Botones ovalados. El tab activo toma el color oscuro principal, los inactivos mantienen fondo claro.

---

## 3. Análisis de Pantallas (Vistas)

### Vista 1: Marketing & Ads (Pantalla Izquierda)
Se divide en un grid asimétrico o layout de mampostería (masonry) con varias tarjetas flotantes.

1.  **Ads Placements (Colocación de Anuncios):**
    *   Selector lateral (Pills verticales) para "Top Banner Placement" y "Page Banner".
    *   Área de descripción, tarifa (ej. "RD $20 / per day").
2.  **Promotion Period:**
    *   Selectores de fecha "Start Date" y "End Date" con iconos de calendario.
3.  **Upload Images:**
    *   Zonas de "Drag & Drop" (Dropzones) para "Desktop Image" y "Mobile Image", mostrando miniaturas (thumbnails) de las imágenes cargadas y requisitos (1248 x 380 px, JPEG/PNG).
4.  **Start Promotion:**
    *   Un Toggle Switch (interruptor) acompañado de un botón principal negro "Save Promotion".
5.  **ID Tag:**
    *   Tarjeta pequeña para configurar integraciones: "Meta ADS ID" y "Google ADS ID" con inputs de texto o etiquetas de solo lectura.
6.  **Inversión Mercadeo:**
    *   Tarjeta de resumen financiero. Lista de conceptos (Home Page 1st Place, Home Page Banner) y el "Total" con un peso tipográfico mayor.

### Vista 2: Check-in / Staff (Pantalla Derecha)
Enfocada en la gestión de personal y accesos.

1.  **Check-in - Staff (Lista de Usuarios):**
    *   Lista de filas para el staff. Cada fila incluye:
        *   Avatar circular oscuro con icono de usuario.
        *   Correo electrónico (ej. francisco@gmail.com).
        *   Contraseña ofuscada (Password: •••••12345).
        *   Fecha de agregado (Added: Mar 17, 2025).
        *   Acción de eliminar (Icono de papelera).
    *   Botón superior derecho "Agregar Staff" (Negro, píldora).
2.  **Get the Check-In App:**
    *   Tarjeta de promoción para descargar la app móvil. Incluye botones de las tiendas "App Store" y "Google Play".
3.  **Tickets:**
    *   Lista de tipos de boletos (General Admission) con fecha y hora asignada.
    *   Opciones para asignar fechas/horas diferentes para el escaneo de tickets (Select Staff).

---

## 4. Componentes Clave (Sugerencias para Implementación)

Para recrear esta interfaz, se recomienda estandarizar los siguientes componentes base:

*   **FloatingCard:** Un componente contenedor.
    ```jsx
    // Ejemplo de clases Tailwind
    <div className="bg-white rounded-3xl p-6 shadow-[0_10px_40px_-10px_rgba(0,0,0,0.08)]">
        {children}
    </div>
    ```
*   **PillButton:** Para las navegaciones.
    ```jsx
    // Clases dinámicas según el estado
    <button className={`px-5 py-2 rounded-full text-sm font-medium transition-colors ${isActive ? 'bg-black text-white' : 'bg-transparent text-gray-700 border border-gray-200 hover:bg-gray-50'}`}>
        {label}
    </button>
    ```
*   **ToggleSwitch:** Para activar campañas. Un switch estilo iOS.
*   **DataRow / StaffRow:** Un flex container `flex items-center justify-between` para las listas de staff, asegurando que los elementos estén alineados verticalmente.

## 5. Recomendaciones de Interacción
*   Aplicar transiciones suaves (`duration-200 ease-in-out`) en los hover de los botones inactivos.
*   Si se implementa un sistema de temas dinámicos en el futuro, las tarjetas deberían soportar un cambio a `bg-gray-800` y las sombras ajustarse para no verse sucias en un fondo oscuro.