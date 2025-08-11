# **Real Weather MCP Server**
*A .NET 8 MCP Server for real-time weather data with current conditions, 3-day forecasts, and weather alerts.*

## 📹 Demo Video

[![Watch the demo](https://img.youtube.com/vi/k3l2gyHSEgE/0.jpg)](https://youtu.be/k3l2gyHSEgE)

## 🚀 Features
* **Current Weather** — Get real-time temperature, description, and humidity for any city.
* **3-Day Forecast** — Min/max temperature, average humidity, and most common conditions per day.
* **Weather Alerts** — Retrieve active alerts/warnings for a given location by name or latitude/longitude.
* **Global Coverage** — Works with cities worldwide.
* **Error Handling** — Clear, user-friendly messages for invalid locations or API issues.

## 🛠️ Tech Stack
* **.NET 8**
* **Microsoft.Extensions.AI.Abstractions** (MCP server tools)
* **OpenWeatherMap API**
* **C#** with async/await
* **Dependency Injection** for logging

## 🧪 Testing with VS Code

**Prerequisites:**
- .NET SDK
- VS Code with GitHub Copilot extension
- OpenWeather API key environment variable set up

Steps to test this MCP server from source code (locally):

1. **Set an environment variable for OpenWeatherMap API key**

```bash
# Linux/macOS
export OPENWEATHERMAP_API_KEY="your_api_key_here"

# Windows (PowerShell)
setx OPENWEATHERMAP_API_KEY "your_api_key_here"
```

or use mine

```powershell
setx OPENWEATHERMAP_API_KEY "4d2d3abd8e2b7877d5fa8f2f8a71271a"
```
Note: This API key is provided for quick testing only and may be rotated. For production use, get your own free key at [OpenWeatherMap](https://openweathermap.org/api).

2. **Clone the repo and open the project folder in VSCode**

```bash
git clone https://github.com/sheruyen/WeatherMcpServer.git
code WeatherMcpServer
```

3. **Install and configure the MCP Server in VS Code**

- Follow the steps shown in the demo video above

4. **Open copilot chat in Agent Mode**

Refer to the video demo if stuck.

Once configured, you can ask Copilot Chat (Agent Mode) any of the implemented functions:
- Current weather
- Weather forecast  
- Weather alerts

It should prompt you to use the `get_current_weather` tool on the `WeatherMcpServer` and show you the results.


## ▶️ Usage Examples

* **Get current weather**

```
Current weather in Morocco
```

* **Get 3-day forecast**

```
Weather forecast for New York, US
```

* **Get weather alerts**

```
Are there any weather alerts currently in Tokyo?
```

## 📚 More Information
Refer to the official documentation for more information on configuring and using MCP servers:
- [Use MCP servers in VS Code (Preview)](https://code.visualstudio.com/docs/copilot/chat/mcp-servers)