import React from 'react';
import { useDispatch } from 'react-redux';
import { Typography, Container, Button, Stack } from '@mui/material';
import { Link } from 'react-router-dom';
import {logout} from '../store/actions/user-actions';
import useAuth from '../hooks/use-auth';

const HomePage: React.FC = () => {
  const dispatch = useDispatch();

  const { user } = useAuth();

  const handleLogout = () => {
    dispatch(logout());
  };

  return (
    <Container>
      <Typography variant="h4" gutterBottom>
        Welcome to Home Page
      </Typography>
      <Typography variant="h6">{user?.email ? `Logged in as: ${user?.email}` : 'You didn\'t log in'}</Typography>
      <Stack spacing={2} direction="column">
        <Button variant="contained" component={Link} to="/login">
          Go to Login
        </Button>
        <Button variant="contained" component={Link} to="/register">
          Go to Register
        </Button>
        <Button variant="contained" component={Link} to="/protected">
          Go to Protected Page
        </Button>
        <Button variant="contained" color="secondary" onClick={handleLogout}>
          Logout
        </Button>
      </Stack>
    </Container>
  );
};

export default HomePage;
