import React from 'react';
import { TextField, Button, Container, Typography } from '@mui/material';

const LoginPage: React.FC = () => (
  <Container>
    <Typography variant="h5">Login Page</Typography>
    <TextField label="Email" fullWidth margin="normal" />
    <TextField label="Password" type="password" fullWidth margin="normal" />
    <Button variant="contained" color="primary">Login</Button>
  </Container>
);

export default LoginPage;
