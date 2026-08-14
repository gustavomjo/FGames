# FIAP Cloud Games API — referência de endpoints

Base path: `/api`. O Swagger fica disponível em `/swagger` quando a aplicação executa em `Development`.

## Autenticação e papéis

- **Público:** não requer token.
- **Autenticado:** requer JWT válido de uma conta ativa.
- **User:** requer a função `User`; usada para compra e biblioteca pessoal.
- **Administrator:** requer a função `Administrator`; usada para gestão de usuários, jogos e promoções.
- No Swagger, informe somente o JWT no botão **Authorize**. Em clientes HTTP, envie
  `Authorization: Bearer <token>`.
- Contas `Inactive` ou `Blocked` não podem acessar endpoints protegidos nem efetuar novo login.

## Convenções importantes

- Datas e horas são retornadas em UTC. Para promoções, envie ISO 8601, por exemplo `2026-08-14T18:00:00Z`.
- IDs de usuário e jogo são GUIDs; IDs de promoção são inteiros.
- Erros de validação retornam 400; recurso inexistente, 404; duplicidade, 409; autenticação inválida, 401;
  falta de permissão, 403.
- Enums aceitam nome ou valor numérico no filtro da rota. Nos corpos JSON, siga o formato exibido pelo Swagger.

## Usuários

### POST `/api/users/register` — público

Cria uma conta comum ativa. O e-mail é aparado, normalizado para minúsculas e deve ser único sem diferenciar caixa.
A senha é persistida como hash. A função sempre será `User`, independentemente do cliente.

Corpo: `name`, `email`, `password`, `birthDate` opcional (`AAAA-MM-DD`).

Respostas: 200 com `UserDto`; 400 para dados inválidos; 409 para e-mail já cadastrado.

### POST `/api/users/login` — público

Valida e-mail e senha e devolve `accessToken`, `expiresAtUtc` e `user`. Contas inativas ou bloqueadas não entram.

Corpo: `email`, `password`. Respostas: 200; 400 para formato inválido; 401 para credenciais inválidas ou conta não ativa.

### GET `/api/users/me` — autenticado

Retorna o perfil do dono do JWT. Não recebe ID. Respostas: 200, 401 ou 404.

### GET `/api/users` — Administrator

Lista todas as contas, incluindo `Active`, `Inactive` e `Blocked`. Respostas: 200, 401 ou 403.

### GET `/api/users/{id}` — Administrator

Consulta qualquer conta pelo GUID. Respostas: 200, 401, 403 ou 404.

### POST `/api/users` — Administrator

Cria uma conta e permite escolher `role`: `User=0` ou `Administrator=1`. A conta nasce ativa e registra o ID do
administrador criador. Respostas: 200, 400, 401, 403 ou 409 para e-mail duplicado.

### PATCH `/api/users/{id}/status` — Administrator

Altera `status`: `Active=0`, `Inactive=1` ou `Blocked=2`. A alteração passa a valer nos endpoints protegidos e no login.
Respostas: 200, 400, 401, 403 ou 404.

## Jogos

Enums:

- `status`: `Draft=0`, `Published=1`, `Inactive=2`;
- `category`: `Action=0`, `Adventure=1`, `RPG=2`, `Strategy=3`, `Sports=4`, `Simulation=5`,
  `Educational=6`, `Other=7`;
- `rating`: `Everyone=0`, `Ten=1`, `Twelve=2`, `Fourteen=3`, `Sixteen=4`, `Eighteen=5`.

### GET `/api/games?status={status}` — público com visão ampliada para Administrator

- Visitante ou `User`, sem filtro: somente `Published`.
- Visitante ou `User`, com `status=Published`: somente `Published`.
- Visitante ou `User`, com `Draft` ou `Inactive`: 403.
- Administrator, sem filtro: todos os status.
- Administrator, com filtro: somente o status solicitado.

A resposta inclui `price`, `discountPercentage` e `finalPrice`; o preço final considera promoção ativa. Um filtro
inválido retorna 400.

### GET `/api/games/{id}` — público com visão ampliada para Administrator

Visitantes e usuários comuns recebem o jogo somente quando ele está `Published`. Para um draft ou inativo, recebem
404, ainda que o ID exista. Administradores podem consultar qualquer status.

### POST `/api/games` — Administrator

Cria um jogo sempre como `Draft` e devolve seu GUID. Guarde esse `id` ou recupere-o pela listagem administrativa.
O nome tem no máximo 150 caracteres e é único após aparar as extremidades e ignorar caixa: `Halo`, ` halo ` e
`HALO` conflitam. O preço não pode ser negativo. Respostas: 200, 400, 401, 403 ou 409.

### PUT `/api/games/{id}` — Administrator

Substitui nome, descrição, categoria, classificação e preço. Não altera o status. O novo nome não pode pertencer a
outro jogo; manter o próprio nome é permitido. Respostas: 200, 400, 401, 403, 404 ou 409.

### POST `/api/games/{id}/publish` — Administrator

Publica um draft. Depois disso, ele aparece no catálogo público e pode ser comprado. Publicar novamente retorna 400.
Respostas: 200, 400, 401, 403 ou 404.

## Promoções

### GET `/api/promotions` — público

Lista somente promoções ativas no instante da consulta. Cada item inclui o percentual e os GUIDs dos jogos associados.
Pode retornar uma lista vazia. Resposta: 200.

### POST `/api/promotions` — Administrator

Cria uma promoção ativa, mas ainda sem jogos. `endDate` deve ser posterior a `startDate`; o desconto deve ser maior
que 0 e no máximo 100. Respostas: 200, 400, 401 ou 403.

### POST `/api/promotions/{promotionId}/games/{gameId}` — Administrator

Associa um jogo publicado à promoção. O mesmo vínculo não pode ser repetido, e as validações impedem conflitos de
períodos promocionais conforme as regras da aplicação. Quando ativa, a promoção afeta `finalPrice` e o valor pago.
Respostas: 200, 400, 401, 403, 404 ou 409.

## Biblioteca

### POST `/api/library/purchase` — User

Corpo: `gameId`. O usuário vem do JWT, portanto não pode comprar em nome de outra pessoa. O jogo precisa estar
`Published`. `pricePaid` congela o preço final no instante da compra, incluindo eventual promoção ativa. Uma segunda
compra do mesmo jogo pelo mesmo usuário retorna 409.

Respostas: 200, 400, 401, 403, 404 ou 409.

### GET `/api/library/mine` — User

Lista exclusivamente as aquisições do dono do JWT. Cada item contém `gameId`, `pricePaid` e `createdAt`.
Pode retornar uma lista vazia. Respostas: 200, 401 ou 403.
