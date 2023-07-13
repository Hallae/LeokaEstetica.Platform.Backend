using LeokaEstetica.Platform.Models.Dto.Output.User;

namespace LeokaEstetica.Platform.Services.Abstractions.User;

/// <summary>
/// Абстракция сервиса пользователей.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Метод создает нового пользователя.
    /// </summary>
    /// <param name="password">Пароль. Он не хранится в БД. Хранится только его хэш.</param>
    /// <param name="email">Почта пользователя.</param>
    /// <returns>Данные пользователя.</returns>
    Task<UserSignUpOutput> CreateUserAsync(string password, string email);

    /// <summary>
    /// Метод подтверждает аккаунт пользователя по коду, который ранее был отправлен пользователю на почту и записан в БД.
    /// </summary>
    /// <param name="code">Код подтверждения.</param>
    /// <returns>Статус подтверждения.</returns>
    Task<bool> ConfirmAccountAsync(Guid code);

    /// <summary>
    /// Метод авторизует пользователя.
    /// </summary>
    /// <param name="email">Email.</param>
    /// <param name="password">Пароль.</param>
    /// <returns>Данные авторизации.</returns>
    Task<UserSignInOutput> SignInAsync(string email, string password);

    /// <summary>
    /// Метод обновляет токен.
    /// </summary>
    /// <param name="account">Аккаунт.</param>
    /// <returns>Новые данные авторизации.</returns>
    Task<UserSignInOutput> RefreshTokenAsync(string account);

    /// <summary>
    /// Метод авторизации через Google. Если аккаунт не зарегистрирован в системе,
    /// то создаем также аккаунт используя данные аккаунта Google пользователя.
    /// </summary>
    /// <param name="googleAuthToken">Токен с данными пользователя.</param>
    /// <returns>Данные пользователя.</returns>
    Task<UserSignInOutput> SignInAsync(string googleAuthToken);

    /// <summary>
    /// Метод авторизации через VK. Если аккаунт не зарегистрирован в системе,
    /// то создаем также аккаунт используя данные аккаунта DR пользователя.
    /// </summary>
    /// <param name="vkUserId">Id пользователя в системе ВК.</param>
    /// <param name="firstName">Имя пользователя в системе ВК.</param>
    /// <param name="firstName">Фамилия пользователя в системе ВК.</param>
    /// <returns>Данные пользователя.</returns>
    Task<UserSignInOutput> SignInAsync(long vkUserId, string firstName, string lastName);

    /// <summary>
    /// Метод отправляет код пользователю на почту для восстановления пароля.
    /// <param name="account">Аккаунт.</param>
    /// <param name="token">Токен.</param>
    /// <returns>Признак успешного прохождения проверки.</returns>
    /// </summary>
    Task<bool> SendCodeRestorePasswordAsync(string account, string token);
    
    /// <summary>
    /// Метод проверяет доступ к восстановлению пароля пользователя.
    /// </summary>
    /// <param name="publicKey">Публичный код, который ранее высалался на почту пользователю.</param>
    /// <param name="account">Аккаунт.</param>
    /// <returns>Признак успешного прохождения проверки.</returns>
    Task<bool> CheckRestorePasswordAsync(Guid publicKey, string account);

    /// <summary>
    /// Метод запускает восстановление пароля пользователя.
    /// </summary>
    /// <param name="password">Новый пароль.</param>
    /// <param name="account">Аккаунт.</param>
    /// <param name="token">Токен.</param>
    Task RestoreUserPasswordAsync(string password, string account, string token);
}