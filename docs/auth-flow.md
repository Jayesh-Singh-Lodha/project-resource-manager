# Authentication & Authorization Flow

This document details the authentication and authorization design implemented in the Project & Resource Manager (PRM) system.

---

## 1. Overview

The PRM system uses **JWT (JSON Web Tokens)** for secure, stateless communication between the Console Client and the Web API.

```
┌────────────────┐          (POST /api/auth/login)           ┌──────────────┐
│                ├──────────────────────────────────────────>│              │
│                │                                           │              │
│                │ <─────────────────────────────────────────┤              │
│ Console Client │        (200 OK + JWT Token + Role)        │   Web API    │
│                │                                           │              │
│                │          (Authorization: Bearer)          │              │
│                ├──────────────────────────────────────────>│              │
└────────────────┘                                           └──────────────┘
```

---

## 2. Server Architecture

Authentication logic is cleanly decoupled using Clean Architecture principles:

- **PRM.Core**:
  - `User` entity containing authentication state (`PasswordHash`, `IsActive`, `ForcePasswordChange`).
  - `Role` enum defining the authorization roles (`Admin`, `Manager`, `Employee`).
  - Core interfaces (`IPasswordHasher`, `IJwtTokenService`, `IUserRepository`).
  - Typed domain exceptions (`InvalidCredentialsException`, `AccountInactiveException`).
- **PRM.Application**:
  - `AuthService` orchestrating login logic and password changes.
  - `PasswordValidator` enforcing strong password criteria (min 8 chars, 1 uppercase letter, 1 digit).
- **PRM.Infrastructure**:
  - `PasswordHasher` utilizing **BCrypt.Net-Next** for password hashing.
  - `JwtTokenService` generating JWTs signed using a HMAC SHA256 key with claims.
- **PRM.API**:
  - `AuthController` exposes endpoints.
  - `ExceptionHandlingMiddleware` intercepts domain exceptions and maps them to clean, consistent JSON error responses (`ApiErrorResponse`).

---

## 3. Database Schema

The `users` table holds user credentials and metadata:

- `username` (VARCHAR, Unique Index): Case-insensitive unique identifier for logins.
- `email` (VARCHAR, Unique Index).
- `password_hash` (VARCHAR): BCrypt password hash.
- `role` (VARCHAR): User role (`Admin`, `Manager`, `Employee`).
- `is_active` (BIT): Boolean indicating whether the account can log in.
- `force_password_change` (BIT): Set to `1` (True) for new accounts. Blocks access to features until a password change succeeds.

---

## 4. API Endpoints

### 4.1. Login Endpoint
`POST /api/auth/login` (Anonymous access)

- **Request Body**:
  ```json
  {
    "username": "admin",
    "password": "Admin@1234"
  }
  ```
- **Responses**:
  - `200 OK`: Returns the JWT token, role, user's full name, and whether a password change is forced.
    ```json
    {
      "token": "eyJhbG...",
      "role": "Admin",
      "forcePasswordChange": true,
      "fullName": "Administrator"
    }
    ```
  - `401 Unauthorized`: Bad username or password.
  - `403 Forbidden`: Account is inactive.

### 4.2. Change Password Endpoint
`POST /api/auth/change-password` (Requires Bearer Token)

- **Request Body**:
  ```json
  {
    "newPassword": "MyNewPassword@1",
    "confirmPassword": "MyNewPassword@1"
  }
  ```
- **Responses**:
  - `204 No Content`: Password changed successfully. `force_password_change` is reset to `0` (False) in the database.
  - `400 Bad Request`: Validation failure (e.g. passwords don't match, or password too weak).
  - `401 Unauthorized`: Invalid or expired JWT token.

---

## 5. Console Client Flow

The Console Client is fully independent and behaves as follows:

1. **Bootstrap / Initial Startup**: Shows the main title screen:
   ```
   [1] Login
   [2] Exit
   ```
2. **Login Input**: Prompts for username and password. The password input is masked with `*` character masks.
3. **Authentication Call**: Calls `POST /api/auth/login`.
   - If a connection or server error occurs, shows a warning.
   - If invalid credentials, displays the API error message.
4. **Force Password Change**:
   - If the server returns `forcePasswordChange: true`, the Console immediately routes the user to the `ChangePasswordScreen`.
   - The user *must* provide a new password. If they exit or enter invalid passwords, the flow restarts. They cannot reach the main menus without completing the password change.
5. **Menu Routing**:
   - Depending on the user's role, the console client starts the loop for:
     - `AdminMenuScreen`
     - `ManagerMenuScreen`
     - `EmployeeMenuScreen`
6. **Logout**:
   - The "Logout" menu option clears the in-memory token from `ApiClient` and returns to the initial screen.
