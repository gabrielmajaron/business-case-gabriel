using System;

namespace Messenger.EventBus.EventBus.EventContents;

public class UserCreatedMessage
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string SecondName { get; set; }
    public decimal MonthlyIncome { get; set; }
    public int CreditCardsQuantity { get; set; }
}