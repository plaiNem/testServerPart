using System.Runtime.Serialization;

namespace testServerPart.Extensions
{
    [Serializable]
    internal class ImageException : Exception
    {
        public ImageException()
        {
        }

        public ImageException(string? message) : base(message)
        {
        }

        public ImageException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ImageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}