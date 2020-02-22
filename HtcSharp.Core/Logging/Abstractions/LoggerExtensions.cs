﻿using System;

namespace HtcSharp.Core.Logging.Abstractions {
    public static class LoggerExtensions {

        public static void LogDebug(this ILogger logger, object obj, Exception ex) {
            logger.Log(LogLevel.Debug, obj, ex);
        }

        public static void LogInfo(this ILogger logger, object obj, Exception ex) {
            logger.Log(LogLevel.Info, obj, ex);
        }

        public static void LogWarn(this ILogger logger, object obj, Exception ex) {
            logger.Log(LogLevel.Warn, obj, ex);
        }

        public static void LogError(this ILogger logger, object obj, Exception ex) {
            logger.Log(LogLevel.Error, obj, ex);
        }

        public static void LogFatal(this ILogger logger, object obj, Exception ex) {
            logger.Log(LogLevel.Fatal, obj, ex);
        }

        public static void LogTrace(this ILogger logger, object obj, Exception ex) {
            logger.Log(LogLevel.Trace, obj, ex);
        }

    }
}