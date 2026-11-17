using Core.Dto;

namespace Core.Domain;

public enum LoanStatus
{
    Open,
    Closed
}

public sealed class Loan
{
    public string Id { get; }
    public string CopyId { get; }
    public string ReaderId { get; }
    public DateTime IssuedOn { get; }
    public DateTime? ReturnedOn { get; private set; }
    public LoanStatus Status { get; private set; }

    private Loan(string id, string copyId, string readerId, DateTime issuedOn)
    {
        Id = id;
        CopyId = copyId;
        ReaderId = readerId;
        IssuedOn = issuedOn;
        Status = LoanStatus.Open;
    }

    public static Loan Open(string id, BookCopy copy, string readerId, DateTime issuedOn)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор видачі обов'язковий", nameof(id));
        ArgumentNullException.ThrowIfNull(copy);
        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(readerId));

        // Перевіряємо інваріант примірника (видаємо його)
        copy.Issue();

        return new Loan(id.Trim(), copy.Id, readerId.Trim(), issuedOn);
    }

    public void Close(DateTime returnedOn, BookCopy copy)
    {
        EnsureTransition(LoanStatus.Closed);
        ArgumentNullException.ThrowIfNull(copy);

        if (copy.Id != CopyId)
            throw new ArgumentException($"Примірник {copy.Id} не відповідає видачі {Id} (очікувався {CopyId})", nameof(copy));

        if (returnedOn < IssuedOn)
            throw new ArgumentOutOfRangeException(nameof(returnedOn), returnedOn, "Дата повернення не може бути раніше дати видачі");

        copy.Return();
        ReturnedOn = returnedOn;
        Status = LoanStatus.Closed;
    }

    private void EnsureTransition(LoanStatus target)
    {
        bool allowed = (Status, target) switch
        {
            (LoanStatus.Open, LoanStatus.Closed) => true,
            _ => false
        };

        if (!allowed)
            throw new InvalidOperationException($"Видача {Id}: недопустимий перехід стану {Status} -> {target}");
    }

    public LoanDto ToDto() => new(Id, CopyId, ReaderId, IssuedOn, ReturnedOn);

    public static Loan FromDto(LoanDto dto, BookCopy copy)
    {
        if (string.IsNullOrWhiteSpace(dto.Id))
            throw new ArgumentException("Ідентифікатор видачі обов'язковий", nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.ReaderId))
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(dto));
        ArgumentNullException.ThrowIfNull(copy);

        var loan = new Loan(dto.Id.Trim(), copy.Id, dto.ReaderId.Trim(), dto.IssuedOn);
        
        if (dto.ReturnedOn.HasValue)
        {
            if (dto.ReturnedOn.Value < dto.IssuedOn)
                throw new ArgumentOutOfRangeException(nameof(dto), "Дата повернення в DTO раніше дати видачі");
            
            loan.ReturnedOn = dto.ReturnedOn;
            loan.Status = LoanStatus.Closed;
        }
        else if (!copy.IsIssued)
        {
            copy.Issue();
        }

        return loan;
    }

    public override string ToString()
    {
        string ret = ReturnedOn.HasValue ? $"повернено {ReturnedOn.Value:d}" : "не повернено";
        return $"{Id}: примірник {CopyId} -> читач {ReaderId}, видано {IssuedOn:d}, {ret}";
    }
}