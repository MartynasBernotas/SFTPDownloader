using SftpDownloader;
using SftpDownloader.Database;
using SftpDownloader.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddDbContextFactory<DatabaseContext>(); //Adding database factory to dependency injection
        services.AddSingleton<IFilesService, FilesService>(); //Adding main file service to dependency injection
        services.AddHostedService<WorkerService>(); //Adding main background worker service 
    })
    .Build();

await host.RunAsync(); //Starting service

Console.ReadKey(); //Prevent console from closing
