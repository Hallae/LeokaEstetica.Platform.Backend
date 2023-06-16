using System.Runtime.CompilerServices;
using LeokaEstetica.Platform.Core.Enums;
using LeokaEstetica.Platform.Core.Exceptions;
using LeokaEstetica.Platform.Database.Abstractions.Commerce;
using LeokaEstetica.Platform.Database.Abstractions.FareRule;
using LeokaEstetica.Platform.Database.Abstractions.Orders;
using LeokaEstetica.Platform.Database.Abstractions.Subscription;
using LeokaEstetica.Platform.Database.Abstractions.User;
using LeokaEstetica.Platform.Models.Dto.Common.Cache;
using LeokaEstetica.Platform.Models.Dto.Input.Commerce;
using LeokaEstetica.Platform.Models.Dto.Output.Commerce;
using LeokaEstetica.Platform.Processing.Abstractions.Commerce;
using LeokaEstetica.Platform.Redis.Abstractions.Commerce;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("LeokaEstetica.Platform.Tests")]

namespace LeokaEstetica.Platform.Processing.Services.Commerce;

/// <summary>
/// TODO: Отрефачить разбив логику заказов в отдельный сервис OrderService.
/// Класс реализует методы сервиса коммерции.
/// </summary>
internal sealed class CommerceService : ICommerceService
{
    private readonly ICommerceRedisService _commerceRedisService;
    private readonly ILogger<CommerceService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IFareRuleRepository _fareRuleRepository;
    private readonly ICommerceRepository _commerceRepository;
    private readonly IOrdersRepository _ordersRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;

    /// <summary>
    /// Конструктор.
    /// <param name="commerceRedisService">Сервис кэша коммерции.</param>
    /// <param name="logger">Сервис логера.</param>
    /// <param name="userRepository">Репозиторий пользователя.</param>
    /// <param name="fareRuleRepository">Репозиторий правил тарифов.</param>
    /// <param name="fareRuleRepository">Репозиторий коммерции.</param>
    /// <param name="ordersRepository">Репозиторий заказов.</param>
    /// <param name="subscriptionRepository">Репозиторий подписок.</param>
    /// <param name="commerceService">Сервис коммерции.</param>
    /// </summary>
    public CommerceService(ICommerceRedisService commerceRedisService, 
        ILogger<CommerceService> logger, 
        IUserRepository userRepository, 
        IFareRuleRepository fareRuleRepository, 
        ICommerceRepository commerceRepository, 
        IOrdersRepository ordersRepository, 
        ISubscriptionRepository subscriptionRepository)
    {
        _commerceRedisService = commerceRedisService;
        _logger = logger;
        _userRepository = userRepository;
        _fareRuleRepository = fareRuleRepository;
        _commerceRepository = commerceRepository;
        _ordersRepository = ordersRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    #region Публичные методы.

    // <summary>
    /// Метод создает заказ в кэше.
    /// </summary>
    /// <param name="createOrderCache">Модель заказа для хранения в кэше.</param>
    /// <param name="account">Аккаунт.</param>
    /// <returns>Данные заказа добавленного в кэш.</returns>
    public async Task<CreateOrderCache> CreateOrderCacheAsync(CreateOrderCacheInput createOrderCacheInput,
        string account)
    {
        try
        {
            var userId = await _userRepository.GetUserByEmailAsync(account);

            if (userId <= 0)
            {
                var ex = new NotFoundUserIdByAccountException(account);
                throw ex;
            }

            // Сохраняем заказ в кэш сроком на 2 часа.
            var publicId = createOrderCacheInput.PublicId;
            var key = await _commerceRedisService.CreateOrderCacheKeyAsync(userId, publicId);
            var orderToCache = await CreateOrderCacheResult(publicId, createOrderCacheInput.PaymentMonth, userId);
            
            var result = await _commerceRedisService.CreateOrderCacheAsync(key, orderToCache);

            return result;
        }
        
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Метод получает услуги и сервисы заказа из кэша.
    /// </summary>
    /// <param name="publicId">Публичный код тарифа.</param>
    /// <param name="account">Аккаунт.</param>
    /// <returns>Услуги и сервисы заказа.</returns>
    public async Task<CreateOrderCache> GetOrderProductsCacheAsync(Guid publicId, string account)
    {
        try
        {
            var userId = await _userRepository.GetUserByEmailAsync(account);

            if (userId <= 0)
            {
                var ex = new NotFoundUserIdByAccountException(account);
                throw ex;
            }
            
            var key = await _commerceRedisService.CreateOrderCacheKeyAsync(userId, publicId);
            var result = await _commerceRedisService.GetOrderCacheAsync(key);

            return result;
        }
        
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Метод вычисляет сумму с оставшихся дней подписки пользователя.
    /// </summary>
    /// <param name="userId">Id пользователя.</param>
    /// <param name="orderId">Id заказа.</param>
    /// <returns>Сумма.</returns>
    public async Task<decimal> CalculatePriceSubscriptionFreeDaysAsync(long userId, long orderId)
    {
        // Вычисляем, сколько прошло дней использования подписки у пользователя.
        var usedDays = await _userRepository.GetUserSubscriptionUsedDateAsync(userId);
        
        // Если одна из дат пустая, то не можем вычислить сумму возврата. Возврат не делаем в таком кейсе.
        if (!usedDays.StartDate.HasValue || !usedDays.EndDate.HasValue)
        {
            _logger.LogWarning("Невозможно вычислить сумму возврата. Одна из дат подписки либо обе были null. " +
                              $"UserId: {userId}. " +
                              $"OrderId: {orderId}");
            return 0;
        }
        
        // Вычисляем кол-во дней, за которые можем учесть ДС пользователя при оплате новой подписки.
        var referenceUsedDays = (int)Math.Round(usedDays.EndDate.Value.Subtract(usedDays.StartDate.Value)
            .TotalDays);

        // Получаем по какой цене был оформлен заказ.
        var orderPrice = (await _ordersRepository.GetOrderDetailsAsync(orderId, userId)).Price;

        // Вычисляем сумму остатка.
        var resultRefundPrice = orderPrice * referenceUsedDays / 100;
        
        // Не можем вычислять остаток себе в ущерб.
        if (resultRefundPrice == 0)
        {
            _logger.LogWarning($"Невозможно сделать возврат на сумму: {resultRefundPrice}. Возврат не будет сделан." +
                               $"UserId: {userId}. " +
                               $"OrderId: {orderId}");
        }

        // Исключительная ситуация, сразу логируем такое.
        if (resultRefundPrice < 0)
        {
            _logger.LogError($"Сумма возврата была отрицательной: {resultRefundPrice}. Возврат не будет сделан." +
                             $"UserId: {userId}. " +
                             $"OrderId: {orderId}");
        }

        return resultRefundPrice;
    }

    /// <summary>
    /// Метод вычисляет, есть ли остаток с прошлой подписки пользователя для учета ее как скидку при оформлении новой подписки.
    /// </summary>
    /// <param name="account">Аккаунт.</param>
    /// <param name="publicId">Публичный ключ тарифа.</param>
    /// <param name="month">Кол-во месяцев подписки.</param>
    /// <returns>Сумма остатка, если она есть.</returns>
    public async Task<OrderFreeOutput> CheckFreePriceAsync(string account, Guid publicId, short month)
    {
        try
        {
            var userId = await _userRepository.GetUserByEmailAsync(account);

            if (userId <= 0)
            {
                var ex = new NotFoundUserIdByAccountException(account);
                throw ex;
            }
            
            // Проверяем, есть ли у пользователя действующая платная подписка.
            var subscription = await _subscriptionRepository.GetUserSubscriptionAsync(userId);

            if (subscription is null)
            {
                throw new InvalidOperationException($"Не удалось получить подписку. UserId: {userId}");
            }

            var subscriptionId = subscription.SubscriptionId;
            var userSubscription = await _subscriptionRepository.GetUserSubscriptionBySubscriptionIdAsync(
                subscriptionId, userId);

            if (userSubscription is null)
            {
                throw new InvalidOperationException("Не удалось получить подписку пользователя." +
                                                    $" UserId: {userId}." +
                                                    $"SubscriptionId: {subscriptionId}");
            }

            // Находим Id заказа текущей подписки пользователя.
            var orderId = await _ordersRepository.GetUserOrderIdAsync(userSubscription.MonthCount, userId);
            
            // Вычисляем остаток суммы подписки (пока без учета повышения/понижения подписки).
            var freePrice = await CalculatePriceSubscriptionFreeDaysAsync(userId, orderId);

            var result = new OrderFreeOutput
            {
                FreePrice = freePrice
            };

            if (freePrice == 0)
            {
                return result;
            }
            
            // Проверяем повышение/понижение подписки.
            // Находим тариф.
            var fareRule = await _fareRuleRepository.GetByPublicIdAsync(publicId);
            var calcPrice = await CalculateServicePriceAsync(month, fareRule.Price);

            // Если сумма тарифа больше суммы остатка с текущей подписки пользователя,
            // то это и будет его выгода и мы учтем это при переходе на другую подписку.
            if (calcPrice > freePrice)
            {
                result.FreePrice = calcPrice - freePrice;
            }

            result.Price = calcPrice;

            return result;
        }
        
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            throw;
        }
    }

    #endregion

    #region Приватные методы.
    
    /// <summary>
    /// Метод создает модель заказа для кэша.
    /// </summary>
    /// <param name="publicId">Публичный код тарифа.</param>
    /// <param name="paymentMonth">Кол-во месяцев подписки.</param>
    /// <param name="userId">Id пользователя.</param>
    /// <returns>Результирующая модель.</returns>
    private async Task<CreateOrderCache> CreateOrderCacheResult(Guid publicId, short paymentMonth, long userId)
    {
        var rule = await _fareRuleRepository.GetByPublicIdAsync(publicId);

        if (rule is null)
        {
            var ex = new InvalidOperationException($"Не удалось найти правило тарифа. PublicId: {publicId}");
            throw ex;
        }

        var products = new List<string>();

        var discount = await GetPercentDiscountAsync(paymentMonth, DiscountTypeEnum.Service);
        var rulePrice = rule.Price;
        var servicePrice = await CalculateServicePriceAsync(paymentMonth, rulePrice);
        var discountPrice = await CalculatePercentPriceAsync(discount, servicePrice);

        // Если была применена скидка.
        if (discountPrice < servicePrice)
        {
            products.Add($"Скидка на тариф {discount}");
        }

        var result = new CreateOrderCache
        {
            RuleId = rule.RuleId,
            Month = paymentMonth,
            Percent = discount,
            Price = discountPrice,
            UserId = userId,
            Products = products,
            FareRuleName = rule.Name
        };

        return result;
    }

    /// <summary>
    /// Метод получает скидку на услугу по ее типу и кол-ву месяцев.
    /// </summary>
    /// <param name="paymentMonth">Кол-во месяцев.</param>
    /// <param name="discountTypeEnum">Тип скидки на услугу</param>
    /// <returns>Скидка на услугу.</returns>
    private async Task<decimal> GetPercentDiscountAsync(short paymentMonth, DiscountTypeEnum discountTypeEnum)
    {
        var result = await _commerceRepository.GetPercentDiscountAsync(paymentMonth, discountTypeEnum);

        return result;
    }

    /// <summary>
    /// Метод вычисляет сумму с учетом скидки.
    /// Если цена null.
    /// </summary>
    /// <param name="percent">% скидки.</param>
    /// <param name="price">Сумму без скидки.</param>
    /// <returns>Сумма с учетом скидки.</returns>
    private async Task<decimal> CalculatePercentPriceAsync(decimal percent, decimal price)
    {
        // Если нет скидки, то оставляем цену такой же.
        if (percent == 0)
        {
            return price;
        }

        return await Task.FromResult(price - Math.Round(price * percent / 100));
    }

    /// <summary>
    /// Метод вычисляет сумму сервиса от кол-ва месяцев подписки.
    /// </summary>
    /// <param name="month">Кол-во месяцев подписки.</param>
    /// <param name="price">Цена.</param>
    /// <returns>Цена.</returns>
    private async Task<decimal> CalculateServicePriceAsync(short month, decimal price)
    {
        return await Task.FromResult(price * month);
    }

    #endregion
}