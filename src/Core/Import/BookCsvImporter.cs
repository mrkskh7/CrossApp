using System.Text;
using Core.Dto;

namespace Core.Import;

public static class BookCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<BookDto> Load(string path)
    {
        var items = new List<BookDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            // Пропуск рядка заголовка
            if (number == 1 && line.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                continue;

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<BookDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            // 1. Патерн властивостей { Length: < 4 }
            { Length: < 4 } => new ParseFailed($"очікую 4 колонки, отримав {parts.Length}"),

            // 2. Патерн списку + логічний or
            [_, "", _, ..] or [_, _, "", ..] => new ParseFailed("ISBN або назва порожні"),

            // 3. Патерн списку + умова when
            [_, _, _, var year, ..] when !int.TryParse(year, out int y) || y < 1450 || y > DateTime.Now.Year
                => new ParseFailed($"рік '{year}' поза допустимими межами (1450..{DateTime.Now.Year})"),

            // 4. Позиційний патерн на 4 колонки
            [var id, var isbn, var title, var year]
                => new ParseOk(new BookDto(id, isbn, title, int.Parse(year))),

            // 5. Позиційний патерн на 5 колонок (з автором)
            [var id, var isbn, var title, var year, var author]
                => new ParseOk(new BookDto(id, isbn, title, int.Parse(year), string.IsNullOrWhiteSpace(author) ? null : author)),

            _ => new ParseFailed($"занадто багато колонок: {parts.Length}")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(BookDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}