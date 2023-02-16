using AutoMapper;
using LeokaEstetica.Platform.Database.Abstractions.Moderation.Resume;
using LeokaEstetica.Platform.Database.Abstractions.User;
using LeokaEstetica.Platform.Logs.Abstractions;
using LeokaEstetica.Platform.Models.Dto.Output.Moderation.Resume;
using LeokaEstetica.Platform.Moderation.Abstractions.Resume;
using LeokaEstetica.Platform.Moderation.Builders;

namespace LeokaEstetica.Platform.Moderation.Services.Resume;

/// <summary>
/// Класс реализует методы сервиса модерации анкет пользователей.
/// </summary>
public class ResumeModerationService : IResumeModerationService
{
    private readonly ILogService _logService;
    private readonly IResumeModerationRepository _resumeModerationRepository;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="logService">Сервис логера.</param>
    /// <param name="resumeModerationRepository">Репозиторий анкет.</param>
    /// <param name="mapper">Автомаппер.</param>
    /// <param name="userRepository">Репозиторий пользователя..</param>
    public ResumeModerationService(ILogService logService, 
        IResumeModerationRepository resumeModerationRepository, 
        IMapper mapper, 
        IUserRepository userRepository)
    {
        _logService = logService;
        _resumeModerationRepository = resumeModerationRepository;
        _mapper = mapper;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Метод получает список анкет для модерации.
    /// </summary>
    /// <returns>Список анкет.</returns>
    public async Task<ResumeModerationResult> ResumesModerationAsync()
    {
        try
        {
            var result = new ResumeModerationResult();
            var items = await _resumeModerationRepository.ResumesModerationAsync();
            var users = await _userRepository.GetAllAsync();
            result.Resumes = CreateResumesModerationDatesBuilder.Create(items, _mapper, users);

            return result;
        }
        
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex);
            throw;
        }
    }
}