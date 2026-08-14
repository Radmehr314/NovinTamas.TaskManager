namespace NovinTamas.TaskManager.Application.Contracts.Contracts
{
    public interface IHttpClientService
    {
        Task<TResponse?> GetAsync<TResponse>(
            string serviceName,
            string endpoint,
            CancellationToken cancellationToken = default);

        Task<TResponse?> PostAsync<TRequest, TResponse>(
            string serviceName,
            string endpoint,
            TRequest data,
            CancellationToken cancellationToken = default);
    }
}
