export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
  role: 'Gerente' | 'Conductor'; 
}

// Esta es la info que recibimos del backend al hacer login
export interface UserToken {
  token: string;
  expiration: string;
  email: string;
  role: 'Gerente' | 'Conductor' | '';
}