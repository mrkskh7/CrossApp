using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Core;

Console.OutputEncoding = Encoding.UTF8;

EnvironmentReport report = EnvironmentInfo.Collect();
bool jsonMode = args.Contains("--json");

if (jsonMode)
{
    var options = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        TypeInfoResolver = EnvironmentReportJsonContext.Default,
        WriteIndented = false
    };
    string json = JsonSerializer.Serialize(report, options);
    Console.WriteLine(json);
}
else
{
    Console.WriteLine("CrossApp - інформація про середовище");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"Студент            : {report.Student}");
    Console.WriteLine($"OC (OSDescription) : {report.OsDescription}");
    Console.WriteLine($"OC (Environment)   : {report.OsVersion}");
    Console.WriteLine($"Архітектура        : {report.Architecture}");
    Console.WriteLine($"Runtime            : {report.Runtime}");
    Console.WriteLine($"RID (визначено)    : {report.DetectedRid}");
    Console.WriteLine($"RID (від .NET)     : {report.ReportedRid}");
    Console.WriteLine($"Каталог застосунку : {report.BaseDirectory}");
    Console.WriteLine($"Поточний каталог   : {report.CurrentDirectory}");
    Console.WriteLine($"Збірка             : {report.BuildNote}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine(report.Domain);
}