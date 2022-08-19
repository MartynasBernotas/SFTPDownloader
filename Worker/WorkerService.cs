using SftpDownloader.Services;

namespace SftpDownloader
{
    public class WorkerService : BackgroundService
    {
        private readonly ILogger<WorkerService> _logger;
        private readonly IFilesService _filesService;

        public WorkerService(ILogger<WorkerService> logger, IFilesService filesService)
        {
            _logger = logger;
            _filesService = filesService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started at: {time}", DateTimeOffset.UtcNow);

            //Starting loop which would repeat the same DownloadNewFiles task, and then wait 1 minute
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {

                    await _filesService.DownloadNewFiles();

                    _logger.LogInformation("Waiting 1 min");
                    await Task.Delay(60000, stoppingToken);
                    
                }
            } 
            catch(Exception e)
            {
                _logger.LogError("An unexpected exception has been caught" + e.ToString());
            }
           
        }
    }
}