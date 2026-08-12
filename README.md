# 🎮 FIAP Cloud Games (FCG)

## Tech Challenge - Fase 1

API REST em **.NET 8** para cadastro de usuários e biblioteca de jogos digitais da plataforma **FIAP Cloud Games (FCG)**, construída como um **monólito modular** seguindo princípios de **Domain-Driven Design (DDD)**.

---

# 📚 Sumário

* Sobre o Projeto
* Tecnologias Utilizadas
* Arquitetura
* Estrutura do Projeto
* Funcionalidades
* Requisitos
* Como Executar
* Autenticação
* Endpoints
* Testes (TDD)
* Domain-Driven Design (DDD)
* Melhorias Futuras (próximas fases)

---

# 📖 Sobre o Projeto

Nesta primeira fase foi desenvolvido o MVP responsável por:

* Cadastro de usuários (nome, e-mail, senha) com validação de formato de e-mail e senha forte
* Autenticação via JWT com dois níveis de acesso: `User` e `Administrator`
* Cadastro e publicação de jogos (Administrator)
* Criação de promoções e vínculo com jogos (Administrator)
* Biblioteca de jogos adquiridos por usuário, com preço já considerando promoção vigente na compra

---

# 🚀 Tecnologias Utilizadas

* .NET 8 / ASP.NET Core (Controllers)
* Entity Framework Core 8 + Npgsql (PostgreSQL)
* MediatR (CQRS) + FluentValidation
* JWT Bearer Authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`)
* `PasswordHasher<T>` (`Microsoft.AspNetCore.Identity`, PBKDF2)
* Swagger / OpenAPI (Swashbuckle) com suporte a Bearer token
* xUnit
* Docker Compose (PostgreSQL local)

---

# 🏛 Arquitetura

O projeto é um **monólito modular** (um único processo/deploy, conforme exigido para o MVP), com 4 módulos de negócio isolados entre si e um `Host` como raiz de composição:

```
FGames.sln
│
├── src/
│   ├── BuildingBlocks/
│   │   ├── FGames.SharedKernel               (Entity, AggregateRoot, ValueObject, Result<T>, Error)
│   │   └── FGames.SharedKernel.Infrastructure
│   │
│   ├── Modules/
│   │   ├── Users/        (Domain, Application, Infrastructure, Api)
│   │   ├── Games/        (Domain, Application, Infrastructure, Api)
│   │   ├── Promotions/   (Domain, Application, Infrastructure, Api)
│   │   └── Library/      (Domain, Application, Infrastructure, Api)
│   │
│   └── Host/
│       └── FGames.Api     (Program.cs — único processo executável, compõe todos os módulos)
│
└── tests/
    ├── FGames.Modules.Users.Tests
    ├── FGames.Modules.Games.Tests
    ├── FGames.Modules.Promotions.Tests
    └── FGames.Modules.Library.Tests
```

Cada módulo segue Clean Architecture internamente:

* **Domain**: entidades ricas com invariantes (private setters, factory methods retornando `Result<T>`), value objects (`Email`, `Password`), enums e interfaces de repositório. Não depende de nada além do `SharedKernel`.
* **Application**: Commands/Queries/Handlers via MediatR, validators FluentValidation, DTOs.
* **Infrastructure**: EF Core (`DbContext` próprio por módulo, todos apontando para o mesmo banco PostgreSQL), repositórios, JWT/hash de senha (módulo Users).
* **Api**: Controllers, regras de autorização (`[Authorize(Roles=...)]`).

**Regra de isolamento entre módulos**: nenhum módulo referencia o projeto de outro módulo diretamente. Leituras entre módulos (ex.: Library precisa saber o preço de um jogo publicado em Games, ou se há promoção ativa) são feitas por interfaces definidas pelo próprio módulo consumidor (`IGameLookupService`, `IActivePromotionLookupService`) e implementadas como adapters em `Host/Adapters`, que despacham a chamada via `IMediator` para o módulo produtor — chamada in-process, sem HTTP, sem acoplamento de projeto.

Cada módulo tem seu próprio `DbContext` (`UsersDbContext`, `GamesDbContext`, `PromotionsDbContext`, `LibraryDbContext`), cada um com seu próprio schema e histórico de migrations, todos compartilhando a mesma instância física do PostgreSQL — mantendo o isolamento de módulo mesmo com banco único.

---

# ✅ Funcionalidades

## Cadastro de usuários
* Nome, e-mail e senha (mín. 8 caracteres, com letra, número e caractere especial)
* Perfis fixos: `User` (acesso à plataforma e biblioteca) e `Administrator` (cadastra jogos, administra usuários, cria promoções)

## Autenticação e Autorização
* Login retorna um JWT com claims de `sub`, `email` e `role`
* Endpoints protegidos por `[Authorize(Roles = "Administrator")]` ou `[Authorize(Roles = "User")]`

## Jogos
* Catálogo de jogos publicados é público (não exige autenticação)
* Administrator cria, edita e publica jogos
* Listagem de jogos já retorna o preço promocional vigente (`finalPrice`), quando o jogo estiver vinculado a uma promoção ativa

## Promoções
* Administrator cria promoções (período + percentual de desconto) e vincula a jogos publicados

## Biblioteca
* `User` compra um jogo publicado (preço já aplica desconto de promoção ativa, se houver)
* Compra duplicada do mesmo jogo é bloqueada
* `User` lista sua própria biblioteca

---

# ⚙️ Requisitos

* .NET SDK 8
* Docker (para subir o PostgreSQL local via `docker-compose`)

---

# ▶️ Como Executar

## 1. Subir o banco de dados (PostgreSQL via Docker)

```bash
docker-compose up -d
```

Isso sobe um PostgreSQL em `localhost:5432` (usuário/senha/banco de desenvolvimento já configurados em `docker-compose.yml` e `appsettings.Development.json`).

## 2. Rodar a API

```bash
dotnet run --project src/Host/FGames.Api
```

As migrations de cada módulo são aplicadas automaticamente no startup (`Database.Migrate()`).

A API estará disponível em:

```
http://localhost:5263
https://localhost:7291
```

Swagger:

```
http://localhost:5263/swagger
```

## 3. Rodar os testes

```bash
dotnet test
```

---

# 🔐 Autenticação

1. `POST /api/users/register` — cadastro público.
2. `POST /api/users/login` — retorna `{ accessToken, expiresAtUtc, user }`.
3. No Swagger, clique em **Authorize** e informe apenas o token (sem o prefixo `Bearer`).

Para testar rotas de Administrator, crie o primeiro administrador diretamente no banco (ajustando a coluna `Role` do primeiro usuário registrado) ou promova via `PATCH /api/users/{id}/status` após já existir um Administrator.

---

# 📡 Endpoints

## Users — `api/users`

| Método | Endpoint              | Acesso                  |
| ------ | --------------------- | ------------------------ |
| POST   | `/register`            | Anônimo                  |
| POST   | `/login`                | Anônimo                  |
| GET    | `/me`                   | Autenticado               |
| GET    | `/`                     | Administrator             |
| GET    | `/{id}`                 | Administrator             |
| POST   | `/`                     | Administrator (cria usuário/admin) |
| PATCH  | `/{id}/status`          | Administrator             |

## Games — `api/games`

| Método | Endpoint            | Acesso        |
| ------ | -------------------- | -------------- |
| GET    | `/`                   | Anônimo (só publicados) |
| GET    | `/{id}`                | Anônimo        |
| POST   | `/`                    | Administrator  |
| PUT    | `/{id}`                | Administrator  |
| POST   | `/{id}/publish`        | Administrator  |

> `GET /` (listagem) retorna, para cada jogo, `price` (preço base), `finalPrice` (preço já com desconto da promoção ativa, se houver) e `discountPercentage` (`null` quando não há promoção ativa).

## Promotions — `api/promotions`

| Método | Endpoint                          | Acesso        |
| ------ | ---------------------------------- | -------------- |
| GET    | `/`                                  | Anônimo (só ativas) |
| POST   | `/`                                  | Administrator  |
| POST   | `/{promotionId}/games/{gameId}`      | Administrator  |

## Library — `api/library`

| Método | Endpoint    | Acesso |
| ------ | ------------ | ------ |
| POST   | `/purchase`   | User   |
| GET    | `/mine`       | User   |

---

# 🧪 Testes (TDD)

O módulo **Users** foi desenvolvido com **TDD**: os testes de `Email`, `Password` e `User` (`tests/FGames.Modules.Users.Tests/Domain`) foram escritos antes da implementação das respectivas classes de domínio.

Os demais módulos têm cobertura de unidade nas principais regras de negócio:

* `Games`: transições de status (`Draft` → `Published`), invariantes de criação
* `Promotions`: período válido (`EndDate > StartDate`), desconto entre 0 e 100, impedir vincular o mesmo jogo duas vezes à mesma promoção
* `Library`: impedir compra duplicada, cálculo de preço com/sem promoção ativa, preço não pode ser negativo

```bash
dotnet test
```

---

# 🧩 Domain-Driven Design (DDD)

* Entidades ricas com invariantes protegidas (construtores privados + factory methods retornando `Result<T>`, nunca setters públicos)
* Value Objects: `Email`, `Password` (módulo Users)
* Agregados: `User`, `Game`, `Promotion` (com `GamePromotion` como entidade filha), `UserGame`
* Repositórios definidos no Domain, implementados na Infrastructure
* Event Storming dos fluxos de criação de usuários e criação de jogos: ver documentação separada (Miro).

---

# 📈 Melhorias Futuras (próximas fases)

* Matchmaking e gerenciamento de servidores
* MongoDB / Dapper para consultas de alta performance
* GraphQL para filtragem avançada do catálogo
* BDD com Gherkin em módulos adicionais

---

# 👥 Equipe

**Grupo:** *(preencher)*

Integrantes:

* *(preencher)*

---

# 📄 Licença

Projeto desenvolvido exclusivamente para fins acadêmicos no **Tech Challenge - FIAP**.
