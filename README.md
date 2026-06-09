# QuantIA

> Sistema de gestão financeira pessoal com autenticação via Keycloak, API REST em .NET 8 e interface web em React 19.

---

## 📋 Descrição do Projeto

O **QuantIA** é uma aplicação full-stack para controle de finanças pessoais. O sistema permite que cada usuário autenticado gerencie:

- **Contas/Cartões** — múltiplos cartões ou contas bancárias
- **Categorias** — categorias de despesas e receitas com cor customizável
- **Tipos de Transação** — classificações reutilizáveis para transações
- **Transações** — lançamentos de receita e despesa com suporte a parcelamento
- **Metas Mensais** — limites de gastos configuráveis por mês
- **Relatórios** — fluxo de caixa diário cumulativo, desempenho mensal anual, distribuição por categoria e mapa de calor de consumo, com filtros por conta, período e modo de visualização
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
| **npm** | 9.x | incluído com Node.js |
| **Docker Desktop** | 4.x | https://www.docker.com/products/docker-desktop |
| **Docker Compose** | v2 | incluído no Docker Desktop |

**Ferramenta adicional obrigatória para migrations:**

```bash
dotnet tool install --global dotnet-ef
```

---

## 🚀 Instalação

### 1. Clone os repositórios

```bash
# Backend
git clone https://github.com/d-ivva/QuantIA.git

# Frontend
git clone https://github.com/d-ivva/QuantIA-Front.git
```

### 2. Suba a infraestrutura com Docker

Na pasta do **backend**, configure as credenciais e suba os serviços:

```bash
cd QuantIA

# Copie o arquivo de exemplo e defina suas senhas
cp .env.example .env
# Edite o .env com as senhas que desejar antes de continuar

# Suba todos os serviços em background
docker-compose up -d
```

Isso inicia três serviços:
- **PostgreSQL 16** na porta `5432`
- **pgAdmin 4** na porta `8080`
- **Keycloak 25** na porta `8180`

Aguarde cerca de 30 segundos para o Keycloak terminar de inicializar.

### 3. Configure o Keycloak (primeira vez)

1. Acesse `http://localhost:8180` → login com o usuário e senha definidos em `KEYCLOAK_ADMIN` e `KEYCLOAK_ADMIN_PASSWORD` no seu `.env`
2. Crie um novo **Realm** com o nome: `quantia`
3. Dentro do realm, crie um **Client** com os seguintes dados:
   - **Client ID:** `quantia-frontend`
   - **Client authentication:** desabilitado (fluxo público)
   - **Valid redirect URIs:** `http://localhost:5173/*`
   - **Web origins:** `http://localhost:5173`
4. Crie um usuário de teste e defina uma senha permanente

### 4. Configure e rode o Backend

```bash
cd QuantIA

# Restaure os pacotes NuGet
dotnet restore

# Aplique as migrations e crie as tabelas no banco
dotnet ef database update

# Inicie a API
dotnet run
```

- **API REST:** `http://localhost:5221/api`

### 5. Configure e rode o Frontend

```bash
cd QuantIA-Front

# Instale as dependências npm
npm install

# Copie e configure as variáveis de ambiente
cp .env.example .env
# Os valores padrão já apontam para localhost — edite apenas se necessário

# Inicie o servidor de desenvolvimento
npm run dev
```

- **Aplicação:** `http://localhost:5173`

---

## 🔐 Variáveis de Ambiente

### Frontend — `QuantIA-Front/.env`

```dotenv
# URL base da API REST do backend
VITE_API_URL=http://localhost:5221/api

# URL do servidor Keycloak
VITE_KEYCLOAK_URL=http://localhost:8180

# Nome do realm configurado no Keycloak
VITE_KEYCLOAK_REALM=quantia

# Client ID registrado no Keycloak
VITE_KEYCLOAK_CLIENT_ID=quantia-frontend
```

> **Atenção:** Em produção, substitua todos os valores `localhost` pelos endereços reais. Variáveis sem o prefixo `VITE_` não são acessíveis pelo browser.

### Backend — `QuantIA/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=quantia;Username=postgres;Password=SUA_SENHA_POSTGRES"
  },
  "Keycloak": {
    "Authority": "http://localhost:8180/realms/quantia",
    "ClientId": "quantia-frontend",
    "AdminUrl": "http://localhost:8180",
    "AdminRealm": "master",
    "AdminClientId": "admin-cli",
    "AdminUsername": "admin",
    "AdminPassword": "SUA_SENHA_KEYCLOAK",
    "Realm": "quantia"
  },
  "Frontend": {
    "Url": "http://localhost:5173"
  }
}
```

> **Importante:** Nunca coloque senhas reais neste arquivo se o repositório for público. O `appsettings.json` já está no `.gitignore` deste projeto — cada desenvolvedor deve criá-lo localmente com suas próprias credenciais. As senhas devem corresponder às definidas no `docker-compose.yml`.

---

## 🗄️ Configuração do Banco de Dados

O projeto usa **PostgreSQL 16** via Docker e **Entity Framework Core 8** com o provider `Npgsql`.

### Subir apenas o banco

```bash
cd QuantIA
docker-compose up -d postgres
```

### Aplicar migrations (criar/atualizar tabelas)

```bash
cd QuantIA
dotnet ef database update
```

### Criar nova migration após alterar Models

```bash
dotnet ef migrations add NomeDaMigration
dotnet ef database update
```

### Reverter para uma migration anterior

```bash
dotnet ef database update NomeDaMigrationAnterior
```

### Acessar o banco via pgAdmin

1. Acesse `http://localhost:8080`
2. Login com as credenciais definidas em `PGADMIN_EMAIL` e `PGADMIN_PASSWORD` no seu `.env`
3. Adicione um servidor:
   - **Host:** `postgres` *(nome do serviço no Docker)*
   - **Port:** `5432`
   - **Username:** valor de `POSTGRES_USER` no seu `.env`
   - **Password:** valor de `POSTGRES_PASSWORD` no seu `.env`

### Tabelas do banco e relacionamentos

| Tabela | Descrição |
|---|---|
| `Users` | Usuários sincronizados com Keycloak |
| `FinancialProfile` | Perfil financeiro do usuário — **1:1 com Users** (unicidade garantida por índice único em `UserId`) |
| `Accounts` | Contas e cartões financeiros do usuário |
| `Categories` | Categorias de transações com cor |
| `TransactionTypes` | Tipos classificatórios reutilizáveis |
| `Transactions` | Lançamentos financeiros (receita/despesa) |
| `InstallmentGroups` | Agrupamento de transações parceladas |
| `MonthlyBudgets` | Metas de gastos por mês/ano |
| `AiConfigs` | Configurações do assistente de IA por usuário |
| `AiMessages` | Histórico de mensagens do chat com IA |

**Diagrama de relacionamentos:**

```
Users ──1:1── FinancialProfile
Users ──1:N── Accounts
Users ──1:N── Categories
Users ──1:N── TransactionTypes
Users ──1:N── MonthlyBudgets
Users ──1:N── AiConfigs
Users ──1:N── AiMessages
Users ──1:N── Transactions
Accounts ──1:N── Transactions
Categories ──1:N── Transactions  (ON DELETE SET NULL)
TransactionTypes ──1:N── Transactions
InstallmentGroups ──1:N── Transactions
```

---

## 🐳 Docker

O arquivo `docker-compose.yml` na raiz do backend gerencia toda a infraestrutura de suporte (banco, painel admin e autenticação).

### Comandos essenciais

```bash
# Subir todos os serviços em background
docker-compose up -d

# Subir apenas o banco de dados
docker-compose up -d postgres

# Verificar status dos containers
docker-compose ps

# Ver logs em tempo real de um serviço
docker-compose logs -f keycloak
docker-compose logs -f postgres

# Pausar os serviços (mantém dados nos volumes)
docker-compose stop

# Retomar serviços pausados
docker-compose start

# Parar e remover containers (dados preservados nos volumes)
docker-compose down

# Parar, remover containers E apagar todos os dados permanentemente
docker-compose down -v
```

### Portas expostas

| Serviço | Porta local | Endereço |
|---|---|---|
| PostgreSQL | `5432` | `localhost:5432` |
| pgAdmin 4 | `8080` | `http://localhost:8080` |
| Keycloak 25 | `8180` | `http://localhost:8180` |

### Persistência de dados

Os dados são armazenados em volumes Docker nomeados (`dados-postgres` e `dados-keycloak`). Eles sobrevivem a `docker-compose stop` e `docker-compose down`, mas são destruídos com `docker-compose down -v`.

---

## ▶️ Como Usar

### Iniciar o Backend

```bash
cd QuantIA
dotnet run
```

### Iniciar o Frontend

```bash
cd QuantIA-Front
npm run dev
```

### Reiniciar o Backend após edições (Windows)

Se o servidor estiver travando o executável:

```powershell
Stop-Process -Name "dotnet" -Force
dotnet run
```

### Build de Produção (Frontend)

```bash
npm run build        # Gera os arquivos em dist/
npm run preview      # Testa o build localmente
```

## 📁 Estrutura de Arquivos

### Backend — `QuantIA/`

```
QuantIA/
├── Controllers/                        # Endpoints da API REST
│   ├── AuthenticatedControllerBase.cs  # Classe base com helper de userId JWT
│   ├── AuthController.cs               # Login, registro e gestão de conta
│   ├── AccountsController.cs           # CRUD de contas/cartões
│   ├── CategoriesController.cs         # CRUD de categorias
│   ├── TransactionsController.cs       # CRUD de transações
│   ├── TransactionTypesController.cs   # CRUD de tipos de transação
│   ├── MonthlyBudgetsController.cs     # CRUD de metas mensais
│   ├── ReportsController.cs            # Dashboard mensal e relatório anual
│   ├── AiChatController.cs             # Chat com assistente de IA
│   ├── AiConfigController.cs           # Configuração do provedor de IA
│   └── ProfileController.cs            # Perfil do usuário
│
├── Models/                             # Entidades mapeadas para o banco (EF Core)
│   ├── User.cs
│   ├── FinancialProfile.cs             # Perfil financeiro — relacionamento 1:1 com User
│   ├── Account.cs
│   ├── Category.cs
│   ├── Transaction.cs
│   ├── TransactionType.cs
│   ├── InstallmentGroup.cs
│   ├── MonthlyBudget.cs
│   ├── AiConfig.cs
│   └── AiMessage.cs
│
├── DTOs/                               # Objetos de transferência (request/response)
│
├── Interface/                          # Contratos (interfaces) de serviços
│   └── IMonthlyBudgetService.cs        # Inclui GetDashboardData e GetAnnualReport
│
├── Services/                           # Implementação da lógica de negócio
│   ├── MonthlyBudgetService.cs         # Agregação de dados para relatórios (com filtro accountId)
│   ├── TransactionService.cs           # Lógica de transações e parcelamentos
│   ├── AiChatService.cs                # Integração com APIs de IA externas
│   ├── KeycloakAdminService.cs         # Comunicação com API admin do Keycloak
│   ├── CurrentUserService.cs           # Extração do userId do token JWT
│   └── ...
│
├── Data/
│   └── AppDbContext.cs                 # DbContext do Entity Framework Core
│
├── Migrations/                         # Histórico de migrations geradas pelo EF Core
│
├── Program.cs                          # Bootstrap da aplicação (DI, CORS, JWT, Swagger)
├── appsettings.json                    # Configurações de ambiente
├── docker-compose.yml                  # Infraestrutura Docker
└── QuantIA.csproj                      # Definição do projeto e pacotes NuGet
```

### Frontend — `QuantIA-Front/`

```
QuantIA-Front/
├── src/
│   ├── auth/
│   │   └── tokenStore.js               # Ciclo de vida do JWT (login, refresh, logout)
│   │
│   ├── components/
│   │   ├── home/                        # Tela inicial (/)
│   │   ├── accounts/                    # Tela de contas/cartões (/accounts)
│   │   ├── categories/                  # Tela de categorias (/categories)
│   │   │   ├── CategoriesPage.jsx       # Orquestrador de estado e operações CRUD
│   │   │   ├── CategoryTable.jsx        # Tabela com busca e ações
│   │   │   ├── CategoryFormModal.jsx    # Modal de criação/edição
│   │   │   └── CategoryDeleteDialog.jsx # Confirmação de exclusão
│   │   ├── transactions/                # Tela de transações (/transactions)
│   │   ├── transactionTypes/            # Tipos de transação (/transaction-types)
│   │   ├── monthlyBudgets/              # Metas mensais (/monthly-budgets)
│   │   ├── reports/                     # Dashboard de relatórios (/reports)
│   │   │   └── ReportsPage.jsx          # Todos os gráficos, filtros, zoom e pan
│   │   ├── aiChat/                      # Chat com IA (/ai-chat)
│   │   ├── aiConfig/                    # Configuração de IA (/ai-config)
│   │   ├── profile/                     # Perfil do usuário
│   │   ├── layout/                      # Sidebar e layout principal
│   │   ├── auth/                        # Tela de login
│   │   └── ui/                          # Componentes reutilizáveis (Modal, etc.)
│   │
│   ├── contexts/
│   │   └── ToastContext.jsx             # Contexto global de notificações toast
│   │
│   ├── hooks/
│   │   └── useToast.js                  # Hook para acesso ao toast
│   │
│   ├── lib/
│   │   └── api.js                       # Instância Axios com interceptors de auth e refresh automático
│   │
│   ├── services/                        # Funções de chamada à API (uma por recurso)
│   │   ├── AccountService.jsx
│   │   ├── CategoryService.js
│   │   ├── TransactionService.jsx
│   │   ├── ReportsService.jsx           # getDashboardData e getAnnualReport (com accountId)
│   │   ├── MonthlyBudgetService.jsx
│   │   └── ...
│   │
│   ├── App.jsx                          # Definição de todas as rotas da SPA
│   └── main.jsx                         # Entry point React
│
├── docs/                                # Documentação técnica dos módulos
├── .env.example                         # Modelo das variáveis de ambiente
├── package.json                         # Dependências e scripts npm
├── vite.config.js                       # Configuração do Vite
└── index.html                           # HTML base da SPA
```

---

## 📦 Dependências Principais

### Backend (.NET 8 / NuGet)

| Pacote | Versão | Propósito |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | 8.0.0 | ORM para acesso ao PostgreSQL |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 8.0.0 | Provider EF Core para PostgreSQL |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.0 | Geração e execução de migrations via CLI |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.0 | Validação de tokens JWT emitidos pelo Keycloak |
| `Swashbuckle.AspNetCore.SwaggerGen` | 8.0.0 | Geração do contrato OpenAPI |
| `Swashbuckle.AspNetCore.SwaggerUI` | 8.0.0 | Interface visual interativa do Swagger |

### Frontend (npm)

| Pacote | Versão | Propósito |
|---|---|---|
| `react` | ^19.2.4 | Framework UI base |
| `react-dom` | ^19.2.4 | Renderização no DOM |
| `react-router-dom` | ^7.13.2 | Roteamento SPA com 9 rotas |
| `axios` | ^1.13.6 | Chamadas HTTP com interceptors de auth |
| `recharts` | ^3.8.1 | Gráficos interativos (LineChart com zoom/pan, BarChart, PieChart, ReferenceArea) |
| `lucide-react` | ^1.6.0 | Ícones (Calendar, ChevronLeft, Maximize2, etc.) |
| `keycloak-js` | ^25.0.6 | SDK de integração com Keycloak |
| `tailwindcss` | ^4.2.2 | Framework CSS utilitário |
| `react-select` | ^5.10.2 | Selects customizáveis |
| `vite` | ^8.0.1 | Bundler e servidor de desenvolvimento |

---

## 🔄 Sequência completa de setup (do zero)

```bash
# 1. Configurar credenciais e subir infraestrutura
cd QuantIA
cp .env.example .env
# Edite o .env e defina suas senhas antes de continuar
docker-compose up -d

# 2. Aguardar ~30 segundos e aplicar migrations
dotnet ef database update

# 3. Iniciar o backend
# (crie o appsettings.json com base no template do README se ainda não existir)
dotnet run

# 4. Em um novo terminal — iniciar o frontend
cd ../QuantIA-Front
npm install
cp .env.example .env
npm run dev
```

Acesse `http://localhost:5173` e faça login com o usuário criado no Keycloak.
