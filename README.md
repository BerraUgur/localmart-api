# Localmart API
Localmart API is a robust .NET 7 WebAPI backend for the Localmart e-commerce platform. It provides secure user authentication, product management, order processing, and address management for seamless integration with frontend clients.

## Features

### User Authentication
- Secure login and registration
- JWT-based authentication
- Password reset and refresh token support
- Email verification via SendGrid

### Product Management
- Add, update, and delete products
- View detailed product information
- Cloud-based image storage with Cloudinary
- Manage product images and descriptions

### Order Management
- Create and update orders
- Track order status
- Order item management

### Address Management
- Add and update user addresses
- Address validation

### User Management
- Admin endpoints for managing users
- Update user information

### API Documentation
- Interactive API docs with Swagger

### Validation & Error Handling
- Request validation using FluentValidation
- Consistent error responses

### Cloud Services Integration
- **Cloudinary**: Image hosting and transformation
- **SendGrid**: Transactional email delivery

## Development Setup

### Prerequisites
- .NET 7 SDK
- Docker (for database)
- DBeaver or SQL Server Management Studio (for database management)

## Project Structure
```
Basic/WebAPI/
├── appsettings.json         # Main configuration file for API and database
├── Data/                   # Entity Framework database context
├── Endpoints/              # Minimal API endpoint definitions
├── Extensions/             # Extension methods for configuration and services
├── Filters/                # Request validation and error handling filters
├── Middleware/             # Custom middleware (e.g., logging)
├── Migrations/             # Database migration files
├── Models/                 # Entity models (User, Product, Log, etc.)
├── Program.cs              # Main entry point and configuration
├── Security/               # JWT, encryption, and security helpers
├── Services/               # Business logic and service classes
├── Validators/             # FluentValidation validators
├── wwwroot/                # Static files (images, etc.)
```
- **appsettings.json**: API and database configuration.
- **Models/**: Contains all entity classes (User, Product, Log, etc.).
- **Services/**: Business logic for products, orders, users, logging.
- **Endpoints/**: Minimal API endpoint definitions.
- **Middleware/**: Custom middleware (logging, error handling).
- **Migrations/**: Database migration history.
- **Program.cs**: Main API startup and configuration.

### Installation
1. Clone the repository:
```bash
git clone https://github.com/BerraUgur/localmart-api.git
```
2. Navigate to the project directory:
```bash
cd localmart-api/Basic/WebAPI
```
3. Restore dependencies:
```bash
dotnet restore
```

### Running the Application
1. Start the API server:
```bash
dotnet run
```
2. The API will be available at:
```
http://localhost:3000/
```
3. Access Swagger UI for API documentation:
```
http://localhost:3000/swagger/index.html
```

### Database Setup
- Use Docker to run a SQL Server or PostgreSQL instance for development.
- Apply Entity Framework Core migrations:
```bash
dotnet ef database update
```
- Manage and inspect the database with DBeaver or SQL Server Management Studio.

### Building the Application
Run the following command to build the project:
```bash
dotnet build
```

### Testing
Run the following command to execute unit tests:
```bash
dotnet test
```

## Technologies Used
- .NET 7 WebAPI
- Entity Framework Core
- JWT Authentication
- Docker
- DBeaver
- Swagger
- PostgreSQL
- FluentValidation
- **Cloudinary** - Cloud-based image storage
- **SendGrid** - Email delivery service
- **Render.com** - Production deployment

### Environment Variables Required

#### Database
```
PostgreSQL=Host=your_host;Port=5432;Database=your_db;Username=your_user;Password=your_password
```

#### JWT Configuration
```
TokenOptions__Audience=localmart
TokenOptions__Issuer=localmart
TokenOptions__AccessTokenExpiration=100
TokenOptions__SecurityKey=your_secret_key
```

#### Cloudinary (Image Hosting)
Sign up at [cloudinary.com](https://cloudinary.com) - Free tier includes:
- 25 GB storage
- 25 GB monthly bandwidth
- 25,000 transformations per month

```
Cloudinary__CloudName=your_cloud_name
Cloudinary__ApiKey=your_api_key
Cloudinary__ApiSecret=your_api_secret
```

#### SendGrid (Email Service)
Sign up at [sendgrid.com](https://sendgrid.com) - Free tier includes:
- 100 emails per day
- Email verification required for sender address

```
SendGrid__ApiKey=your_sendgrid_api_key
SendGrid__SenderEmail=your_verified_email@example.com
SendGrid__SenderName=Localmart
```

## Contribution
Contributions are welcome! Please fork the repository and submit a pull request.

## License
This project is licensed under the MIT License.
