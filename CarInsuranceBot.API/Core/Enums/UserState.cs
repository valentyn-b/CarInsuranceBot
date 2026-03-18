namespace CarInsuranceBot.API.Application.Enums;

/// <summary>
/// Represents the current state of the user in the car insurance purchasing process.
/// Used to manage the dialogue logic within the State Machine.
/// </summary>
public enum UserState
{
    /// <summary>
    /// Initial state: the user has just started the conversation or reset the process using the /start command.
    /// </summary>
    New,

    /// <summary>
    /// Waiting for the user to upload a photo of their passport or ID card.
    /// </summary>
    WaitingForPassport,

    /// <summary>
    /// Waiting for the user to confirm the data automatically extracted from their passport.
    /// </summary>
    ConfirmingPassport,

    /// <summary>
    /// Waiting for the user to upload a photo of their vehicle registration document (tech passport) or driver's license.
    /// </summary>
    WaitingForVehicleDoc,

    /// <summary>
    /// Waiting for the user to confirm the data automatically extracted from their vehicle documents.
    /// </summary>
    ConfirmingVehicleDoc,

    /// <summary>
    /// Waiting for the user to agree to the fixed price of the insurance policy (100 USD).
    /// </summary>
    AwaitingPriceAgreement,

    /// <summary>
    /// Final state: the purchase is confirmed, and the insurance policy has been generated and sent to the user.
    /// </summary>
    Finished
}