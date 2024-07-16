using System.ComponentModel;
namespace Messenger.Enums;

public enum EventNameEnum
{
    [Description("customer.created")]
    CustomerCreated,
    [Description("creditproposal.created")]
    CreditProposalCreated,
    [Description("creditproposalgeneration.error")]
    CreditProposalGenerationError,
    [Description("creditcardgeneration.error")]
    CreditCardGenerationError,
    [Description("creditcards.created")]
    CreditCardsCreated
}