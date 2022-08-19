namespace SftpDownloader.Models
{
    public class FileRecord
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public DateTime? CreatedOn { get; set; }

        public FileRecord(Guid id, string? name, string? path, DateTime? createdOn)
        {
            Id = id;
            Name = name;
            Path = path;
            CreatedOn = createdOn;
        }
    }
}
