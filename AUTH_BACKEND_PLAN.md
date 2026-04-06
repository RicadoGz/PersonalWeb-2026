# Auth Backend Plan

## 1. Feature Goal

Build the smallest C# auth backend module for the personal website.

This module only needs to support:

- login
- logout
- current user check
- admin page protection

Do not add registration yet.

## 2. Why This Is First

Auth should be built first because:

- admin features depend on login
- project create, edit, and delete all depend on auth
- frontend login page should follow backend behavior, not define it

## 3. Scope

### In Scope

- validate username and password
- create login session
- destroy login session
- return current login status
- block unauthenticated access to protected APIs

### Out of Scope

- signup
- forgot password
- OAuth
- email verification
- role system beyond simple admin use

## 4. User Rule

Only the site owner or admin user needs access to protected features.

Visitor:

- cannot log into admin features unless valid credentials are provided

Admin:

- can log in
- can log out
- can access protected APIs after login

## 5. API Contract

## 5.1 POST /api/auth/login

Purpose:

- authenticate user with username and password

Request body:

```json
{
  "username": "demo-admin",
  "password": "safe-password-123"
}
```

Success response:

- status: `200`

```json
{
  "authenticated": true,
  "username": "demo-admin"
}
```

Failure response:

- status: `400`

```json
{
  "authenticated": false,
  "error": "Invalid username or password"
}
```

Backend behavior:

- verify credentials
- create session if valid
- return authenticated user info

## 5.2 POST /api/auth/logout

Purpose:

- clear current login session

Request body:

- empty body allowed

Success response:

- status: `200`

```json
{
  "authenticated": false,
  "message": "Logged out successfully"
}
```

Failure behavior:

- if already logged out, can still return safe response or `401`
- for first version, prefer simple and predictable behavior

## 5.3 GET /api/auth/me

Purpose:

- check current auth state

Success response when logged in:

- status: `200`

```json
{
  "authenticated": true,
  "username": "demo-admin"
}
```

Success response when not logged in:

- status: `200`

```json
{
  "authenticated": false
}
```

## 6. Protection Rule

Any protected admin API should check session first.

If user is not authenticated:

- return `401`

Example:

```json
{
  "error": "Authentication required"
}
```

This rule will be reused later for:

- create project
- edit project
- delete project

## 7. Backend Logic Flow

## 7.1 Login Flow

1. receive username and password
2. validate fields are present
3. check credentials against stored admin user
4. if valid, create auth cookie or session
5. return authenticated response
6. if invalid, return error response

## 7.2 Logout Flow

1. receive logout request
2. clear current session
3. return logged-out response

## 7.3 Current User Flow

1. inspect session
2. if session exists, return authenticated user info
3. if not, return unauthenticated response

## 8. Development Order

Build this feature in this exact order:

1. create auth controller or minimal API endpoints
2. create login endpoint
3. create logout endpoint
4. create current-user endpoint
5. add auth check helper for protected routes
6. connect Blazor login page to these endpoints

Do not build project CRUD before these work.

## 9. Recommended Implementation Notes

- use ASP.NET Core authentication middleware
- keep the first version simple with one admin user
- use cookie auth first because it works well with Blazor and admin flows
- keep response shapes small and predictable for frontend binding

## 10. Definition of Done

Auth backend is done when:

- `/api/auth/login` works
- `/api/auth/logout` works
- `/api/auth/me` works
- protected route returns `401` for guest
- Blazor frontend can read current auth state correctly
