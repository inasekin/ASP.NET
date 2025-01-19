import { useSelector } from 'react-redux';
import { AuthState } from '../types';

const useAuth = () => useSelector((state: { user: AuthState }) => state.user);

export default useAuth;
