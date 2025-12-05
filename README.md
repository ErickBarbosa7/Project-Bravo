# 🚛 Proyecto Bravo - Gestión de Flotillas

**Sistema integral para la administración, monitoreo y optimización de vehículos corporativos.**

Permite administrar el mantenimiento de autos, registrar gastos de combustible y monitorear el estado de la flota en tiempo real.

Este proyecto utiliza una arquitectura desacoplada moderna con **.NET 9** para el backend y **Angular 19** para el frontend. Integra procesos automáticos, validaciones avanzadas, cálculos de mantenimiento y monitoreo en tiempo real.

---

## Tecnologías Utilizadas

- **Frontend:** Angular 19  
- **Backend:** .NET 9 (C#)  
- **Base de Datos:** MySQL 8  
- **Infraestructura:** Docker  
---

## Prerrequisitos

Antes de comenzar, asegúrate de tener instalado:
1.  **Docker Desktop**: [Descargar aquí](https://www.docker.com/products/docker-desktop/) (Debe estar corriendo).
2.  **.NET 9.0**: [Descargar aquí](https://dotnet.microsoft.com/download).

---

## Instrucciones de Instalación

Sigue estos pasos para levantar el proyecto en cualquier localmente.

### Paso 1: Levantar la Base de Datos (Docker)

1.  Abre una terminal en la raíz del proyecto (donde está el `docker-compose.yml`).
2.  Ejecuta el siguiente comando para descargar y encender MySQL:
    ```bash
    docker-compose up -d
    ```
    *(Esto creará el contenedor y la base de datos `BravoDB` automáticamente).*

### Paso 2: Configurar y Ejecutar el Backend

1.  Entra a la carpeta del backend:
    ```bash
    cd BravoBack
    ```
2.  Instala la herramienta dotnet:
    ```bash
    dotnet tool install --global dotnet ef
    ```
3.  Si ya la tienes instalada, actualizada (opcional):
    ```bash
    dotnet tool install --global dotnet ef
    ```
4.  Ejecuta las migraciones para crear las tablas:
    ```bash
    dotnet ef database update
    ```
5.  Inicia el servidor:
    ```bash
    dotnet run
    ```

### Paso 3: Ejecutar el Frontend

1.  Abre una **nueva terminal** y entra a la carpeta del frontend:
    ```bash
    cd BravoFront
    ```
2.  Instala las dependencias (solo la primera vez):
    ```bash
    npm install
    ```
3.  Corre el servidor de desarrollo:
    ```bash
    ng serve
    ```
4.  Abre tu navegador en `http://localhost:4200`.

---

## Usuario Administrador

El proyecto ya incluye un usuario Administrador(Gerente) preconfigurado en la base de datos.  
Puedes iniciar sesión directamente sin necesidad de registrarte.

- **Email:** b1gerente@bravo.com  
- **Contraseña:** 987654321  

---

## Funcionalidades Clave

### Panel de Gerente (Web)
* **Semáforo Automático:** El sistema revisa el kilometraje de cada auto en tiempo real y te avisa con colores (🟢 Verde / 🔴 Rojo) si ya le toca servicio, para que no tengas que llevar cuentas manuales.
* **Predicción de Gastos:** Una herramienta que revisa cuánto has gastado en el pasado para decirte cuánto dinero deberías apartar para el mantenimiento del próximo mes.
* **Control de Taller:** Flujo completo para mandar autos a reparación, registrar cuánto costó y que el sistema los ponga como "Disponibles" otra vez automáticamente.

### Vista del Conductor
* **Asistente de Viaje:** Si tienes que salir a ruta, el sistema te recomienda cuál es el mejor auto disponible para ahorrar gasolina y evitar los que están por necesitar servicio.
* **Registro de Gasolina:** Los conductores suben su carga de combustible y kilometraje directo desde el celular.
---

**Desarrollado por:** Erick Barbosa
