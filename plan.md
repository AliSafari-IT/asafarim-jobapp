# Job Application Project Troubleshooting Plan

## Notes

- PostgreSQL database and user were created manually.
- Docker Compose for PostgreSQL exists but Docker is not running; using local PostgreSQL.
- Migrations initially failed due to connection string mismatch (SQLite override in appsettings.Development.json).
- Updated appsettings.Development.json to match production PostgreSQL settings.
- Migrations now apply successfully; schema is created.
- The frontend is running but registration API returns 500 Internal Server Error.
- Backend now configured to use PostgreSQL in all environments.
- Backend process conflict resolved: backend running via pnpm api.
- Registration 500 error likely due to missing "User" role in database; need to verify role seeding.
- Confirmed "User" and "Admin" roles exist in AspNetRoles table; continue investigating 500 error root cause.
- Backend server was stopped; now restarting for further debugging of registration 500 error.
- Backend server is currently running and accessible; registration endpoint is being tested.
- Registration endpoint is returning valid errors (e.g., duplicate email), indicating backend is functional.
- Registration endpoint returns 500 Internal Server Error only for new users; duplicate emails return 400 as expected.
- Login endpoint is returning 500 Internal Server Error for new users; root cause still needs investigation.
- ResumesController/Swagger issues are currently blocking backend server validation and registration testing; must resolve these before confirming registration works end-to-end.
- Backend is experiencing a file lock/build issue (dotnet watch cannot copy backend.exe due to file lock by another process); this must be resolved before backend can start and registration can be tested.
- Attempts to terminate the locking process using taskkill, wmic, and ps commands have failed; backend file lock issue remains unresolved.
- Swagger/OpenAPI fails to load due to ResumesController file upload (IFormFile) configuration; not directly related to registration but blocks API docs.
- Attempted to use FormFileOperationFilter for Swagger file upload support, but this type does not exist; need alternative solution for Swagger and IFormFile compatibility.
- Updated CreateResumeDto to include IFormFile property; need to update ResumesController to use this property.
- Updated appsettings.Development.json to enable detailed/verbose logging for ASP.NET Core Identity and registration debugging.
- Registration 500 error root cause identified: JWT secret key in appsettings.Development.json is too short ("IDX10653: The encryption algorithm ... requires a key size of at least '128' bits. Key ... is of size: '120'."). Update secret key to at least 128 bits (16+ characters).
- JWT secret key updated in appsettings.json and appsettings.Development.json to meet required length.
- Registration with a new user (test6@example.com) still returns 500 error after JWT secret update; further backend investigation required.
- Backend must be restarted to apply configuration changes (JWT secret key); current 500 error likely persists due to file lock preventing reload of new config.
- Immediate priority: resolve backend file lock and restart backend server to apply configuration changes and continue debugging registration/login errors.
- Backend process lock resolved using PowerShell; backend can now be restarted to apply config changes.
- Backend server successfully restarted; ready to test registration and login endpoints.
- JWT secret key must be at least 256 bits (32+ characters) according to latest backend error; secret updated in both appsettings.json and appsettings.Development.json.
- Backend process killed; ready to restart backend server to apply new JWT secret key.
- Backend server restarted and running with updated JWT secret key; ready to test registration and login endpoints.
- Registration attempt for test7@example.com resulted in 400 (Email already exists); preparing new registration for test8@example.com.
- Registration endpoint successfully tested with new user (test8@example.com); received 200 response and JWT token.

## Task List

- [x] Create PostgreSQL user and database manually
- [x] Fix connection string in appsettings.Development.json
- [x] Apply Entity Framework migrations
- [x] Start frontend and backend
- [x] Confirm backend server is running and accessible
- [x] Resolve backend process lock/build error (blocking backend restart)
- [x] Kill backend process to apply new JWT secret key
- [x] Restart backend server to apply updated JWT secret key and config changes
- [ ] Fix ResumesController/Swagger issues blocking backend validation
- [x] Investigate cause of 500 error on /api/auth/register
  - [x] Check and ensure roles (e.g. "User") are seeded in database
  - [x] Diagnose and fix Swagger/OpenAPI documentation error (ResumesController IFormFile)
    - [x] Find alternative solution for Swagger and IFormFile compatibility
      - [x] Update CreateResumeDto to include IFormFile property
      - [x] Update ResumesController to use File property from CreateResumeDto
- [x] Fix backend registration endpoint to work with PostgreSQL
  - [x] Update backend to use PostgreSQL in development mode
- [ ] Confirm user registration works end-to-end
  - [ ] Investigate and fix 500 error on /api/auth/register for new users
    - [x] Check backend logs for detailed error message
    - [x] Examine JwtSettings configuration and AuthController logic
    - [x] Enable detailed logging in appsettings.Development.json for debugging
    - [x] Update JWT secret key in appsettings.json and appsettings.Development.json to be at least 128 bits
    - [x] Update JWT secret key in appsettings.json and appsettings.Development.json to be at least 256 bits (32+ characters)
    - [ ] Investigate backend for additional causes of 500 error after JWT fix
  - [ ] Investigate and fix 500 error on /api/auth/login for new users
- [ ] Test registration and login endpoints to confirm fix
  - [x] Test registration endpoint with new user (test8@example.com)
  - [ ] Test login endpoint with new user

## Current Goal

Test login endpoint to confirm fix
