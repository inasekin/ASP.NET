import React, { ComponentType } from 'react';
import { Navigate } from 'react-router-dom';
import useAuth from '../hooks/use-auth';

interface WithAuthProps {
  [key: string]: unknown;
}

const withAuth = <P extends object>(Component: ComponentType<P>) => {
  const WrappedComponent = (props: P & WithAuthProps) => {
    const { isAuthenticated } = useAuth();
    return isAuthenticated ? <Component {...props} /> : <Navigate to="/login" />;
  };

  WrappedComponent.displayName = `WithAuth(${Component.displayName || Component.name || 'Component'})`;

  return WrappedComponent;
};

export default withAuth;
