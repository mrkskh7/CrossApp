using System;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Core;

public sealed record EnvironmentReport(
    string Student,
    string OsDescription,
    string OsVersion,
    string Architecture,
    string Runtime,
    string DetectedRid,
    string ReportedRid,
    string BaseDirectory,
    string CurrentDirectory,
    string BuildNote,
    string Domain
);

[JsonSerializable(typeof(EnvironmentReport))]
public partial class EnvironmentReportJsonContext : JsonSerializerContext
{
}

public static class EnvironmentInfo
{
#if NET9_0_OR_GREATER
    public const string BuildNote = "збірка під net9.0";
#else
    public const string BuildNote = "збірка під net8.0";
#endif

    public static EnvironmentReport Collect()
    {
        string student = "Сах Маркіян, група ФЕІ-34";
        string domain = "Предметна область: Бібліотека (Book, BookCopy, Reader, Loan)";

        return new EnvironmentReport(
            Student: student,
            OsDescription: RuntimeInformation.OSDescription,
            OsVersion: Environment.OSVersion.ToString(),
            Architecture: RuntimeInformation.ProcessArchitecture.ToString(),
            Runtime: RuntimeInformation.FrameworkDescription,
            DetectedRid: DetectRid(),
            ReportedRid: RuntimeInformation.RuntimeIdentifier,
            BaseDirectory: AppContext.BaseDirectory,
            CurrentDirectory: Environment.CurrentDirectory,
            BuildNote: BuildNote,
            Domain: domain
        );
    }

    private static string DetectRid()
    {
        string os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" :
                    RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "unknown";

        string arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => "unknown"
        };

        return $"{os}-{arch}";
    }
}