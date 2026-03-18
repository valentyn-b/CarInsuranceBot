using CarInsuranceBot.API.Application.Enums;

namespace CarInsuranceBot.API.Core.Entities
{
    public class UserSession
    {
        public UserState State { get; set; } = UserState.New;
        public string Language { get; set; } = "English";
        public string TelegramName { get; set; } = "Friend";
        public PassportData? Passport { get; set; }
        public VehicleData? Vehicle { get; set; }
    }
}
