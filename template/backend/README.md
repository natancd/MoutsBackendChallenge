# Developer Evaluation - Backend

API de vendas (Sales) em .NET 8 com DDD, MediatR, EF Core e testes unitários.

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Git](https://git-scm.com/)
- Cursor, Visual Studio ou VS Code (opcional, para debug com F5)

Não é necessário instalar PostgreSQL para desenvolvimento local. O ambiente **Development** usa **SQLite em arquivo temporário**, recriado do zero a cada execução.

## Rodar localmente (terminal)

```powershell
cd D:\Natan\Empresas\Mouts\template\backend
dotnet restore Ambev.DeveloperEvaluation.sln
dotnet build Ambev.DeveloperEvaluation.sln
dotnet run --project src\Ambev.DeveloperEvaluation.WebApi
```

Abra o Swagger em: **http://localhost:5119/swagger**

## Debug no Cursor / VS Code

1. Abra a pasta `backend` como workspace.
2. Pressione **F5** (perfil **WebApi**).
3. Coloque breakpoints nos handlers, controllers ou entidades de domínio.
4. Teste os endpoints pelo Swagger ou por uma extensão REST (Thunder Client, etc.).

O banco SQLite fica em:

```
%TEMP%\Ambev.DeveloperEvaluation\devevaluation.db
```

Esse arquivo é **apagado e recriado** toda vez que a API sobe em `Development`. Ao fechar a aplicação, não há nada para manter online.

## Documentação da API

Guia completo com exemplos `curl`, payloads, erros e mapa dos testes:

**[docs/API.md](docs/API.md)**

## Testes

### Todos os testes

```powershell
dotnet test Ambev.DeveloperEvaluation.sln
```

### Testes funcionais (endpoints + erros)

Os testes em `tests/Ambev.DeveloperEvaluation.Functional/` documentam na prática cada endpoint e os retornos de erro. **Não é necessário** subir a API — eles usam `WebApplicationFactory`.

```powershell
# Resumo
dotnet test tests\Ambev.DeveloperEvaluation.Functional

# Ver cada cenário com status HTTP no nome do teste
dotnet test tests\Ambev.DeveloperEvaluation.Functional --logger "console;verbosity=detailed"

# Filtrar por recurso
dotnet test tests\Ambev.DeveloperEvaluation.Functional --filter "FullyQualifiedName~SalesApiTests"
```

Consulte [docs/API.md](docs/API.md#testes-funcionais) para o mapa completo teste → endpoint → status.

### Testes unitários (regras de domínio)

```powershell
dotnet test tests\Ambev.DeveloperEvaluation.Unit
```

## Publicar no GitHub

### 1. Criar o repositório

No GitHub: **New repository** → nome, por exemplo `developer-evaluation-backend` → criar sem README (já existe localmente).

### 2. Enviar o código

Na pasta `backend`:

```powershell
git init
git add .
git commit -m "feat: implement sales API with ephemeral sqlite dev database"
git branch -M main
git remote add origin https://github.com/SEU_USUARIO/developer-evaluation-backend.git
git push -u origin main
```

Substitua `SEU_USUARIO` e o nome do repositório pelos seus.

### 3. Testar em outra máquina

```powershell
git clone https://github.com/SEU_USUARIO/developer-evaluation-backend.git
cd developer-evaluation-backend
dotnet restore
dotnet test
dotnet run --project src\Ambev.DeveloperEvaluation.WebApi
```

Qualquer pessoa com .NET 8 consegue rodar sem Docker nem banco externo.

### 4. CI automático (GitHub Actions)

O workflow em `.github/workflows/ci.yml` executa **build + testes** em cada push e pull request. Confira a aba **Actions** no GitHub.

## Banco de dados por ambiente

| Ambiente | Banco | Comportamento |
|----------|-------|----------------|
| `Development` | SQLite (temp) | Zerado a cada `dotnet run` |
| `Production` / Docker | PostgreSQL | Usa migrations (`dotnet ef database update`) |

Para produção com PostgreSQL, ajuste `ConnectionStrings:DefaultConnection` em `appsettings.json` e suba com `ASPNETCORE_ENVIRONMENT=Production`.

## Endpoints

| Recurso | Métodos |
|---------|---------|
| **Users** | `POST`, `GET /{id}`, `DELETE /{id}` |
| **Auth** | `POST` |
| **Sales** | `POST`, `GET`, `GET /{id}`, `PUT /{id}`, `DELETE /{id}`, `PATCH /{id}/cancel`, `PATCH /{saleId}/items/{itemId}/cancel` |

Exemplos completos, erros e payloads: **[docs/API.md](docs/API.md)**
