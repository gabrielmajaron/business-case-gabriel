namespace CreditProposalWorker.Interfaces;

public interface ICreditProposalCalculator
{
    decimal GetMaxCreditLimit(decimal monthlyIncome);
}