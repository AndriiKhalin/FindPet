using System.Runtime.CompilerServices;

namespace FindPet.BusinessLogicLayer.Interfaces.ILoggerService;

public interface ILoggerManager
{
    void LogInfo(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);

    void LogWarn(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);

    void LogDebug(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);

    void LogError(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);
}