# Reversed Tetris API

This is the backend API for the Reversed Tetris game, built with ASP.NET Core and SignalR.

## Table of Contents

- [Getting Started](#getting-started)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Docker](#docker)
- [Environment Variables](#environment-variables)
- [Contributing](#contributing)
- [License](#license)

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started)

### Installation

1. Clone the repository:
    ```sh
    git clone https://github.com/yourusername/reversed-tetris-api.git
    cd reversed-tetris-api
    ```

2. Restore the dependencies:
    ```sh
    dotnet restore
    ```

## Running the Application

### Using .NET CLI

1. Build the project:
    ```sh
    dotnet build
    ```

2. Run the project:
    ```sh
    dotnet run
    ```

The API will be available at `http://localhost:5159`.

### Using Docker

1. Build the Docker image:
    ```sh
    docker-compose build
    ```

2. Run the Docker container:
    ```sh
    docker-compose up
    ```

The API will be available at `http://localhost:8080`.

## API Endpoints

### GET /

Returns a simple greeting message.

### GET /roomId

Generates a unique room ID.

### SignalR Hub

The SignalR hub is available at `/MessageHub`. It supports the following methods:

- `JoinRoom(string roomId)`: Allows a user to join a specified room. If the room is empty, the user's status is set to `ReadyToBegin`. If another player is already in the room and ready, the game starts.

- `SendMovement(string data)`: Sends the movement data of a player to other players in the same room. The data includes the previous and new coordinates of the player's piece and its color.

- `ClearRows(int[] rows)`: Notifies other players in the same room to clear the specified rows.

- `RequestContinue()`: Requests to continue the game. If all players in the room are ready, the game continues.

- `GameOver()`: Notifies other players in the same room that the game is over and they have won.

- `NotifyPause()`: Pauses the game for all players in the same room.

## Docker

The project includes a `docker-compose.yml` file for running the application in a Docker container.

## Environment Variables

The application uses environment variables defined in a `.env` file. The `docker-compose.yml` file references these variables.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## License

This project is licensed under the MIT License.