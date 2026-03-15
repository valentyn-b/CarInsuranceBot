using CarInsuranceBot.API.Application.Enums;

namespace CarInsuranceBot.API.Application.Interfaces
{
    public interface IUserStateStorage
    {
        UserState GetUserState(long chatId);

        void SetUserState(long chatId, UserState state);
    }
}
