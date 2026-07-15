# STA — Sistema de Transferência de Arquivos

<div align="center">

![Status](https://img.shields.io/badge/status-active-3DDC84?style=flat-square)
![Coverage](https://img.shields.io/badge/coverage-72%2F72%20tests-3DDC84?style=flat-square&logo=xunit&logoColor=white)
![Phase](https://img.shields.io/badge/fase-5.3%20%E2%9C%93%20%E2%86%92%206-FF6B6B?style=flat-square)
![Stack](https://img.shields.io/badge/stack-.NET%2010%20%2B%20EF%20Core%20%2B%20Postgres-512BD4?style=flat-square&logo=.net&logoColor=white)

</div>

<br>

> Serviço que move arquivos entre servidores de produção. Roda 24/7, dorme entre ciclos, acorda, transfere, dorme de novo.

<br>

<p align="center">
  <img src="./sta-dashboard.svg" width="100%">
</p>

<br>

## 🚀 Subindo o ambiente

```bash
# 1. Postgres
docker compose up -d postgres

# 2. Schema (Worker cria as tabelas via EF migrations)
cd src/STA.Worker
dotnet ef database update

# 3. Worker rodando
dotnet run

# 4. Validar que tudo funciona
dotnet test STA.sln
```

> **💡 Ambientes:** em *Desenvolvimento* lê `appsettings.Development.json` (credenciais locais, gitignored). Em *Produção* usa env vars (`STA_DB_CONN`, etc).

## 📁 Estrutura

```
src/
├── STA.Core/          # Domínio, entidades, repos, services, models
├── STA.Worker/        # BackgroundService + migrations + Program.cs
├── STA.Api/           # Web API REST (Fase 6)
tests/
└── STA.Tests/         # xUnit, 72 testes
docker-compose.yml     # Postgres dev
STA.sln                # Solução
```

## 💡 Por que esse repo existe

Migração de VB.NET Framework 2.0 → .NET 10 não é trivial. Mas é o tipo de projeto que mostra **disciplina técnica**: refactor incremental, testes crescendo junto com features.

## 📜 Licença

Privado. Trabalho original.
