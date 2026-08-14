using NovinTamas.TaskManager.Application.Contracts.Contracts;
using System.Collections.Concurrent;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Services
{
    public class SessionRevocationCache : ISessionRevocationCache
    {
        private readonly ConcurrentDictionary<string, DateTime> _revoked = new();

        public void MarkRevoked(string jti, DateTime expiresAt)
        {
            if (string.IsNullOrWhiteSpace(jti)) return;

            _revoked[jti] = expiresAt;
            CleanupExpired();
        }

        public bool IsRevoked(string jti)
        {
            if (string.IsNullOrWhiteSpace(jti)) return false;

            return _revoked.ContainsKey(jti);
        }

        // بعد از انقضای طبیعی توکن، JwtBearer خودش ردش می‌کند؛ نگه‌داشتن jti فقط دیکشنری را بی‌نهایت بزرگ می‌کند.
        private void CleanupExpired()
        {
            var now = DateTime.UtcNow;

            foreach (var kvp in _revoked)
            {
                if (kvp.Value < now)
                    _revoked.TryRemove(kvp.Key, out _);
            }
        }
    }
}
