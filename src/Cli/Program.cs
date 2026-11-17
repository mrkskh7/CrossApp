using System.Text;
using Core.Domain;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("CrossApp — Лабораторна робота 4 (Студент: Сах Маркіян, група ФЕІ-34)");
Console.WriteLine("Предметна область: Бібліотека (Book, BookCopy, Reader, Loan)\n");

Console.WriteLine("=== Лабораторна 4: доменна модель (BookCopy + Loan) ===");

Console.WriteLine("\n--- Сценарій 1: успіх ---");
BookCopy copy1 = BookCopy.Create("C-001", "978-966-03-5548-1");
Console.WriteLine(copy1);

Loan loan1 = Loan.Open("L-001", copy1, "R-001", new DateTime(2026, 10, 1));
Console.WriteLine(copy1);
Console.WriteLine(loan1);

loan1.Close(new DateTime(2026, 10, 15), copy1);
Console.WriteLine(copy1);
Console.WriteLine(loan1);

Console.WriteLine("\n--- Сценарій 2: порушення інваріантів ---");
BookCopy copy2 = BookCopy.Create("C-002", "978-617-7764-11-3");
Loan loan2 = Loan.Open("L-002", copy2, "R-002", new DateTime(2026, 10, 1));

TryDo("повторна видача вже виданого примірника", () => copy2.Issue());
TryDo("порожній ISBN", () => BookCopy.Create("C-003", "   "));
TryDo("дата повернення раніше дати видачі", () => loan2.Close(new DateTime(2026, 9, 1), copy2));

// Закриваємо коректно, а потім пробуємо закрити повторно
loan2.Close(new DateTime(2026, 10, 10), copy2);
TryDo("повторне закриття вже закритої видачі", () => loan2.Close(new DateTime(2026, 10, 12), copy2));

Console.WriteLine("\n=== Додаткове завдання 1: ImportResult -> доменні сутності ===");
string csvPath = args.Length > 0 ? args[0] : "data/sample.csv";
if (File.Exists(csvPath))
{
    var csvResult = BookCsvImporter.Load(csvPath);
    var domainResult = BookCopyImportAdapter.ToDomain(csvResult);
    Console.WriteLine($"Створено примірників: {domainResult.Items.Count}, відхилено: {domainResult.Errors.Count}");
}
else
{
    Console.WriteLine($"Файл {csvPath} не знайдено для демонстрації імпорту.");
}

Console.WriteLine("\n=== Додаткове завдання 2: ліміт 5 відкритих видач на читача ===");
List<Loan> allLoans = [];
for (int i = 1; i <= 5; i++)
{
    var tempCopy = BookCopy.Create($"C-20{i}", $"978-000-000-00{i}");
    allLoans.Add(LoanLimitPolicy.OpenWithLimit($"L-20{i}", tempCopy, "R-999", DateTime.Today, allLoans));
}
var extraCopy = BookCopy.Create("C-206", "978-000-000-006");
TryDo("6-та відкрита видача тому самому читачу (ліміт 5)", () =>
    LoanLimitPolicy.OpenWithLimit("L-206", extraCopy, "R-999", DateTime.Today, allLoans));

Console.WriteLine("\n=== Додаткове завдання 3: явний стан видачі (LoanStatus) ===");
Console.WriteLine($"Статус {loan1.Id}: {loan1.Status}");
Console.WriteLine($"Статус {loan2.Id}: {loan2.Status}");

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");
    }
}