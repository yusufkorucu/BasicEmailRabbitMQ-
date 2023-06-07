using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Shared.Dto;
using RabbitMQ.Shared.Utils;
using System.Net;
using System.Net.Mail;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.Json.Serialization;

//Connection
ConnectionFactory connectionFactory = new ConnectionFactory();
connectionFactory.Uri = new Uri(RabbitMQConnection.Url);

//Connection active Channel Open

using var connection = connectionFactory.CreateConnection();
using var channnel = connection.CreateModel();

//Quee oluşurma
channnel.QueueDeclare(queue: RabbitMQConst.EmailQueue, exclusive: false, durable: true);

EventingBasicConsumer consumer = new EventingBasicConsumer(channnel);
// autoack true value kuyruktan siler false ise consumerdan onay bekelenecektir

channnel.BasicConsume(queue: RabbitMQConst.EmailQueue, autoAck: false, consumer: consumer);
channnel.BasicQos(0, 1, false);
consumer.Received += Consumer_Received;

void Consumer_Received(object? sender, BasicDeliverEventArgs e)
{
    //response message operation
    //e.Body:quue message data
    Console.WriteLine(Encoding.UTF8.GetString(e.Body.Span));

    SendMail(Encoding.UTF8.GetString(e.Body.Span));
    //e.tag uniqe multiple true denirse oncekileride başarılı der false olursa sadece o mesaj
    channnel.BasicAck(e.DeliveryTag, multiple: false);
}
Console.Read();


async void SendMail(string message)
{
    var messageDto = JsonConvert.DeserializeObject<EmailSendRequestDto>(message);

    var mail = new MailMessage();

    mail.To.Add(messageDto.ToMailAddress);
    mail.CC.Add(messageDto.CcMailAddress);
    mail.Body = messageDto.Body;
    mail.Subject = messageDto.Subject;
    mail.From = new(RabbitMQConst.SenderEmailAddress, RabbitMQConst.SenderName, System.Text.Encoding.UTF8);
    var smtp = new SmtpClient();
    smtp.Credentials = new NetworkCredential(RabbitMQConst.SenderEmailAddress, RabbitMQConst.SenderEmailPassword);
    smtp.EnableSsl = true;
    smtp.Host = "smtp.gmail.com";

    await smtp.SendMailAsync(mail);

}