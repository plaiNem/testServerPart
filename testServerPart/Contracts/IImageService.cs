using System.Collections.Concurrent;
using testServerPart.Models;

namespace testServerPart.Contracts
{
    public interface IImageService
    {
        Task<ConcurrentQueue<ImageData>> Get();
        Task Save(ImageData data);
        Task Edit(ImageData newData);
        Task Delete(string imageDataPath);
    }
}
