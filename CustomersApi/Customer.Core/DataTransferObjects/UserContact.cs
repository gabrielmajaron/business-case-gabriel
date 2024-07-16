namespace Customer.Core.DataTransferObjects;

public class UserContact
{
    public string Street { get; set; }
    public string Neighborhood { get; set; }
    public string City { get; set; } 
    public string State { get; set; }
    public string PostalCode { get; set; } 
    public string HomePhone { get; set; }  
    public string MobilePhone { get; set; }
    public string Email { get; set; } 

    public override string ToString()
    {
        return $"Endereço: {Street}, {Neighborhood}, {City}, {State}, {PostalCode}\n" +
               $"Telefone residencial: {HomePhone}\n" +
               $"Telefone móvel: {MobilePhone}\n" +
               $"Email: {Email}";
    }
}