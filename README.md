# ?? InstantSync Backend

**A modern, high-performance digital banking backend that solves the trust gap in instant transactions.**

## ?? Problem Statement

**The Trust Gap:** When customers are told they've received money, they expect it in their account balance immediately. Every second of delay, whether due to system downtime or technical glitches, breaks that trust. The core challenge is shielding customers from internal system complexities to make every digital transaction feel instant and completely reliable, even when things go wrong behind the scenes.

## ?? Solution Overview

InstantSync Backend provides a robust, state-driven transaction processing system that ensures customers always have visibility and trust in their money movement, while handling complex interbank operations seamlessly in the background.

### Key Features:
- ? **Instant Response:** Immediate transaction acknowledgment 
- ? **State Management:** Transparent transaction lifecycle tracking
- ? **Trust Building:** Proactive communication and automatic rollback
- ? **Scalable Architecture:** Clean separation of concerns with queue-based processing
- ? **Security First:** JWT authentication with comprehensive validation

## ??? Architecture

### Clean Architecture Layers:
```
?? InstantSyncBackend.WebApi      ? Controllers, Middleware, Configuration
?? InstantSyncBackend.Application ? DTOs, Interfaces, Validation, Business Logic
?? InstantSyncBackend.Persistence ? Services, Repositories, Database Context
?? InstantSyncBackend.Domain      ? Entities, Enums, Business Rules
```

### Technology Stack:
- **Framework:** .NET 8
- **Database:** PostgreSQL with Entity Framework Core
- **Authentication:** JWT with ASP.NET Core Identity
- **Validation:** FluentValidation
- **Logging:** Serilog
- **Email:** MailKit
- **API Documentation:** Swagger/OpenAPI

## ?? Transaction Flow

### Enhanced State Management:
```
INITIATED ? PENDING ? SENT ? COMPLETED
    ?         ?        ?        ?
Customer   Bank     NIP     Final
Request   Checks   Network Settlement
```

### Transaction Types:
- **Deposits:** Add funds from external sources
- **Transfers:** Send money to other accounts (NIP)
- **Withdrawals:** Remove funds from account

### Balance Management:
- **Available Balance:** Money ready to spend
- **Pending Balance:** Funds being processed
- **Automatic Rollback:** Failed transactions restore balances

## ?? API Endpoints

### Authentication
```
POST /api/auth/create-account     - Register new user with banking account
POST /api/auth/signin            - User login
POST /api/auth/reset-password-request - Request password reset
POST /api/auth/reset-password    - Reset password with token
```

### Account Management  
```
GET  /api/account/my-account     - Get current user's account details
GET  /api/account/balance        - Get account balance information
GET  /api/account/user/{userId}  - Get specific user's account (admin)
```

### Transactions
```
POST /api/transaction/send-money - Transfer funds (NIP)
POST /api/transaction/deposit    - Add funds to account  
GET  /api/transaction/history    - Transaction history with filters
GET  /api/transaction/status/{ref} - Real-time transaction status
```

## ??? Database Schema

### Key Entities:

**ApplicationUser** (ASP.NET Identity)
- Id, Email, PhoneNumber, FullName
- AccountNumber (10-digit unique)

**Account** (Banking Account)  
- UserId, AccountNumber, Balance, PendingBalance
- CreatedAt, UpdatedAt

**Transaction** (Transaction Records)
- TransactionReference, AccountId, Amount
- Type (Transfer/Deposit/Withdrawal)
- Status (Initiated/Pending/Sent/Completed/Failed)
- Timestamps for each state
- Beneficiary details for transfers

## ?? Getting Started

### Prerequisites:
- .NET 8 SDK
- PostgreSQL 12+
- Visual Studio 2022 or VS Code

### Installation:

1. **Clone the repository:**
```bash
git clone <repository-url>
cd InstantSyncBackend
```

2. **Setup Database:**
```bash
# Update connection string in appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=InstantSyncDB;Username=postgres;Password=yourpassword"
}
```

3. **Run Migrations:**
```bash
dotnet ef database update --project InstantSyncBackend.Persistence --startup-project InstantSyncBackend.WebApi
```

4. **Configure JWT:**
```json
"Jwt": {
  "Key": "your-super-secret-jwt-key-here-at-least-32-characters",
  "Issuer": "InstantSyncAuthService", 
  "Audience": "InstantSyncAPIClients"
}
```

5. **Run the Application:**
```bash
dotnet run --project InstantSyncBackend.WebApi
```

6. **Access Swagger UI:**
```
https://localhost:7107/swagger
```

## ?? Testing the System

### 1. Create Account:
```bash
POST /api/auth/create-account
{
  "fullName": "John Doe",
  "email": "john@example.com",
  "phoneNumber": "+1234567890", 
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!"
}
```

### 2. Login:
```bash
POST /api/auth/signin
{
  "emailOrPhone": "john@example.com",
  "password": "SecurePass123!"
}
```

### 3. Add Funds:
```bash
POST /api/transaction/deposit
Authorization: Bearer {jwt-token}
{
  "amount": 1000,
  "paymentMethod": "bank-transfer"
}
```

### 4. Send Money:
```bash
POST /api/transaction/send-money  
Authorization: Bearer {jwt-token}
{
  "beneficiaryAccountNumber": "1234567890",
  "beneficiaryBankName": "First Bank",
  "amount": 500,
  "description": "Lunch money"
}
```

### 5. Check Status:
```bash
GET /api/transaction/status/{transaction-reference}
Authorization: Bearer {jwt-token}
```

## ?? Security Features

- **JWT Authentication:** Secure token-based authentication
- **Password Requirements:** Strong password validation  
- **Input Validation:** Comprehensive request validation using FluentValidation
- **CORS Configuration:** Secure cross-origin resource sharing
- **SQL Injection Protection:** Entity Framework parameterized queries
- **Error Handling:** Global exception handling middleware

## ?? Business Value

### For Customers:
- ? **Instant Feedback:** Immediate transaction confirmation
- ? **Trust & Transparency:** Real-time status tracking
- ? **Reliability:** Automatic issue resolution and rollback
- ? **Peace of Mind:** Clear communication throughout process

### For Banks:
- ? **Reduced Support:** Fewer "where's my money?" tickets
- ? **Improved NPS:** Better customer satisfaction scores
- ? **Compliance Ready:** Complete audit trails and logging
- ? **Scalable:** Handle high transaction volumes efficiently

### For Developers:
- ? **Clean Code:** Well-structured, maintainable codebase
- ? **Testable:** Comprehensive interfaces and dependency injection
- ? **Observable:** Detailed logging and monitoring capabilities
- ? **Extensible:** Easy to add new features and integrations

## ?? Performance Optimizations

- **Database Connection Pooling:** Efficient PostgreSQL connections
- **Async Processing:** Non-blocking transaction processing
- **Memory Caching:** Reduced database calls for frequent data
- **Optimized Queries:** Entity Framework query optimization
- **Background Processing:** Async transaction status updates (2-5 seconds)

## ?? Production Considerations

### Environment Configuration:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "production-connection-string"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.production.com",
    "Port": 587,
    "Username": "noreply@yourbank.com"
  }
}
```

### Deployment Checklist:
- [ ] Update connection strings for production database
- [ ] Configure production JWT secrets
- [ ] Setup SMTP for password reset emails  
- [ ] Configure CORS for your frontend domain
- [ ] Enable HTTPS and security headers
- [ ] Setup monitoring and alerting
- [ ] Configure backup strategies

## ?? Support

For technical issues or questions:
- Check the API documentation at `/swagger`
- Review the application logs in the `Logs/` directory
- Ensure all required environment variables are set

---

**InstantSync Backend** - *Making every transaction feel instant and trustworthy* ??