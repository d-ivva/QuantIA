# QuantIA

> Sistema de gestão financeira pessoal com autenticação via Keycloak, API REST em .NET 8 e interface web em React.

---

## 📋 Descrição do Projeto

O **QuantIA** é uma aplicação full-stack para controle de finanças pessoais. O sistema permite que cada usuário autenticado gerencie:

- **Contas/Cartões** — múltiplos cartões ou contas bancárias
- **Categorias** — categorias de despesas e receitas com cor customizável
- **Tipos de Transação** — classificações reutilizáveis para transações
- **Transações** — lançamentos de receita e despesa com suporte a parcelamento
- **Metas Mensais** — limites de gastos por mês
- **Relatórios** — fluxo de caixa diário, desempenho mensal e distribuição por categoria
- **IA Assistente** — chat integrado com modelos de linguagem para auxílio financeiro

O projeto está dividido em dois repositórios:

| Repositório | Tecnologia | Porta padrão |
|---|---|---|
| `QuantIA` | ASP.NET Core 8 (Backend API) | `5221` |
| `QuantIA-Front` | React 19 + Vite 8 (Frontend) | `5173` |

---

## ✅ Pré-requisitos

Certifique-se de ter instalado:

| Ferramenta | Versão mínima | Download |
|---|---|---|
| **.NET SDK** | 8.0 | https://dotnet.microsoft.com/download/dotnet/8.0 |
| **Node.js** | 18.x | https://nodejs.org |
| **npm** | 9.x | (incluído com Node.js) |
| **Docker Desktop** | 4.x | https://www.docker.com/products/docker-desktop |
| **Docker Compose** | v2 | (incluído no Docker Desktop) |

**Ferramentas opcionais mas recomendadas:**

```bash
# CLI do Entity Framework Core (para rodar migrations)
dotnet tool install --global dotnet-ef
```

---

## 🚀 Instalação

### 1. Clone os repositórios

```bash
# Backend
git clone https://github.com/seu-usuario/QuantIA.git

# Frontend
git clone https://github.com/seu-usuario/QuantIA-Front.git
```

### 2. Suba a infraestrutura com Docker

Na pasta do **backend**, execute:

```bash
cd QuantIA
docker-compose up -d
```

Isso inicia três serviços:
- **PostgreSQL 16** na porta `5432`
- **pgAdmin 4** na porta `8080`
- **Keycloak 25** na porta `8180`

Aguarde cerca de 30 segundos para o Keycloak terminar de inicializar.

### 3. Configure o Keycloak

1. Acesse `http://localhost:8180`
2. Faça login com `admin` / `admin123`
3. Crie um novo **Realm** chamado `quantia`
4. Dentro do realm, crie um **Client** com:
   - **Client ID:** `quantia-frontend`
   - **Client authentication:** desabilitado (público)
   - **Valid redirect URIs:** `http://localhost:5173/*`
   - **Web origins:** `http://localhost:5173`
5. Crie um usuário de teste e defina uma senha permanente

### 4. Configure e rode o Backend

```bash
cd QuantIA

# Restaure as dependências
dotnet restore

# Aplique as migrations ao banco de dados
dotnet ef database update

# Inicie a API
dotnet run
```

A API estará disponível em `http://localhost:5221`.

### 5. Configure e rode o Frontend

```bash
cd QuantIA-Front

# Instale as dependências
npm install

# Configure as variáveis de ambiente
cp .env.example .env
# Edite o .env se necessário (os valores padrão já apontam para localhost)

# Inicie o servidor de desenvolvimento
npm run dev
```

O frontend estará disponível em `http://localhost:5173`.

---

## 🔐 Variáveis de Ambiente

### Frontend — `QuantIA-Front/.env`

Crie o arquivo copiando o exemplo:

```bash
cp .env.example .env
```

```dotenv
# URL base da API REST do backend
VITE_API_URL=http://localhost:5221/api

# URL do servidor Keycloak
VITE_KEYCLOAK_URL=http://localhost:8180

# Realm configurado no Keycloak
VITE_KEYCLOAK_REALM=quantia

# Client ID registrado no Keycloak
VITE_KEYCLOAK_CLIENT_ID=quantia-frontend
```

> **Atenção:** Em produção, substitua `localhost` pelos endereços reais dos seus servidores.

### Backend — `QuantIA/appsettings.json`

O arquivo `appsettings.json` já existe no repositório. Ajuste conforme seu ambiente:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=quantia;Username=postgres;Password=CHANGE_ME"
  },
  "Keycloak": {
    "Authority": "http://localhost:8180/realms/quantia",
    "ClientId": "quantia-frontend",
    "AdminUrl": "http://localhost:8180",
    "AdminRealm": "master",
    "AdminClientId": "admin-cli",
    "AdminUsername": "admin",
    "AdminPassword": "admin123",
    "Realm": "quantia"
  },
  "Frontend": {
    "Url": "http://localhost:5173"
  }
}
```

> **Importante:** Nunca suba senhas reais no `appsettings.json` em repositórios públicos. Use `appsettings.Development.json` (ignorado pelo git) ou variáveis de ambiente do sistema operacional em produção.

---

## 🗄️ Configuração do Banco de Dados

O projeto usa **PostgreSQL 16** via Docker e **Entity Framework Core 8** com o provider `Npgsql` para gerenciamento do schema.

### Subindo o banco com Docker

```bash
# Na pasta QuantIA
docker-compose up -d postgres
```

### Aplicando as Migrations

Com o banco em execução, aplique todas as migrations para criar as tabelas:

```bash
cd QuantIA
dotnet ef database update
```

### Criando novas Migrations

Ao alterar modelos (`Models/`), gere uma nova migration:

```bash
dotnet ef migrations add NomeDaMigration
dotnet ef database update
```

### Visualizando o banco com pgAdmin

1. Acesse `http://localhost:8080`
2. Login: `admin@admin.com` / `admin123`
3. Adicione um servidor com:
   - **Host:** `postgres` (nome do serviço Docker)
   - **Port:** `5432`
   - **Username:** `postgres`
   - **Password:** `CHANGE_ME`

### Tabelas do Banco

| Tabela | Descrição |
|---|---|
| `Users` | Usuários do sistema (sincronizados com Keycloak) |
| `Accounts` | Contas/cartões financeiros por usuário |
| `Categories` | Categorias de transações com cor |
| `TransactionTypes` | Tipos classificatórios de transações |
| `Transactions` | Lançamentos financeiros (receita/despesa) |
| `InstallmentGroups` | Agrupamento de transações parceladas |
| `MonthlyBudgets` | Metas de gastos mensais por usuário |
| `AiConfigs` | Configurações do assistente de IA por usuário |
| `AiMessages` | Histórico do chat com IA |

---

## 🐳 Docker

O arquivo `docker-compose.yml` na raiz do backend gerencia toda a infraestrutura de suporte:

```yaml
# Serviços disponíveis:
# - postgres   → banco de dados PostgreSQL 16
# - pgadmin    → interface web para administração do banco
# - keycloak   → servidor de autenticação/autorização
```

### Comandos essenciais

```bash
# Subir todos os serviços em background
docker-compose up -d

# Subir apenas o banco de dados
docker-compose up -d postgres

# Ver logs de um serviço específico
docker-compose logs -f keycloak

# Parar todos os serviços (mantém os dados nos volumes)
docker-compose stop

# Parar e remover containers (mantém os dados nos volumes)
docker-compose down

# Parar e remover containers E volumes (APAGA TODOS OS DADOS)
docker-compose down -v

# Ver status dos containers
docker-compose ps
```

### Portas expostas

| Serviço | Porta local | Endereço |
|---|---|---|
| PostgreSQL | `5432` | `localhost:5432` |
| pgAdmin | `8080` | `http://localhost:8080` |
| Keycloak | `8180` | `http://localhost:8180` |

### Persistência de dados

Os dados do PostgreSQL e do Keycloak são mantidos em volumes Docker nomeados (`dados-postgres` e `dados-keycloak`), sobrevivendo a reinicializações dos containers.

---

## ▶️ Como Usar

### Iniciando o Backend

```bash
cd QuantIA
dotnet run
```

- **API REST:** `http://localhost:5221/api`
- **Swagger UI:** `http://localhost:5221/swagger` *(disponível no modo Development)*

### Iniciando o Frontend

```bash
cd QuantIA-Front
npm run dev
```

- **Aplicação:** `http://localhost:5173`

### Build de Produção (Frontend)

```bash
npm run build
# Arquivos gerados em: dist/
npm run preview  # para testar o build localmente
```

### Fluxo de uso básico

1. Acesse `http://localhost:5173`
2. Faça login com um usuário criado no Keycloak
3. Cadastre suas **contas** (cartões/bancos)
4. Crie **categorias** para organizar seus gastos
5. Lance **transações** de receita e despesa
6. Acesse **Relatórios** para visualizar seu desempenho financeiro

---

## 📁 Estrutura de Arquivos

### Backend — `QuantIA/`

```
QuantIA/
├── Controllers/                   # Endpoints da API REST (um controller por recurso)
│   ├── AuthController.cs          # Login, registro e gestão de perfil via Keycloak
│   ├── AccountsController.cs      # CRUD de contas/cartões
│   ├── CategoriesController.cs    # CRUD de categorias
│   ├── TransactionsController.cs  # CRUD de transações
│   ├── TransactionTypesController.cs
│   ├── MonthlyBudgetsController.cs # Metas de gastos mensais
│   ├── ReportsController.cs       # Relatórios (dashboard mensal e anual)
│   ├── AiChatController.cs        # Chat com assistente de IA
│   ├── AiConfigController.cs      # Configuração do provedor de IA
│   └── AuthenticatedControllerBase.cs # Classe base com helper de userId
│
├── Models/                        # Entidades do banco de dados (EF Core)
│   ├── User.cs
│   ├── Account.cs
│   ├── Category.cs
│   ├── Transaction.cs
│   ├── TransactionType.cs
│   ├── InstallmentGroup.cs
│   ├── MonthlyBudget.cs
│   ├── AiConfig.cs
│   └── AiMessage.cs
│
├── DTOs/                          # Objetos de transferência de dados (request/response)
│
├── Interface/                     # Contratos (interfaces) dos serviços
│
├── Services/                      # Implementação da lógica de negócio
│   ├── TransactionService.cs      # Lógica de transações e parcelamentos
│   ├── MonthlyBudgetService.cs    # Agregação de dados para relatórios
│   ├── AiChatService.cs           # Integração com APIs de IA
│   ├── KeycloakAdminService.cs    # Comunicação com API admin do Keycloak
│   └── CurrentUserService.cs      # Extração do userId do token JWT
│
├── Data/
│   └── AppDbContext.cs            # Contexto do Entity Framework Core
│
├── Migrations/                    # Histórico de migrations do banco de dados
│
├── Program.cs                     # Configuração da aplicação (DI, middlewares, CORS, JWT)
├── appsettings.json               # Configurações de ambiente
├── docker-compose.yml             # Infraestrutura Docker (PostgreSQL, pgAdmin, Keycloak)
└── QuantIA.csproj                 # Definição do projeto e dependências NuGet
```

### Frontend — `QuantIA-Front/`

```
QuantIA-Front/
├── src/
│   ├── auth/
│   │   └── tokenStore.js          # Gerenciamento do ciclo de vida do JWT (login, refresh, logout)
│   │
│   ├── components/                # Componentes React organizados por domínio
│   │   ├── accounts/              # Tela de contas/cartões
│   │   ├── categories/            # Tela de categorias (CRUD)
│   │   ├── transactions/          # Tela de transações
│   │   ├── transactionTypes/      # Tipos de transação
│   │   ├── monthlyBudgets/        # Metas mensais
│   │   ├── reports/               # Dashboard de relatórios e gráficos
│   │   ├── aiChat/                # Assistente de IA
│   │   ├── aiConfig/              # Configuração do provedor de IA
│   │   ├── profile/               # Perfil do usuário
│   │   ├── layout/                # Layout principal (sidebar, header)
│   │   ├── auth/                  # Tela de login
│   │   └── ui/                    # Componentes reutilizáveis (Modal, Toast, etc.)
│   │
│   ├── contexts/
│   │   └── ToastContext.jsx       # Contexto global de notificações
│   │
│   ├── hooks/
│   │   └── useToast.js            # Hook de acesso ao toast
│   │
│   ├── lib/
│   │   └── api.js                 # Instância Axios com interceptors de auth e refresh
│   │
│   ├── services/                  # Funções de chamada à API (uma por recurso)
│   │   ├── AccountService.jsx
│   │   ├── CategoryService.js
│   │   ├── TransactionService.jsx
│   │   ├── ReportsService.jsx
│   │   ├── MonthlyBudgetService.jsx
│   │   └── ...
│   │
│   ├── App.jsx                    # Roteamento principal da aplicação
│   └── main.jsx                   # Entry point React
│
├── docs/                          # Documentação técnica dos módulos
├── .env.example                   # Modelo das variáveis de ambiente
├── package.json                   # Dependências npm e scripts
├── vite.config.js                 # Configuração do bundler Vite
└── index.html                     # HTML base da SPA
```

---

## 📦 Dependências Principais

### Backend (.NET 8 / NuGet)

| Pacote | Versão | Propósito |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | 8.0.0 | ORM para acesso ao banco de dados |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 8.0.0 | Provider EF Core para PostgreSQL |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.0 | Ferramentas de migrations (CLI) |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.0 | Validação de tokens JWT do Keycloak |
| `Swashbuckle.AspNetCore.SwaggerGen` | 8.0.0 | Geração automática do contrato OpenAPI |
| `Swashbuckle.AspNetCore.SwaggerUI` | 8.0.0 | Interface visual do Swagger |

### Frontend (npm)

| Pacote | Versão | Propósito |
|---|---|---|
| `react` | ^19.2.4 | Framework UI base |
| `react-router-dom` | ^7.13.2 | Roteamento SPA |
| `axios` | ^1.13.6 | Chamadas HTTP à API REST |
| `recharts` | ^3.8.1 | Gráficos interativos (LineChart, BarChart, PieChart) |
| `lucide-react` | ^1.6.0 | Biblioteca de ícones |
| `keycloak-js` | ^25.0.6 | SDK de integração com Keycloak |
| `tailwindcss` | ^4.2.2 | Framework CSS utilitário |
| `react-select` | ^5.10.2 | Componente de select customizável |
| `vite` | ^8.0.1 | Bundler e servidor de desenvolvimento |

---

## 🔧 Troubleshooting

### ❌ `docker-compose up` falha com "porta já em uso"

**Causa:** Outra aplicação está usando as portas `5432`, `8080` ou `8180`.

**Solução:** Identifique e pare o processo conflitante, ou altere a porta no `docker-compose.yml`:
```bash
# Windows — ver o que está usando a porta 5432
netstat -ano | findstr :5432

# Linux/macOS
lsof -i :5432
```

---

### ❌ `dotnet ef database update` falha com "connection refused"

**Causa:** O container do PostgreSQL ainda não está pronto.

**Solução:**
```bash
# Verifique o status dos containers
docker-compose ps

# Aguarde o PostgreSQL estar "healthy" e tente novamente
docker-compose up -d postgres
# Espere ~10 segundos
dotnet ef database update
```

---

### ❌ Login no frontend falha com "invalid_client" ou redirect loop

**Causa:** O Keycloak não está configurado corretamente ou ainda está inicializando.

**Solução:**
1. Aguarde o Keycloak terminar de iniciar (~30-60 segundos após `docker-compose up`)
2. Verifique se o realm `quantia` existe em `http://localhost:8180`
3. Confirme que o Client `quantia-frontend` tem `http://localhost:5173/*` nas **Valid Redirect URIs**
4. Verifique o valor de `VITE_KEYCLOAK_REALM` no `.env` do frontend

---

### ❌ API retorna 401 em todas as requisições

**Causa:** Token JWT ausente, expirado ou com audience incorreto.

**Solução:**
1. Verifique se `Keycloak.Authority` no `appsettings.json` corresponde ao realm correto
2. Confira se `Keycloak.ClientId` é `quantia-frontend`
3. Faça logout e login novamente no frontend para obter um novo token

---

### ❌ CORS bloqueando requisições do frontend

**Causa:** A URL do frontend não está autorizada no backend.

**Solução:** Verifique `Frontend.Url` no `appsettings.json` do backend:
```json
{
  "Frontend": {
    "Url": "http://localhost:5173"
  }
}
```
A URL deve ser **idêntica** à origem do frontend (sem barra final).

---

### ❌ `dotnet run` falha com "Failed to build" após editar código

**Causa:** Processo anterior da API ainda está rodando e bloqueando o executável.

**Solução:** Encerre o processo anterior antes de rodar novamente:
```bash
# Windows (PowerShell) — encerra todos os processos dotnet
Stop-Process -Name "dotnet" -Force

# Linux/macOS
pkill -f "dotnet run"
```

---

### ❌ `npm run dev` abre mas gráficos não carregam

**Causa:** Backend não está rodando ou a URL da API no `.env` está incorreta.

**Verificação:**
1. Confirme que `dotnet run` está ativo e sem erros
2. Acesse `http://localhost:5221/swagger` — a UI do Swagger deve abrir
3. Verifique `VITE_API_URL` no `.env` do frontend

---

### ❌ Migrations falham com "relation already exists"

**Causa:** O banco já possui tabelas de uma versão anterior das migrations.

**Solução:**
```bash
# Opção 1: Dropar e recriar o banco (perde os dados)
docker-compose down -v
docker-compose up -d postgres
dotnet ef database update

# Opção 2: Marcar a migration como aplicada sem executar
dotnet ef database update <NomeDaMigrationAnterior>
```

---

## 🗺️ Endpoints da API

A documentação interativa completa dos endpoints está disponível via Swagger:

```
http://localhost:5221/swagger
```

Autentique-se clicando em **Authorize** e inserindo `Bearer <seu_token_jwt>`.

---

*Última atualização: maio de 2026*
