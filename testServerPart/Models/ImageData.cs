using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace testServerPart.Models
{
    /// <summary>
    /// ImageDataPath используется как Id :D
    /// </summary>

    
    public class ImageData
    {
        public string? ImageDataPath { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public required byte[] Image { get; set; }
    }
}
