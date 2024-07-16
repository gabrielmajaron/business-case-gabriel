namespace Customer.Core.DataTransferObjects;

public class CreateCustomerRequest
{
    public UserPersonalData UserPersonalData { get; set; }
    public UserContact UserContact { get; set; }
    public UserBusinessInformation UserBusinessInformation { get; set; }
    public int CreditCardsQuantity { get; set; }
}
