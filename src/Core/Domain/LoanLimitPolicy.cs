namespace Core.Domain;

public static class LoanLimitPolicy
{
    public const int MaxOpenLoansPerReader = 5;

    public static Loan OpenWithLimit(
        string loanId,
        BookCopy copy,
        string readerId,
        DateTime issuedOn,
        IEnumerable<Loan> existingLoans)
    {
        int activeCount = existingLoans.Count(l =>
            l.ReaderId.Equals(readerId.Trim(), StringComparison.OrdinalIgnoreCase) &&
            l.Status == LoanStatus.Open);

        if (activeCount >= MaxOpenLoansPerReader)
            throw new InvalidOperationException(
                $"Читач {readerId} вже має {activeCount} відкритих видач (ліміт {MaxOpenLoansPerReader}), видача примірника {copy.Id} неможлива");

        return Loan.Open(loanId, copy, readerId, issuedOn);
    }
}