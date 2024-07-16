using Customer.Core.Enums;

namespace Customer.Core.DataTransferObjects;

public class UserPersonalData
{
    public string Name { get; set; }
    public string SecondName { get; set; }
    public DateTime BirthDate { get; set; }
    public string CPF { get; set; }
    public string RG { get; set; }
    public MaritalStatusEnum MaritalStatus { get; set; }
    public string Nationality { get; set; }
}