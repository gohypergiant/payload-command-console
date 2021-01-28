using System;

namespace Hypergiant.HIVE
{
    public interface ILogService
    {
        void InformationIf(bool condition, string message);
        void Information(string message);
        void WarningIf(bool condition, string message);
        void Warning(string message);
        void ErrorIf(bool condition, string message);
        void Error(string message);
        void Error(Exception exception);
    }
}
