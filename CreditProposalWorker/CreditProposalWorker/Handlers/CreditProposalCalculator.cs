using CreditProposalWorker.Interfaces;

namespace CreditProposalWorker.Handlers;

public class CreditProposalCalculator : ICreditProposalCalculator
{
    private List<decimal> creditLimits = new()
    {
        1000,
        2000,
        4000,
        8000,
        10000,
        20000,
        30000,
        40000
    };

    public decimal GetMaxCreditLimit(decimal monthlyIncome)
    {
        var limit = monthlyIncome * 2;

        var closestLimit = creditLimits[0];
        var smallestDifference = Math.Abs(creditLimits[0] - limit);

        foreach (var creditLimit in creditLimits)
        {
            var difference = Math.Abs(creditLimit - limit);
            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                closestLimit = creditLimit;
            }
        }

        return closestLimit;
    }
}