using LeokaEstetica.Platform.Access.Enums;
using LeokaEstetica.Platform.Models.Dto.Output.Configs;
using LeokaEstetica.Platform.Models.Dto.Output.Project;
using LeokaEstetica.Platform.Models.Enums;

namespace LeokaEstetica.Platform.Services.Abstractions.Project;

/// <summary>
/// Абстракция сервиса работы с проектами.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Метод создает новый проект пользователя.
    /// </summary>
    /// <param name="projectName">Название проекта.</param>
    /// <param name="projectDetails">Описание проекта.</param>
    /// <param name="account">Аккаунт пользователя.</param>
    /// <param name="projectStage">Стадия проекта.</param>
    /// <returns>Данные нового проекта.</returns>
    Task<CreateProjectOutput> CreateProjectAsync(string projectName, string projectDetails, string account, ProjectStageEnum projectStage);

    /// <summary>
    /// Метод получает названия полей для таблицы проектов пользователя.
    /// Все названия столбцов этой таблицы одинаковые у всех пользователей.
    /// </summary>
    /// <returns>Список названий полей таблицы.</returns>
    Task<IEnumerable<ColumnNameOutput>> UserProjectsColumnsNamesAsync();

    /// <summary>
    /// Метод получает список проектов пользователя.
    /// </summary>
    /// <param name="account">Аккаунт пользователя.</param>
    /// <returns>Список проектов.</returns>
    Task<UserProjectResultOutput> UserProjectsAsync(string account);

    /// <summary>
    /// TODO: Подумать, давать ли всем пользователям возможность просматривать каталог проектов или только тем, у кого есть подписка.
    /// Метод получает список проектов для каталога.
    /// </summary>
    /// <returns>Список проектов.</returns>
    Task<IEnumerable<CatalogProjectOutput>> CatalogProjectsAsync();

    /// <summary>
    /// Метод обновляет проект пользователя.
    /// </summary>
    /// <param name="projectName">Название проекта.</param>
    /// <param name="projectDetails">Описание проекта.</param>
    /// <param name="account">Аккаунт пользователя.</param>
    /// <param name="projectId">Id проекта.</param>
    /// <returns>Данные нового проекта.</returns>
    Task<UpdateProjectOutput> UpdateProjectAsync(string projectName, string projectDetails, string account, long projectId);

    /// <summary>
    /// Метод получает проект для изменения или просмотра.
    /// </summary>
    /// <param name="projectId">Id проекта.</param>
    /// <param name="mode">Режим. Чтение или изменение.</param>
    /// <param name="account">Аккаунт.</param>
    /// <returns>Данные проекта.</returns>
    Task<ProjectOutput> GetProjectAsync(long projectId, ModeEnum mode, string account);
}