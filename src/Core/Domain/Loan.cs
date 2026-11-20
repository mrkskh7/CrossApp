using Core.Dto;

namespace Core.Domain;

public enum LoanStatus
{
    Reserved,
    Open,
    Closed
}

public sealed class Loan
{
    public string Id { get; }
    public string CopyId { get; }
    public string ReaderId { get; }
    public DateTime IssuedOn { get; private set; }
    public DateTime? ReservedUntil { get; private set; }
    public DateTime? ReturnedOn { get; private set; }
    public LoanStatus Status { get; private set; }

    private Loan(string id, string copyId, string readerId, DateTime issuedOn, LoanStatus status, DateTime? reservedUntil = null)
    {
        Id = id;
        CopyId = copyId;
        ReaderId = readerId;
        IssuedOn = issuedOn;
        Status = status;
        ReservedUntil = reservedUntil;
    }

    public static Loan Reserve(string id, BookCopy copy, string readerId, DateTime reservedOn, DateTime reservedUntil)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор броні не може бути порожнім", nameof(id));
        ArgumentNullException.ThrowIfNull(copy);
        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Ідентифікатор читача не може бути порожнім", nameof(readerId));
        if (reservedUntil <= reservedOn)
            throw new ArgumentOutOfRangeException(nameof(reservedUntil), reservedUntil, "Дата завершення броні має бути пізніше дати бронювання");

        copy.Reserve();
        return new Loan(id.Trim(), copy.Id, readerId.Trim(), reservedOn, LoanStatus.Reserved, reservedUntil);
    }

    public static Loan Open(string id, BookCopy copy, string readerId, DateTime issuedOn)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор видачі не може бути порожнім", nameof(id));
        ArgumentNullException.ThrowIfNull(copy);
        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Ідентифікатор читача не може бути порожнім", nameof(readerId));
        if (copy.IsReserved)
            throw new InvalidOperationException($"Примірник {copy.Id} заброньований іншим читачем");

        copy.Issue();
        return new Loan(id.Trim(), copy.Id, readerId.Trim(), issuedOn, LoanStatus.Open);
    }

    public void IssueFromReservation(DateTime issuedOn, BookCopy copy)
    {
        ArgumentNullException.ThrowIfNull(copy);
        if (copy.Id != CopyId)
            throw new ArgumentException($"Примірник {copy.Id} не відповідає броні {Id} (очікувався {CopyId})", nameof(copy));

        EnsureTransition(LoanStatus.Open);

        if (ReservedUntil.HasValue && issuedOn > ReservedUntil.Value)
            throw new InvalidOperationException($"Бронь {Id} прострочена (діяла до {ReservedUntil:dd.MM.yyyy})");

        copy.Issue();
        IssuedOn = issuedOn;
        Status = LoanStatus.Open;
    }

    public void Close(DateTime returnedOn, BookCopy copy)
    {
        ArgumentNullException.ThrowIfNull(copy);
        if (copy.Id != CopyId)
            throw new ArgumentException($"Примірник {copy.Id} не відповідає видачі {Id} (очікувався {CopyId})", nameof(copy));

        EnsureTransition(LoanStatus.Closed);

        if (Status == LoanStatus.Open && returnedOn < IssuedOn)
            throw new ArgumentOutOfRangeException(nameof(returnedOn), returnedOn, "Дата повернення не може бути раніше дати видачі");

        if (Status == LoanStatus.Open)
            copy.Return();
        else if (Status == LoanStatus.Reserved)
            copy.CancelReservation();

        ReturnedOn = returnedOn;
        Status = LoanStatus.Closed;
    }

    private void EnsureTransition(LoanStatus target)
    {
        bool allowed = (Status, target) switch
        {
            (LoanStatus.Reserved, LoanStatus.Open) => true,
            (LoanStatus.Reserved, LoanStatus.Closed) => true,
            (LoanStatus.Open, LoanStatus.Closed) => true,
            _ => false
        };

        if (!allowed)
            throw new InvalidOperationException($"Видача {Id}: недопустимий перехід стану {Status} -> {target}");
    }

    public LoanDto ToDto() => new(Id, CopyId, ReaderId, IssuedOn, ReturnedOn);

    public static Loan FromDto(LoanDto dto, BookCopy copy)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var loan = Open(dto.Id, copy, dto.ReaderId, dto.IssuedOn);
        if (dto.ReturnedOn.HasValue)
            loan.Close(dto.ReturnedOn.Value, copy);
        return loan;
    }

    public override string ToString()
    {
        return Status switch
        {
            LoanStatus.Reserved => $"{Id} [{Status}]: примірник {CopyId} -> читач {ReaderId}, бронь до {ReservedUntil:dd.MM.yyyy}",
            LoanStatus.Open => $"{Id} [{Status}]: примірник {CopyId} -> читач {ReaderId}, видано {IssuedOn:dd.MM.yyyy}, не повернено",
            LoanStatus.Closed => $"{Id} [{Status}]: примірник {CopyId} -> читач {ReaderId}, видано {IssuedOn:dd.MM.yyyy}, повернено {ReturnedOn:dd.MM.yyyy}",
            _ => $"{Id} [{Status}]"
        };
    }
}