using FGames.Modules.Users.Domain.Enums;

namespace FGames.Modules.Users.Api.Controllers;

/// <summary>Dados para o registro público de um usuário comum.</summary>
/// <param name="Name">Nome do usuário.</param>
/// <param name="Email">E-mail válido; será normalizado para minúsculas.</param>
/// <param name="Password">Senha conforme as regras de validação da aplicação.</param>
/// <param name="BirthDate">Data de nascimento opcional no formato ISO `AAAA-MM-DD`.</param>
public sealed record RegisterUserRequest(string Name, string Email, string Password, DateOnly? BirthDate);

/// <summary>Credenciais de autenticação.</summary>
/// <param name="Email">E-mail da conta; a comparação não diferencia maiúsculas de minúsculas.</param>
/// <param name="Password">Senha da conta.</param>
public sealed record LoginRequest(string Email, string Password);

/// <summary>Dados para criação administrativa de usuário.</summary>
/// <param name="Name">Nome do usuário.</param>
/// <param name="Email">E-mail válido e único.</param>
/// <param name="Password">Senha conforme as regras de validação.</param>
/// <param name="BirthDate">Data de nascimento opcional no formato ISO `AAAA-MM-DD`.</param>
/// <param name="Role">Função: User=0 ou Administrator=1.</param>
public sealed record CreateUserRequest(string Name, string Email, string Password, DateOnly? BirthDate, Role Role);

/// <summary>Nova situação do usuário.</summary>
/// <param name="Status">Situação: Active=0, Inactive=1 ou Blocked=2.</param>
public sealed record SetUserStatusRequest(UserStatus Status);
