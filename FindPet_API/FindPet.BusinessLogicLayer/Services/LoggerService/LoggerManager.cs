using System.Diagnostics;
using System.Runtime.CompilerServices;
using FindPet.Domain.Interfaces.ILoggerService;
using NLog;

namespace FindPet.BusinessLogicLayer.Services.LoggerService;

public class LoggerManager : ILoggerManager
{
    private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

    public void LogDebug(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var className = new StackTrace().GetFrame(1).GetMethod().DeclaringType.FullName;
        logger.Debug(
            $"<span style='background:rgb(100,220,0)'> {DateTime.Now} | DEBUG | Project: {className} | Method: {memberName} | Message: {message} | Line: {sourceLineNumber}</span>");
    }

    public void LogError(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var className = new StackTrace().GetFrame(1).GetMethod().DeclaringType.FullName;
        logger.Error(
            $"<span style='background:rgb(160,0,0)'> {DateTime.Now} | ERROR | Project: {className} | Method: {memberName} | Message: {message} | Line: {sourceLineNumber}</span>");
    }

    public void LogInfo(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var className = new StackTrace().GetFrame(1).GetMethod().DeclaringType.FullName;
        logger.Info(
            $"<span style='background:rgb(0,160,0)'> {DateTime.Now} | INFO | Project: {className} | Method: {memberName} | Message: {message} | Line: {sourceLineNumber}</span>");
    }

    public void LogWarn(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var className = new StackTrace().GetFrame(1).GetMethod().DeclaringType.FullName;
        logger.Warn(
            $"<span style='background:rgb(200,200,0)'> {DateTime.Now} | WARN | Project: {className} | Method: {memberName} | Message: {message} | Line: {sourceLineNumber}</span>");
    }
}