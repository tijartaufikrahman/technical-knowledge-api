<p align="center">
  <img src="https://raw.githubusercontent.com/devicons/devicon/master/icons/dot-net/dot-net-original.svg" alt="ASP.NET Core Web API" width="70" height="70" />
</p>

<h1 align="center">Technical Report Knowledge Management - RESTful Web API</h1>

<p align="center">
  <b>High-performance ASP.NET Core 8 Web API backend for Technical Report Knowledge Management, providing RESTful endpoints, JWT authentication, SQL Server Entity Framework Core, role-based authorization, and AI microservice integration.</b>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/ASP.NET_Core-Web_API-512BD4?style=flat&logo=dotnet&logoColor=white" alt="ASP.NET Core Web API" />
  <img src="https://img.shields.io/badge/Entity_Framework-Core_8.0-512BD4?style=flat&logo=dotnet&logoColor=white" alt="EF Core 8" />
  <img src="https://img.shields.io/badge/Microsoft_SQL_Server-Enterprise-CC292B?style=flat&logo=microsoftsqlserver&logoColor=white" alt="SQL Server" />
  <img src="https://img.shields.io/badge/JWT-Authentication-000000?style=flat&logo=jsonwebtokens&logoColor=white" alt="JWT" />
  <img src="https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=flat&logo=swagger&logoColor=black" alt="Swagger" />
</p>

---

## 📌 Overview

**Technical Knowledge Web API** serves as the central data engine and orchestration layer of the Technical Report Knowledge Management System. It encapsulates core business logic, relational persistence, secure JWT authentication with Role-Based Access Control (RBAC), and communication with the AI RAG Microservice for vector embeddings and conversational inferences.

Built on **ASP.NET Core 8.0** and **Entity Framework Core 8**, this backend implements the **Repository & Unit-of-Work pattern** to maintain separation of concerns, strict testability, and high database query performance.

---

## 🚀 Key Features

- **Robust RESTful Architecture:** Clean separation using Controller-Service-Repository pattern.
- **Enterprise Security & JWT Authentication (`AuthController`):**
  - Symmetric key JWT token generation and validation.
  - Supports both `Authorization: Bearer <token>` HTTP headers and secure HTTP-only cookies.
  - Granular RBAC authorization attributes guarding administrative endpoints.
- **Incident & Technical Report Management (`ReportsController`):**
  - Full CRUD operations for technical incident reports and maintenance logs.
  - Automated report numbering, severity classification, status workflows, and PDF attachment handling.
  - Aggregated dashboard statistics and historical incident trends.
- **Document Repository & Synchronization (`DocumentsController`):**
  - Binary file storage, metadata tagging, and direct ingestion dispatch into the AI vector store.
- **AI Knowledge Assistant Integration (`KnowledgeController` & `AiRagService`):**
  - Resilient `HttpClient` communication connecting web clients to the Python FastAPI AI RAG service.
  - Forwards semantic queries, retrieves ranked incident matches, and relays generative answers.
- **Dynamic RBAC & Navigation Management:**
  - `RolesController`, `MenusController`, `MenuRolesController`, and `GroupsController`.
  - Configurable permission matrix for dynamic client-side menu rendering.
- **Resilient Database Operations:**
  - Microsoft SQL Server integration with automatic connection retry resiliency on transient failures.

---

## 🛠️ Technology Stack

| Component | Technologies |
|---|---|
| **Framework** | ASP.NET Core 8.0 Web API |
| **Language** | C# 12 |
| **ORM & Database** | Entity Framework Core 8, Microsoft SQL Server |
| **Authentication** | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| **API Documentation** | OpenAPI / Swagger (`Swashbuckle.AspNetCore`) |
| **Microservice Client** | Typed `HttpClient` (`IHttpClientFactory`) |
| **Mailing / Notifications**| MailKit integration |

---

## 📂 Project Architecture

```plaintext
RestAPISystemKnowledge/
├── Controllers/              # API Action Controllers
│   ├── AuthController.cs     # Login, registration, token verification
│   ├── DocumentsController.cs# Document metadata & PDF attachment APIs
│   ├── GroupsController.cs   # Organizational group CRUD
│   ├── KnowledgeController.cs# RAG & AI search proxy endpoints
│   ├── MenuRolesController.cs# RBAC menu-permission matrix endpoints
│   ├── MenusController.cs    # Application navigation menus
│   ├── ReportsController.cs  # Technical report lifecycle & metrics
│   ├── RolesController.cs    # User roles administration
│   └── UsersController.cs    # User profile & account management
├── Data/
│   └── AppDbContext.cs       # Entity Framework Core database context & mappings
├── Interfaces/               # Repository & service contract abstractions
├── Models/                   # Entity domain models & Request/Response DTOs
├── Repositories/             # Data access repository implementations
├── Services/                 # Business logic, JWT utilities, AI Microservice client
├── Properties/
│   └── launchSettings.json   # Kestrel local network profiles
├── appsettings.json          # Database connection strings & JWT configuration
└── Program.cs                # Dependency injection, middleware pipeline & CORS
```

---

## 🔌 API Endpoints Summary

| Method | Endpoint | Description | Auth Required |
|---|---|---|:---:|
| `POST` | `/api/auth/login` | Authenticate user & issue JWT | No |
| `GET` | `/api/auth/profile` | Get current authenticated user profile | Yes |
| `GET` | `/api/reports` | Get paginated technical incident reports | Yes |
| `POST` | `/api/reports` | Create new incident report with attachments | Yes |
| `GET` | `/api/reports/{id}` | Get report detail, actions taken, and history | Yes |
| `GET` | `/api/reports/statistics`| Get KPI summary metrics for dashboard | Yes |
| `GET` | `/api/documents` | Retrieve archived documents & attachments | Yes |
| `POST` | `/api/documents/upload` | Upload PDF technical document | Yes |
| `POST` | `/api/knowledge/ask` | Send inquiry to AI Knowledge Assistant | Yes |
| `POST` | `/api/knowledge/search`| Execute semantic similarity search | Yes |
| `GET` | `/api/users` | List system users | Admin |
| `GET` | `/api/roles` | List available roles | Admin |
| `GET` | `/api/menus/my-menu` | Get dynamic menu tree based on user role | Yes |

---

## ⚙️ Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Microsoft SQL Server (LocalDB, Express, or standard SQL Server)
- Running instance of **[AI RAG Microservice](https://github.com/tijartaufikrahman/technical-knowledge-ai-rag)** (Default: `http://localhost:22004`)

### Configuration

Update your connection string and JWT keys in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SistemKnowledgeDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyWithMinimum32CharactersLong!",
    "Issuer": "TechnicalKnowledgeApi",
    "Audience": "TechnicalKnowledgeApp"
  },
  "AiRagService": {
    "BaseUrl": "http://localhost:22004"
  }
}
```

### Installation & Run

1. **Clone the repository:**
   ```bash
   git clone https://github.com/tijartaufikrahman/technical-knowledge-api.git
   cd technical-knowledge-api
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Run the API:**
   ```bash
   dotnet run
   ```

4. **Explore Swagger UI:**
   Navigate to `https://localhost:21004/swagger` or `http://localhost:21004/swagger` in your browser.

---

## 👤 Author

Developed by **Tijar Taufik Rahman**  
- GitHub: [@tijartaufikrahman](https://github.com/tijartaufikrahman)
- Role: Full Stack Engineer & AI Solutions Engineer
