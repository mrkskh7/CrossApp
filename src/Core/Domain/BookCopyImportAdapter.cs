using Core.Dto;

namespace Core.Domain;

public static class BookCopyImportAdapter
{
    public static DomainImportResult<BookCopy> ToDomain(ImportResult<BookDto> importResult)
    {
        List<BookCopy> copies = [];
        List<string> errors = [.. importResult.Errors];

        foreach (BookDto dto in importResult.Items)
        {
            try
            {
                // Використовуємо Id книги як Id її примірника для демонстрації
                BookCopy copy = BookCopy.Create($"C-{dto.Id}", dto.Isbn);
                copies.Add(copy);
            }
            catch (Exception ex)
            {
                errors.Add($"Запис '{dto.Id}': {ex.Message}");
            }
        }

        return new DomainImportResult<BookCopy>(copies.AsReadOnly(), errors.AsReadOnly());
    }
}