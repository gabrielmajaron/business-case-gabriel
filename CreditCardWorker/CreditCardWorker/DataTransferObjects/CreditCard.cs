namespace CreditCardWorker.DataTransferObjects;

public class CreditCard
{
    public string CardHolderName { get; set; }
    public string CardNumber { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal CreditLimit { get; set; }
}