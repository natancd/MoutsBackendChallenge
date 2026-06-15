# Documentação da API

Base URL local (Development): **http://localhost:5119**

Swagger interativo: **http://localhost:5119/swagger**

---

## Índice

1. [Formato das respostas](#formato-das-respostas)
2. [Erros](#erros)
3. [Users](#users)
4. [Auth](#auth)
5. [Sales](#sales)
6. [Regras de desconto](#regras-de-desconto)
7. [Enums](#enums)
8. [Testes funcionais](#testes-funcionais)

---

## Formato das respostas

### Sucesso com dados (`200`, `201`)

```json
{
  "success": true,
  "message": "User created successfully",
  "data": { },
  "errors": []
}
```

### Sucesso sem dados (`200` em DELETE)

```json
{
  "success": true,
  "message": "User deleted successfully",
  "errors": []
}
```

### Listagem paginada (`GET /api/Sales`)

```json
{
  "success": true,
  "data": [ ],
  "currentPage": 1,
  "totalPages": 1,
  "totalCount": 2,
  "message": "",
  "errors": []
}
```

---

## Erros

A API usa **dois formatos** de erro, dependendo da origem.

### 1. Validação no controller (400 Bad Request)

Quando o payload não passa no FluentValidation do controller, a resposta é um **array** de erros:

```json
[
  {
    "propertyName": "Email",
    "errorMessage": "Email is required",
    "attemptedValue": "",
    "customState": null,
    "severity": 0,
    "errorCode": "NotEmptyValidator",
    "formattedMessagePlaceholderValues": {
      "PropertyName": "Email",
      "PropertyValue": ""
    }
  }
]
```

**Quando ocorre:** campos obrigatórios vazios, formato inválido, quantidade > 20 em item de venda, etc.

### 2. Exceções de domínio / negócio (middleware)

| HTTP | `type` | Quando ocorre |
|------|--------|---------------|
| 404 | `ResourceNotFound` | Recurso (usuário, venda) não encontrado |
| 401 | `AuthenticationError` | Email/senha incorretos na autenticação |
| 400 | `DomainError` | Regra de domínio violada |
| 400 | `BusinessRuleViolation` | Operação inválida no estado atual |

```json
{
  "type": "ResourceNotFound",
  "error": "User with id '...' was not found",
  "detail": "User with id '...' was not found"
}
```

```json
{
  "type": "AuthenticationError",
  "error": "Invalid credentials",
  "detail": "Invalid credentials"
}
```

---

## Users

### POST `/api/Users` — Criar usuário

**Status:** `201 Created`

**Body:**

| Campo | Tipo | Obrigatório | Regras |
|-------|------|-------------|--------|
| `username` | string | Sim | 3–50 caracteres |
| `password` | string | Sim | Mín. 8 chars, maiúscula, minúscula, número e caractere especial |
| `phone` | string | Sim | Formato internacional, ex: `+5511999999999` |
| `email` | string | Sim | Email válido |
| `status` | int | Sim | `1` = Active (não use `0` = Unknown) |
| `role` | int | Sim | `1` = Customer (não use `0` = None) |

**Exemplo (curl):**

```bash
curl -X POST http://localhost:5119/api/Users \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"natan-dev\",\"password\":\"NatanNatan1!\",\"phone\":\"+5511999999999\",\"email\":\"natan@exemplo.com\",\"status\":1,\"role\":1}"
```

**Exemplo (PowerShell):**

```powershell
$body = @{
  username = "natan-dev"
  password = "NatanNatan1!"
  phone    = "+5511999999999"
  email    = "natan@exemplo.com"
  status   = 1
  role     = 1
} | ConvertTo-Json

Invoke-RestMethod -Method Post -Uri "http://localhost:5119/api/Users" -Body $body -ContentType "application/json"
```

**Resposta 201:**

```json
{
  "success": true,
  "message": "User created successfully",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "natan-dev",
    "email": "natan@exemplo.com",
    "phone": "+5511999999999",
    "role": 1,
    "status": 1
  },
  "errors": []
}
```

**Erros comuns:**

| Cenário | Status | Formato |
|---------|--------|---------|
| Body vazio ou campos inválidos | 400 | Array de validação |
| Email duplicado | 400 | `DomainError` |

---

### GET `/api/Users` — Listar usuários (paginado)

**Status:** `200 OK`

**Query params:**

| Parâmetro | Descrição | Padrão |
|-----------|-----------|--------|
| `_page` | Número da página | 1 |
| `_size` | Itens por página | 10 |
| `_order` | Ordenação, ex: `username asc, email desc` | `username` asc |

Campos de ordenação suportados: `username`, `email`, `phone`, `status`, `role`, `createdAt`.

**Exemplo:**

```bash
curl "http://localhost:5119/api/Users?_page=1&_size=10&_order=username%20asc"
```

**Resposta 200:**

```json
{
  "success": true,
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "natan-dev",
      "email": "natan@exemplo.com",
      "phone": "+5511999999999",
      "role": 1,
      "status": 1
    }
  ],
  "currentPage": 1,
  "totalPages": 1,
  "totalCount": 2,
  "message": "",
  "errors": []
}
```

> Na especificação original, `totalCount` corresponde a `totalItems`.

**Erros:**

| Cenário | Status | Formato |
|---------|--------|---------|
| `_page` ou `_size` inválidos | 400 | Validação |

---

### GET `/api/Users/{id}` — Buscar usuário

**Status:** `200 OK`

**Exemplo:**

```bash
curl http://localhost:5119/api/Users/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Resposta 200:**

```json
{
  "success": true,
  "message": "User retrieved successfully",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "natan-dev",
    "email": "natan@exemplo.com",
    "phone": "+5511999999999",
    "role": 1,
    "status": 1
  },
  "errors": []
}
```

**Erros:**

| Cenário | Status | `type` |
|---------|--------|--------|
| ID inexistente | 404 | `ResourceNotFound` |

---

### DELETE `/api/Users/{id}` — Excluir usuário

**Status:** `200 OK`

**Exemplo:**

```bash
curl -X DELETE http://localhost:5119/api/Users/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Resposta 200:**

```json
{
  "success": true,
  "message": "User deleted successfully",
  "errors": []
}
```

**Erros:**

| Cenário | Status | `type` |
|---------|--------|--------|
| ID inexistente | 404 | `ResourceNotFound` |

---

## Auth

### POST `/api/Auth` — Autenticar usuário

**Status:** `200 OK`

O usuário precisa existir e estar com `status: Active` (1).

**Body:**

| Campo | Tipo | Obrigatório |
|-------|------|-------------|
| `email` | string | Sim |
| `password` | string | Sim |

**Exemplo:**

```bash
curl -X POST http://localhost:5119/api/Auth \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"natan@exemplo.com\",\"password\":\"NatanNatan1!\"}"
```

**Resposta 200:**

```json
{
  "success": true,
  "message": "User authenticated successfully",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "email": "natan@exemplo.com",
    "name": "natan-dev",
    "role": "Customer"
  },
  "errors": []
}
```

**Erros:**

| Cenário | Status | Formato |
|---------|--------|---------|
| Email ou senha vazios | 400 | Array de validação |
| Senha incorreta | 401 | `AuthenticationError` |
| Usuário inexistente | 401 | `AuthenticationError` |

---

## Sales

### POST `/api/Sales` — Criar venda

**Status:** `201 Created`

**Body:**

| Campo | Tipo | Obrigatório |
|-------|------|-------------|
| `saleNumber` | string | Sim (máx. 50, único) |
| `saleDate` | datetime (ISO 8601) | Sim |
| `customer.id` | guid | Sim |
| `customer.name` | string | Sim |
| `branch.id` | guid | Sim |
| `branch.name` | string | Sim |
| `items` | array | Sim (mín. 1 item) |
| `items[].productId` | guid | Sim |
| `items[].productName` | string | Sim |
| `items[].quantity` | int | Sim (1–20) |
| `items[].unitPrice` | decimal | Sim (> 0) |

**Exemplo — 10 itens com 20% de desconto:**

Subtotal: 10 × 5,50 = 55,00 → com 20% de desconto = **44,00**

```bash
curl -X POST http://localhost:5119/api/Sales \
  -H "Content-Type: application/json" \
  -d "{\"saleNumber\":\"SALE-001\",\"saleDate\":\"2026-06-14T10:00:00Z\",\"customer\":{\"id\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\"name\":\"Cliente ABC\"},\"branch\":{\"id\":\"7c9e6679-7425-40de-944b-e07fc1f90ae7\",\"name\":\"Filial Centro\"},\"items\":[{\"productId\":\"a1b2c3d4-e5f6-7890-abcd-ef1234567890\",\"productName\":\"Produto A\",\"quantity\":10,\"unitPrice\":5.50}]}"
```

**Resposta 201:**

```json
{
  "success": true,
  "message": "Sale created successfully",
  "data": {
    "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
    "saleNumber": "SALE-001",
    "saleDate": "2026-06-14T10:00:00Z",
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "customerName": "Cliente ABC",
    "branchId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "branchName": "Filial Centro",
    "totalAmount": 44.00,
    "isCancelled": false,
    "createdAt": "2026-06-14T10:00:01Z",
    "updatedAt": null,
    "items": [
      {
        "id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
        "productId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "productName": "Produto A",
        "quantity": 10,
        "unitPrice": 5.50,
        "discountPercentage": 20.0,
        "totalAmount": 44.00,
        "isCancelled": false
      }
    ]
  },
  "errors": []
}
```

**Erros:**

| Cenário | Status | Formato |
|---------|--------|---------|
| Payload vazio | 400 | Array de validação |
| Quantidade > 20 no mesmo produto | 400 | Array de validação |
| `saleNumber` duplicado | 400 | `DomainError` |

---

### GET `/api/Sales/{id}` — Buscar venda

**Status:** `200 OK`

```bash
curl http://localhost:5119/api/Sales/b2c3d4e5-f6a7-8901-bcde-f12345678901
```

**Erros:**

| Cenário | Status | `type` |
|---------|--------|--------|
| ID inexistente | 404 | `ResourceNotFound` |

---

### GET `/api/Sales` — Listar vendas (paginado)

**Status:** `200 OK`

**Query params:**

| Parâmetro | Descrição | Padrão |
|-----------|-----------|--------|
| `_page` | Página | 1 |
| `_size` | Itens por página | 10 |
| `_order` | Ordenação, ex: `saleDate desc` | — |
| `customerId` | Filtrar por cliente | — |
| `branchId` | Filtrar por filial | — |
| `isCancelled` | Filtrar canceladas (`true`/`false`) | — |
| `_minSaleDate` | Data mínima | — |
| `_maxSaleDate` | Data máxima | — |

**Exemplo:**

```bash
curl "http://localhost:5119/api/Sales?_page=1&_size=10&_order=saleDate%20desc"
```

**Resposta 200:**

```json
{
  "success": true,
  "data": [ { } ],
  "currentPage": 1,
  "totalPages": 1,
  "totalCount": 2,
  "message": "",
  "errors": []
}
```

---

### PUT `/api/Sales/{id}` — Atualizar venda

**Status:** `200 OK`

Substitui data, cliente, filial e **todos os itens** da venda.

**Exemplo — 4 itens com 10% de desconto:**

4 × 10,00 = 40,00 → com 10% = **36,00**

```bash
curl -X PUT http://localhost:5119/api/Sales/b2c3d4e5-f6a7-8901-bcde-f12345678901 \
  -H "Content-Type: application/json" \
  -d "{\"saleDate\":\"2026-06-14T12:00:00Z\",\"customer\":{\"id\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\"name\":\"Cliente ABC\"},\"branch\":{\"id\":\"7c9e6679-7425-40de-944b-e07fc1f90ae7\",\"name\":\"Filial Centro\"},\"items\":[{\"productId\":\"d4e5f6a7-b8c9-0123-def0-234567890123\",\"productName\":\"Produto B\",\"quantity\":4,\"unitPrice\":10.00}]}"
```

**Erros:**

| Cenário | Status | Formato |
|---------|--------|---------|
| Payload inválido | 400 | Array de validação |
| Venda inexistente | 404 | `ResourceNotFound` |

---

### PATCH `/api/Sales/{id}/cancel` — Cancelar venda

**Status:** `200 OK`

```bash
curl -X PATCH http://localhost:5119/api/Sales/b2c3d4e5-f6a7-8901-bcde-f12345678901/cancel
```

**Resposta:** venda com `isCancelled: true`.

**Erros:**

| Cenário | Status | `type` |
|---------|--------|--------|
| Venda inexistente | 404 | `ResourceNotFound` |

---

### PATCH `/api/Sales/{saleId}/items/{itemId}/cancel` — Cancelar item

**Status:** `200 OK`

```bash
curl -X PATCH http://localhost:5119/api/Sales/b2c3d4e5-f6a7-8901-bcde-f12345678901/items/c3d4e5f6-a7b8-9012-cdef-123456789012/cancel
```

**Resposta:** item com `isCancelled: true` e `totalAmount` da venda recalculado.

---

### DELETE `/api/Sales/{id}` — Excluir venda

**Status:** `200 OK`

```bash
curl -X DELETE http://localhost:5119/api/Sales/b2c3d4e5-f6a7-8901-bcde-f12345678901
```

**Resposta 200:**

```json
{
  "success": true,
  "message": "Sale deleted successfully",
  "errors": []
}
```

**Erros:**

| Cenário | Status | `type` |
|---------|--------|--------|
| Venda inexistente | 404 | `ResourceNotFound` |

---

## Regras de desconto

Aplicadas **por produto** (mesma quantidade do mesmo item):

| Quantidade | Desconto |
|------------|----------|
| 1–3 | 0% |
| 4–9 | 10% |
| 10–20 | 20% |
| > 20 | Não permitido (400) |

**Exemplos de cálculo:**

| Qtd | Preço unit. | Subtotal | Desconto | Total |
|-----|-------------|----------|----------|-------|
| 3 | 5,50 | 16,50 | 0% | 16,50 |
| 4 | 10,00 | 40,00 | 10% | 36,00 |
| 10 | 5,50 | 55,00 | 20% | 44,00 |

---

## Enums

### UserStatus

| Valor | Nome |
|-------|------|
| 0 | Unknown (inválido para criação) |
| 1 | Active |
| 2 | Inactive |
| 3 | Suspended |

### UserRole

| Valor | Nome |
|-------|------|
| 0 | None (inválido para criação) |
| 1 | Customer |
| 2 | Manager |
| 3 | Admin |

---

## Testes funcionais

Os testes em `tests/Ambev.DeveloperEvaluation.Functional/` cobrem **todos os endpoints** com cenários de sucesso e erro. Cada teste tem um nome descritivo com o status HTTP esperado.

### Executar todos os testes da API

```powershell
cd D:\Natan\Empresas\Mouts\template\backend
dotnet test tests\Ambev.DeveloperEvaluation.Functional\Ambev.DeveloperEvaluation.Functional.csproj
```

### Ver detalhes de cada cenário (recomendado)

```powershell
dotnet test tests\Ambev.DeveloperEvaluation.Functional\Ambev.DeveloperEvaluation.Functional.csproj `
  --logger "console;verbosity=detailed"
```

A saída mostra o nome de cada teste, por exemplo:

```
POST /api/Users - 201 Created com dados válidos
POST /api/Users - 400 ValidationError com dados inválidos
GET /api/Users/{id} - 404 ResourceNotFound quando usuário não existe
...
```

### Filtrar por recurso

```powershell
# Apenas Users
dotnet test tests\Ambev.DeveloperEvaluation.Functional --filter "FullyQualifiedName~UsersApiTests"

# Apenas Auth
dotnet test tests\Ambev.DeveloperEvaluation.Functional --filter "FullyQualifiedName~AuthApiTests"

# Apenas Sales
dotnet test tests\Ambev.DeveloperEvaluation.Functional --filter "FullyQualifiedName~SalesApiTests"
```

### Filtrar um cenário específico

```powershell
dotnet test tests\Ambev.DeveloperEvaluation.Functional --filter "DisplayName~404"
dotnet test tests\Ambev.DeveloperEvaluation.Functional --filter "DisplayName~AuthenticationError"
```

### Mapa completo: teste → endpoint → retorno

| Teste | Endpoint | Status | Tipo de erro (se houver) |
|-------|----------|--------|--------------------------|
| `CreateUser_ValidRequest_ReturnsCreated` | `POST /api/Users` | 201 | — |
| `CreateUser_InvalidRequest_ReturnsBadRequest` | `POST /api/Users` | 400 | Validação |
| `ListUsers_ReturnsPaginatedResult` | `GET /api/Users` | 200 | — |
| `GetUser_ExistingUser_ReturnsOk` | `GET /api/Users/{id}` | 200 | — |
| `GetUser_NotFound_Returns404` | `GET /api/Users/{id}` | 404 | `ResourceNotFound` |
| `DeleteUser_ExistingUser_ReturnsOk` | `DELETE /api/Users/{id}` | 200 | — |
| `DeleteUser_NotFound_Returns404` | `DELETE /api/Users/{id}` | 404 | `ResourceNotFound` |
| `Authenticate_ValidCredentials_ReturnsToken` | `POST /api/Auth` | 200 | — |
| `Authenticate_InvalidPassword_Returns401` | `POST /api/Auth` | 401 | `AuthenticationError` |
| `Authenticate_InvalidRequest_ReturnsBadRequest` | `POST /api/Auth` | 400 | Validação |
| `CreateSale_ValidRequest_ReturnsCreatedWithDiscount` | `POST /api/Sales` | 201 | — (total 44,00) |
| `CreateSale_MoreThanTwentyItems_ReturnsBadRequest` | `POST /api/Sales` | 400 | Validação |
| `CreateSale_InvalidRequest_ReturnsBadRequest` | `POST /api/Sales` | 400 | Validação |
| `GetSale_ExistingSale_ReturnsOk` | `GET /api/Sales/{id}` | 200 | — |
| `GetSale_NotFound_Returns404` | `GET /api/Sales/{id}` | 404 | `ResourceNotFound` |
| `ListSales_ReturnsPaginatedResult` | `GET /api/Sales` | 200 | — |
| `UpdateSale_ExistingSale_ReturnsOk` | `PUT /api/Sales/{id}` | 200 | — (total 36,00) |
| `CancelSale_ExistingSale_ReturnsCancelled` | `PATCH /api/Sales/{id}/cancel` | 200 | — |
| `CancelSaleItem_ExistingItem_RecalculatesTotal` | `PATCH /api/Sales/{id}/items/{itemId}/cancel` | 200 | — |
| `DeleteSale_ExistingSale_ReturnsOk` | `DELETE /api/Sales/{id}` | 200 | — |

### Testes unitários (regras de domínio)

```powershell
dotnet test tests\Ambev.DeveloperEvaluation.Unit
```

### Todos os testes do projeto

```powershell
dotnet test Ambev.DeveloperEvaluation.sln
```

### Fluxo recomendado para demonstrar retornos

1. Suba a API: `dotnet run --project src\Ambev.DeveloperEvaluation.WebApi`
2. Em outro terminal, execute os testes funcionais com verbosidade detalhada
3. Use os exemplos `curl` deste documento no Swagger ou no terminal para reproduzir manualmente

Os testes funcionais usam `WebApplicationFactory` — **não precisam** da API rodando; eles sobem uma instância em memória com SQLite isolado por execução.
