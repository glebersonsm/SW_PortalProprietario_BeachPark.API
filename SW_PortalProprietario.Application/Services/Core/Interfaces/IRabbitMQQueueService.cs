using SW_PortalProprietario.Application.Models.SystemModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IRabbitMQQueueService
    {
        Task<RabbitMQQueueViewModel?> SaveQueue(RabbitMQQueueInputModel model);
        Task<List<RabbitMQQueueViewModel>> GetAllQueues();
        Task<RabbitMQQueueViewModel?> GetQueueById(int id);
        Task<RabbitMQQueueViewModel?> GetQueueByNome(string nome);
        Task<bool> IsQueueActiveByNome(string nome);
        Task<bool> DeleteQueue(int id);
        Task<RabbitMQQueueViewModel?> ToggleQueueStatus(int id);
    }
}
