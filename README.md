# Agar.io-clone
A real-time multiplayer arcade game inspired by Agar.io, built entirely in C# using .NET and Raylib.Features a custom binary protocol, authoritative server architecture, and hardware-accelerated rendering.

![.NET](https://img.shields.io/badge/.NET_8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Raylib](https://img.shields.io/badge/Raylib-FFFFFF?style=for-the-badge&logo=cplusplus&logoColor=black)
![TCP](https://img.shields.io/badge/Protocol-TCP_Binary-blue?style=for-the-badge)

## Key Features
* **Hybrid Networking Protocol**:
    * **JSON** (`System.Text.Json`) for reliable handshake and metadata exchange.
    * **Binary Streams** (`BinaryWriter/Reader`) for high-frequency gameplay updates (minimizing bandwidth).
* **Authoritative Server Architecture**: All physics and collision logic happen on the server to prevent cheating.
* **Multithreading & Concurrency**: Uses `Task.Run` and `async/await` to handle multiple clients simultaneously without blocking the main game loop.
* **Thread Safety**: Implements explicit synchronization using `lock` primitives to manage shared `GameState` across network threads.
* **Smooth Rendering**: Client-side interpolation and dynamic camera zoom based on player size.

## Technologies & Concepts Used
* C# 10 / .NET 8
* Raylib-cs
* System.Net.Sockets
* System.Numerics
* LINQ
* Events & Delegates

## Architecture & System Design
```mermaid
graph LR
    classDef client fill:#e1f5fe,stroke:#01579b,stroke-width:2px;
    classDef server fill:#fff9c4,stroke:#fbc02d,stroke-width:2px;

    Client["Game Client (Raylib)"]:::client

    subgraph Server_Architecture [Server Architecture]
        direction TB
        Listener["TCP Listener"]
        Session["Client Session Task"]
        SharedState["Shared Game State"]
        Engine["Physics Engine"]
        Broadcaster["State Broadcaster"]
        
        Listener -->|Spawns| Session
        Session -->|Writes to| SharedState
        Engine -->|Updates| SharedState
        Broadcaster -->|Reads| SharedState
    end

    Client -->|1. Connect & JSON Handshake| Listener
    Client -->|2. Binary Input Stream| Session
    Broadcaster -->|3. Binary State Broadcast| Client
    class Server_Architecture server
