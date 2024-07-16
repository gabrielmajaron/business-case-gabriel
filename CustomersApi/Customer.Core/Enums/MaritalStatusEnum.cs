using System.ComponentModel;

namespace Customer.Core.Enums;

public enum MaritalStatusEnum
{
    [Description("Solteiro(a)")]
    Single,
    [Description("Casado(a)")]
    Married,
    [Description("Divorciado(a)")]
    Divorced,
    [Description("Viúvo(a)")]
    Widowed,
    [Description("Separado(a)")]
    Separated,
    [Description("Em união estável")]
    InDomesticPartnership,
    [Description("União de facto")]
    IFactoRelashionship
}