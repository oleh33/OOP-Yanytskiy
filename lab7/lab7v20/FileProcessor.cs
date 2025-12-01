using System;
using System.IO;

namespace Lab7v20
{
    public class FileProcessor
    {
        private int _attempts = 0;

        public string GetNotificationPayload(string path)
        {
            _attempts++;
            Console.WriteLine($"[FileProcessor] Attempt #{_attempts} to read payload from '{path}'");

            if (_attempts <= 3)
                throw new IOException("Simulated IOException while reading file.");

            return $"{{ \"path\": \"{path}\", \"message\": \"Hello from FileProcessor\" }}";
        }
    }
}
