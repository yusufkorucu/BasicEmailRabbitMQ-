using RabbitMQ.Shared.Dto;

namespace RabbitMQ.Api.Services.Abstract
{
    public interface IPublishService
    {
        Task PublishQuee(EmailSendRequestDto dto);
        
    }
}
