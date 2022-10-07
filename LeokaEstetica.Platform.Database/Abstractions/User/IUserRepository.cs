using LeokaEstetica.Platform.Models.Entities.User;

namespace LeokaEstetica.Platform.Database.Abstractions.User;

/// <summary>
/// Абстракция репозитория пользователей.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Метод сохраняет нового пользователя в базу.
    /// </summary>
    /// <param name="user">Данные пользователя для добавления.</param>
    /// <returns>Данные пользователя.</returns>
    Task<UserEntity> SaveUserAsync(UserEntity user);
}