export interface User {
  id: string;
  name: string;
  email: string;
}

export interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
}

export interface LoginPayload {
  email: string;
  password: string;
}

export interface LoginAction {
  type: 'LOGIN_SUCCESS';
  payload: User;
}

export interface LogoutAction {
  type: 'LOGOUT';
}

export type AuthActionTypes = LoginAction | LogoutAction;
