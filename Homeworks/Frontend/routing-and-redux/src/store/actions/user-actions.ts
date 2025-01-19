import { User, LoginAction, LogoutAction } from '../../types';

export const loginSuccess = (user: User): LoginAction => ({
  type: 'LOGIN_SUCCESS',
  payload: user,
});

export const logout = (): LogoutAction => ({
  type: 'LOGOUT',
});
