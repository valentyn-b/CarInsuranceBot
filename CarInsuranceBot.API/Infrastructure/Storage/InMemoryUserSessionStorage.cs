using CarInsuranceBot.API.Application.Interfaces;
using CarInsuranceBot.API.Core.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace CarInsuranceBot.API.Infrastructure.Storage
{
    public class InMemoryUserSessionStorage : IUserSessionStorage
    {
        private readonly IMemoryCache _memoryCache;
        private readonly TimeSpan _sessionExpiration = TimeSpan.FromMinutes(30);

        public InMemoryUserSessionStorage(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public UserSession GetSession(long chatId)
        {
            string cacheKey = $"session_{chatId}";

            if (!_memoryCache.TryGetValue(cacheKey, out UserSession? session) || session == null)
            {
                session = new UserSession();
                SaveSession(chatId, session);
            }

            return session;
        }

        public void SaveSession(long chatId, UserSession session)
        {
            string cacheKey = $"session_{chatId}";

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(_sessionExpiration);

            _memoryCache.Set(cacheKey, session, cacheEntryOptions);
        }
    }
}
