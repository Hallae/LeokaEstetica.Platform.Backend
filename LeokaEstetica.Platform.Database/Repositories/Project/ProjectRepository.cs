using System.Data;
using LeokaEstetica.Platform.Core.Data;
using LeokaEstetica.Platform.Database.Abstractions.Project;
using LeokaEstetica.Platform.Models.Dto.Output.Project;
using LeokaEstetica.Platform.Models.Entities.Configs;
using LeokaEstetica.Platform.Models.Entities.Moderation;
using LeokaEstetica.Platform.Models.Entities.Project;
using Microsoft.EntityFrameworkCore;

namespace LeokaEstetica.Platform.Database.Repositories.Project;

/// <summary>
/// Класс реализует метод репозитория проектов.
/// </summary>
public sealed class ProjectRepository : IProjectRepository
{
    private readonly PgContext _pgContext;
    
    public ProjectRepository(PgContext pgContext)
    {
        _pgContext = pgContext;
    }

    /// <summary>
    /// Метод создает новый проект пользователя.
    /// </summary>
    /// <param name="projectName">Название проекта.</param>
    /// <param name="projectDetails">Описание проекта.</param>
    /// <param name="userId">Id пользователя.</param>
    /// <param name="statusSysName">Системное название статуса.</param>
    /// <param name="statusId">Id статуса.</param>
    /// <param name="statusName">Русское название статуса.</param>
    /// <returns>Данные нового проекта.</returns>
    public async Task<UserProjectEntity> CreateProjectAsync(string projectName, string projectDetails, long userId, string statusSysName, int statusId, string statusName)
    {
        var transaction = await _pgContext.Database
            .BeginTransactionAsync(IsolationLevel.ReadCommitted);
        
        try
        {
            var project = new UserProjectEntity
            {
                ProjectName = projectName,
                ProjectDetails = projectDetails,
                UserId = userId,
                ProjectCode = Guid.NewGuid(),
                DateCreated = DateTime.UtcNow
            };
            await _pgContext.UserProjects.AddAsync(project);
            await _pgContext.SaveChangesAsync();
            
            // Проставляем проекту статус "На модерации".
            await _pgContext.ProjectStatuses.AddAsync(new ProjectStatusEntity
            {
                ProjectId = project.ProjectId,
                StatusId = statusId,
                ProjectStatusSysName = statusSysName,
                ProjectStatusName = statusName
            });
            await _pgContext.SaveChangesAsync();
            
            // Отправляем проект на модерацию.
            await _pgContext.ModerationProjects.AddAsync(new ModerationProjectEntity
            {
                DateModeration = DateTime.UtcNow,
                ProjectId = project.ProjectId
            });
            await _pgContext.SaveChangesAsync();
            
            await transaction.CommitAsync();

            return project;
        }

        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Метод получает названия полей для таблицы проектов пользователя.
    /// Все названия столбцов этой таблицы одинаковые у всех пользователей.
    /// </summary>
    /// <returns>Список названий полей таблицы.</returns>
    public async Task<IEnumerable<ColumnNameEntity>> UserProjectsColumnsNamesAsync()
    {
        var result = await _pgContext.ColumnsNames
            .OrderBy(o => o.Position)
            .ToListAsync();

        return result;
    }

    /// <summary>
    /// Метод проверяет, создан ли уже такой заказ под текущим пользователем с таким названием.
    /// </summary>
    /// <param name="projectName">Название проекта.</param>
    /// <param name="userId">Id пользователя.</param>
    /// <returns>Создал либо нет.</returns>
    public async Task<bool> CheckCreatedProjectByProjectNameAsync(string projectName, long userId)
    {
        var result = await _pgContext.UserProjects
            .AnyAsync(p => p.UserId == userId 
                           && p.ProjectName.Equals(projectName));

        return result;
    }

    /// <summary>
    /// Метод получает список проектов пользователя.
    /// </summary>
    /// <param name="userId">Id пользователя.</param>
    /// <returns>Список проектов.</returns>
    public async Task<IEnumerable<UserProjectOutput>> UserProjectsAsync(long userId)
    {
        var result = await _pgContext.ProjectStatuses
            .Include(p => p.UserProject)
            .Select(p => new UserProjectOutput
            {
                ProjectName = p.UserProject.ProjectName,
                ProjectDetails = p.UserProject.ProjectDetails,
                ProjectIcon = p.UserProject.ProjectIcon,
                ProjectStatusName = p.ProjectStatusName,
                ProjectStatusSysName = p.ProjectStatusSysName,
                ProjectCode = p.UserProject.ProjectCode
            })
            .ToListAsync();

        return result;
    }
}