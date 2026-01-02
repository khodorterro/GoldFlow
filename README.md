# GoldFlow – Gold Trading & Wallet Management System (Backend)

GoldFlow is a backend system built with **ASP.NET Core Web API** for managing gold trading operations, and real-time price updates.  
The project is designed using a **layered architecture (Data Access Layer / Business Layer / API)** with a strong focus on clean code, scalability, and real-time communication.

---

## Tech Stack
- C# / ASP.NET Core Web API
- SQL Server
- ADO.NET
- SignalR (real-time communication)
- Layered Architecture (DAL / BLL / API)

---

## Architecture Overview
The system follows a clean separation of concerns:

- **DataAccessLayer**
  - Database access using ADO.NET
  - SQL queries and data models
- **BusinessLayer**
  - Business logic and validations
  - Transaction rules and wallet operations
- **API Layer**
  - RESTful endpoints
  - Controllers handling client requests
- **SignalR Hubs**
  - Real-time updates for gold prices and transactions

---

## Core Features
- Gold price management
-  wallet system (balances, updates)
- Trader and user management
- Gold transactions and history tracking
- Commission handling
- Request workflow (e.g. request to become a trader)
- Real-time updates using **SignalR**
  - Gold price changes
  - Wallet balance updates
  - Transaction notifications

---

## Real-Time Communication (SignalR)
SignalR is used to push live updates to connected clients without polling:
- Live gold price updates
- Instant wallet balance changes
- Transaction status notifications

This allows the system to behave like a real-time trading platform.

---

## Authentication & Security
- This project does **not** use JWT authentication.
- Focus is placed on:
  - Business logic
  - Data consistency
  - Real-time system behavior
- Authentication can be added later as a future enhancement.

---

## Database
<img width="1072" height="564" alt="Screenshot 2026-01-02 152358" src="https://github.com/user-attachments/assets/bd48f998-579c-4546-b9f5-6c33214712d8" />

## Configuration
Connection strings are managed securely:
- `appsettings.json` contains placeholders
- `appsettings.Development.json` contains local configuration (ignored by Git)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_CONNECTION_STRING_HERE"
  }
}
