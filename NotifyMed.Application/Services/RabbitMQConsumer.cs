using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace NotifyMed.Application.Services
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly EmailService _emailService;

        public RabbitMQConsumer(EmailService emailService)
        {
            _emailService = emailService;

            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            //var factory = new ConnectionFactory()
            //{
            //    HostName = "localhost",
            //    Port = 5672,
            //    UserName = "guest",
            //    Password = "guest"
            //};

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "new_appointment",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var emailRequest = JsonSerializer.Deserialize<EmailNotificationRequest>(message);

                Console.WriteLine($"[✔] Recebida notificação de consulta para: {emailRequest.DoctorEmail}");
                await _emailService.SendEmailAsync(
                    "matheusfonsecamfo@gmail.com",
                    emailRequest.DoctorEmail,
                    "Health&Med - Atualização Consulta",
                    $"{emailRequest.Body}"
                );

                if (!string.IsNullOrEmpty(emailRequest.PatientEmail))
                {
                    await _emailService.SendEmailAsync(
                    emailRequest.DoctorEmail,
                    emailRequest.PatientEmail,
                    "Health&Med - Atualização Consulta",
                    $"{emailRequest.Body}"
                );
                }
            };

            _channel.BasicConsume(queue: "new_appointment",
                                 autoAck: true,
                                 consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }

    public class EmailNotificationRequest
    {
        public string DoctorEmail { get; set; }
        public string PatientEmail { get; set; }
        public string Body { get; set; }
    }
}
