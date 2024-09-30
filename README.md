# API Aggregation Service

## Overview

The **API Aggregation Service** provides a unified interface for accessing data from three external services: 
- **OpenWeatherAPI**
- **NewsAPI**
- **GitHubAPI**

It consolidates this data, enabling developers to retrieve it using sorting and filtering options via a single API endpoint.

---

## Base URL

https://localhost:44325


---

## Authentication

All requests to the API Aggregation Service must be authenticated using an API key. You can obtain the necessary API keys by registering on the following developer portals:

- [OpenWeatherAPI](https://openweathermap.org/)
- [NewsAPI](https://newsapi.org/)
- [GitHubAPI](https://github.com/)



---

## Available Endpoints

### **GET /api/Aggregator/aggregated-data**

#### **Description**

This endpoint aggregates data from the following sources:
- **Weather API**: Retrieves weather data from OpenWeather.
- **News API**: Retrieves news data from multiple sources globally.
- **GitHub API**: Retrieves repository information from GitHub users.

#### **Query Parameters**

| Parameter | Type   | Required | Description                          |
|-----------|--------|----------|--------------------------------------|
| `filter`  | string | No       | Filter results by source (e.g., `weather`, `news`, `github`). |
| `sort`    | string | No       | Sort results by a specific attribute, such as `source` or `date`. |

#### **Sample Request**

```http
GET https://localhost:44325/api/Aggregator/aggregated-data?filter=weather&sort=source
```
#### **Sample Response**
```
[
    { 
        "source": "OpenWeatherMap",
        "category": "Weather", 
        "date": "2024-09-30T09:19:30.0755309Z",
        "data": "City: London, Temperature: 15.31°C, Humidity: 93%, Description: overcast clouds"
    }
]
```

## Caching Mechanism
Each request is cached to improve performance and reduce redundant API calls to external services.

When a request is made, the system checks if a cached response exists for the same request URL.
If a cached response is found, it is returned immediately.
If no cached response exists, the external API is called, and the response is stored in the cache for future requests.
This reduces unnecessary external API calls and optimizes response times.

## Unit Testing
The services have been designed with dependency injection to facilitate easy testing. Mock services have been implemented to simulate real conditions and test various edge cases, ensuring the system behaves correctly under different scenarios. By using mocks for external dependencies, we can thoroughly test each service in isolation, covering both standard use cases and potential failure points. 
