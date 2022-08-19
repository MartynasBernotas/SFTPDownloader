using Microsoft.EntityFrameworkCore;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using SftpDownloader.Database;
using SftpDownloader.Models;

namespace SftpDownloader.Services
{
    public class FilesService : IFilesService
    {
        private readonly ILogger<FilesService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDbContextFactory<DatabaseContext> _dbContext;

        public FilesService(ILogger<FilesService> logger, IConfiguration configuration, IDbContextFactory<DatabaseContext> dbContext)
        {
            _logger = logger;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public async Task DownloadNewFiles()
        {
            _logger.LogInformation("Starting scanning a SFTP server for new files");

            try
            {
                //Opening connection with SFTP client
                using SftpClient client = new(_configuration["SftpHost"], int.Parse(_configuration["SftpPort"]), _configuration["SftpUsername"], _configuration["SftpPassword"]);
                client.Connect();

                var files = client.ListDirectory(_configuration["ClientFilesDirectory"]); //Listing all files in SFTP server directory
                files = files.Where(x => !x.IsDirectory); //Filtering from directories disguised as files (i.e. '.', '..')
                files = await FilterDownloadedFiles(files); //Filtering from already downloaded files

                if (!files.Any())
                {
                    _logger.LogInformation("0 new files not found. Skipping");
                    return;
                }

                //Creating asynchronous download tasks from files
                var downloadTasks = files.Select(x => DownloadFile(x, client));

                //Starting all async download tasks
                _logger.LogInformation("Found {count} new files. Starting downloading", downloadTasks.Count());
                await Task.WhenAll(downloadTasks);

                //Ending connection with SFTP client
                client.Disconnect();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task DownloadFile(SftpFile file, SftpClient client)
        {
            //Creating filepath
            var filePath = Path.Combine(_configuration["FilesStoragePath"], file.Name);

            //Downloading and writing files, and closing stream
            using (Stream fileStream = File.OpenWrite(filePath))
            {
                _logger.LogInformation("Downloading {} file", file.Name);
                client.DownloadFile(file.Name, fileStream);
            }

            //Checking if downloaded succeeded, if yes - saving record into database
            if (File.Exists(filePath))
            {
                await SaveFileRecord(file.ToFileRecord(Guid.NewGuid(), filePath));
            }
            else
            {
                _logger.LogError("File download failed. File record was not added to database");
            }

        }

        private async Task SaveFileRecord(FileRecord file)
        {
            try
            {
                //Creating database context - adding file record - saving transaction
                using var ctx = await _dbContext.CreateDbContextAsync();
                ctx.FileRecords.Add(file);
                await ctx.SaveChangesAsync();

                _logger.LogInformation("File {} was downloaded. Record added to database", file.Name);
            }
            catch (Exception e)
            {
                _logger.LogError("An exception has been caught while saving file record " + e.ToString());
                //not rethrowing exception as we will try to add record on the next cycle
            }
        }

        private async Task<IEnumerable<SftpFile>> FilterDownloadedFiles(IEnumerable<SftpFile> sftpFiles)
        {
            //Creating database context and listing all files records
            try
            {
                using var ctx = await _dbContext.CreateDbContextAsync();
                var allDownloadedFiles = await ctx.FileRecords.ToListAsync();

                //Filtering new files if name and creation date do not match any of already existing files records
                var newFiles = sftpFiles.Where(sf => !allDownloadedFiles.Any(df => df.Name == sf.Name && df.CreatedOn == sf.LastWriteTimeUtc)); //Using LastWriteTimeUtc as file creation time
                return newFiles;
            } 
            catch(Exception e)
            {
                //Rethrowing database related exception - as DB most probably won't be available for further actions
                _logger.LogError("An exception has been caught while listing files records");
                throw;
            }
           
        }
    }
}
