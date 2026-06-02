using MimeDetective;
using SixLabors.ImageSharp;


namespace WebApplication1.Service
{
    //圖片儲存邏輯服務
    public class FileProcess
    {
        private static string[] _mimeTypeArray { get; } = new string[2] { "image/jpeg", "image/png" };
        private readonly IWebHostEnvironment _env;
        private readonly IContentInspector _imageInspector;
        private readonly ILogger<FileProcess> _logger;

		public FileProcess(IWebHostEnvironment env, IContentInspector imageInspector, ILogger<FileProcess> logger)
        {
            _env = env;
            _imageInspector = imageInspector;
            _logger = logger;
        }

        internal bool SaveFile(IFormFile file,string productID)
        {
            if (file is null) return true;
            try
            {
                using(var uploadFileSteam = file.OpenReadStream())
                {
                    var resultsArray = _imageInspector.Inspect(uploadFileSteam);
                    if (!resultsArray.Any())
                    {
                        _logger.LogWarning("{warningTime}:使用者上傳的檔案，MimeDetective無法辨識", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
						return false;
					}
                    var result = resultsArray.MaxBy(resultPossible => resultPossible.Points);
                    if (!_mimeTypeArray.Contains(result.Definition.File.MimeType))
                    {
						_logger.LogWarning("{warningTime}:使用者上傳的檔案格式，不是jpeg、png", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
						return false;
					}
                    uploadFileSteam.Position = 0;
                    using (Image imagefile = Image.Load(uploadFileSteam))
                    {
                        imagefile.SaveAsJpeg(Path.Combine(_env.WebRootPath, "ProductPicture", productID+".jpeg"));
                    }
                    return true;
                }
            }
            catch(Exception e)
            {
                _logger.LogWarning("{warningTime}:發生預期外的錯誤。{exception}:{exceptionMessage}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),e,e.Message);
				return false;
            }
        }
    
        internal bool DeleteFile(string idDeleted)
        {
            string filePath = Path.Combine(_env.WebRootPath, "ProductPicture", idDeleted + ".jpeg");
            if(File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return true;
        }
    }
}
