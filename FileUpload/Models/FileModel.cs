namespace FileUpload.Models
{
    public abstract class FileModel
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? FileType { get; set; }

        public string? UploadedBy { get; set; }

        public string? Extention { get; set; }

        public string? Description { get; set; }

        public DateTime? CreatedOn { get; set; }
    }
}
