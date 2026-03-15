using CarInsuranceBot.API.Application.Enums;
using CarInsuranceBot.API.Application.Interfaces;
using System.Collections.Concurrent;

namespace CarInsuranceBot.API.Infrastructure.Storage
{
    public class InMemoryUserStateStorage : IUserStateStorage
    {
        private readonly ConcurrentDictionary<long, UserState> _states = new();

        public UserState GetUserState(long chatId)
        {
            return _states.TryGetValue(chatId, out var state) ? state : UserState.New;
        }

        public void SetUserState(long chatId, UserState state)
        {
            _states[chatId] = state;
        }
    }
}
