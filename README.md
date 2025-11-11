## 📘 Overview

The app is live

<img width="1892" height="833" alt="image" src="https://github.com/user-attachments/assets/bdd7d051-f7d4-4b6c-bf44-9590af1e24df" />
<img width="1110" height="402" alt="image" src="https://github.com/user-attachments/assets/13bf10e0-eeb1-4c23-93d7-d605b8d8c8a6" />


This project is a full-stack HackerNews client built using:
- **Backend**: ASP.NET Core 8 (C#)
- **Frontend**: Angular 18.2.21 (TypeScript + Bootstrap)
- **Cache**: Azure Redis Cache for story persistence and performance

It provides:
- The newest HackerNews stories  
- Search functionality  
- Pagination and responsive UI  
- Caching for improved speed and reduced API calls  

---
## ⚙️ Requirements

### 🔹 Backend — HackerNews.API

**Prerequisites**
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Visual Studio 2022 / VS Code
- Internet access (for HackerNews public API)
- Azure Redis Cache (optional for local, used in production)

**Dependencies**
- ASP.NET Core 8 (Web API)
- Caching layer (`ICache`, `RedisCacheService`)
- Background service (`TopStoriesCacheWarmer`)
- Logging with `ILogger`
- Unit testing with `xUnit` + `Moq`

🧱 Setup Instructions
1️⃣ Clone the Repository

https://github.com/kattaspandana1994/NexTech-HackerNewsFeed-Assessment.git

2️⃣ Backend Setup — HackerNews.API
Run via Visual Studio:

Open the solution NexTech.HackerNews.Feed.sln

Set HackerNews.API as the startup project

Press F5 or click Run

OR run via Command Line:
cd NexTech.HackerNews.Feed.API
dotnet restore
dotnet build
dotnet run


API will be available at:
👉 https://localhost:7078/api/news

✅ Run Unit Tests
dotnet test

# 🧩 Angular HackerNews Client

This project is built with **Angular 18.2.21** and serves as the front-end for the **NexTech HackerNews API**.  
It includes story listing, search functionality, and pagination, using a responsive Bootstrap-based design.

---

## 🚀 Prerequisites

Before running this project, make sure you have the following installed:

| Tool | Minimum Version | Check Command |
|------|------------------|----------------|
| **Node.js** | 20.19.5 | `node -v` |
| **npm** | 10.x or higher | `npm -v` |
| **Angular CLI** | 18.2.21 | `ng version` |

If Angular CLI is not installed globally, install it with:

```bash
npm install -g @angular/cli@18.2.21
```

---

## ⚙️ Environment Setup

### 1️⃣ Clone the Repository
```bash
git clone <your-repo-url>
cd angular-17-app
```

### 2️⃣ Install Dependencies
Install all project dependencies using:
```bash
npm install
```

This downloads and sets up all required packages as defined in `package.json`.

---

## 🌍 Configure Environment Files

Update your environment configuration files to point to the correct API base URL.

### ➤ `src/environments/environment.ts`
```ts
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:7078/api/news'
};
```

### ➤ `src/environments/environment.prod.ts`
```ts
export const environment = {
  production: true,
  apiBaseUrl: 'https://<your-production-api>.azurewebsites.net/api/news'
};
```

> 💡 These environment files are automatically chosen based on the build configuration (`development` or `production`).

---

## 🧠 Setting Up System Environment Variables (PATH)

If you encounter errors such as:
> `'node' is not recognized`  
> `'npm' is not recognized`  
> `'ng' is not recognized`

Follow these steps to configure your system PATH properly:

### 🔹 Step 1 — Find Install Locations

Run these commands in a new **Command Prompt (CMD)**:

```bash
where node
where npm
where ng
```

You should see paths like:
```
C:\Program Files\nodejs\node.exe
C:\Users\<YourUser>\AppData\Roaming\npm\ng.cmd
```

### 🔹 Step 2 — Add to PATH

1. Press **Win + R**, type `sysdm.cpl`, and press **Enter**
2. Go to **Advanced → Environment Variables**
3. Under **User variables**, select `Path` → **Edit**
4. Add the following entries (adjust for your user name):
   ```
   C:\Program Files\nodejs\
   C:\Users\<YourUser>\AppData\Roaming\npm
   ```
5. Click **OK → OK → OK**
6. Restart your terminal or VS Code

### 🔹 Step 3 — Verify the Setup
Run:
```bash
node -v
npm -v
ng version
```

If all commands return version numbers, your setup is complete 🎯

---

## 💻 Run the App Locally

Start the Angular development server:
```bash
ng serve
```

Then open your browser and navigate to:
👉 [http://localhost:4200](http://localhost:4200)

The app will automatically reload when changes are made to source files.

---

## 🧪 Running Unit Tests

Run unit tests using **Karma**:

```bash
ng test
```

You can review test results directly in the terminal.

---

## 📘 Additional Notes

- Always run `npm install` after pulling new changes that modify dependencies.
- Ensure your API endpoint (`apiBaseUrl`) matches your backend host when switching between local and cloud environments.
- Avoid hardcoding URLs; use environment files instead.

---

## ✅ Summary

| Step | Command |
|------|----------|
| Install dependencies | `npm install` |
| Run app locally | `ng serve` |
| Build for production | `ng build --configuration production` |
| Run tests | `ng test` |
| Verify setup | `node -v`, `npm -v`, `ng version` |

---
