namespace CarInsuranceBot.API.Application.Enums;

public enum UserState
{
    /// <summary>
    /// User has just started the conversation.
    /// </summary>
    New,

    /// <summary>
    /// Waiting for the user to upload their documents.
    /// </summary>
    WaitingForDocuments,

    /// <summary>
    /// Waiting for the user to confirm the extracted data.
    /// </summary>
    ConfirmingData,

    /// <summary>
    /// Waiting for the user to agree to the fixed price.
    /// </summary>
    AwaitingPriceAgreement,

    /// <summary>
    /// Transaction completed and the user has received their insurance policy.
    /// </summary>
    Finished
}