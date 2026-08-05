# 🎮 FIAP Cloud Games (FCG)

## Tech Challenge - Fase 1

API REST desenvolvida em **.NET 8** para gerenciamento de usuários e biblioteca de jogos digitais da plataforma **FIAP Cloud Games (FCG)**.

O objetivo deste projeto é disponibilizar uma base sólida para as próximas fases da plataforma, permitindo o cadastro de usuários, autenticação segura, gerenciamento de jogos e controle da biblioteca de jogos adquiridos.

---

# 📚 Sumário

* Sobre o Projeto
* Tecnologias Utilizadas
* Arquitetura
* Funcionalidades
* Estrutura do Projeto
* Requisitos
* Como Executar
* Banco de Dados
* Autenticação
* Endpoints
* Testes
* Domain-Driven Design (DDD)
* Melhorias Futuras

---

# 📖 Sobre o Projeto

A **FIAP Cloud Games (FCG)** é uma plataforma de distribuição de jogos digitais voltados para educação em tecnologia.

Nesta primeira fase foi desenvolvido um MVP responsável por:

* Cadastro de usuários
* Autenticação via JWT
* Controle de permissões (Administrador e Usuário)
* Cadastro de jogos
* Biblioteca de jogos adquiridos

Todo o projeto foi desenvolvido utilizando boas práticas de arquitetura, separação de responsabilidades e princípios de Domain-Driven Design (DDD).

---

# 🚀 Tecnologias Utilizadas

* .NET 8
* ASP.NET Core Web API
* Entity Framework Core
* SQL Server *(ou PostgreSQL, conforme configuração do projeto)*
* JWT Authentication
* Swagger / OpenAPI
* FluentValidation
* Serilog
* xUnit
* FluentAssertions

---

# 🏛 Arquitetura

O projeto foi desenvolvido utilizando um **Monólito Modular** inspirado em **Clean Architecture** e **DDD**.

```
src
│
├── FCG.Api
│
├── FCG.Application
│
├── FCG.Domain
│
├── FCG.Infrastructure
│
└── FCG.Tests
```

### Camadas

### API

Responsável por:

* Controllers
* Autenticação
* Middlewares
* Configurações
* Swagger

### Application

Contém:

* Casos de uso
* Serviços
* DTOs
* Validators

### Domain

Contém:

* Entidades
* Regras de negócio
* Interfaces
* Enums

### Infrastructure

Responsável por:

* Entity Framework Core
* Repositórios
* Persistência
* Configurações do banco

### Tests

Projeto contendo testes unitários das principais regras de negócio.

---

# ✅ Funcionalidades

## Usuários

* Cadastro
* Login
* Atualização
* Exclusão *(Administrador)*

### Validações

* Nome obrigatório
* E-mail válido
* Senha segura

A senha deve conter:

* mínimo de 8 caracteres
* letra maiúscula
* letra minúscula
* número
* caractere especial

---

## Autenticação

Autenticação realizada utilizando **JWT (JSON Web Token)**.

Após o login, um token é gerado permitindo acesso aos endpoints protegidos.

---

## Perfis

### Usuário

Pode:

* acessar a plataforma
* visualizar jogos
* comprar jogos
* visualizar sua biblioteca

### Administrador

Pode:

* cadastrar jogos
* editar jogos
* excluir jogos
* administrar usuários

---

## Jogos

O administrador pode:

* criar jogos
* editar jogos
* remover jogos
* listar jogos

---

## Biblioteca

Cada usuário possui sua biblioteca particular.

É possível:

* comprar jogo
* listar jogos adquiridos

---

# 📂 Estrutura do Projeto

```
src/

FCG.Api/

Controllers

Middlewares

Configurations

Extensions

Program.cs

FCG.Application/

Users

Games

Library

Auth

DTOs

Validators

Interfaces

FCG.Domain/

Entities

Enums

Repositories

Shared

FCG.Infrastructure/

Persistence

Repositories

Identity

Configurations

tests/

FCG.Tests/
```

---

# ⚙️ Requisitos

* .NET SDK 8
* SQL Server *(ou PostgreSQL)*
* Visual Studio 2022 ou superior

---

# ▶️ Como Executar

## Clonar o projeto

```bash
git clone https://github.com/seu-usuario/fcg.git
```

---

## Restaurar pacotes

```bash
dotnet restore
```

---

## Aplicar migrations

```bash
dotnet ef database update
```

---

## Executar

```bash
dotnet run
```

---

A API estará disponível em:

```
https://localhost:5001
```

Swagger:

```
https://localhost:5001/swagger
```

---

# 🗄 Banco de Dados

Persistência realizada utilizando **Entity Framework Core**.

Principais entidades:

* Users
* Games
* UserGames

Relacionamentos:

```
User

1 ---- N

UserGames

N ---- 1

Game
```

---

# 🔐 Autenticação

Após realizar o login, será retornado um JWT.

Exemplo:

```json
{
  "token": "eyJhbGciOi..."
}
```

No Swagger, utilize:

```
Bearer {token}
```

---

# 📡 Endpoints

## Auth

| Método | Endpoint           |
| ------ | ------------------ |
| POST   | /api/auth/register |
| POST   | /api/auth/login    |

---

## Usuários

| Método | Endpoint        |
| ------ | --------------- |
| GET    | /api/users      |
| GET    | /api/users/{id} |
| PUT    | /api/users/{id} |
| DELETE | /api/users/{id} |

---

## Jogos

| Método | Endpoint        |
| ------ | --------------- |
| GET    | /api/games      |
| GET    | /api/games/{id} |
| POST   | /api/games      |
| PUT    | /api/games/{id} |
| DELETE | /api/games/{id} |

---

## Biblioteca

| Método | Endpoint             |
| ------ | -------------------- |
| POST   | /api/library/buy     |
| GET    | /api/library/mygames |

---

# 🧪 Testes

Os testes unitários foram desenvolvidos utilizando:

* xUnit
* FluentAssertions

Principais cenários testados:

* Cadastro de usuário válido
* Validação de e-mail
* Validação de senha
* Compra de jogo
* Impedir compra duplicada
* Regras de autorização

Para executar:

```bash
dotnet test
```

---

# 🧩 Domain-Driven Design (DDD)

O domínio foi modelado utilizando os conceitos apresentados durante a disciplina.

Fluxos modelados:

* Cadastro de Usuários
* Cadastro de Jogos
* Compra de Jogos

Também foram definidos:

* Entidades
* Agregados
* Casos de Uso
* Regras de Negócio

A documentação completa encontra-se disponível no Miro.

---

# 📈 Melhorias Futuras

Próximas fases do projeto poderão incluir:

* Matchmaking
* Carrinho de compras
* Promoções
* Pagamentos
* Servidores dedicados
* Sistema de amigos
* Ranking
* Histórico de partidas
* GraphQL
* Cache com Redis
* Mensageria
* Microsserviços

---

# 👥 Equipe

**Grupo:** *(Preencher)*

Integrantes:

* Nome 1
* Nome 2
* Nome 3

---

# 📄 Licença

Projeto desenvolvido exclusivamente para fins acadêmicos no **Tech Challenge - FIAP**.
