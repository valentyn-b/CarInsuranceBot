using CarInsuranceBot.API.Core.Entities;

namespace CarInsuranceBot.API.Application.Interfaces
{
    public interface IUserSessionStorage
    {
        UserSession GetSession(long chatId);
        void SaveSession(long chatId, UserSession session);
    }
}
