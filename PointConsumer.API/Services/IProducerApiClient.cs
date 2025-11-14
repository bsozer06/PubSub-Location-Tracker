namespace PointConsumer.API.Services
{
    public interface IProducerApiClient
    {
        Task<string> GetInitialPointsAsync();
    }
}
