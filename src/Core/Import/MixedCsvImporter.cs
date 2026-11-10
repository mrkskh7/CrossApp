using System.Text;
using Core.Dto;

namespace Core.Import;

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static MixedImportResult Load(string path)
    {
        var books = new List<BookDto>();
        var readers = new List<ReaderDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

            switch (parts)
            {
                // Книга з префіксом B
                case ["B", var id, var isbn, var title, var year]
                    when int.TryParse(year, out int y) && y >= 1450 && y <= DateTime.Now.Year:
                    books.Add(new BookDto(id, isbn, title, y));
                    break;

                case ["B", ..]:
                    errors.Add($"рядок {number}: некоректні дані книги");
                    break;

                // Читач з префіксом R
                case ["R", var id, var name, var phone] when !string.IsNullOrWhiteSpace(name):
                    readers.Add(new ReaderDto(id, name, string.IsNullOrWhiteSpace(phone) ? null : phone));
                    break;

                case ["R", ..]:
                    errors.Add($"рядок {number}: некоректні дані читача");
                    break;

                default:
                    errors.Add($"рядок {number}: невідомий тип запису");
                    break;
            }
        }

        return new MixedImportResult(books, readers, errors);
    }
}