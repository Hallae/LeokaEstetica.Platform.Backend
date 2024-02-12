namespace LeokaEstetica.Platform.ProjectManagment.Models.Input;

/// <summary>
/// Класс входной модели файлов задачи.
/// </summary>
public class ProjectTaskFileInput
{
    /// <summary>
    /// Id проекта.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Id задачи.
    /// </summary>
    public long TaskId { get; set; }
}