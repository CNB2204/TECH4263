using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Packets;

namespace MqttSubscriber
{
    public class SensorData
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public float FloatValue1 { get; set; }
        public float FloatValue2 { get; set; }
    }

    class Program
    {
        private const string TOPIC = "myapp/sensor/data";
        private const string BROKER = "broker.hivemq.com";
        private const int PORT = 1883;

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== MQTT Subscriber Starting ===\n");

            // STEP 1: In v5, MqttClientFactory replaces MqttFactory
            var mqttClient = new MqttClientFactory().CreateMqttClient();

            // STEP 2: Build connection options
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(BROKER, PORT)
                .WithClientId("Subscriber_" + Guid.NewGuid().ToString("N").Substring(0, 8))
                .WithCleanSession(true)
                .Build();

            // STEP 3: Hook up message handler BEFORE connecting
            // In v5 this is an async event handler
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine($"\nMessage received at {DateTime.Now:HH:mm:ss}");
                Console.WriteLine($"   Topic   : {e.ApplicationMessage.Topic}");

                // Convert bytes to string
                string rawJson = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                Console.WriteLine($"   Raw JSON: {rawJson}");

                try
                {
                    SensorData data = JsonSerializer.Deserialize<SensorData>(rawJson);

                    if (data != null)
                    {
                        Console.WriteLine("\n   Parsed Sensor Data:");
                        Console.WriteLine($"      Latitude    : {data.Latitude:F6}");
                        Console.WriteLine($"      Longitude   : {data.Longitude:F6}");
                        Console.WriteLine($"      FloatValue1 : {data.FloatValue1:F2}");
                        Console.WriteLine($"      FloatValue2 : {data.FloatValue2:F2}");
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"   Error parsing JSON: {ex.Message}");
                }

                Console.WriteLine(new string('-', 50));

                return Task.CompletedTask;
            };

            // STEP 4: Connect
            Console.WriteLine($"Connecting to broker: {BROKER}:{PORT}");
            var connectResult = await mqttClient.ConnectAsync(options, CancellationToken.None);

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                Console.WriteLine("Connected successfully!\n");
            }
            else
            {
                Console.WriteLine($"Failed to connect: {connectResult.ResultCode}");
                return;
            }

            // STEP 5: Subscribe
            // In v5, SubscribeAsync takes MqttTopicFilter directly
            var topicFilter = new MqttTopicFilter
            {
                Topic = TOPIC,
                QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce
            };

            await mqttClient.SubscribeAsync(topicFilter);

            Console.WriteLine($"Subscribed to topic: [{TOPIC}]");
            Console.WriteLine("Waiting for messages... (Press ENTER to stop)\n");
            Console.WriteLine(new string('-', 50));

            // STEP 6: Keep alive until Enter
            Console.ReadLine();

            // STEP 7: Disconnect
            await mqttClient.DisconnectAsync();
            Console.WriteLine("Disconnected. Goodbye!");
        }
    }
}