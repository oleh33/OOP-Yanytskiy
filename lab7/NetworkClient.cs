using System;
using System.Net.Http;

namespace OOP_Yanytskiy.Lab7
{
    /// <summary>
    /// Клас, що імітує мережевий клієнт для відправки push-сповіщень.
    /// </summary>
    public class NetworkClient
    {
        private int _sendAttempts = 0;

        /// <summary>
        /// Сценарій: відправка push-сповіщення.
        /// Перші 2 рази кидає HttpRequestException, потім успіх.
        /// </summary>
        public void SendPushNotification(string deviceId, string payload)
        {
            _sendAttempts++;

            if (_sendAttempts <= 2)
            {
                throw new HttpRequestException($"[NetworkClient] Simulated HTTP error on attempt #{_sendAttempts}");
            }

            // Імітація успішної відправки
            Console.WriteLine($"[NetworkClient] Push sent to device '{deviceId}' with payload: {payload}");
        }
    }
}
