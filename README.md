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

##  Requerimientos del Sistema

Antes de comenzar, asegúrate de tener instaladas las siguientes herramientas en tu computadora:

1.  **Docker Desktop**: [Descargar aquí](https://www.docker.com/products/docker-desktop/) (Necesario para la base de datos).
2.  **.NET SDK 9.0**: [Descargar aquí](https://dotnet.microsoft.com/download).
3.  **Node.js (v18 o superior)**: [Descargar aquí](https://nodejs.org/) (Necesario para el Frontend).

---
## Configuración Inicial

El proyecto utiliza las siguientes configuraciones por defecto.

* **Base de Datos:** Se levantará en el puerto **3307** (definido en `docker-compose.yml`).
* **Variables de Entorno (Backend):** La cadena de conexión a la base de datos se encuentra en `BravoBack/appsettings.json`.
    * *Nota:* Si necesitas cambiar el usuario o contraseña de la base de datos, edita este archivo.

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

## 🔌 Endpoints de la API

Estos son los más importantes porque hacen los cálculos y automatizaciones del proyecto.

| Endpoint | Método | ¿Qué hace? (Relevancia) |
| :--- | :---: | :--- |
| **`/api/auth/login`** | `POST` | **Seguridad:** Inicia sesión y te da el Token para saber si eres Gerente o Conductor. |
| **`/api/vehiculos/recomendar`** | `GET` | **Algoritmo:** Te dice cuál es el mejor carro para un viaje (el que gasta menos y no se va a descomponer). |
| **`/api/conductores/registrar-uso`** | `POST` | **Automatización:** Al subir el ticket de gasolina, actualiza el kilometraje del carro y revisa si se activó una alerta. |
| **`/api/vehiculos/{id}/estatus-servicio`** | `GET` | **Semáforo:** Calcula en el momento si el auto está en Verde (Bien) o Rojo (Taller) según su uso. |
| **`/api/vehiculos/proyeccion-gastos`** | `GET` | **Predicción:** Revisa el historial de gastos para calcular cuánto presupuesto necesitas el siguiente mes. |

### Endpoints (Operativos)

Estos son los endpoints para administrar los datos (Altas, Bajas y Consultas).

**Usuarios y Accesos**
* `POST /api/auth/register`: Crear nuevos usuarios (Choferes o Gerentes).
* `POST /api/auth/login`: Entrar al sistema.

**Vehículos (Gerente)**
* `GET /api/vehiculos`: Ver la lista de carros.
* `POST /api/vehiculos`: Registrar un carro nuevo.
* `GET /api/vehiculos/{id}`: Ver detalles de un carro.
* `PUT /api/vehiculos/{id}`: Editar datos del carro.
* `DELETE /api/vehiculos/{id}`: Borrar un carro.

**Mantenimiento**
* `POST /api/vehiculos/simular-pago`: Pagar el taller y poner el carro en "Disponible".
* `GET /api/vehiculos/{id}/estatus-servicio`: Consultar el semáforo de un auto.

**Conductores y Reportes**
* `GET /api/conductores`: Ver lista de choferes.
* `GET /api/conductores/{id}/combustible`: Ver cuánto ha gastado un chofer específico.
* `GET /api/conductores/reporte-general`: Ver el rendimiento de toda la flota.

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

## 🎥 Demostración del Proyecto


[![Ver Video en YouTube]()

**Desarrollado por:** Erick Barbosa
