import { Injectable } from '@angular/core';
import { Notify } from 'notiflix/build/notiflix-notify-aio';
import { Confirm } from 'notiflix/build/notiflix-confirm-aio';
import { Report } from 'notiflix/build/notiflix-report-aio';

@Injectable({
  providedIn: 'root'
})
export class AlertService {

  constructor() {
    // Inicializamos los toasts (notificaciones pequeñas que aparecen y desaparecen solas)
    Notify.init({
      position: 'right-top',
      timeout: 3000,           
      cssAnimationStyle: 'zoom',
      closeButton: true,       //  botón X
      pauseOnHover: false,     // No espera al mouse
    });

    // Configuracion de los modales de confirmacion
    Confirm.init({
      titleColor: '#7c3aed',             // Color del titulo
      okButtonBackground: '#7c3aed',     // Color del boton aceptar
      cancelButtonBackground: '#3f3f46', // Color del boton cancelar
      backgroundColor: '#1c1c24',        // Fondo del modal
      messageColor: '#fff',               // Color del mensaje
      titleFontSize: '20px',              // Tamano del titulo
      borderRadius: '12px',               // Bordes redondeados
    });

    // Estilo comun para los reportes (exito, fallo, info, advertencia)
    const reportCommonStyle = {
      backgroundColor: '#1c1c24',
      titleColor: '#fff',          // Titulo blanco
      messageColor: '#a1a1aa',     // Mensaje gris
      buttonBackground: '#7c3aed', // Boton morado
      buttonColor: '#fff',
      backOverlayColor: 'rgba(0,0,0,0.6)',
      borderRadius: '12px',
    };

    // Inicializacion de los reportes, separamos por tipo
    Report.init({
      backgroundColor: '#1c1c24',
      borderRadius: '12px',
      backOverlayColor: 'rgba(0,0,0,0.6)',

      success: {
        ...reportCommonStyle,
        svgColor: '#10b981', // Icono verde para exito
      },
      failure: {
        ...reportCommonStyle,
        svgColor: '#ef4444', // Icono rojo para fallo
      },
      warning: {
        ...reportCommonStyle,
        svgColor: '#f59e0b', // Icono amarillo para advertencia
      },
      info: {
        ...reportCommonStyle,
        svgColor: '#3b82f6', // Icono azul para info
      },
    });
  }

  // Atajos para mostrar notificaciones simples
  success(msg: string) { Notify.success(msg); }
  error(msg: string) { Notify.failure(msg); }
  warning(msg: string) { Notify.warning(msg); }
  info(msg: string) { Notify.info(msg); }

  // Metodo para mostrar un modal de confirmacion y esperar la respuesta
  confirm(title: string, message: string, okText: string = 'Si', cancelText: string = 'Cancelar'): Promise<boolean> {
    return new Promise((resolve) => {
      Confirm.show(
        title,
        message,
        okText,
        cancelText,
        () => resolve(true),
        () => resolve(false)
      );
    });
  }

  // Metodo para mostrar reportes segun el tipo
  report(title: string, message: string, type: 'success' | 'failure' | 'warning' | 'info' = 'info') {
    if (type === 'success') Report.success(title, message, 'Entendido');
    else if (type === 'failure') Report.failure(title, message, 'Cerrar');
    else if (type === 'warning') Report.warning(title, message, 'Ok');
    else Report.info(title, message, 'Ok');
  }
}
