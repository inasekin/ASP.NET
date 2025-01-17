import React, { useState } from 'react';
import axios from 'axios';

const LoginForm: React.FC = () => {
    const [username, setUsername] = useState('admin');
    const [password, setPassword] = useState('password');
    const [message, setMessage] = useState('');

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const response = await axios.post('http://localhost:5050/api/login', { username, password });
            setMessage(response.data.message);
        } catch (error) {
            setMessage('Login failed: Invalid credentials');
        }
    };

    return (
        <form onSubmit={handleSubmit}>
            <div>
                <label>Username:</label>
                <input
                    type="text"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                />
            </div>
            <div>
                <label>Password:</label>
                <input
                    type="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                />
            </div>
            <button type="submit">Login</button>
            {message && <p>{message}</p>}
        </form>
    );
};

export default LoginForm;
