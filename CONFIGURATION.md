# Configuration Guide

This document explains how to configure the application to handle dynamic ports and different environments.

## Frontend Configuration

### Environment Variables

The frontend uses environment variables to configure the API URL. Create a `.env` file in the `gym-management-ui` directory:

**`.env` (for development):**
```env
REACT_APP_API_BASE_URL=http://localhost:5202/api
```

**`.env.production` (for production):**
```env
REACT_APP_API_BASE_URL=https://your-production-api-url.com/api
```

### Changing Frontend Port

If your React app runs on a different port (e.g., 3001 instead of 3000):

1. **Update the backend CORS configuration** (see below)
2. **Start React on specific port:**
   ```bash
   PORT=3001 npm start
   ```

### Proxy Configuration

The `package.json` includes a proxy configuration as a fallback:
```json
"proxy": "http://localhost:5202"
```

This allows you to use relative URLs like `/api/Auth/Login` during development without CORS issues.

---

## Backend Configuration

### Changing Backend Port

The backend port is configured in `Properties/launchSettings.json`:

```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5202"
    },
    "https": {
      "applicationUrl": "https://localhost:7273;http://localhost:5202"
    }
  }
}
```

**To change the port:**
1. Update the `applicationUrl` in `launchSettings.json`
2. Update the frontend `.env` file with the new port
3. Restart both applications

### CORS Configuration

The backend CORS settings are now configurable via `appsettings.json`:

**`appsettings.json`:**
```json
{
  "CORS": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:3001"]
  }
}
```

**`appsettings.Development.json`:**
```json
{
  "CORS": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:3001", "http://localhost:3002"]
  }
}
```

**To add more allowed origins:**
Simply add them to the `AllowedOrigins` array in the appropriate configuration file.

---

## Common Scenarios

### Scenario 1: React App Runs on Port 3001

1. Update `appsettings.Development.json`:
   ```json
   {
     "CORS": {
       "AllowedOrigins": ["http://localhost:3001"]
     }
   }
   ```

2. Create `.env` file:
   ```env
   REACT_APP_API_BASE_URL=http://localhost:5202/api
   ```

3. Start React:
   ```bash
   PORT=3001 npm start
   ```

### Scenario 2: Backend Runs on Port 5000

1. Update `Properties/launchSettings.json`:
   ```json
   "applicationUrl": "http://localhost:5000"
   ```

2. Update frontend `.env`:
   ```env
   REACT_APP_API_BASE_URL=http://localhost:5000/api
   ```

3. Update `package.json` proxy:
   ```json
   "proxy": "http://localhost:5000"
   ```

### Scenario 3: Production Deployment

1. Update `.env.production`:
   ```env
   REACT_APP_API_BASE_URL=https://api.yourdomain.com/api
   ```

2. Update `appsettings.json` on server:
   ```json
   {
     "CORS": {
       "AllowedOrigins": ["https://yourdomain.com", "https://www.yourdomain.com"]
     }
   }
   ```

3. Build frontend:
   ```bash
   npm run build
   ```

---

## Troubleshooting

### CORS Errors

**Error:** `Access to XMLHttpRequest has been blocked by CORS policy`

**Solution:**
1. Check that your frontend URL is in the `AllowedOrigins` array
2. Restart the backend after changing configuration
3. Clear browser cache

### Port Already in Use

**Error:** `Port 5202 is already in use`

**Solution:**
1. Change the port in `launchSettings.json`
2. Update frontend `.env` file
3. Kill the process using the port (optional)

### API Not Found (404)

**Error:** `GET http://localhost:5202/api/Auth/Login 404 (Not Found)`

**Solution:**
1. Verify backend is running
2. Check the API URL in `.env` file
3. Ensure the route matches the controller name

---

## Quick Start

### Development Mode

1. **Start Backend:**
   ```bash
   cd GymManagmentSystem
   dotnet run
   ```
   Backend will run on `http://localhost:5202`

2. **Start Frontend:**
   ```bash
   cd gym-management-ui
   npm start
   ```
   Frontend will run on `http://localhost:3000`

3. **Access:**
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5202
   - Swagger: http://localhost:5202/swagger

### Custom Ports

If ports conflict, you can use custom ports:

```bash
# Backend (change launchSettings.json first)
cd GymManagmentSystem
dotnet run --urls "http://localhost:5000"

# Frontend (create .env with REACT_APP_API_BASE_URL first)
cd gym-management-ui
PORT=3001 npm start
```

---

## Environment Variables Reference

### Frontend (React)

| Variable | Description | Default |
|----------|-------------|---------|
| `REACT_APP_API_BASE_URL` | Backend API base URL | `http://localhost:5202/api` |
| `PORT` | Frontend development server port | `3000` |

### Backend (.NET)

Configuration is managed through `appsettings.json`:

| Setting | Description | Default |
|---------|-------------|---------|
| `CORS:AllowedOrigins` | Array of allowed frontend URLs | `["http://localhost:3000"]` |
| `ConnectionStrings:DefaultConnection` | Database connection string | (Your SQL Server) |
| `JWT:ValidAudience` | JWT audience | `GymManagementSystem` |
| `JWT:ValidIssuer` | JWT issuer | `GymManagementSystem` |
| `JWT:Secret` | JWT signing key | (Your secret key) |

---

## Notes

- **Always restart both applications** after changing configuration files
- **Environment variables** in React must start with `REACT_APP_`
- **CORS settings** in Development can be more permissive than Production
- **Never commit** `.env` files with sensitive data to version control
- Add `.env` to `.gitignore` to prevent accidental commits

