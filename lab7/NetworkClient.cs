using System;
using System.Net.Http;

namespace Lab7v20
{
    public class NetworkClient
    {
        private int _sendAttempts = 0;

        public void SendPushNotification(string deviceId, string payload)
        {
            _sendAttempts++;
            Console.WriteLine($"[NetworkClient] Attempt #{_sendAttempts} to send to '{deviceId}'");

            if (_sendAttempts <= 2)
                throw new HttpRequestException("Simulated HttpRequestException while sending push notification.");

            Console.WriteLine($"[NetworkClient] Push notification sent to '{deviceId}'. Payload: {payload}");
        }
    }
}
