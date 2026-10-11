using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

// Коректне відображення кирилиці
Console.OutputEncoding = Encoding.UTF8;

// Перевірка наявності прапорця для додаткового завдання
bool jsonMode = args.Contains("--json");

if (jsonMode)
{
    var info = new
    {
        Student = "Сах Маркіян, група ФЕІ-34",
        OsDescription = RuntimeInformation.OSDescription,
        OsEnvironment = Environment.OSVersion.ToString(),
        Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
        DotNetVersion = Environment.Version.ToString(),
        Runtime = RuntimeInformation.FrameworkDescription,
        BaseDirectory = AppContext.BaseDirectory,
        CurrentDirectory = Environment.CurrentDirectory,
        Domain = "Предметна область: Бібліотека (Book, BookCopy, Reader, Loan)"
    };

    var options = new JsonSerializerOptions 
    { 
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    
    Console.WriteLine(JsonSerializer.Serialize(info, options));
}
else
{
    Console.WriteLine("CrossApp - практикум з крос-платформного програмування");
    Console.WriteLine("Студент: Сах Маркіян, група ФЕІ-34"); // Вкажіть вашу реальну групу
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription) : {RuntimeInformation.OSDescription}");
    Console.WriteLine($"ОС (Environment)   : {Environment.OSVersion}");
    Console.WriteLine($"Архітектура процесу: {RuntimeInformation.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR)  : {Environment.Version}");
    Console.WriteLine($"Runtime            : {RuntimeInformation.FrameworkDescription}");
    Console.WriteLine($"Каталог застосунку : {AppContext.BaseDirectory}");
    Console.WriteLine($"Поточний каталог   : {Environment.CurrentDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine("Предметна область: Бібліотека (Book, BookCopy, Reader, Loan)");
}