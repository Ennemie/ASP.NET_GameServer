# 🎮 ASP.NET Game Server

### Backend & Game Server for Multiplayer Game

> Một backend/game server được xây dựng bằng **ASP.NET Core**, cung cấp hệ thống quản lý người chơi, vật phẩm, phương tiện, nhiệm vụ, quái vật, giao dịch và xác thực người dùng.

<p align="center">
  <img src="https://img.shields.io/badge/C%23-.NET%209.0-512BD4?style=for-the-badge&logo=dotnet" alt=".NET 9">
  <img src="https://img.shields.io/badge/ASP.NET%20Core-9.0-512BD4?style=for-the-badge&logo=dotnet" alt="ASP.NET Core">
  <img src="https://img.shields.io/badge/Entity%20Framework%20Core-9.0-512BD4?style=for-the-badge" alt="Entity Framework Core">
  <img src="https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=for-the-badge&logo=swagger" alt="Swagger">
  <img src="https://img.shields.io/badge/JWT-Authentication-black?style=for-the-badge" alt="JWT">
</p>

---

## 📌 Tổng quan

**ASP.NET Game Server** là một backend được phát triển bằng **ASP.NET Core MVC/Web API**, hướng tới việc cung cấp các dịch vụ phía server cho một game.

Project tập trung vào việc xây dựng các thành phần backend như:

* 👤 Quản lý người chơi
* 🎒 Quản lý Item
* 🚗 Quản lý Vehicle
* 👾 Quản lý Monster
* ⚔️ Monster Kill
* 📜 Quest System
* 🛒 Purchase System
* 🔐 Authentication & Authorization
* 🗄️ Database Management
* 📡 REST API
* 📖 Swagger / OpenAPI

Kiến trúc project được chia thành các lớp Controller, Model và Data để tách biệt phần xử lý request, dữ liệu nghiệp vụ và database access.

---

# ✨ Features

### 👤 Player Management

Hệ thống quản lý thông tin người chơi với các model và API liên quan đến:

* Player
* Player DTO
* Player Quest
* Monster Kill
* Player-related responses

Các controller chính bao gồm:

```text
PlayerManagerController
```

---

### 🎒 Item System

Backend hỗ trợ quản lý Item và quan hệ giữa Item với các hệ thống khác.

```text
Item
ItemVehicleDTO
ItemManagerController
```

---

### 🚗 Vehicle System

Hệ thống quản lý phương tiện trong game:

```text
Vehicle
ItemVehicleDTO
VehicleManagerController
```

Cho phép backend xử lý các dữ liệu liên quan đến Vehicle và Item.

---

### 👾 Monster System

Backend có các model phục vụ hệ thống quái vật:

```text
Monster
MonsterKill
```

Qua đó có thể quản lý thông tin Monster và lịch sử Monster Kill của Player.

---

### 📜 Quest System

Project có các model:

```text
Quest
PlayerQuest
```

để biểu diễn quest và trạng thái quest của từng player.

---

### 🛒 Purchase System

Hệ thống Purchase được xây dựng với model:

```text
Purchase
```

và controller:

```text
PurchaseManagerController
```

cho phép backend quản lý các giao dịch mua vật phẩm/game content.

---

# 🔐 Authentication

Project sử dụng **JWT Bearer Authentication**.

Authentication pipeline được cấu hình với:

```text
JWT Bearer
     │
     ▼
Token Validation
     │
     ├── Issuer
     ├── Audience
     ├── Lifetime
     └── Signing Key
     │
     ▼
Authorization
```

ASP.NET Core được cấu hình để:

* Xác thực JWT token
* Kiểm tra token expiration
* Kiểm tra issuer
* Kiểm tra audience
* Kiểm tra signing key
* Phân quyền request

---

# 🗄️ Database

Project sử dụng **Entity Framework Core** cho database access.

Các package chính:

```text
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.Sqlite
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.EntityFrameworkCore.Design
Microsoft.EntityFrameworkCore.Tools
```

Database context được tổ chức trong:

```text
Data/
└── ApplicationDbContext.cs
```

Project hiện có hỗ trợ cả:

* SQLite
* SQL Server

thông qua Entity Framework Core.

---

# 📖 Swagger / OpenAPI

API được tích hợp **Swagger** thông qua:

```text
Swashbuckle.AspNetCore
```

Swagger được cấu hình với API documentation:

```text
ServerMinecraft v1
```

và hỗ trợ nhập JWT Bearer token trực tiếp trong Swagger UI để test các endpoint yêu cầu authentication.

Sau khi chạy server, Swagger UI có thể được truy cập tại:

```text
/swagger
```

---

# 🏗️ Architecture

Project sử dụng cấu trúc MVC/Web API với các thành phần chính:

```text
                    Client / Game
                          │
                          │ HTTP Request
                          ▼
                ┌──────────────────┐
                │    Controller    │
                └────────┬─────────┘
                         │
                         ▼
                ┌──────────────────┐
                │      Models      │
                │   DTO / Entity   │
                └────────┬─────────┘
                         │
                         ▼
                ┌──────────────────┐
                │  Entity          │
                │  Framework Core  │
                └────────┬─────────┘
                         │
                         ▼
                    Database
```

ASP.NET Core pipeline hiện xử lý theo thứ tự:

```text
Request
   │
   ▼
Routing
   │
   ▼
Authentication
   │
   ▼
Authorization
   │
   ▼
Controller
   │
   ▼
Entity Framework Core
   │
   ▼
Database
```

---

# 📂 Project Structure

```text
ASP.NET_GameServer/
│
├── Controllers/
│   ├── AssignmentController.cs
│   ├── HomeController.cs
│   ├── ItemManagerController.cs
│   ├── PlayerManagerController.cs
│   ├── PurchaseManagerController.cs
│   └── VehicleManagerController.cs
│
├── Data/
│   └── ApplicationDbContext.cs
│
├── Models/
│   ├── AccountModels/
│   ├── CreateRequest/
│   ├── ViewModels/
│   ├── GameMode.cs
│   ├── Item.cs
│   ├── ItemVehicleDTO.cs
│   ├── Monster.cs
│   ├── MonsterKill.cs
│   ├── Player.cs
│   ├── PlayerDTO.cs
│   ├── PlayerQuest.cs
│   ├── Purchase.cs
│   ├── Quest.cs
│   ├── Vehicle.cs
│   └── ...
│
├── Migrations/
│
├── Properties/
│
├── Views/
│
├── wwwroot/
│   └── data/
│
├── Program.cs
├── Minecraft.csproj
├── Minecraft.sln
├── appsettings.json
└── README.md
```

---

# 🛠️ Tech Stack

| Technology                  | Purpose                     |
| --------------------------- | --------------------------- |
| **C#**                      | Backend programming         |
| **.NET 9**                  | Runtime & framework         |
| **ASP.NET Core**            | Web server / API            |
| **Entity Framework Core 9** | ORM / database access       |
| **SQLite**                  | Local database              |
| **SQL Server**              | Relational database         |
| **JWT**                     | Authentication              |
| **Swagger / OpenAPI**       | API documentation & testing |
| **Razor / MVC**             | Web UI                      |
| **Microsoft Identity**      | User identity management    |

Các package và target framework được khai báo trực tiếp trong `Minecraft.csproj`; project target **.NET 9.0**.

---

# 🚀 Getting Started

## 1. Clone repository

```bash
git clone https://github.com/Ennemie/ASP.NET_GameServer.git
cd ASP.NET_GameServer
```

Project hiện có branch `Server`, vì vậy nếu cần làm việc trực tiếp trên branch này:

```bash
git checkout Server
```

---

## 2. Requirements

Cài đặt:

* **.NET 9 SDK**
* Visual Studio 2022 hoặc Rider
* SQL Server / LocalDB nếu sử dụng SQL Server
* Git

Project target:

```text
.NET 9.0
```

---

## 3. Restore dependencies

```bash
dotnet restore
```

---

## 4. Build project

```bash
dotnet build
```

---

## 5. Run server

```bash
dotnet run
```

Sau khi server khởi động, mở Swagger để kiểm tra API:

```text
/swagger
```

---

# 🔑 Authentication Flow

Client trước tiên thực hiện đăng nhập.

```text
Client
  │
  │ Login
  ▼
Server
  │
  │ Validate credentials
  ▼
JWT Token
  │
  ▼
Client
  │
  │ Authorization: Bearer <token>
  ▼
Protected API
```

JWT token sau đó được gửi trong HTTP Authorization header khi gọi các API yêu cầu đăng nhập.

---

# 🧩 Main Controllers

| Controller                  | Responsibility                   |
| --------------------------- | -------------------------------- |
| `PlayerManagerController`   | Player management                |
| `ItemManagerController`     | Item management                  |
| `VehicleManagerController`  | Vehicle management               |
| `PurchaseManagerController` | Purchase management              |
| `AssignmentController`      | Assignment-related functionality |
| `HomeController`            | Web application / home           |

---

# 🧠 What I Learned

Project này tập trung vào những kỹ năng backend/game-server quan trọng:

* 💻 ASP.NET Core development
* 🔐 JWT Authentication
* 👤 User & Player management
* 🗄️ Entity Framework Core
* 🧱 Database design
* 🔄 Entity relationships
* 📡 REST API development
* 📖 Swagger / OpenAPI
* 🛡️ Authentication & Authorization
* 🧩 MVC architecture
* 🔧 Entity Framework migrations
* 🎮 Backend architecture cho game

---

# 📸 API Demo

<img width="1808" height="912" alt="image" src="https://github.com/user-attachments/assets/236e3ebf-395b-4483-8ce6-87d1159e0432" />

---

# 🔮 Future Improvements

Một số hướng có thể mở rộng:

* [ ] Hoàn thiện API documentation
* [ ] Thêm refresh token
* [ ] Rate limiting
* [ ] Logging & monitoring
* [ ] Docker support
* [ ] CI/CD với GitHub Actions
* [ ] Unit tests
* [ ] Integration tests
* [ ] Redis caching
* [ ] Production database configuration
* [ ] API versioning
* [ ] Health check endpoint

---

# ⚠️ Security Note

**Không nên commit JWT secret thật hoặc credential database vào repository public.**

Trong `appsettings.json` hiện có cấu hình JWT key và connection string được lưu trực tiếp trong file.

Đối với project portfolio/public repository, nên chuyển secret sang:

```text
User Secrets
Environment Variables
Azure Key Vault
Docker Secrets
```

Ví dụ:

```json
{
  "Jwt": {
    "Key": "<YOUR_SECRET_KEY>"
  }
}
```

và không commit secret thật lên GitHub.

> **Nếu key hiện tại đã từng được sử dụng thật, nên rotate/revoke nó.**

---

# 👨‍💻 Author

**Ennemie**

GitHub:

https://github.com/Ennemie

---

# 🔗 Repository

<p align="center">

<a href="https://github.com/Ennemie/ASP.NET_GameServer">
<img src="https://img.shields.io/badge/View%20Source%20Code-GitHub-181717?style=for-the-badge&logo=github" alt="View Source Code">
</a>

</p>

---

<p align="center">
  🎮 <b>ASP.NET Game Server</b>
  <br>
  <i>Building the backend behind the game.</i>
</p>
