# Job Application Tracker with AI Resume Customization (Clean Architecture)

This document outlines the **functional requirements** and initial development guidance for building a **Job Application Tracker** web application. The app should be built with **React + Redux** frontend, **.NET (C#)** backend, and **PostgreSQL** database, and include **AI-powered resume customization features**.

---

## 🧩 Tech Stack

* **Frontend**: React, Redux, TypeScript, TailwindCSS (preferred)
* **Backend**: ASP.NET Core Web API (.NET 8+)
* **Database**: PostgreSQL
* **Authentication**: JWT Bearer Auth
* **AI Integration**: OpenAI (or Azure OpenAI), with prompt templates
* **File Storage**: Local folder or AWS S3

---

## ✅ Core Modules and Features

### 1. User Management

* Register
* Login / Logout
* JWT Authentication
* Forgot Password (Email token reset)
* Roles: Admin, User

---

### 2. Dashboard

* Total applications, status breakdown
* Calendar view of follow-ups
* Recent activities / updates

---

### 3. Job Applications CRUD

Each application includes:

* Job Title
* Company Name
* Location (city, remote, hybrid)
* Job URL
* Status: applied, interviewing, offer, rejected, accepted
* Date applied
* Source (LinkedIn, referral, etc.)
* Tags (e.g., .NET, React, Belgium)
* Contact Person (name, email, phone)
* Notes
* Resume/Cover Letter used (linked from CV manager)
* Attachments (optional)

CRUD operations:

* Add new application
* View application details
* Edit / update status
* Delete application

---

### 4. Company Management

* Create / update / delete companies
* View all jobs linked to a company
* Store contact persons, general notes

---

### 5. Feedback and Follow-ups

* Add feedback entries per application
* Interview notes
* Status history
* Schedule follow-ups (calendar + notification)

---

### 6. Resume Management System

#### a. CV Storage

* Upload multiple CV/resume versions
* Add metadata: tags, description, last updated
* File support: PDF, DOCX, Markdown

#### b. AI CV Customizer

* Input: job post URL or description
* Extract keywords and job requirements using OpenAI
* Match with candidate’s CV and suggest improvements
* Generate customized CV version (editable preview)
* Export to PDF or DOCX

#### c. AI Cover Letter Generator (optional)

* Auto-generate a cover letter based on job description and resume
* Allow manual editing and export

---

### 7. Search and Filtering

* Filter by:

  * Status
  * Tags
  * Source
  * Resume used
  * Company
* Full-text search on job title, notes, company

---

### 8. Notifications

* Email/in-app reminders for:

  * Scheduled follow-ups
  * Pending responses
* Weekly application summary (optional)

---

### 9. Reports and Analytics

* Applications by month
* Offer rate, rejection rate
* Resume performance (how often used, success rate)
* Export data to CSV/PDF

---

### 10. Audit Log

* Track changes made to applications, CVs, feedback
* Timestamp + user ID

---

## 🔐 Security

* JWT Auth
* Role-based Access Control (RBAC)
* Input validation
* File upload sanitization
* Rate limiting for login

---

## 🗂️ Backend API Modules

* AuthController: login, register, reset password
* ApplicationsController: CRUD job apps
* CompaniesController
* FeedbacksController
* ResumesController
* AIController: resume analysis and generation

---

## 🧠 AI Integration Tasks

* Setup OpenAI API key securely in backend
* Define prompt templates for:

  * Job requirement extraction
  * Resume improvement
  * Cover letter generation
* Use `System.Text.Json` or `Newtonsoft.Json` for response parsing
* Token limit handling

---

## 🚀 Dev Setup Guide

### Backend (.NET)

```bash
cd backend
# Configure appsettings.json with PostgreSQL connection
# Run EF Core migrations

> dotnet ef migrations add InitialCreate
> dotnet ef database update

> dotnet run
```

### Frontend (React)

```bash
cd frontend
pnpm install
pnpm dev
```

---

## 🧪 Testing

* Unit Tests: API Controllers, Services
* Integration Tests: Resume generation, feedback updates
* Frontend: React Testing Library / Vitest or Jest

---

## 📦 Optional Future Enhancements

* Browser Extension to auto-import job posts
* LinkedIn/Indeed scraping integration
* OAuth login (Google, GitHub)
* Team collaboration (shared job boards)
* Mobile PWA support

---

## 📁 Deliverables to Developer

* This spec as `requirements.md`
* Access to Git repo
* OpenAI API key (for dev)
* PostgreSQL database URL
