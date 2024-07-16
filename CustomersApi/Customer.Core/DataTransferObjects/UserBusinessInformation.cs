namespace Customer.Core.DataTransferObjects;

public class UserBusinessInformation
{
    public string CompanyName { get; set; }
    public string JobTitle { get; set; }
    public decimal MonthlyIncome { get; set; }
    public string BusinessStreet { get; set; }
    public string BusinessNeighborhood { get; set; }
    public string BusinessCity { get; set; }
    public string BusinessState { get; set; }
    public string BusinessPostalCode { get; set; }
    public string BusinessPhone { get; set; }

    public override string ToString()
    {
        return $"Nome da Empresa: {CompanyName}\n" +
               $"Cargo: {JobTitle}\n" +
               $"Renda Mensal: {MonthlyIncome:C}\n" +
               $"Endereço Comercial: {BusinessStreet}, {BusinessNeighborhood}, {BusinessCity}, {BusinessState}, {BusinessPostalCode}\n" +
               $"Telefone Comercial: {BusinessPhone}";
    }
}