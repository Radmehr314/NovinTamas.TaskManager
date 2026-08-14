namespace NovinTamas.TaskManager.Application.Contracts.Contracts
{
    // cache این‌مموری محلی از jtiهای revoke‌شده که از طریق RabbitMQ (event SessionRevokedEvent از IAM) پر می‌شه.
    // چک هر request فقط یک lookup این‌مموری‌ست، بدون هیچ network call ای به IAM.
    public interface ISessionRevocationCache
    {
        void MarkRevoked(string jti, DateTime expiresAt);
        bool IsRevoked(string jti);
    }
}
