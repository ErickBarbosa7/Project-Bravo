export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  paternalLastName: string;
  maternalLastName: string;
  role: 'Gerente' | 'Conductor';
}

// Info que recibimos del backend al hacer login
export interface UserToken {
  token: string;
  expiration: string;
  email: string;
  role: 'Gerente' | 'Conductor' | '';
  firstName: string;
  paternalLastName: string;
  maternalLastName: string;
}
