using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class BookJsonImporter
{
    public static ImportResult<BookDto> Load(string path)
    {
        var items = new List<BookDto>();
        var errors = new List<string>();

        try
        {
            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var deserialized = JsonSerializer.Deserialize<List<BookDto>>(json, options) ?? [];

            foreach (var book in deserialized)
            {
                if (string.IsNullOrWhiteSpace(book.Id) || string.IsNullOrWhiteSpace(book.Isbn) || string.IsNullOrWhiteSpace(book.Title))
                    errors.Add($"об'єкт '{book.Id}': обов'язкові поля не заповнені");
                else if (book.Year < 1450 || book.Year > DateTime.Now.Year)
                    errors.Add($"об'єкт '{book.Id}': рік '{book.Year}' поза допустимими межами");
                else
                    items.Add(book);
            }
        }
        catch (JsonException ex)
        {
            errors.Add($"Помилка структури JSON: {ex.Message}");
        }

        return new ImportResult<BookDto>(items, errors);
    }
}