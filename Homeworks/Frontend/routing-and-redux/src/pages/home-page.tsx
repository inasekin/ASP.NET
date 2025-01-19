import React from 'react';
import { Typography, Container, Button, Stack } from '@mui/material';
import { Link } from 'react-router-dom';

const HomePage: React.FC = () => (
  <Container>
    <Typography variant="h4" gutterBottom>
      Welcome to Home Page
    </Typography>
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
    </Stack>
  </Container>
);

export default HomePage;
