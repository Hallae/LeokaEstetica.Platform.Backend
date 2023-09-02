using AutoMapper;
using LeokaEstetica.Platform.Database.Abstractions.User;
using LeokaEstetica.Platform.Database.Chat;
using LeokaEstetica.Platform.Models.Dto.Chat.Output;

namespace LeokaEstetica.Platform.Messaging.Builders;

/// <summary>
/// Класс билдера сообщений.
/// </summary>
public static class CreateDialogMessagesBuilder
{
    /// <summary>
    /// Метод создает результат для диалогов.
    /// </summary>
    /// <param name="dialogs">Список диалогов.</param>
    /// <param name="chatRepository">Репозиторий чата.</param>
    /// <param name="userRepository">Репозиторий пользователя.</param>
    /// <param name="userId">Id пользователя.</param>
    /// <param name="mapper">Маппер.</param>
    /// <returns>Список диалогов.</returns>
    public static async Task<List<DialogOutput>> CreateDialogAsync(
        (List<DialogOutput> Dialogs, List<ProfileDialogOutput> ProfileDialogs) dialogs, IChatRepository chatRepository,
        IUserRepository userRepository, long userId, IMapper mapper)
    {
        var result = await CreateDialogsResultAsync(dialogs, chatRepository, userRepository, userId, mapper);

        return result.Dialogs;
    }

    /// <summary>
    /// Метод создает результат для диалогов ЛК.
    /// </summary>
    /// <param name="dialogs">Кортеж со списком диалогов.</param>
    /// <param name="chatRepository">Репозиторий чата.</param>
    /// <param name="userRepository">Репозиторий пользователя.</param>
    /// <param name="userId">Id пользователя.</param>
    /// <param name="mapper">Маппер.</param>
    /// <returns>Кортеж со списком диалогов.</returns>
    public static async Task<List<ProfileDialogOutput>> CreateProfileDialogAsync(
        (List<DialogOutput> Dialogs, List<ProfileDialogOutput> ProfileDialogs) dialogs, IChatRepository chatRepository,
        IUserRepository userRepository, long userId, IMapper mapper)
    {
        var result = await CreateDialogsResultAsync(dialogs, chatRepository, userRepository, userId, mapper);
    
        return result.ProfileDialogs;
    }

    /// <summary>
    /// Метод создает результат для разных моделей диалогов.
    /// </summary>
    /// <param name="dialogs">Кортеж со списком диалогов.</param>
    /// <param name="chatRepository">Репозиторий чата.</param>
    /// <param name="userRepository">Репозиторий пользователя.</param>
    /// <param name="userId">Id пользователя.</param>
    /// <param name="mapper">Маппер.</param>
    /// <returns>Кортеж со списком диалогов.</returns>
    private static async Task<(List<DialogOutput> Dialogs, List<ProfileDialogOutput> ProfileDialogs)>
        CreateDialogsResultAsync((List<DialogOutput> Dialogs, List<ProfileDialogOutput> ProfileDialogs) dialogs,
            IChatRepository chatRepository, IUserRepository userRepository, long userId, IMapper mapper)
    {
        var profileDialogs = mapper.Map<List<ProfileDialogOutput>>(dialogs.Dialogs);
        
        foreach (var dialog in dialogs.Dialogs)
        {
            var dialogId = dialog.DialogId;
            
            // Получаем диалог для ЛК пользователя, так как менять будем и его данные.
            var profileDialog = profileDialogs.Find(d => d.DialogId == dialogId);

            if (profileDialog is null)
            {
                throw new InvalidOperationException($"Не удалось получить диалог для ЛК. DialogId: {dialogId}");
            }
            
            var lastMessage = await chatRepository.GetLastMessageAsync(dialogId);

            // Подтягиваем последнее сообщение для каждого диалога и проставляет после 40 символов ...
            if (lastMessage is not null)
            {
                var lastMsg = lastMessage.Length > 40
                    ? string.Concat(lastMessage.Substring(0, 40), "...")
                    : lastMessage;
                
                dialog.LastMessage = lastMsg;
                profileDialog.LastMessage = lastMsg;
            }

            // Найдет Id участников диалога по DialogId.
            var membersIds = await chatRepository.GetDialogMembersAsync(dialogId);

            if (membersIds == null)
            {
                throw new InvalidOperationException($"Не найдено участников для диалога с DialogId {dialogId}");
            }

            // Записываем имя и фамилию участника диалога, с которым идет общение.
            var otherUserId = membersIds.FirstOrDefault(m => !m.Equals(userId));
            var otherData = await userRepository.GetUserByUserIdAsync(otherUserId);
            var fullName = otherData?.FirstName + " " + otherData?.LastName;
            
            dialog.FullName = fullName;
            profileDialog.FullName = fullName;

            // Если дата диалога совпадает с сегодняшней, то заполнит часы и минуты, иначе оставит их null.
            if (DateTime.UtcNow.ToString("d")
                .Equals(Convert.ToDateTime(dialog.Created).ToString("d")))
            {
                // Запишет только часы и минуты.
                var calcTime = Convert.ToDateTime(dialog.Created).ToString("t");
                
                dialog.CalcTime = calcTime;
                profileDialog.CalcTime = calcTime;
            }

            // Если дата диалога не совпадает с сегодняшней.
            else
            {
                // Записываем только дату.
                var calcShortDate = Convert.ToDateTime(dialog.Created).ToString("d");
                
                dialog.CalcShortDate = calcShortDate;
                profileDialog.CalcShortDate = calcShortDate;
            }

            // Форматируем дату убрав секунды.
            var created = Convert.ToDateTime(dialog.Created).ToString("g");
            
            dialog.Created = created;
            profileDialog.Created = created;

            var id = membersIds.Except(new[] { userId }).FirstOrDefault();

            var user = await userRepository.GetUserByUserIdAsync(id);

            // Если имя и фамилия заполнены, то берем их.
            if (user is not null 
                && !string.IsNullOrEmpty(user.FirstName) 
                && !string.IsNullOrEmpty(user.LastName))
            {
                var fName = user.FirstName + " " + user.LastName;
                
                dialog.FullName = fName;
                profileDialog.FullName = fName;
            }

            // Иначе берем почту.
            else
            {
                var otherFullName = user?.Email;
                
                dialog.FullName = otherFullName;
                profileDialog.FullName = otherFullName;
            }
        }

        return (dialogs.Dialogs, profileDialogs);
    }
}