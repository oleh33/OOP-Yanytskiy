using System;
using System.IO;

namespace OOP_Yanytskiy.Lab7
{
    /// <summary>
    /// Клас, що імітує роботу з файлами та може кидати IO-вийнятки.
    /// </summary>
    public class FileProcessor
    {
        private int _payloadAttempts = 0;

        /// <summary>
        /// Сценарій: отримання payload для push-сповіщення.
        /// Перші 3 рази кидає IOException, потім повертає "успішний" payload.
        /// </summary>
        public string GetNotificationPayload(string path)
        {
            _payloadAttempts++;

            // Імітуємо тимчасові проблеми з IO перші 3 спроби
            if (_payloadAttempts <= 3)
            {
                throw new IOException($"[FileProcessor] Simulated IO problem on attempt #{_payloadAttempts}");
            }

            // Тут могла б бути реальна робота з файлом, але ми імітуємо успіх
            return $"{{ "devicePath": "{path}", "message": "Hello from FileProcessor" }}";
        }
    }
}
