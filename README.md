# Leoka.Estetica.Platform.Backend- платформа для бизнеса.

## Возможности платформы:
- Находить проекты, которые Вам интересны.
- Создавать вакансии в ваши проекты, на которые люди будут откликаться.
- Создавать проекты.
- Просматривать анкеты пользователей в базе резюме.
- Управлять вашими проектами на доске задач. Этот модуль будет под управлением нейросети. Это платный модуль (имеет бесплатный тариф).
- Вести документацию вашего проекта. Этот модуль будет под управлением нейросети. Это платный модуль (имеет бесплатный тариф).
- Создавать ТЗ (техническое задание) вашего проекта прямо на нашей платформе. Этот модуль будет под управлением нейросети. Это платный модуль.
- Тестировать навыки, знания и опыт специалистов в модуле “Тестирование специалистов” по разным профессиям. Этот модуль будет под управлением нейросети. Это платный модуль.
- HR-система для удобного управления отпусками сотрудников, удобного управления персоналом. Этот модуль будет под управлением нейросети. Это платный модуль.

# Стек проекта бэка:
<strong>Серверный язык:</strong> C#, ASP.NET Core Web API (.NET 6).<br/>
<strong>Tests:</strong> NUnit (юнит-тесты + интеграционные).<br/>
<strong>ORM:</strong> LINQ, Entity Framework Core. Но переходим полностью на Dapper (EF выпилим).<br/>
<strong>SQL:</strong> Postgres.<br/>
<strong>NoSQL:</strong> Redis.<br/>
<strong>DI:</strong> Autofac.<br/>
<strong>CI/CD:</strong> Github, TeamCity.<br/>
<strong>Работа с очередями сообщений:</strong> RabbitMQ.<br/>
<strong>Real-time:</strong> SignalR.<br/>
<strong>OC:</strong> Linux (CentOS 7).<br/>
<strong>Хранение логов и метрики:</strong> ClickHouse.<br/>
<strong>Мониторинг:</strong> Grafana.<br/>
<strong>Для поиска в памяти используем:</strong> Lucene.NET.<br/>
<strong>Работа в фоне:</strong> Worker Services (BackgroundService).<br/>
<strong>Ведение задач и управление проектом, командой:</strong> Kaiten.<br/>
