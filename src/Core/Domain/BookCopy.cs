using Core.Dto;

namespace Core.Domain;

public sealed class BookCopy
{
    public string Id { get; }
    public string Isbn { get; }
    public bool IsIssued { get; private set; }
    public bool IsReserved { get; private set; }

    private BookCopy(string id, string isbn, bool isIssued, bool isReserved = false)
    {
        Id = id;
        Isbn = isbn;
        IsIssued = isIssued;
        IsReserved = isReserved;
    }

    public static BookCopy Create(string id, string isbn)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор примірника не може бути порожнім", nameof(id));
        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN не може бути порожнім", nameof(isbn));

        return new BookCopy(id.Trim(), isbn.Trim(), isIssued: false, isReserved: false);
    }

    public void Reserve()
    {
        if (IsIssued)
            throw new InvalidOperationException($"Примірник {Id} вже виданий, бронювання неможливе");
        if (IsReserved)
            throw new InvalidOperationException($"Примірник {Id} вже заброньований");

        IsReserved = true;
    }

    public void CancelReservation()
    {
        if (!IsReserved)
            throw new InvalidOperationException($"Примірник {Id} не перебуває в броні");

        IsReserved = false;
    }

    public void Issue()
    {
        if (IsIssued)
            throw new InvalidOperationException($"Примірник {Id} вже виданий, повторна видача неможлива");

        IsReserved = false;
        IsIssued = true;
    }

    public void Return()
    {
        if (!IsIssued)
            throw new InvalidOperationException($"Примірник {Id} не був виданий, повернення неможливе");

        IsIssued = false;
    }

    public BookCopyDto ToDto() => new(Id, Isbn, IsIssued);

    public static BookCopy FromDto(BookCopyDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var copy = Create(dto.Id, dto.Isbn);
        if (dto.IsIssued)
            copy.Issue();
        return copy;
    }

    public override string ToString()
    {
        string state = IsIssued ? "видано" : (IsReserved ? "заброньовано" : "на полиці");
        return $"{Id} [{Isbn}] — {state}";
    }
}