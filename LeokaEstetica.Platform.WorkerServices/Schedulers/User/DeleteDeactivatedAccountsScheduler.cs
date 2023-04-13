using LeokaEstetica.Platform.WorkerServices.Jobs.User;
using Quartz;
using Quartz.Impl;

namespace LeokaEstetica.Platform.WorkerServices.Schedulers.User;

/// <summary>
/// Класс планировщика работы джобы DeleteDeactivatedAccounts.
/// </summary>
public class DeleteDeactivatedAccountsScheduler
{
    /// <summary>
    /// Метод настраивает и запускает джобу.
    /// </summary>
    public static async void Start()
    {
        var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        await scheduler.Start();

        var job = JobBuilder.Create<DeleteDeactivatedAccountsJob>().Build();

        // Создаем триггер.
        // Идентифицируем триггер с именем и группой.
        // Запуск сразу после начала выполнения.
        // Настраиваем выполнение действия.
        // Раз в сутки.
        // Бесконечное повторение.
        // Создаем триггер.
        var trigger = TriggerBuilder.Create()
            .WithIdentity("DeleteDeactivatedAccountsJobTrigger", "DeleteDeactivatedAccountsJobGroup")
            .StartNow()
            .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever())
            .Build();

        // Начинаем выполнение работы.
        await scheduler.ScheduleJob(job, trigger);
    }
}