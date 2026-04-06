# Project SDLC Plan

## 1. Goal

Build a small full-stack personal website with:

- public homepage
- project list
- project detail
- admin login
- admin project management

This project has two purposes:

1. be a usable portfolio-style website
2. give me a clean project to practice full-stack development with C# and Blazor

## 2. First Version Scope

Only build the necessary features first.

### In Scope

- homepage
- projects page
- project detail page
- login
- logout
- admin dashboard
- create project
- edit project
- delete project
- protected admin access
- basic form validation

### Not Needed Yet

- registration
- comments
- likes
- search
- advanced analytics
- multiple user roles
- cloud upload

## 3. Users

### Visitor

- can open homepage
- can view projects
- can open project detail
- can contact me by email link

### Admin

- can log in
- can log out
- can access admin dashboard
- can create project
- can edit project
- can delete project

## 4. Core Pages

- `/`
- `/projects`
- `/projects/[slug]`
- `/login`
- `/admin/projects`
- `/admin/projects/new`
- `/admin/projects/[slug]/edit`

## 5. Core Data

### User

- id
- username
- email
- password

### Project

- id
- title
- slug
- summary
- description
- tech_stack
- github_url
- demo_url
- published
- created_at
- updated_at

## 6. Build Order

Do not build everything at once.

### Phase 1: Auth

- login
- logout
- check current user
- protect admin page

### Phase 2: Public Project Read

- project list
- project detail

### Phase 3: Admin Project Write

- create project
- edit project
- delete project

### Phase 4: Frontend Integration

- connect frontend forms to backend
- show real data on pages
- handle error states

### Phase 5: Core Management Flow

- login
- create project
- edit project
- delete project

## 7. Tech Stack

### Backend

- ASP.NET Core Web API
- C#
- cookie-based auth or ASP.NET Core Identity later if needed

### Frontend

- Blazor Web App

### Database

- SQLite first

## 8. Architecture Direction

- frontend renders UI with Blazor components
- backend exposes API endpoints in ASP.NET Core
- frontend calls backend for auth and project data
- admin pages stay protected behind authenticated backend routes
- keep the first version small and server-friendly before adding extra infrastructure

## 9. Immediate Next Step

Start with only one module:

### Module 1: Auth

Do these in order:

1. define auth endpoints
2. build login
3. build logout
4. build current-user check
5. connect Blazor login flow to backend session state

Do not start projects CRUD before auth is stable.
