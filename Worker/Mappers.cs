using Renci.SshNet.Sftp;
using SftpDownloader.Models;

namespace SftpDownloader
{
    public static class Mappers
    {
        public static FileRecord ToFileRecord(this SftpFile sftpFile, Guid id, string path)
        {
            return new FileRecord(id, sftpFile.Name, path, sftpFile.LastWriteTimeUtc); //Using LastWriteTimeUtc as file creation time
        }
    }
}
