# Gym Management System

A comprehensive gym management system built with ASP.NET Core 8.0 Web API, Entity Framework Core, and JWT Authentication.

## Features

### Core Functionality
- **User Management**: Complete user registration, authentication, and role-based authorization
- **Enquiry Management**: Track potential members from initial inquiry to membership conversion
- **Membership Plans**: Create and manage different gym membership plans
- **Member Subscriptions**: Handle member subscriptions, payments, and renewals
- **Payment Tracking**: Track payments, outstanding amounts, and due dates
- **History Tracking**: Complete audit trail for all enquiries and changes

### API Endpoints

#### Authentication (`/api/Auth`)
- `POST /Login` - User login with JWT token generation
- `POST /Register` - User registration
- `POST /RefreshToken` - Refresh expired JWT tokens
- `POST /Logout` - User logout

#### Enquiry Management (`/api/Enquiry`)
- `GET /` - Get all enquiries
- `GET /{id}` - Get specific enquiry
- `POST /` - Create new enquiry
- `PUT /{id}` - Update enquiry
- `DELETE /{id}` - Delete enquiry
- `POST /{id}/ConvertToMember` - Convert enquiry to member
- `GET /{id}/History` - Get enquiry history

#### Membership Plans (`/api/MembershipPlan`)
- `GET /` - Get all membership plans
- `GET /{id}` - Get specific plan
- `GET /Active` - Get active plans only
- `GET /ByType/{planType}` - Get plans by type
- `POST /` - Create new plan
- `PUT /{id}` - Update plan
- `DELETE /{id}` - Soft delete plan
- `PUT /{id}/Activate` - Activate plan
- `GET /{id}/Members` - Get members for plan
- `GET /Stats` - Get plan statistics

#### Member Subscriptions (`/api/MembersMembership`)
- `GET /` - Get all memberships
- `GET /{id}` - Get specific membership
- `GET /Active` - Get active memberships
- `GET /Expired` - Get expired memberships
- `GET /ExpiringSoon` - Get memberships expiring in 30 days
- `GET /PendingPayments` - Get memberships with pending payments
- `POST /` - Create new membership
- `PUT /{id}` - Update membership
- `POST /{id}/Payment` - Process payment
- `PUT /{id}/Renew` - Renew membership
- `DELETE /{id}` - Delete membership
- `GET /Stats` - Get membership statistics

#### User Management (`/api/User`)
- `GET /` - Get all users
- `GET /{id}` - Get specific user
- `GET /ByEmail/{email}` - Get user by email
- `POST /Register` - Register new user
- `PUT /{id}` - Update user
- `POST /{id}/ChangePassword` - Change password
- `POST /{id}/ResetPassword` - Reset password
- `POST /{id}/AssignRole` - Assign role to user
- `DELETE /{id}/RemoveRole` - Remove role from user
- `GET /{id}/Roles` - Get user roles
- `DELETE /{id}` - Delete user
- `GET /Stats` - Get user statistics
- `GET /Profile` - Get current user profile

## Database Schema

### Core Entities
- **Users**: Gym staff and members with authentication
- **Enquiries**: Potential members who have shown interest
- **MembershipPlans**: Different gym membership options
- **MembersMemberships**: Active member subscriptions
- **EnquiryHistory**: Audit trail for enquiry changes

### Key Relationships
- Users can have multiple roles (Admin, Staff, Member)
- Enquiries can be converted to MembersMemberships
- MembersMemberships link Enquiries to MembershipPlans
- EnquiryHistory tracks all changes to enquiries

## Setup Instructions

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or Express)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd GymManagmentSystem
   ```

2. **Update connection string**
   Edit `appsettings.json` and update the connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=GymManagementDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
     }
   }
   ```

3. **Install packages**
   ```bash
   dotnet restore
   ```

4. **Create database**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI**
   Navigate to `https://localhost:7000/swagger` (or the port shown in console)

### Configuration

#### JWT Settings
Update JWT configuration in `appsettings.json`:
```json
{
  "JWT": {
    "ValidAudience": "GymManagementSystem",
    "ValidIssuer": "GymManagementSystem",
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"
  }
}
```

#### Database Migrations
- Create new migration: `dotnet ef migrations add MigrationName`
- Update database: `dotnet ef database update`
- Remove migration: `dotnet ef migrations remove`

## Usage Examples

### Register a new user
```http
POST /api/Auth/Register
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "gender": "Male",
  "address": "123 Main St",
  "dateOfBirth": "1990-01-01",
  "occupation": "Software Developer"
}
```

### Create a membership plan
```http
POST /api/MembershipPlan
Content-Type: application/json
Authorization: Bearer YOUR_JWT_TOKEN

{
  "planName": "Premium Monthly",
  "planType": "Monthly",
  "durationInMonths": 1,
  "price": 99.99,
  "description": "Premium monthly membership with all facilities",
  "createdBy": "admin@example.com"
}
```

### Process a payment
```http
POST /api/MembersMembership/1/Payment
Content-Type: application/json
Authorization: Bearer YOUR_JWT_TOKEN

{
  "amount": 50.00,
  "paymentMethod": "Credit Card",
  "notes": "Partial payment"
}
```

## Security Features

- **JWT Authentication**: Secure token-based authentication
- **Role-based Authorization**: Different access levels for Admin, Staff, and Members
- **Password Hashing**: Secure password storage using ASP.NET Identity
- **Input Validation**: Comprehensive model validation
- **SQL Injection Protection**: Entity Framework parameterized queries

## Business Logic

### Enquiry to Member Conversion
1. Create enquiry for potential member
2. Track enquiry history for all changes
3. Convert enquiry to member when they join
4. Create membership subscription with payment tracking

### Payment Management
- Track total amount, paid amount, and remaining balance
- Calculate next payment due dates
- Monitor expired and expiring memberships
- Generate payment reports

### Membership Lifecycle
- Create membership plan
- Subscribe members to plans
- Track membership status (active/expired)
- Handle renewals and extensions

## API Documentation

The API includes comprehensive Swagger documentation available at `/swagger` when running in development mode.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License.

## Support

For support and questions, please contact the development team or create an issue in the repository.