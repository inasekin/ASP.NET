const express = require('express');
const cors = require('cors');
const bodyParser = require('body-parser');

const app = express();
const PORT = 5050;

app.use(cors());
app.use(bodyParser.json());

app.post('/api/login', (req, res) => {
    const { username, password } = req.body;

    if (username === 'admin' && password === 'password') {
        return res.status(200).json({ message: 'Вход в систему прошел успешно' });
    }
    return res.status(401).json({ message: 'Неверные учетные данные' });
});

app.listen(PORT, () => {
    console.log(`Сервер запущен на http://localhost:${PORT}`);
});
