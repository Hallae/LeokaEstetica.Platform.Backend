using LeokaEstetica.Platform.Models.Entities.Configs;
using LeokaEstetica.Platform.Models.Entities.Project;

namespace LeokaEstetica.Platform.Database.Abstractions.Project;

/// <summary>
/// Абстракция репозитория пользователя.
/// </summary>
public interface IProjectRepository
{
    /// <summary>
    /// Метод создает новый проект пользователя.
    /// </summary>
    /// <param name="projectName">Название проекта.</param>
    /// <param name="projectDetails">Описание проекта.</param>
    /// <param name="userId">Id пользователя.</param>
    /// <returns>Данные нового проекта.</returns>
    Task<ProjectEntity> CreateProjectAsync(string projectName, string projectDetails, long userId);
    
    /// <summary>
    /// Метод получает названия полей для таблицы проектов пользователя.
    /// Все названия столбцов этой таблицы одинаковые у всех пользователей.
    /// </summary>
    /// <returns>Список названий полей таблицы.</returns>
    Task<IEnumerable<ColumnNameEntity>> UserProjectsColumnsNamesAsync();
}