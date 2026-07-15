# STA — Sistema de Transferência de Arquivos

<div align="center">

![Status](https://img.shields.io/badge/status-ativo-3DDC84?style=flat-square)
![Cobertura](https://img.shields.io/badge/cobertura-72%2F72%20testes-3DDC84?style=flat-square&logo=xunit&logoColor=white)
![Fase](https://img.shields.io/badge/fase-5.3%20%E2%9C%93%20%E2%86%92%206-FF6B6B?style=flat-square)
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
# 1. Banco de dados
docker compose up -d postgres

# 2. Criar tabelas (migrations EF Core)
cd src/STA.Worker
dotnet ef database update

# 3. Rodar o Worker
dotnet run

# 4. Rodar os testes
dotnet test STA.sln
```

> **💡 Ambientes:** em *Desenvolvimento* lê `appsettings.Development.json` (credenciais locais, gitignored). Em *Produção* usa variáveis de ambiente (`STA_DB_CONN`, etc).

## 📁 Estrutura do projeto

```
src/
├── STA.Core/          # Domínio, entidades, repositórios, serviços
├── STA.Worker/        # Serviço Windows + migrations + Program.cs
├── STA.Api/           # API REST (Fase 6)
tests/
└── STA.Tests/         # 72 testes unitários e de integração
docker-compose.yml     # PostgreSQL para desenvolvimento
STA.sln                # Solução
```

## 💡 Sobre o projeto

Migração de VB.NET Framework 2.0 → .NET 10. Refatoração incremental, testes crescendo junto com features, sem reescrita big-bang.

## 📜 Licença

Privado. Trabalho original.
