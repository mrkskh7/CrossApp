using System.Globalization;
using System.Text;
using Core.Domain;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

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

Console.WriteLine($"До спроб порушень: {copy2}");
TryDo("повторна видача вже виданого примірника", () => copy2.Issue());
TryDo("порожній ISBN", () => BookCopy.Create("C-003", "   "));
TryDo("дата повернення раніше дати видачі", () => loan2.Close(new DateTime(2026, 9, 1), copy2));

Console.WriteLine($"Після трьох спроб порушення: {copy2}");
Console.WriteLine("Спостережений факт: жодна з трьох спроб вище не змінила стан об'єкта.");

// Легітимне закриття та повторна спроба закрити вже закриту видачу
loan2.Close(new DateTime(2026, 10, 10), copy2);
TryDo("повторне закриття вже закритої видачі", () => loan2.Close(new DateTime(2026, 10, 12), copy2));
Console.WriteLine($"Після легітимного закриття: {copy2}");

Console.WriteLine("\n=== Додаткове завдання 1: ImportResult -> доменні сутності ===");
string csvPath = args.Length > 0 ? args[0] : "data/sample.csv";
if (File.Exists(csvPath))
{
    var csvResult = BookCsvImporter.Load(csvPath);
    var domainResult = BookCopyImportAdapter.ToDomain(csvResult);
    Console.WriteLine($"Створено примірників: {domainResult.Items.Count}, відхилено: {domainResult.Errors.Count}");
    Console.WriteLine("Приклади створених доменних сутностей:");
    foreach (var item in domainResult.Items.Take(3))
        Console.WriteLine($"  + {item}");
    Console.WriteLine("Причини відхилення некоректних рядків:");
    foreach (var err in domainResult.Errors)
        Console.WriteLine($"  - {err}");
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

Console.WriteLine("\n=== Додаткове завдання 3: явний стан видачі (Reserved -> Open -> Closed) ===");
BookCopy copy3 = BookCopy.Create("C-003", "978-966-03-9999-0");
Loan loan3 = Loan.Reserve("L-003", copy3, "R-003", new DateTime(2026, 10, 5), new DateTime(2026, 10, 8));
Console.WriteLine($"1) Бронювання: {copy3} | {loan3}");

loan3.IssueFromReservation(new DateTime(2026, 10, 6), copy3);
Console.WriteLine($"2) Видача за бронню: {copy3} | {loan3}");

loan3.Close(new DateTime(2026, 10, 14), copy3);
Console.WriteLine($"3) Повернення: {copy3} | {loan3}");

TryDo("спроба видати вже закриту бронь/видачу (Closed -> Open)", () =>
    loan3.IssueFromReservation(new DateTime(2026, 10, 15), copy3));

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