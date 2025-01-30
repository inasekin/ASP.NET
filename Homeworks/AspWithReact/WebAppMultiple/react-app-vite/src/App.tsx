import { useEffect, useState } from 'react';
import './App.css';

interface WeatherForecast {
    date: string;
    temperatureC: number;
    summary: string;
}

function App() {
    const [forecasts, setForecasts] = useState<WeatherForecast[]>([]);

    useEffect(() => {
        fetch(`${import.meta.env.VITE_API_URL}/weatherforecast`)
            .then(response => response.json())
            .then(data => setForecasts(data))
            .catch(error => console.error('Error fetching weather data:', error));
    }, []);

    return (
        <div className="App">
            <h1>Погода</h1>
            <ul>
                {forecasts.map((forecast, index) => (
                    <li key={index}>
                        {forecast.date}: {forecast.temperatureC}°C - {forecast.summary}
                    </li>
                ))}
            </ul>
        </div>
    );
}

export default App;