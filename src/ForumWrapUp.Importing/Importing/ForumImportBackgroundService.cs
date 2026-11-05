using ForumWrapUp.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ForumWrapUp.Importing;

public sealed class ForumImportBackgroundService : BackgroundService
{
    // Il worker è singleton: apre uno scope per ciclo, così i servizi scoped e il DbContext non vivono quanto il worker.
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ForumImportBackgroundService> _logger;
    private readonly TimeSpan _queueCheckInterval;

    public ForumImportBackgroundService(IServiceScopeFactory serviceScopeFactory, IOptions<ForumImportWorkerOptions> options, ILogger<ForumImportBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;

        TimeSpan queueCheckInterval = options.Value.QueueCheckInterval;
        if (queueCheckInterval <= TimeSpan.Zero)
            throw new InvalidOperationException("ForumImport:QueueCheckInterval must be a positive time span.");

        _queueCheckInterval = queueCheckInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bool queueRecovered = false;

        while (!stoppingToken.IsCancellationRequested)
        {
            bool operationProcessed = false;
            ImportOperation? operation = null;

            try
            {
                // Lo scope dura per il controllo e, se presente, per l'intera importazione; alla chiusura dispone il DbContext.
                await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
                ForumImportOperationManager operationManager = scope.ServiceProvider.GetRequiredService<ForumImportOperationManager>();

                if (!queueRecovered)
                {
                    // Al primo ciclo rimette in coda le operazioni rimaste Running dopo un arresto improvviso.
                    await operationManager.RequeueInterruptedOperationsAsync(stoppingToken);
                    queueRecovered = true;
                }

                operation = await operationManager.TryStartNextQueuedAsync(stoppingToken);
                if (operation is not null)
                {
                    // Risolve il processore dopo aver acquisito il job, così anche un errore di risoluzione può marcarlo Failed.
                    ForumImportProcessor processor = scope.ServiceProvider.GetRequiredService<ForumImportProcessor>();
                    await processor.ProcessAsync(operation, stoppingToken);
                    operationProcessed = true;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                if (operation is not null)
                    await MarkCancelledAsync(operation);

                break;
            }
            catch (Exception exception)
            {
                if (operation is not null)
                    await MarkFailedAsync(operation, exception);
                else
                    _logger.LogError("The forum import queue worker failed with {ExceptionType}.", exception.GetType().Name);
            }

            if (!operationProcessed)
                await Task.Delay(_queueCheckInterval, stoppingToken);
        }
    }

    // Il try ha già disposto lo scope del tentativo; lo stato terminale usa un DbContext nuovo.
    private async Task MarkCancelledAsync(ImportOperation operation)
    {
        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        ForumImportOperationManager operationManager = scope.ServiceProvider.GetRequiredService<ForumImportOperationManager>();

        await operationManager.MarkCancelledAsync(operation.Id);
    }

    private async Task MarkFailedAsync(ImportOperation operation, Exception exception)
    {
        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        ForumImportOperationManager operationManager = scope.ServiceProvider.GetRequiredService<ForumImportOperationManager>();

        await operationManager.MarkFailedAsync(operation.Id, exception);
    }
}
