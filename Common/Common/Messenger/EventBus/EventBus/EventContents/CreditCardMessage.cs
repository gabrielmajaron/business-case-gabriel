using System;

namespace Messenger.EventBus.EventBus.EventContents;

public class CreditCardMessage
{
    public string CardHolderName { get; set; }
    public string LastCardNumbers { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal CreditLimit { get; set; }
}

