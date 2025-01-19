import React from 'react';
import { TextField, Button, Container, Typography } from '@mui/material';

const RegistrationPage: React.FC = () => (
  <Container>
    <Typography variant="h5">Registration Page</Typography>
    <TextField label="Name" fullWidth margin="normal" />
    <TextField label="Email" fullWidth margin="normal" />
    <TextField label="Password" type="password" fullWidth margin="normal" />
    <Button variant="contained" color="primary">Register</Button>
  </Container>
);

export default RegistrationPage;
