using System;

namespace CTrue.DcsConnect
{
    public interface IDcsConnectLogProvider
    {
        IDcsConnectLog GetLogger(string name);
    }

    public interface IDcsConnectLog
    {
        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsErrorEnabled { get; }

        void Debug(string message);
        void Debug(string message, Exception ex);
        void Info(string message);
        void Info(string message, Exception ex);
        void Warn(string message);
        void Warn(string message, Exception ex);
        void Error(string message);
        void Error(string message, Exception ex);
    }
}