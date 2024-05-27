using System.Diagnostics;

namespace FindPet.API.Configurations.ServiceExtensions;

public static class StartUpLoggingExtensions
{
    public static void OpenLogFile(this IApplicationBuilder app)
    {
        //TODO Сделать чтобы он открывался , при наличие ошибки или при наличие самого файла

        // Открытие файла логирования
        var logFile = $"d:/IT/FindPet/FindPet_API/LogInfo/{DateTime.Today:yyyy-MM-dd}_logfile.html";

        if (File.Exists(logFile))
        {
            Process.Start("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe", logFile);
        }
    }
}