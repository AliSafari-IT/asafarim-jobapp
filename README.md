# Job Application Tracker with AI Resume Customization

A full-stack web application built with React + Redux frontend, ASP.NET Core Web API backend, and PostgreSQL database, featuring AI-powered resume customization.

## 🚀 Quick Start

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for PostgreSQL)
- [.NET 8+ SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/) and [pnpm](https://pnpm.io/)

### 1. Start the Database

```bash
# Start PostgreSQL with Docker Compose
docker-compose up -d postgres

# Optional: Start pgAdmin for database management
docker-compose --profile tools up -d pgadmin
```

### 2. Setup Backend

```bash
cd backend

# Update EF Core tools (if needed)
dotnet tool update --global dotnet-ef

# Apply database migrations
dotnet ef database update

# Run the backend API
dotnet run
```

The API will be available at `https://localhost:5213`

### 3. Setup Frontend

```bash
cd frontend

# Install dependencies
pnpm install

# Start development server
pnpm dev
```

The frontend will be available at `http://localhost:5173`

## 🗄️ Database Access

- **PostgreSQL**: `localhost:5432`
  - Database: `jobappdb`
  - Username: `jobappuser`
  - Password: `jobappuser+123!`

- **pgAdmin** (optional): `http://localhost:8080`
  - Email: `admin@jobapp.com`
  - Password: `admin123`

## 🏗️ Architecture

### Backend (.NET 8)

- **Controllers**: JobApplications, Companies, Resumes, Feedback, Auth
- **Models**: JobApplication, Company, Resume, Feedback, AuditLog
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT Bearer tokens

### Frontend (React + TypeScript)

- **State Management**: Redux Toolkit
- **Styling**: TailwindCSS
- **Build Tool**: Vite

## 🤖 AI Features

- Resume analysis and customization based on job descriptions
- Cover letter generation
- Job requirement extraction
- OpenAI integration for content generation

## 📁 Project Structure

```
├── backend/                 # ASP.NET Core Web API
│   ├── Controllers/         # API endpoints
│   ├── Models/             # Domain models
│   ├── Data/               # DbContext and migrations
│   └── DTOs/               # Data transfer objects
├── frontend/               # React application
│   └── src/                # Source code
├── scripts/                # Database initialization
└── docker-compose.yml     # PostgreSQL setup
```

## 🔧 Development Commands

### Backend

```bash
# Create new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Run tests
dotnet test
```

### Frontend

```bash
# Install dependencies
pnpm install

# Start dev server
pnpm dev

# Build for production
pnpm build

# Run tests
pnpm test
```

### Database

```bash
# Start database
docker-compose up -d postgres

# Stop database
docker-compose down

# Reset database (removes all data)
docker-compose down -v
```

## 🚀 Deployment

The application is ready for deployment to:

- **Backend**: Azure App Service, AWS Elastic Beanstalk, or any .NET hosting
- **Frontend**: Netlify, Vercel, or any static hosting
- **Database**: Azure PostgreSQL, AWS RDS, or managed PostgreSQL

## 🔐 Environment Variables

Create `appsettings.Production.json` for production settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_PRODUCTION_DATABASE_CONNECTION_STRING"
  },
  "Jwt": {
    "Secret": "YOUR_JWT_SECRET_KEY"
  },
  "OpenAI": {
    "ApiKey": "YOUR_OPENAI_API_KEY"
  }
}
```

## 📝 API Documentation

Once the backend is running, visit:

- Swagger UI: `https://localhost:5213/swagger`
- OpenAPI spec: `https://localhost:5213/swagger/v1/swagger.json`

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.
