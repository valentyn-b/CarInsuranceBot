using CarInsuranceBot.API.Application.Interfaces;
using CarInsuranceBot.API.Core.Entities;
using System.Collections.Concurrent;

namespace CarInsuranceBot.API.Infrastructure.Storage
{
    public class InMemoryUserSessionStorage : IUserSessionStorage
    {
        private readonly ConcurrentDictionary<long, UserSession> _sessions = new();

        public UserSession GetSession(long chatId)
        {
            return _sessions.GetOrAdd(chatId, _ => new UserSession());
        }

        public void SaveSession(long chatId, UserSession session)
        {
            _sessions[chatId] = session;
        }
    }
}
