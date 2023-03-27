﻿using LeokaEstetica.Platform.Access.Enums;
using LeokaEstetica.Platform.Core.Extensions;
using LeokaEstetica.Platform.Database.Abstractions.FareRule;
using LeokaEstetica.Platform.Database.Abstractions.Subscription;
using LeokaEstetica.Platform.Models.Dto.Output.Project;
using LeokaEstetica.Platform.Services.Abstractions.Project;

namespace LeokaEstetica.Platform.Services.Services.Project;

/// <summary>
/// Класс реализует методы сервиса выделение цветом пользователей.
/// </summary>
public class FillColorProjectsService : IFillColorProjectsService
{
    /// <summary>
    /// Список названий тарифов, которые дают выделение цветом.
    /// </summary>
    private static readonly List<string> _fareRuleTypesNames = new()
    {
        FareRuleTypeEnum.Business.GetEnumDescription(),
        FareRuleTypeEnum.Professional.GetEnumDescription()
    };

    /// <summary>
    /// Метод выделяет цветом пользователей у которых есть подписка выше бизнеса.
    /// </summary>
    public async Task<IEnumerable<CatalogProjectOutput>> SetColorBusinessProjects(
        List<CatalogProjectOutput> projects, ISubscriptionRepository subscriptionRepository,
        IFareRuleRepository fareRuleRepository)

    {
        // Получаем список юзеров для проставления цветов.
        var userIds = projects.Select(p => p.UserId).Distinct();

        // Выбираем список подписок пользователей.
        var userSubscriptions = await subscriptionRepository.GetUsersSubscriptionsAsync(userIds);

        // Получаем список подписок.
        var subscriptions = await subscriptionRepository.GetSubscriptionsAsync();

        // Получаем список тарифов, чтобы взять названия тарифов.
        var fareRules = await fareRuleRepository.GetFareRulesAsync();
        var rules = fareRules.ToList();

        // Выбираем пользователей, у которых есть подписка выше бизнеса. Только их выделяем цветом.
        foreach (var project in projects)
        {
            // Смотрим подписку пользователя.
            var userSubscription = userSubscriptions.Find(s => s.UserId == project.UserId);

            if (userSubscription is null)
            {
                continue;
            }

            var subscriptionId = userSubscription.SubscriptionId;
            var subscription = subscriptions.Find(s => s.ObjectId == subscriptionId);

            if (subscription is null)
            {
                continue;
            }

            // Получаем название тарифа подписки.
            var fareRule = rules.Find(fr => fr.RuleId == subscription.ObjectId);

            if (fareRule is null)
            {
                continue;
            }

            // Подписка позволяет. Проставляем выделение цвета.
            if (_fareRuleTypesNames.Contains(fareRule.Name))
            {
                project.IsSelectedColor = true;
            }
        }
        return projects;
    }
}
