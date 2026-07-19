# TAE-STA — Manual de Deploy

Guia passo-a-passo para instalar e atualizar o TAE-STA em ambientes Windows.

---

## Pré-requisitos

| Software | Versão | Obrigatório | Observação |
|----------|--------|:-----------:|------------|
| Windows Server | 2019+ | ✅ | Ou Windows 10/11 Pro (dev) |
| PostgreSQL | 15+ | ✅ | Pode ser remoto |
| 7-Zip | Latest | ✅ | Worker usa pra compactação |
| IIS | 10+ | ⚠️ | Opcional se usar Kestrel direto |
| .NET 10 Runtime | 10.0+ | ❌ | Não precisa — binários são self-contained |
| Node.js | — | ❌ | Não precisa — frontend é estático |

---

## Instalação Nova

### 1. Baixar os binários

**Opção A — GitHub Actions (recomendado):**
- Acesse: https://github.com/rodrigoericson/Transferencia-de-arquivo-enterprise/actions
- Clique no último workflow verde
- Baixe o artifact `tae-sta-release`
- Extraia em pasta temporária

**Opção B — Build manual:**
```powershell
git clone https://github.com/rodrigoericson/Transferencia-de-arquivo-enterprise.git
cd Transferencia-de-arquivo-enterprise
dotnet publish src/STA.Api -c Release -r win-x64 --self-contained -o publish/api
dotnet publish src/STA.Worker -c Release -r win-x64 --self-contained -o publish/worker
cd src/STA.Web
npm ci
npm run build
cd ../..
Copy-Item src/STA.Web/dist publish/web -Recurse
```

### 2. Executar o instalador

```powershell
# Abra PowerShell como Administrador
cd \caminho\para\binarios
.\scripts\Install-STA.ps1
```

O script vai perguntar:
- Ambiente (Dev/Hml/Prod)
- Host do PostgreSQL
- Usuário e senha do banco
- Senha do admin inicial

### 3. Verificar

```powershell
# Worker rodando?
Get-Service TAE-STA-Worker

# API respondendo?
Invoke-WebRequest http://localhost:5000/health

# Frontend acessível?
Start-Process http://localhost:3000
```

---

## Atualização

### 1. Baixar nova versão

Mesmo processo da instalação (GitHub Actions ou build manual).

### 2. Executar o atualizador

```powershell
# Abra PowerShell como Administrador
.\scripts\Update-STA.ps1 -InstallPath C:\TAE-STA -SourcePath C:\deploy\publish
```

O script automaticamente:
1. Para o Worker e IIS
2. Faz backup dos binários atuais
3. Copia novos binários
4. Preserva configuração existente
5. Aplica migrations pendentes
6. Reinicia serviços
7. Valida (health check)

**Se falhar:** rollback automático restaura a versão anterior.

---

## Estrutura de Instalação

```
C:\TAE-STA\
├── api\                    # Binários da API (self-contained .exe)
├── worker\                 # Binários do Worker (Windows Service)
├── web\                    # Frontend estático (HTML/JS/CSS)
├── config\
│   └── appsettings.json    # Configuração do ambiente
├── logs\                   # Logs da aplicação
└── backup-YYYYMMDD-HHmmss\ # Backups de atualizações anteriores
```

---

## Configuração

Arquivo: `C:\TAE-STA\config\appsettings.json`

| Seção | Chave | Descrição |
|-------|-------|----------|
| ConnectionStrings | StaDb | Connection string PostgreSQL |
| Jwt | Secret | Chave de assinatura JWT (mín 32 chars) |
| Jwt | ExpirationHours | Validade do token (padrão 8h) |
| Ldap | Enabled | true/false — habilita autenticação AD |
| Ldap | Server | Host do AD/LDAP |
| Ldap | BaseDn | Base DN (ex: DC=empresa,DC=local) |
| Ldap | Domain | Domínio (ex: EMPRESA.LOCAL) |
| StaSettings | Arquivo7Zip | Caminho do 7z.exe |
| StaSettings | QtdDiasExcluirLog | Retenção de logs (dias) |
| AllowedOrigins | [0] | URL do frontend (CORS) |
| Logging | LogLevel:Default | Debug/Information/Warning |

### Diferenças por ambiente

| Config | Dev | Hml | Prod |
|--------|-----|-----|------|
| LogLevel | Debug | Information | Warning |
| LDAP | Opcional | Habilitado | Habilitado |
| Swagger | Visível | Visível | Desabilitado |

---

## Banco de Dados

### Criar banco (primeira vez)

```sql
CREATE DATABASE sta;
CREATE USER sta_user WITH PASSWORD 'SENHA_SEGURA';
GRANT ALL PRIVILEGES ON DATABASE sta TO sta_user;
```

### Aplicar migrations

As migrations são aplicadas automaticamente pelo `Install-STA.ps1`. Para aplicar manualmente:

```powershell
$env:STA_DB_CONN = "Host=localhost;Port=5432;Database=sta;Username=sta_user;Password=SENHA"
cd C:\TAE-STA\worker
dotnet ef database update
```

### Backup

```powershell
pg_dump -h localhost -U sta_user -d sta > backup_sta_$(Get-Date -Format yyyyMMdd).sql
```

---

## Windows Service (Worker)

### Comandos úteis

```powershell
# Status
Get-Service TAE-STA-Worker

# Parar
Stop-Service TAE-STA-Worker

# Iniciar
Start-Service TAE-STA-Worker

# Reiniciar
Restart-Service TAE-STA-Worker

# Ver logs (Event Viewer)
Get-EventLog -LogName Application -Source TAE-STA-Worker -Newest 20
```

### Remover serviço

```powershell
Stop-Service TAE-STA-Worker
sc.exe delete TAE-STA-Worker
```

---

## IIS (API + Frontend)

### Configuração manual (se não usar Install-STA.ps1)

**API:**
1. Criar Application Pool `TAE-STA-Pool` (No Managed Code)
2. Criar Site `TAE-STA-API` apontando para `C:\TAE-STA\api` (porta 5000)
3. Variáveis de ambiente no App Pool: `ASPNETCORE_ENVIRONMENT=Production`

**Frontend:**
1. Criar Site `TAE-STA-Web` apontando para `C:\TAE-STA\web` (porta 3000)
2. Adicionar URL Rewrite: todas rotas → `index.html` (SPA fallback)
3. Proxy reverso: `/api/*` → `http://localhost:5000`

---

## Troubleshooting

| Problema | Causa provável | Solução |
|----------|---------------|--------|
| API não inicia | Connection string errada | Verificar `config/appsettings.json` |
| Worker para sozinho | Postgres inacessível | Verificar rede/firewall |
| Login falha | JWT Secret diferente entre API/Worker | Usar mesmo appsettings |
| Frontend em branco | CORS bloqueando | Verificar `AllowedOrigins` |
| 7-Zip falha | Caminho errado | Verificar `StaSettings:Arquivo7Zip` |
| LDAP timeout | Servidor AD inacessível | Verificar porta 636 (LDAPS) |
| Arquivo não transfere | Sem permissão na pasta UNC | Conta do serviço precisa acesso |

### Logs

- **Worker:** Event Viewer → Application → Source `TAE-STA-Worker`
- **API:** `C:\TAE-STA\api\logs\` (se Serilog configurado) ou Event Viewer
- **IIS:** `C:\inetpub\logs\LogFiles\`

---

## Checklist de Validação

- [ ] PostgreSQL acessível na porta 5432
- [ ] 7-Zip instalado e path correto no appsettings
- [ ] Worker rodando (`Get-Service TAE-STA-Worker`)
- [ ] API respondendo (`http://localhost:5000/health`)
- [ ] Frontend carregando (`http://localhost:3000`)
- [ ] Login funcionando (admin + senha configurada)
- [ ] Pastas UNC acessíveis pela conta do serviço
- [ ] Firewall liberado (5000, 3000, 5432)
- [ ] LDAP acessível (se habilitado)

---

## Desinstalar

```powershell
# Parar e remover serviço
Stop-Service TAE-STA-Worker -Force
sc.exe delete TAE-STA-Worker

# Remover sites IIS
Remove-Website -Name TAE-STA-API
Remove-Website -Name TAE-STA-Web
Remove-WebAppPool -Name TAE-STA-Pool

# Remover arquivos
Remove-Item -Recurse -Force C:\TAE-STA

# Banco (opcional)
# DROP DATABASE sta;
```
