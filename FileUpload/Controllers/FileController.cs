using FileUpload.Data;
using FileUpload.Models;
using FileUpload.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FileUpload.Controllers
{
    public class FileController : Controller
    {
        private readonly AppDbContext _dbContext;

        public FileController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IActionResult> Index()
        {
            FileUploadViewModel viewModel = await UploadAllFiles();

            ViewBag.Message = TempData["Message"];

            return View(viewModel);
        }

        private async Task<FileUploadViewModel> UploadAllFiles()
        {
            var viewModel = new FileUploadViewModel();
            viewModel.FilesOnFileSystem = await _dbContext.FilesOnFileSystem.ToListAsync();
            viewModel.FilesOnDatabase = await _dbContext.FilesOnDatabase.ToListAsync();

            return viewModel;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadToFileSystem(List<IFormFile> files, string description)
        {
            foreach (var file in files)
            {
                var FilesFolderPath = Path.Combine(Directory.GetCurrentDirectory() + @"\Files\");
                bool checkFilesFolderExist = System.IO.Directory.Exists(FilesFolderPath);

                if(!checkFilesFolderExist) Directory.CreateDirectory(FilesFolderPath);

                var fileName = Path.GetFileNameWithoutExtension(file.FileName);

                var fileURl = Path.Combine(FilesFolderPath, fileName);
                var extention = Path.GetExtension(file.FileName);

                if (!System.IO.File.Exists(fileURl))
                {
                    using(var stream = new FileStream(fileURl, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    var fileModel = new FileOnFileSystem
                    {
                        CreatedOn = DateTime.UtcNow,
                        Description = description,
                        Extention = extention,
                        FilePath = fileURl,
                        Name = fileName,
                        FileType = file.ContentType
                    };
                    _dbContext.FilesOnFileSystem.Add(fileModel);
                    _dbContext.SaveChanges();
                }
            }
            TempData["Message"] = "File successfully uploaded to File System.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DownloadFileFromFileSystem(int id)
        {
            var file = await _dbContext.FilesOnFileSystem.FirstOrDefaultAsync(x => x.Id == id);
            if (file == null) return NotFound();

            var memory = new MemoryStream();

            using(var stream  = new FileStream(file.FilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            
            memory.Position = 0;

            return File(memory, file.FileType, file.Name + file.Extention);
        }

        public async Task<IActionResult> DeleteFileFromFileSystem(int id)
        {
            var file = await _dbContext.FilesOnFileSystem.FirstOrDefaultAsync(x => x.Id == id);

            if(file == null) return NotFound();

            if (System.IO.File.Exists(file.FilePath))
            {
                System.IO.File.Delete(file.FilePath);
            }

            _dbContext.FilesOnFileSystem.Remove(file);
            _dbContext.SaveChanges();

            TempData["Message"] = $"Removed {file.Name + file.Extention} successfully from File System.";

            return RedirectToAction(nameof(Index));
        }


        //Code For Upload Files To Database

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadToDatabase(List<IFormFile> files, string description)
        {
            foreach(var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var extension = Path.GetExtension(file.FileName);

                var fileModel = new FileOnDatabase
                {
                    CreatedOn = DateTime.UtcNow,
                    Description = description,
                    Extention = extension,
                    FileType = file.ContentType,
                    Name = fileName
                };
                using(var dataStream = new MemoryStream())
                {
                    await file.CopyToAsync(dataStream);
                    fileModel.Data = dataStream.ToArray();
                }
                _dbContext.FilesOnDatabase.Add(fileModel);
                _dbContext.SaveChanges();
            }

            TempData["Message"] = "File successfully uploaded to Database";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DownloadFileFromDatabase(int id)
        {
            var file = await _dbContext.FilesOnDatabase.FirstOrDefaultAsync(x => x.Id == id);

            if (file == null) return NotFound();

            return File(file.Data, file.FileType, file.Name + file.Extention);
        }

        public async Task<IActionResult> DeleteFileFromDatabase(int id)
        {
            var file = await _dbContext.FilesOnDatabase.FirstOrDefaultAsync(x => x.Id == id);

            if (file == null) return NotFound();

            _dbContext.FilesOnDatabase.Remove(file);
            _dbContext.SaveChanges();

            TempData["Message"] = $"Removed {file.Name + file.Extention} successfully from Database.";

            return RedirectToAction(nameof(Index));
        }
    }
}
