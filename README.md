# UserAnalyzer

UserAnalyzer is a C# tool designed to analyze and compare user records across three environments — **Dev**, **UAT**, and **Production** — by querying Microsoft Entra B2C (via Microsoft Graph), Cosmos DB, and Azure SQL.

## 🌐 Purpose

Ensure consistency and accuracy of user data across your system's environments. This tool helps detect:
- Missing users between environments
- Inconsistencies in user profiles
- Stale or inactive accounts

## 🚀 Features

- Reads user data from:
  - Microsoft Entra B2C via Microsoft Graph API
  - Cosmos DB (Mongo API or Core API)
  - Azure SQL Database
- Compares users across Dev, UAT, and Production
- Outputs differences to console or file
- Designed for monthly scheduled runs (CI/CD or containerized job)

## 🛠️ Tech Stack

- .NET 8
- C#
- Microsoft Graph SDK
- Azure Cosmos SDK
- Azure SQL with Dapper or EF Core
- Docker support for container-based runs
- Visual Studio Code for development

## 📦 Setup Instructions

### 1. Clone the Repository

```bash
git clone https://github.com/jaydavis/UserAnalyzer.git
cd UserAnalyzer
```

### 2. Set Environment Variables

You'll need environment variables for:
- Microsoft Graph credentials (client ID, secret, tenant ID)
- Cosmos DB connection strings
- Azure SQL connection strings for each environment

Example:

```bash
export GRAPH_CLIENT_ID=...
export GRAPH_CLIENT_SECRET=...
export GRAPH_TENANT_ID=...

export COSMOS_DEV_CONN=...
export COSMOS_UAT_CONN=...
export COSMOS_PROD_CONN=...

export SQL_DEV_CONN=...
export SQL_UAT_CONN=...
export SQL_PROD_CONN=...
```

### 3. Build and Run

```bash
dotnet build
dotnet run
```

### 4. Run in Docker (optional)

```bash
docker build -t user-analyzer .
docker run -e GRAPH_CLIENT_ID=... -e COSMOS_DEV_CONN=... user-analyzer
```

## 🗓️ Use Cases

- Detect missing or out-of-sync user records
- Validate environment promotion procedures
- Schedule monthly reports in CI/CD (e.g., GitHub Actions)

## ✅ Roadmap

- [ ] Generate HTML/CSV reports
- [ ] Integrate with GitHub Actions for scheduled runs
- [ ] Email or Slack notifications for drift detection

## 🤝 Contributing

Please, no PR's at this time.

## 📄 License

This project is licensed under the MIT License. See the `LICENSE` file for details.
