using FileUpload.Models;

namespace FileUpload.ViewModel
{
    public class FileUploadViewModel
    {
        public List<FileOnDatabase> FilesOnDatabase { get; set; }

        public List<FileOnFileSystem> FilesOnFileSystem { get; set; }
    }
}
