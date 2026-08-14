# GitHub Daily Report

GitHub Daily Report is a configurable web application for organizations
that use GitHub Projects. It collects project activity through the
GitHub API and webhooks, stores project and status-change data in
PostgreSQL, and provides a dashboard for viewing daily activity and
generating Markdown status reports.

The application is designed to be reusable across GitHub organizations
rather than being tied to a specific company or organization.

> **Current status:** The application has been developed and tested
> locally. A production environment has not been provisioned as part of
> this project.

## Features

-   GitHub OAuth authentication
-   GitHub organization membership verification
-   JWT-based authentication using an HTTP-only cookie
-   Project list based on the authenticated user's project memberships
-   Daily project activity dashboard
-   Summary statistics for project activity
-   Markdown daily report generation
-   Project-level member management
-   Project roles: `Leader` and `Member`
-   Admin-only user and role management
-   Backend-enforced role-based authorization
-   GitHub Projects webhook integration
-   Automatic tracking of GitHub Project status changes
-   Protection against removing the last Leader from a project
-   Loading, error, empty, and confirmation UI states

## Tech Stack

### Backend

-   C# / ASP.NET Core
-   .NET 10
-   PostgreSQL
-   Npgsql
-   JWT Bearer authentication
-   GitHub OAuth
-   GitHub REST API
-   GitHub GraphQL API
-   GitHub Webhooks

### Frontend

-   React 19
-   Vite
-   React Router
-   Tailwind CSS
-   shadcn/ui
-   Recharts
-   Lucide React
-   date-fns

## Project Structure

``` text
GitHub-Daily-Report/
├── backend/
│   ├── DashboardApi/
│   │   ├── Authorization/
│   │   ├── Endpoints/
│   │   ├── Models/
│   │   ├── Repositories/
│   │   ├── Services/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   └── DailyReport.slnx
│
├── frontend/
│   ├── public/
│   ├── src/
│   │   ├── components/
│   │   ├── context/
│   │   ├── lib/
│   │   └── pages/
│   ├── package.json
│   └── vite.config.js
│
└── README.md
```

## How the Application Works

At a high level:

``` text
GitHub Organization
        │
        ├── GitHub OAuth ──────────────┐
        │                              │
        ├── GitHub API ────────────────┤
        │                              ▼
        └── GitHub Webhooks ────── Backend API
                                      │
                                      ├── PostgreSQL
                                      │
                                      ▼
                                React Frontend
                                      │
                                      ▼
                              Daily Reports
```

The GitHub organization is provided through configuration. A different
organization can use the application by creating its own GitHub OAuth
application, API credentials, webhook, and database configuration.

The application code itself is not tied to a particular organization.

## Prerequisites

Before running the application, install:

-   .NET 10 SDK
-   Node.js and npm
-   PostgreSQL
-   A GitHub organization
-   A GitHub OAuth application
-   A GitHub token with the permissions required by the application's
    GitHub API operations

## Configuration

Secrets and environment-specific configuration should not be committed
to Git.

### Backend configuration

The backend reads the following configuration values:

``` text
ConnectionStrings:Postgres

Jwt:Secret
Jwt:Issuer
Jwt:Audience

GitHub:Token
GitHub:WebhookSecret

GitHubOAuth:ClientId
GitHubOAuth:ClientSecret
GitHubOAuth:CallbackUrl
GitHubOAuth:Organization
```

### PostgreSQL

Configure the PostgreSQL connection string:

``` json
{
  "ConnectionStrings": {
    "Postgres": "Host=...;Port=5432;Database=...;Username=...;Password=..."
  }
}
```

The database must contain the tables used by the application, including
users, projects, project memberships, issues, and status changes.

### JWT

The backend uses a symmetric signing key to create and validate JWTs.

Configure:

``` text
Jwt:Secret
Jwt:Issuer
Jwt:Audience
```

The JWT secret should be kept private.

### GitHub OAuth

Configure:

``` text
GitHubOAuth:ClientId
GitHubOAuth:ClientSecret
GitHubOAuth:CallbackUrl
GitHubOAuth:Organization
```

The application requests the `read:org` OAuth scope and verifies that
the authenticated GitHub account is an active member of the configured
organization.

The GitHub OAuth application's callback URL must match
`GitHubOAuth:CallbackUrl`.

### GitHub API

The backend uses:

``` text
GitHub:Token
```

for GitHub API operations that do not use the user's OAuth access token.

### GitHub Webhook

Configure:

``` text
GitHub:WebhookSecret
```

The webhook endpoint is:

``` text
POST /webhooks/github
```

GitHub should be configured to send the relevant Project events to this
endpoint.

## Running the Backend

From the repository root:

``` bash
cd backend/DashboardApi
dotnet restore
dotnet run
```

ASP.NET Core will print the local HTTP/HTTPS addresses when the
application starts.

The repository also contains:

``` text
backend/DashboardApi/DashboardApi.http
```

which can be used for manually testing API requests during development.

## Running the Frontend

In a separate terminal:

``` bash
cd frontend
npm install
npm run dev
```

The Vite development server normally runs at:

``` text
http://localhost:5173
```

The frontend expects the backend API URL through:

``` text
VITE_API_URL
```

For local development, for example:

``` text
VITE_API_URL=http://localhost:5016
```

Environment files containing local or deployment-specific values should
not be committed.

## Building the Frontend

Create a production build:

``` bash
cd frontend
npm run build
```

Preview the production build locally:

``` bash
npm run preview
```

Run the frontend linter:

``` bash
npm run lint
```

## Authentication

The application uses GitHub OAuth for login.

The authentication flow is:

``` text
User
 │
 ▼
GitHub OAuth login
 │
 ▼
GitHub authorization
 │
 ▼
Backend exchanges the OAuth code
 │
 ▼
Backend verifies GitHub organization membership
 │
 ▼
User is created or updated in PostgreSQL
 │
 ▼
Backend creates a JWT
 │
 ▼
JWT is stored in an HTTP-only "auth" cookie
 │
 ▼
Frontend accesses authenticated API endpoints
```

Only users who are active members of the configured GitHub organization
can authenticate successfully.

## Authorization

The application has three effective project-level roles.

### Admin

Admins have global administrative permissions.

Admins can:

-   View users
-   Add project Members
-   Add project Leaders
-   Change project member roles
-   Remove project members
-   Manage membership across projects

### Leader

A Leader can manage membership for projects where they are a Leader.

Leaders can:

-   View project members
-   Add Members
-   Remove Members

Leaders cannot:

-   Assign another user as a Leader
-   Remove another Leader
-   Manage membership for projects where they are not a Leader

### Member

Members can access normal project functionality but cannot manage
project membership.

### Backend enforcement

Authorization is enforced by the backend through ASP.NET Core
authorization policies and the custom `AdminOrLeaderHandler`.

Frontend controls such as hiding the "Manage Members" button are only UI
behavior and are not relied upon for security.

## Project Membership

Project membership is stored separately from the global user record.

A project membership contains:

``` text
Project
User
Role
```

Supported project roles are:

``` text
Leader
Member
```

The application prevents a project from being left without a Leader.

For example:

``` text
Project A
├── Alice — Leader
├── Bob   — Leader
└── Carol — Member
```

Alice can be removed because Bob remains a Leader.

If Alice is the only Leader, attempting to remove her returns a conflict
instead of leaving the project without a Leader.

## Daily Reports

The dashboard provides:

-   Date selection
-   Daily project changes
-   Summary statistics
-   Member filtering for Leaders
-   Copyable daily reports

Reports are generated by the backend as Markdown.

Example:

``` markdown
## Alice

### Yesterday

**Done**

- **repo#123**: Implement user authentication
- **repo#127**: Fix project membership validation

### Today

**In progress**

- **repo#130**: Add daily report filtering
```

If a day has no reportable changes, that day is omitted from the
generated report instead of displaying an empty `Yesterday` or `Today`
section.

## GitHub Webhooks

The backend exposes:

``` text
POST /webhooks/github
```

The endpoint verifies the `X-Hub-Signature-256` header using the
configured webhook secret.

The application handles GitHub Project events and uses them to keep
project activity data up to date.

When a relevant Project item is created, the application resolves the
associated GitHub Issue and stores information about it.

When a Project item's Status field changes, the application records the
status transition and the associated user.

For deployment, the webhook endpoint must be publicly reachable by
GitHub, normally through an HTTPS URL.

## API Overview

### Authentication

``` text
GET  /auth/github/login
GET  /auth/github/callback
GET  /auth/me
POST /auth/logout
```

### Projects and reports

``` text
GET /api/projects
GET /api/projects/{projectKey}/changes
GET /api/projects/{projectKey}/summary
GET /api/projects/{projectKey}/report
GET /api/projects/{projectKey}/membership
```

### Project membership

``` text
GET    /api/projects/{projectKey}/members
GET    /api/projects/{projectKey}/members/available-users
POST   /api/projects/{projectKey}/members
DELETE /api/projects/{projectKey}/members/{userId}
```

### Admin

``` text
GET   /api/admin/users
PATCH /api/admin/projects/{projectKey}/members/{userId}
```

### GitHub webhook

``` text
POST /webhooks/github
```

## Development Workflow

Start the backend:

``` bash
cd backend/DashboardApi
dotnet run
```

Then, in another terminal, start the frontend:

``` bash
cd frontend
npm run dev
```

The frontend communicates with the backend using `VITE_API_URL`.

## Production / Deployment Notes

The application has been tested locally, but a production environment
has not been provisioned as part of this project.

A deployment for another organization would require
organization-specific configuration for:

-   PostgreSQL
-   JWT secrets
-   GitHub OAuth application
-   GitHub API token
-   GitHub webhook secret
-   GitHub OAuth callback URL
-   GitHub organization name
-   Frontend/backend URLs
-   Backend CORS allowed origin

Production secrets should be supplied through the server's configuration
or environment rather than committed to Git.

The GitHub webhook also requires a publicly accessible HTTPS endpoint.

## Known Limitations and Future Improvements

Possible future improvements include:

-   Automated database migrations/setup
-   Automated backend and frontend tests
-   CI/CD pipeline
-   Production logging and monitoring
-   More comprehensive API error handling
-   Configuration-driven CORS settings
-   Additional GitHub Project event handling
-   More extensive database/setup documentation
-   Deployment documentation for a specific hosting environment

## Current Project Status

The application currently includes:

-   GitHub OAuth authentication
-   GitHub organization membership verification
-   Project access control
-   Daily project activity reports
-   Markdown report generation
-   Admin and Leader authorization
-   Project membership management
-   Project role management
-   Last-Leader protection
-   GitHub Project webhook processing
-   Frontend loading and error states
-   Delete confirmation dialogs
-   User/member management UI

The project is designed as a reusable GitHub organization/project
reporting application rather than an application tied to a single
company.
