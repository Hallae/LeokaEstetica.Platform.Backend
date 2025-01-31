﻿using Autofac;
using AutoMapper;
using LeokaEstetica.Platform.Access.Services.AvailableLimits;
using LeokaEstetica.Platform.Access.Services.Moderation;
using LeokaEstetica.Platform.Access.Services.User;
using LeokaEstetica.Platform.Base.Factors;
using LeokaEstetica.Platform.Base.Repositories.Chat;
using LeokaEstetica.Platform.Base.Repositories.User;
using LeokaEstetica.Platform.Base.Services.Connection;
using LeokaEstetica.Platform.CallCenter.Services.Project;
using LeokaEstetica.Platform.CallCenter.Services.Resume;
using LeokaEstetica.Platform.CallCenter.Services.Ticket;
using LeokaEstetica.Platform.CallCenter.Services.Vacancy;
using LeokaEstetica.Platform.Core.Data;
using LeokaEstetica.Platform.Core.Utils;
using LeokaEstetica.Platform.Database.Abstractions.ProjectManagment;
using LeokaEstetica.Platform.Database.Repositories.Access.Ticket;
using LeokaEstetica.Platform.Database.Repositories.Access.User;
using LeokaEstetica.Platform.Database.Repositories.AvailableLimits;
using LeokaEstetica.Platform.Database.Repositories.Commerce;
using LeokaEstetica.Platform.Database.Repositories.Config;
using LeokaEstetica.Platform.Database.Repositories.FareRule;
using LeokaEstetica.Platform.Database.Repositories.Knowledge;
using LeokaEstetica.Platform.Database.Repositories.Landing;
using LeokaEstetica.Platform.Database.Repositories.Metrics;
using LeokaEstetica.Platform.Database.Repositories.Moderation.Access;
using LeokaEstetica.Platform.Database.Repositories.Moderation.Project;
using LeokaEstetica.Platform.Database.Repositories.Moderation.Resume;
using LeokaEstetica.Platform.Database.Repositories.Moderation.Vacancy;
using LeokaEstetica.Platform.Database.Repositories.Notification;
using LeokaEstetica.Platform.Database.Repositories.Orders;
using LeokaEstetica.Platform.Database.Repositories.Press;
using LeokaEstetica.Platform.Database.Repositories.Profile;
using LeokaEstetica.Platform.Database.Repositories.Project;
using LeokaEstetica.Platform.Database.Repositories.ProjectManagment;
using LeokaEstetica.Platform.Database.Repositories.Resume;
using LeokaEstetica.Platform.Database.Repositories.Search;
using LeokaEstetica.Platform.Database.Repositories.Subscription;
using LeokaEstetica.Platform.Database.Repositories.Templates;
using LeokaEstetica.Platform.Database.Repositories.TIcket;
using LeokaEstetica.Platform.Database.Repositories.Vacancy;
using LeokaEstetica.Platform.Diagnostics.Services.Metrics;
using LeokaEstetica.Platform.Finder.Services.Project;
using LeokaEstetica.Platform.Finder.Services.Resume;
using LeokaEstetica.Platform.Finder.Services.Vacancy;
using LeokaEstetica.Platform.Integrations.Abstractions.Discord;
using LeokaEstetica.Platform.Integrations.Abstractions.Reverso;
using LeokaEstetica.Platform.Integrations.Services.Discord;
using LeokaEstetica.Platform.Integrations.Services.Reverso;
using LeokaEstetica.Platform.Integrations.Services.Telegram;
using LeokaEstetica.Platform.Messaging.Services.Chat;
using LeokaEstetica.Platform.Messaging.Services.Project;
using LeokaEstetica.Platform.Messaging.Services.RabbitMq;
using LeokaEstetica.Platform.Notifications.Services;
using LeokaEstetica.Platform.Processing.Services.Commerce;
using LeokaEstetica.Platform.Processing.Services.PayMaster;
using LeokaEstetica.Platform.Redis.Services.Commerce;
using LeokaEstetica.Platform.Redis.Services.User;
using LeokaEstetica.Platform.Services.Services.FareRule;
using LeokaEstetica.Platform.Services.Services.Knowledge;
using LeokaEstetica.Platform.Services.Services.Landing;
using LeokaEstetica.Platform.Services.Services.Orders;
using LeokaEstetica.Platform.Services.Services.Press;
using LeokaEstetica.Platform.Services.Services.Profile;
using LeokaEstetica.Platform.Services.Services.Project;
using LeokaEstetica.Platform.Services.Services.ProjectManagment;
using LeokaEstetica.Platform.Services.Services.Refunds;
using LeokaEstetica.Platform.Services.Services.Resume;
using LeokaEstetica.Platform.Services.Services.Search.ProjectManagment;
using LeokaEstetica.Platform.Services.Services.Subscription;
using LeokaEstetica.Platform.Services.Services.User;
using LeokaEstetica.Platform.Services.Services.Vacancy;
using LeokaEstetica.Platform.Services.Strategies.ProjectManagement.AgileObjectSearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ProjectFinderService = LeokaEstetica.Platform.Services.Services.Search.Project.ProjectFinderService;

namespace LeokaEstetica.Platform.Tests;

/// <summary>
/// Базовый класс тестов с настройками конфигурации.
/// </summary>
internal class BaseServiceTest
{
    private IConfiguration AppConfiguration { get; }
    private string PostgreConfigString { get; }

    protected readonly UserService UserService;
    protected readonly ProfileService ProfileService;
    protected readonly ProjectService ProjectService;
    protected readonly VacancyService VacancyService;
    protected readonly VacancyModerationService VacancyModerationService;
    protected readonly ChatService ChatService;
    protected readonly AccessModerationService AccessModerationService;
    protected readonly ProjectModerationService ProjectModerationService;
    protected readonly ProjectCommentsService ProjectCommentsService;
    protected readonly ProjectFinderService ProjectFinderService;
    protected readonly ResumeService ResumeService;
    protected readonly VacancyFinderService VacancyFinderService;
    protected readonly Finder.Services.Project.ProjectFinderService FinderProjectService;
    protected readonly ResumeFinderService ResumeFinderService;
    protected readonly VacancyPaginationService VacancyPaginationService;
    protected readonly ProjectPaginationService ProjectPaginationService;
    protected readonly FareRuleService FareRuleService;
    protected readonly PayMasterService PayMasterService;
    protected readonly SubscriptionService SubscriptionService;
    protected readonly UserBlackListService UserBlackListService;
    protected readonly ResumeModerationService ResumeModerationService;
    protected readonly LandingService LandingService;
    protected readonly KnowledgeService KnowledgeService;
    protected readonly PgContext PgContext;
    protected readonly ProjectModerationRepository ProjectModerationRepository;
    protected readonly CommerceService CommerceService;
    protected readonly OrdersService OrdersService;
    protected readonly UserMetricsService UserMetricsService;
    protected readonly RefundsService RefundsService;
    protected readonly TicketService TicketService;
    protected readonly TelegramBotService TelegramBotService;
    protected readonly ProjectMetricsService ProjectMetricsService;
    protected readonly TelegramService TelegramService;
    protected readonly FareRuleRepository FareRuleRepository;
    protected readonly ChatRepository ChatRepository;
    protected readonly PressService PressService;
    protected readonly ProjectManagmentService ProjectManagmentService;
    protected readonly ReversoService ReversoService;
    protected readonly SearchProjectManagementService SearchProjectManagementService;
    protected readonly BaseSearchAgileObjectAlgorithm BaseSearchSprintTaskAlgorithm;
    protected readonly IProjectManagmentRepository ProjectManagmentRepository;

    protected BaseServiceTest()
    {
        // Настройка тестовых строк подключения.
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
        AppConfiguration = builder.Build();
        PostgreConfigString = AppConfiguration["ConnectionStrings:NpgDevSqlConnection"] ?? string.Empty;
        var container = new ContainerBuilder();

        AutoFac.RegisterMapper(container);
        var mapper = AutoFac.Resolve<IMapper>();

        // Настройка тестовых контекстов.
        var optionsBuilder = new DbContextOptionsBuilder<PgContext>();
        optionsBuilder.UseNpgsql(PostgreConfigString);
        var pgContext = new PgContext(optionsBuilder.Options);

        PgContext = pgContext;
        var npgSqlConnectionFactory = new NpgSqlConnectionFactory(PostgreConfigString);
        var connectionProvider = new ConnectionProvider(npgSqlConnectionFactory);
        
        var optionsForCache = new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions());
        var distributedCache = new MemoryDistributedCache(optionsForCache);
        var userRepository = new UserRepository(pgContext, null, AppConfiguration);
        var profileRepository = new ProfileRepository(pgContext);
        var subscriptionRepository = new SubscriptionRepository(pgContext);
        ChatRepository = new ChatRepository(pgContext);
        var resumeModerationRepository = new ResumeModerationRepository(pgContext);
        var accessUserRepository = new AccessUserRepository(pgContext);
        var accessUserService = new AccessUserService(accessUserRepository);
        var userRedisService = new UserRedisService(distributedCache, mapper);
        FareRuleRepository = new FareRuleRepository(pgContext, AppConfiguration);
        
        var availableLimitsRepository = new AvailableLimitsRepository(pgContext);
        var globalConfigRepository = new GlobalConfigRepository(pgContext, null, AppConfiguration, connectionProvider);
        var discordService = new DiscordService(AppConfiguration, null);
        
        ProjectManagmentRepository = new ProjectManagmentRepository(connectionProvider);

        UserService = new UserService(null, userRepository, mapper, null, pgContext, profileRepository,
            subscriptionRepository, resumeModerationRepository, accessUserService, userRedisService,
            FareRuleRepository, availableLimitsRepository, globalConfigRepository, discordService, null,
            ProjectManagmentRepository);
        ProfileService = new ProfileService(null, profileRepository, userRepository, mapper, null, null,
            accessUserService, resumeModerationRepository, discordService);

        var projectRepository = new ProjectRepository(pgContext, ChatRepository, connectionProvider);
        var projectNotificationsRepository = new ProjectNotificationsRepository(pgContext);
        var vacancyRepository = new VacancyRepository(pgContext);
        var projectNotificationsService = new ProjectNotificationsService(null, null, userRepository, mapper,
            projectNotificationsRepository, null, projectRepository, null, null, vacancyRepository);
        var vacancyModerationRepository = new VacancyModerationRepository(pgContext);
        var vacancyNotificationsService = new VacancyNotificationsService(null, null);
        var availableLimitsService = new AvailableLimitsService(null, availableLimitsRepository);
        
        TelegramBotService = new TelegramBotService(null, AppConfiguration, globalConfigRepository);

        VacancyModerationService = new VacancyModerationService(vacancyModerationRepository, null, mapper, null,
            vacancyRepository, userRepository, projectRepository, null, null, TelegramBotService);
        
        // Тут если нужен будет ProjectService, то тут проблема с порядком следования.
        // Не получится сделать просто, VacancyService и ProjectService нужны друг другу тесно.
        VacancyService = new VacancyService(null, vacancyRepository, mapper, null, userRepository,
            VacancyModerationService, subscriptionRepository, FareRuleRepository, availableLimitsService,
            vacancyNotificationsService, null, null, null, vacancyModerationRepository, discordService);

        var projectResponseRepository = new ProjectResponseRepository(pgContext);

        ChatService = new ChatService(null, userRepository, projectRepository, vacancyRepository, ChatRepository,
            mapper, projectResponseRepository);

        var accessModerationRepository = new AccessModerationRepository(pgContext);

        AccessModerationService = new AccessModerationService(null, accessModerationRepository, userRepository);

        ProjectModerationRepository = new ProjectModerationRepository(pgContext);

        ProjectModerationService = new ProjectModerationService(ProjectModerationRepository, null, mapper, null, 
            userRepository, projectRepository, null, null, TelegramBotService);

        var projectCommentsRepository = new ProjectCommentsRepository(pgContext);

        ProjectCommentsService = new ProjectCommentsService(null, userRepository, projectCommentsRepository, null, null,
            null);
        ResumeModerationService = new ResumeModerationService(null, resumeModerationRepository, mapper,
            userRepository, null);
        ProjectFinderService = new ProjectFinderService(null, userRepository, projectNotificationsService, ResumeModerationService);

        var resumeRepository = new ResumeRepository(pgContext);

        var fillColorProjectsService = new FillColorProjectsService();

        ProjectService = new ProjectService(projectRepository, null, userRepository, mapper,
            projectNotificationsService, VacancyService, vacancyRepository, availableLimitsService,
            subscriptionRepository, FareRuleRepository, VacancyModerationService, projectNotificationsRepository, null,
            accessUserService, fillColorProjectsService, null, ProjectModerationRepository, discordService);
        
        var ordersRepository = new OrdersRepository(pgContext);
        var commerceRepository = new CommerceRepository(pgContext, AppConfiguration);
        var commerceRedisService = new CommerceRedisService(distributedCache);
        var rabbitMqService = new RabbitMqService(AppConfiguration);
        
        PayMasterService = new PayMasterService(null, AppConfiguration, userRepository,
            commerceRepository, accessUserService, null, commerceRedisService, rabbitMqService, mapper, null, null);

        CommerceService = new CommerceService(commerceRedisService, null, userRepository, FareRuleRepository,
            commerceRepository, ordersRepository, subscriptionRepository, availableLimitsService, accessUserService,
            null, null, PayMasterService, mapper, null);

        SubscriptionService = new SubscriptionService(null, userRepository, subscriptionRepository,
            FareRuleRepository);
        var fillColorResumeService = new FillColorResumeService();
        ResumeService = new ResumeService(null, resumeRepository, mapper, subscriptionRepository,
            FareRuleRepository, userRepository, fillColorResumeService, resumeModerationRepository, accessUserService,
            discordService);
        VacancyFinderService = new VacancyFinderService(vacancyRepository, null);
        FinderProjectService = new Finder.Services.Project.ProjectFinderService(projectRepository, null);
        ResumeFinderService = new ResumeFinderService(null, resumeRepository);
        VacancyPaginationService = new VacancyPaginationService(vacancyRepository, null);
        ProjectPaginationService = new ProjectPaginationService(projectRepository, null);
        FareRuleService = new FareRuleService(FareRuleRepository, null);

        var userBlackListService = new UserBlackListRepository(pgContext);
        UserBlackListService = new UserBlackListService(null, userBlackListService);

        var landingRepository = new LandingRepository(pgContext);
        LandingService = new LandingService(null, landingRepository, mapper);

        var KnowledgeRepository = new KnowledgeRepository(pgContext);
        KnowledgeService = new KnowledgeService(KnowledgeRepository, null);

        OrdersService = new OrdersService(null, ordersRepository, userRepository);

        var userMetricsRepository = new UserMetricsRepository(pgContext);
        UserMetricsService = new UserMetricsService(null, userMetricsRepository, userRepository);
        RefundsService = new RefundsService(null, null, subscriptionRepository, userRepository, ordersRepository, null,
            PayMasterService, null, CommerceService, commerceRepository, mapper);

        var ticketRepository = new TicketRepository(pgContext, null);
        var accessTicketRepository = new AccessTicketRepository(pgContext);
        TicketService = new TicketService(ticketRepository, null, userRepository, mapper, accessTicketRepository,
            null);

        ProjectMetricsService = new ProjectMetricsService(projectCommentsRepository, mapper, projectRepository);
        
        TelegramService = new TelegramService(globalConfigRepository, null);

        var pressRepository = new PressRepository(pgContext);
        PressService = new PressService(pressRepository, null);

        var transactionScopeFactory = new TransactionScopeFactory();
        
        var projectManagmentTemplateRepository = new ProjectManagmentTemplateRepository(connectionProvider);
        var projectSettingsConfigRepository = new ProjectSettingsConfigRepository(pgContext);
        ReversoService = new ReversoService(null);
        ProjectManagmentService = new ProjectManagmentService(null, ProjectManagmentRepository, mapper, userRepository,
            projectRepository, discordService, projectManagmentTemplateRepository, transactionScopeFactory,
            projectSettingsConfigRepository, new Lazy<IReversoService>(ReversoService), null, null, UserService);

        var searchProjectManagementRepository = new SearchProjectManagementRepository(connectionProvider);
        SearchProjectManagementService = new SearchProjectManagementService(null,
            searchProjectManagementRepository, ProjectManagmentRepository, projectSettingsConfigRepository,
            userRepository, new Lazy<IDiscordService>(discordService));

        BaseSearchSprintTaskAlgorithm = new BaseSearchAgileObjectAlgorithm();
    }
}