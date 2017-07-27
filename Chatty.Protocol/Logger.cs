using System;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Chatty.Protocol
{
    public class Logger
    {
        private static readonly Lazy<Logger> Inst = new Lazy<Logger>(() => new Logger());
        public static Logger Instance => Inst.Value;

        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private readonly object lockObj = new object();

        private Logger()
        {
            LoggingConfiguration config = new LoggingConfiguration();

            FileTarget fileTarget = new FileTarget
            {
                KeepFileOpen = true,
                DeleteOldFileOnStartup = false
            };

            config.AddTarget("file", fileTarget);
            fileTarget.FileName = "chatty_server.log";
            fileTarget.Layout = @"[${date:format=HH\:mm\:ss}] ${message}";

            LoggingRule ruleI = new LoggingRule("*", LogLevel.Info, fileTarget);
            ruleI.Final = true;

            LoggingRule ruleE = new LoggingRule("*", LogLevel.Error, fileTarget);
            ruleE.Final = true;

            config.LoggingRules.Add(ruleI);
            config.LoggingRules.Add(ruleE);

            LogManager.Configuration = config;
        }

        public void Error(string message)
        {
            lock (lockObj)
            {
                logger.Error(message);
                Console.WriteLine(message);
            }
        }

        public void Info(string message)
        {
            lock (lockObj)
            {
                logger.Info(message);
                Console.WriteLine(message);
            }
        }
    }
}
