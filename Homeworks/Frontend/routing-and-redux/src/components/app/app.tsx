import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Provider } from 'react-redux';
import { Container, Typography } from '@mui/material';
import store from '../../store';
import HomePage from '../../pages/home-page';
import LoginPage from '../../pages/login-page';
import NotFoundPage from '../../pages/not-found-page';
import RegistrationPage from '../../pages/registration-page';
import withAuth from '../../hoc/with-auth';

const ProtectedRoute = withAuth(HomePage);

const App: React.FC = () => (
  <Provider store={store}>
    <Router>
      <Container maxWidth="sm">
        <Typography variant="h3" gutterBottom>
            React Redux App
        </Typography>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegistrationPage />} />
          <Route path="/protected" element={<ProtectedRoute />} />
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </Container>
    </Router>
  </Provider>
);

export default App;
