// Interceptor para agregar el token de autenticacion a todas las peticiones HTTP
import { HttpInterceptorFn } from "@angular/common/http";
import { inject } from "@angular/core";
import { AuthService } from "../../features/auth/services/auth.service";

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    // Inyectamos el servicio de autenticacion
    const auth = inject(AuthService);

    // Obtenemos el token actual (guardado como signal)
    const token = auth.token(); 

    // Si existe token, clonamos la request original y agregamos el header Authorization
    if (token) {
        const authReq = req.clone({
            setHeaders: {
                Authorization: `Bearer ${token}` 
            }
        });
        // Continuamos con la request modificada
        return next(authReq);
    }

    // Si no hay token, enviamos la request tal cual
    return next(req);
}
