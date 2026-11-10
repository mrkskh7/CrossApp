using System.Globalization;
using System.Text;
using Core;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

// 1. Перевірка наявності файлу перед читанням (захист від винятку)
if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

// 2. Системний звіт
var report = EnvironmentInfo.Collect();
Console.WriteLine("CrossApp — інформація про середовище");
Console.WriteLine("Студент: Сах Маркіян, група ФЕІ-34");
Console.WriteLine(report);
Console.WriteLine("Предметна область: Бібліотека (Book, BookCopy, Reader, Loan)\n");

// 3. Вибір імпортера через switch-expression за розширенням файлу
string extension = Path.GetExtension(path).ToLowerInvariant();

ImportResult<BookDto> result = extension switch
{
    ".csv" => BookCsvImporter.Load(path),
    ".json" => BookJsonImporter.Load(path),
    var ext => throw new NotSupportedException($"Непідтримуване розширення файлу: {ext}")
};

// 4. Друк успішно завантажених записів
Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (BookDto book in result.Items.Take(5))
{
    Console.WriteLine($"  {book.Id,-6} {book.Isbn,-22} {book.Title,-32} {book.Year}");
}

// 5. Друк помилок із номерами рядків
if (result.Errors.Count > 0)
{
    Console.WriteLine($"\nПропущено рядків: {result.Errors.Count}");
    foreach (string err in result.Errors)
    {
        Console.WriteLine($"  ! {err}");
    }
}

// 6. Додаткове завдання: статистика одним рядком
int total = result.Items.Count + result.Errors.Count;
double percent = total > 0 ? (double)result.Errors.Count / total * 100 : 0;
Console.WriteLine($"\nУсього: {total}, прийнято: {result.Items.Count}, пропущено: {result.Errors.Count}, % помилок: {percent:F1}%");

// 7. Додаткове завдання: демонстрація змішаного імпорту
string mixedPath = Path.Combine("data", "sample-mixed.csv");
if (File.Exists(mixedPath) && path.EndsWith("sample.csv", StringComparison.OrdinalIgnoreCase))
{
    var mixed = MixedCsvImporter.Load(mixedPath);
    Console.WriteLine($"\nМішаний імпорт книг: {mixed.Books.Count}, читачів: {mixed.Readers.Count}, помилок: {mixed.Errors.Count}");
}

return 0;