import { useSelector } from 'react-redux';
import { AuthState } from '../types';

const useAuth = () => {
  const userState = useSelector((state: { user: AuthState }) => state.user);
  return {
    isAuthenticated: userState.isAuthenticated,
    user: userState.user,
  };
};

export default useAuth;
