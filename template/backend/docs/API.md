# Documentação da API

Base URL local (Development): **http://localhost:5119**

Swagger interativo: **http://localhost:5119/swagger**

---

## Como testar no Swagger

1. Suba a API: `dotnet run --project src\Ambev.DeveloperEvaluation.WebApi`
2. Abra **http://localhost:5119/swagger**
3. Expanda o endpoint desejado → **Try it out**
4. Para `POST` / `PUT`: copie o JSON da seção **Request body** e cole no campo do Swagger
5. Para `GET` / `DELETE` / `PATCH`: preencha os parâmetros de path ou query indicados
6. Clique em **Execute** e confira o corpo da resposta abaixo

> **Dica:** execute primeiro `POST /api/Users` e use o `id` retornado nos endpoints que pedem `{id}`.

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
  "data": {},
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

### Listagem paginada (`GET /api/Users`, `GET /api/Sales`)

```json
{
  "success": true,
  "data": [],
  "currentPage": 1,
  "totalPages": 1,
  "totalCount": 2,
  "message": "",
  "errors": []
}
```

---

## Erros

### 1. Validação no controller — `400 Bad Request`

**Request body inválido (exemplo para testar no Swagger — `POST /api/Users`):**

```json
{}
```

**Resposta:**

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

### 2. Exceções de domínio / negócio (middleware)

| HTTP | `type` | Quando ocorre |
|------|--------|---------------|
| 404 | `ResourceNotFound` | Recurso não encontrado |
| 401 | `AuthenticationError` | Credenciais inválidas |
| 400 | `DomainError` | Regra de domínio violada |
| 400 | `BusinessRuleViolation` | Operação inválida no estado atual |

**404 — recurso não encontrado:**

```json
{
  "type": "ResourceNotFound",
  "error": "User with ID 00000000-0000-0000-0000-000000000000 not found",
  "detail": "User with ID 00000000-0000-0000-0000-000000000000 not found"
}
```

**401 — autenticação falhou:**

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

| | |
|---|---|
| **Status** | `201 Created` |
| **Swagger** | `Users` → `POST /api/Users` → Try it out |

**Request body (copiar no Swagger):**

```json
{
  "username": "natan-dev",
  "password": "NatanNatan1!",
  "phone": "+5511999999999",
  "email": "natan@exemplo.com",
  "status": 1,
  "role": 1
}
```

| Campo | Tipo | Regras |
|-------|------|--------|
| `username` | string | 3–50 caracteres |
| `password` | string | Mín. 8 chars, maiúscula, minúscula, número e caractere especial |
| `phone` | string | Formato internacional, ex: `+5511999999999` |
| `email` | string | Email válido e único |
| `status` | int | `1` = Active |
| `role` | int | `1` = Customer |

**Resposta `201`:**

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

**Testar erro `400` — body vazio:**

```json
{}
```

**Testar erro `400` — email já cadastrado** (use o mesmo email do passo anterior; a mensagem é genérica e **não** confirma que a conta existe):

```json
{
  "username": "outro-usuario",
  "password": "NatanNatan1!",
  "phone": "+5511888888888",
  "email": "natan@exemplo.com",
  "status": 1,
  "role": 1
}
```

**Resposta `400`:**

```json
{
  "success": false,
  "message": "Validation Failed",
  "errors": [
    {
      "error": "InvalidEmail",
      "detail": "Invalid email address"
    }
  ]
}
```

---

### GET `/api/Users` — Listar usuários (paginado)

| | |
|---|---|
| **Status** | `200 OK` |
| **Swagger** | `Users` → `GET /api/Users` → Try it out |

**Parâmetros no Swagger:**

| Campo Swagger | Valor exemplo | Padrão |
|---------------|---------------|--------|
| `_page` | `1` | 1 |
| `_size` | `10` | 10 |
| `_order` | `username asc` | `username` asc |

Ordenação suportada: `username`, `email`, `phone`, `status`, `role`, `createdAt` (ex: `username asc, email desc`).

**Resposta `200`:**

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

> `totalCount` equivale ao `totalItems` da especificação original.

---

### GET `/api/Users/{id}` — Buscar usuário

| | |
|---|---|
| **Status** | `200 OK` |
| **Swagger** | `Users` → `GET /api/Users/{id}` → Try it out |

**Parâmetro no Swagger:**

| Campo | Valor exemplo |
|-------|---------------|
| `id` | `3fa85f64-5717-4562-b3fc-2c963f66afa6` |

Use o `id` retornado no `POST /api/Users`.

**Resposta `200`:**

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

**Testar erro `404` — ID inexistente:**

| Campo | Valor |
|-------|-------|
| `id` | `00000000-0000-0000-0000-000000000000` |

---

### DELETE `/api/Users/{id}` — Excluir usuário

| | |
|---|---|
| **Status** | `200 OK` |
| **Swagger** | `Users` → `DELETE /api/Users/{id}` → Try it out |

**Parâmetro no Swagger:**

| Campo | Valor exemplo |
|-------|---------------|
| `id` | `3fa85f64-5717-4562-b3fc-2c963f66afa6` |

**Resposta `200`:**

```json
{
  "success": true,
  "message": "User deleted successfully",
  "errors": []
}
```

---

## Auth

### POST `/api/Auth` — Autenticar usuário

| | |
|---|---|
| **Status** | `200 OK` |
| **Swagger** | `Auth` → `POST /api/Auth` → Try it out |

O usuário precisa existir (criado via `POST /api/Users`) com `status: Active` (1).

**Request body (copiar no Swagger):**

```json
{
  "email": "natan@exemplo.com",
  "password": "NatanNatan1!"
}
```

**Resposta `200`:**

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

**Testar erro `400` — campos vazios:**

```json
{}
```

**Testar erro `401` — senha incorreta:**

```json
{
  "email": "natan@exemplo.com",
  "password": "SenhaErrada1!"
}
```

---

## Sales

### POST `/api/Sales` — Criar venda

| | |
|---|---|
| **Status** | `201 Created` |
| **Swagger** | `Sales` → `POST /api/Sales` → Try it out |

10 itens × R$ 5,50 = R$ 55,00 → desconto 20% → **total R$ 44,00**

**Request body (copiar no Swagger):**

```json
{
  "saleNumber": "SALE-001",
  "saleDate": "2026-06-14T10:00:00Z",
  "customer": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Cliente ABC"
  },
  "branch": {
    "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "name": "Filial Centro"
  },
  "items": [
    {
      "productId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "productName": "Produto A",
      "quantity": 10,
      "unitPrice": 5.50
    }
  ]
}
```

| Campo | Regras |
|-------|--------|
| `saleNumber` | Único, máx. 50 caracteres |
| `saleDate` | ISO 8601 |
| `items[].quantity` | 1–20 por produto |
| `items[].unitPrice` | Maior que 0 |
| `customer.id` ≠ `branch.id` | Devem ser identidades externas **diferentes** |

> **Customer e Branch:** são entidades externas distintas (padrão External Identities). Os IDs **não** precisam ser iguais — na verdade, **devem ser diferentes**. No PUT, envie os IDs que deseja manter ou alterar; a ordem dos itens no array é a ordem da venda.

**Resposta `201`:**

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

**Testar erro `400` — body vazio:**

```json
{}
```

**Testar erro `400` — quantidade acima de 20:**

```json
{
  "saleNumber": "SALE-002",
  "saleDate": "2026-06-14T10:00:00Z",
  "customer": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Cliente ABC"
  },
  "branch": {
    "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "name": "Filial Centro"
  },
  "items": [
    {
      "productId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "productName": "Produto A",
      "quantity": 21,
      "unitPrice": 5.50
    }
  ]
}
```

---

### GET `/api/Sales` — Listar vendas (paginado)

| | |
|---|---|
| **Status** | `200 OK` |
| **Swagger** | `Sales` → `GET /api/Sales` → Try it out |

**Parâmetros no Swagger:**

| Campo Swagger | Valor exemplo | Padrão |
|---------------|---------------|--------|
| `_page` | `1` | 1 |
| `_size` | `10` | 10 |
| `_order` | `saleDate desc` | — |
| `customerId` | *(vazio ou GUID)* | — |
| `branchId` | *(vazio ou GUID)* | — |
| `isCancelled` | `false` | — |
| `_minSaleDate` | `2026-01-01T00:00:00Z` | — |
| `_maxSaleDate` | `2026-12-31T23:59:59Z` | — |

**Resposta `200`:**

```json
{
  "success": true,
  "data": [
    {
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
      "items": []
    }
  ],
  "currentPage": 1,
  "totalPages": 1,
  "totalCount": 2,
  "message": "",
  "errors": []
}
```

---

### GET `/api/Sales/{id}` — Buscar venda

| | |
|---|---|
| **Status** | `200 OK` |
| **Swagger** | `Sales` → `GET /api/Sales/{id}` → Try it out |

**Parâmetro no Swagger:**

| Campo | Valor exemplo |
|-------|---------------|
| `id` | `b2c3d4e5-f6a7-8901-bcde-f12345678901` |

Use o `id` retornado no `POST /api/Sales`.

**Resposta `200`:** mesmo formato do item em `POST /api/Sales` (campo `data`).

**Testar erro `404`:**

| Campo | Valor |
|-------|-------|
| `id` | `00000000-0000-0000-0000-000000000000` |

---

### PUT `/api/Sales/{id}` — Atualizar venda

| | |
|---|---|
| **Status** | `200 OK` |
| **Swagger** | `Sales` → `PUT /api/Sales/{id}` → Try it out |

Substitui data, cliente, filial e **todos os itens** da venda. Os itens antigos são removidos e substituídos pelo array enviado.

4 itens × R$ 10,00 = R$ 40,00 → desconto 10% → **total R$ 36,00**

**Parâmetro no Swagger:**

| Campo | Valor exemplo |
|-------|---------------|
| `id` | `b2c3d4e5-f6a7-8901-bcde-f12345678901` |

**Request body (copiar no Swagger):**

```json
{
  "saleDate": "2026-06-14T12:00:00Z",
  "customer": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Cliente ABC"
  },
  "branch": {
    "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "name": "Filial Centro"
  },
  "items": [
    {
      "productId": "d4e5f6a7-b8c9-0123-def0-234567890123",
      "productName": "Produto B",
      "quantity": 4,
      "unitPrice": 10.00
    }
  ]
}
```

**Resposta `200`:** venda atualizada com `totalAmount: 36.00`.

---

### PATCH `/api/Sales/{id}/cancel` — Cancelar venda

| | |
|---|---|
| **Status** | `200 OK` |
| **Swagger** | `Sales` → `PATCH /api/Sales/{id}/cancel` → Try it out |

**Parâmetro no Swagger:**

| Campo | Valor exemplo |
|-------|---------------|
| `id` | `b2c3d4e5-f6a7-8901-bcde-f12345678901` |

Sem request body.

**Resposta `200`:**

```json
{
  "success": true,
  "message": "Sale cancelled successfully",
  "data": {
    "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
    "saleNumber": "SALE-001",
    "totalAmount": 44.00,
    "isCancelled": true,
    "items": []
  },
  "errors": []
}
```

---

### PATCH `/api/Sales/{saleId}/items/{itemId}/cancel` — Cancelar item

| | |
|---|---|
| **Status** | `200 OK` |
| **Swagger** | `Sales` → `PATCH /api/Sales/{saleId}/items/{itemId}/cancel` → Try it out |

**Parâmetros no Swagger:**

| Campo | Valor exemplo |
|-------|---------------|
| `saleId` | `b2c3d4e5-f6a7-8901-bcde-f12345678901` |
| `itemId` | `c3d4e5f6-a7b8-9012-cdef-123456789012` |

Use `saleId` e `items[0].id` retornados no `POST /api/Sales`.

Sem request body.

**Resposta `200`:** item com `isCancelled: true` e `totalAmount` da venda recalculado.

---

### DELETE `/api/Sales/{id}` — Excluir venda

| | |
|---|---|
| **Status** | `200 OK` |
| **Swagger** | `Sales` → `DELETE /api/Sales/{id}` → Try it out |

**Parâmetro no Swagger:**

| Campo | Valor exemplo |
|-------|---------------|
| `id` | `b2c3d4e5-f6a7-8901-bcde-f12345678901` |

**Resposta `200`:**

```json
{
  "success": true,
  "message": "Sale deleted successfully",
  "errors": []
}
```

---

## Fluxo completo no Swagger

Ordem sugerida para testar tudo em sequência:

```
1. POST /api/Users          → copiar JSON de criação → guardar data.id
2. POST /api/Auth           → usar mesmo email/senha → guardar data.token
3. GET  /api/Users          → preencher _page=1, _size=10
4. GET  /api/Users/{id}     → colar id do passo 1
5. POST /api/Sales          → copiar JSON de venda → guardar data.id e data.items[0].id
6. GET  /api/Sales          → preencher _page=1, _size=10
7. GET  /api/Sales/{id}     → colar id da venda
8. PUT  /api/Sales/{id}     → colar id + JSON de atualização
9. PATCH /api/Sales/{id}/cancel                          → colar id da venda
   (ou PATCH /api/Sales/{saleId}/items/{itemId}/cancel  → saleId + itemId)
10. DELETE /api/Sales/{id}  → colar id da venda
11. DELETE /api/Users/{id}  → colar id do usuário
```

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

Os testes em `tests/Ambev.DeveloperEvaluation.Functional/` cobrem todos os endpoints com cenários de sucesso e erro.

### Executar todos os testes da API

```powershell
cd D:\Natan\Empresas\Mouts\template\backend
dotnet test tests\Ambev.DeveloperEvaluation.Functional
```

### Ver cada cenário com detalhes

```powershell
dotnet test tests\Ambev.DeveloperEvaluation.Functional --logger "console;verbosity=detailed"
```

### Filtrar por recurso

```powershell
dotnet test tests\Ambev.DeveloperEvaluation.Functional --filter "FullyQualifiedName~UsersApiTests"
dotnet test tests\Ambev.DeveloperEvaluation.Functional --filter "FullyQualifiedName~AuthApiTests"
dotnet test tests\Ambev.DeveloperEvaluation.Functional --filter "FullyQualifiedName~SalesApiTests"
```

### Mapa: teste → endpoint → retorno

| Teste | Endpoint | Status | Erro |
|-------|----------|--------|------|
| `CreateUser_ValidRequest_ReturnsCreated` | `POST /api/Users` | 201 | — |
| `CreateUser_InvalidRequest_ReturnsBadRequest` | `POST /api/Users` | 400 | Validação |
| `CreateUser_DuplicateEmail_ReturnsGenericError` | `POST /api/Users` | 400 | Email genérico |
| `ListUsers_ReturnsPaginatedResult` | `GET /api/Users` | 200 | — |
| `GetUser_ExistingUser_ReturnsOk` | `GET /api/Users/{id}` | 200 | — |
| `GetUser_NotFound_Returns404` | `GET /api/Users/{id}` | 404 | `ResourceNotFound` |
| `DeleteUser_ExistingUser_ReturnsOk` | `DELETE /api/Users/{id}` | 200 | — |
| `DeleteUser_NotFound_Returns404` | `DELETE /api/Users/{id}` | 404 | `ResourceNotFound` |
| `Authenticate_ValidCredentials_ReturnsToken` | `POST /api/Auth` | 200 | — |
| `Authenticate_InvalidPassword_Returns401` | `POST /api/Auth` | 401 | `AuthenticationError` |
| `Authenticate_InvalidRequest_ReturnsBadRequest` | `POST /api/Auth` | 400 | Validação |
| `CreateSale_ValidRequest_ReturnsCreatedWithDiscount` | `POST /api/Sales` | 201 | — |
| `CreateSale_MoreThanTwentyItems_ReturnsBadRequest` | `POST /api/Sales` | 400 | Validação |
| `CreateSale_InvalidRequest_ReturnsBadRequest` | `POST /api/Sales` | 400 | Validação |
| `GetSale_ExistingSale_ReturnsOk` | `GET /api/Sales/{id}` | 200 | — |
| `GetSale_NotFound_Returns404` | `GET /api/Sales/{id}` | 404 | `ResourceNotFound` |
| `ListSales_ReturnsPaginatedResult` | `GET /api/Sales` | 200 | — |
| `UpdateSale_ExistingSale_ReturnsOk` | `PUT /api/Sales/{id}` | 200 | — |
| `CancelSale_ExistingSale_ReturnsCancelled` | `PATCH /api/Sales/{id}/cancel` | 200 | — |
| `CancelSaleItem_ExistingItem_RecalculatesTotal` | `PATCH /api/Sales/.../items/.../cancel` | 200 | — |
| `DeleteSale_ExistingSale_ReturnsOk` | `DELETE /api/Sales/{id}` | 200 | — |

Os testes funcionais usam `WebApplicationFactory` — **não precisam** da API rodando.
