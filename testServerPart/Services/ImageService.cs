using Common.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using testServerPart.Contracts;
using testServerPart.Models;

namespace testServerPart.Services
{
    public class ImageService : IImageService
    {
        private readonly string _imageDirectory;
        private readonly string _imageDataDirectory;
        private readonly ILog _logger;
        private readonly SemaphoreSlim _semaphore = new(1);
        public ImageService(IConfiguration configuration)
        {
            _imageDirectory = configuration["PathSettings:ImageDirectory"];
            _imageDataDirectory = configuration["PathSettings:ImageDataDirectory"];
            _logger = LogManager.GetLogger(this.GetType());

            Directory.CreateDirectory(_imageDirectory);
            Directory.CreateDirectory(_imageDataDirectory);
        }

        public async Task<ConcurrentQueue<ImageData>> Get()
        {
            var imageDataQueue = new ConcurrentQueue<ImageData>();
            var imageDataFiles = Directory.GetFiles(_imageDataDirectory);

            var tasks = imageDataFiles.Select(async imageDataFile =>
            {
                try
                {
                    var jsonData = await File.ReadAllTextAsync(imageDataFile);
                    var imageDataJson = JsonSerializer.Deserialize<ImageDataJson>(jsonData);

                    if (imageDataJson != null)
                    {
                        var imageData = new ImageData
                        {
                            ImageDataPath = imageDataFile,
                            Name = imageDataJson.Name,
                            Description = imageDataJson.Description,
                            Image = await File.ReadAllBytesAsync(imageDataJson.ImagePath)
                        };

                        imageDataQueue.Enqueue(imageData);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"File reading error! {imageDataFile}", ex);
                    throw new Extensions.ImageException($"File reading error! {imageDataFile}", ex);
                }
            });

            await Task.WhenAll(tasks);

            return imageDataQueue;
        }

        public async Task Save(ImageData data)
        {
            try
            {
                await _semaphore.WaitAsync();

                try
                {
                    var imagePath = Path.Combine(_imageDirectory, $"{data.Name}.jpg");
                    imagePath = ValidatePath(imagePath);
                    var imageDataPath = Path.Combine(_imageDataDirectory, $"{data.Name}.json");
                    imageDataPath = ValidatePath(imageDataPath);

                    await File.WriteAllBytesAsync(imagePath, data.Image);

                    var dataWithImagePath = new ImageDataJson
                    {
                        Name = data.Name,
                        Description = data.Description,
                        ImagePath = imagePath
                    };

                    string jsonString = JsonSerializer.Serialize(dataWithImagePath, new JsonSerializerOptions { WriteIndented = true });

                    await File.WriteAllTextAsync(imageDataPath, jsonString);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Data saving error!", ex);
                throw new Extensions.ImageException("Data saving error!", ex);
            }
        }

        public async Task Edit(ImageData newData)
        {
            try
            {
                await _semaphore.WaitAsync();

                try
                {
                    if (!File.Exists(newData.ImageDataPath))
                    {
                        throw new Extensions.ImageException("The file was not found!");
                    }

                    string imageDataJson = await File.ReadAllTextAsync(newData.ImageDataPath);
                    var imageData = JsonSerializer.Deserialize<ImageDataJson>(imageDataJson);

                    imageData.Description = newData.Description;
                    imageData.Name = newData.Name;

                    string updatedJsonString = JsonSerializer.Serialize(imageData, new JsonSerializerOptions { WriteIndented = true });

                    await File.WriteAllTextAsync(newData.ImageDataPath, updatedJsonString);
                    await File.WriteAllBytesAsync(imageData.ImagePath, newData.Image);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Data update error!", ex);
                throw new Extensions.ImageException("Data update error!", ex);
            }
        }

        public async Task Delete(string imageDataPath)
        {
            try
            {
                await _semaphore.WaitAsync();

                try
                {
                    if (!File.Exists(imageDataPath))
                    {
                        throw new Extensions.ImageException("The file was not found!");
                    }

                    string imageDataJson = await File.ReadAllTextAsync(imageDataPath);
                    string imagePath = JsonSerializer.Deserialize<ImageDataJson>(imageDataJson).ImagePath;

                    File.Delete(imageDataPath);
                    File.Delete(imagePath);
                }
                finally
                {
                    _semaphore.Release();
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"Deletion error!", ex);
                throw new Extensions.ImageException("Deletion error!", ex);
            }
        }

        #region Helpers methods

        public string ValidatePath(string path)
        {
            if (File.Exists(path))
            {
                int index = 1;
                string directory = Path.GetDirectoryName(path);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                string fileExtension = Path.GetExtension(path);

                while (File.Exists(path))
                {
                    string newFileName = $"{fileNameWithoutExtension}_{index}{fileExtension}";
                    path = Path.Combine(directory, newFileName);
                    index++;
                }
            }

            return path;
        }
        #endregion
    }
}
