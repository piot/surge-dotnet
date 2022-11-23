/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

// ReSharper disable CheckNamespace

namespace Piot.Clog
{
    public sealed class Log : ILog, ILogConfiguration
    {
        readonly string prefix;
        readonly ILogTarget target;

        public Log(ILogTarget target, LogLevel logLevel = LogLevel.Debug, string prefix = "")
        {
            LogLevel = logLevel;
            this.target = target;
            this.prefix = prefix;
        }

#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Info(string message)
        {
            if (LogLevel > LogLevel.Info)
            {
                return;
            }

            target.Log(LogLevel.Info, prefix, message, Array.Empty<object>());
        }

        #if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Info<T>(string message, T arg) where T : notnull
        {
            if (LogLevel > LogLevel.Info)
            {
                return;
            }

            target.Log(LogLevel.Info, prefix, message, new object[]
            {
                arg
            });
        }

#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif
        public void Info<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (LogLevel > LogLevel.Info)
            {
                return;
            }

            target.Log(LogLevel.Info, prefix, message, new object[]
            {
                arg0, arg1
            });
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Info<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (LogLevel > LogLevel.Info)
            {
                return;
            }

            target.Log(LogLevel.Info, prefix, message, new object[]
            {
                arg0, arg1, arg2
            });
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Info(string message, params object[] args)
        {
            if (LogLevel > LogLevel.Info)
            {
                return;
            }

            target.Log(LogLevel.Info, prefix, message, args);
        }

        #if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Warn(string message)
        {
            if (LogLevel > LogLevel.Warning)
            {
                return;
            }

            target.Log(LogLevel.Warning, prefix, message, Array.Empty<object>());
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Warn<T>(string message, T arg) where T : notnull
        {
            if (LogLevel > LogLevel.Warning)
            {
                return;
            }

            target.Log(LogLevel.Warning, prefix, message, new object[]
            {
                arg
            });
        }

        #if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Warn<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (LogLevel > LogLevel.Warning)
            {
                return;
            }

            target.Log(LogLevel.Warning, prefix, message, new object[]
            {
                arg0, arg1
            });
        }

        #if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Warn<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (LogLevel > LogLevel.Warning)
            {
                return;
            }

            target.Log(LogLevel.Warning, prefix, message, new object[]
            {
                arg0, arg1, arg2
            });
        }

        #if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Warn(string message, params object[] args)
        {
            if (LogLevel > LogLevel.Warning)
            {
                return;
            }

            target.Log(LogLevel.Warning, prefix, message, args);
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Debug(string message)
        {
            if (LogLevel > LogLevel.Debug)
            {
                return;
            }

            target.Log(LogLevel.Debug, prefix, message, Array.Empty<object>());
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Debug<T>(string message, T arg) where T : notnull
        {
            if (LogLevel > LogLevel.Debug)
            {
                return;
            }

            target.Log(LogLevel.Debug, prefix, message, new object[]
            {
                arg
            });
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Debug<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (LogLevel > LogLevel.Debug)
            {
                return;
            }

            target.Log(LogLevel.Debug, prefix, message, new object[]
            {
                arg0, arg1
            });
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Debug<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (LogLevel > LogLevel.Debug)
            {
                return;
            }

            target.Log(LogLevel.Debug, prefix, message, new object[]
            {
                arg0, arg1, arg2
            });
        }

        #if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Debug(string message, params object[] args)
        {
            if (LogLevel > LogLevel.Debug)
            {
                return;
            }

            target.Log(LogLevel.Debug, prefix, message, args);
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void DebugLowLevel(string message)
        {
            if (LogLevel > LogLevel.LowLevel)
            {
                return;
            }

            target.Log(LogLevel.LowLevel, prefix, message, Array.Empty<object>());
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void DebugLowLevel<T>(string message, T arg) where T : notnull
        {
            if (LogLevel > LogLevel.LowLevel)
            {
                return;
            }

            target.Log(LogLevel.LowLevel, prefix, message, new object[]
            {
                arg
            });
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void DebugLowLevel<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (LogLevel > LogLevel.LowLevel)
            {
                return;
            }

            target.Log(LogLevel.LowLevel, prefix, message, new object[]
            {
                arg0, arg1
            });
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void DebugLowLevel<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (LogLevel > LogLevel.LowLevel)
            {
                return;
            }

            target.Log(LogLevel.LowLevel, prefix, message, new object[]
            {
                arg0, arg1, arg2
            });
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void DebugLowLevel(string message, params object[] args)
        {
            if (LogLevel > LogLevel.LowLevel)
            {
                return;
            }

            target.Log(LogLevel.LowLevel, prefix, message, args);
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif


        public void Notice(string message)
        {
            if (LogLevel > LogLevel.Notice)
            {
                return;
            }

            target.Log(LogLevel.Notice, prefix, message, Array.Empty<object>());
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Notice<T>(string message, T arg) where T : notnull
        {
            if (LogLevel > LogLevel.Notice)
            {
                return;
            }

            target.Log(LogLevel.Notice, prefix, message, new object[]
            {
                arg
            });
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Notice<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (LogLevel > LogLevel.Notice)
            {
                return;
            }

            target.Log(LogLevel.Notice, prefix, message, new object[]
            {
                arg0, arg1
            });
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Notice<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (LogLevel > LogLevel.Notice)
            {
                return;
            }

            target.Log(LogLevel.Notice, prefix, message, new object[]
            {
                arg0, arg1, arg2
            });
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Notice(string message, params object[] args)
        {
            if (LogLevel > LogLevel.Notice)
            {
                return;
            }

            target.Log(LogLevel.Notice, prefix, message, args);
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif


        public void Error(string message)
        {
            if (LogLevel > LogLevel.Error)
            {
                return;
            }

            target.Log(LogLevel.Error, prefix, message, Array.Empty<object>());
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Error<T>(string message, T arg) where T : notnull
        {
            if (LogLevel > LogLevel.Error)
            {
                return;
            }

            target.Log(LogLevel.Error, prefix, message, new object[]
            {
                arg
            });
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Error<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (LogLevel > LogLevel.Error)
            {
                return;
            }

            target.Log(LogLevel.Error, prefix, message, new object[]
            {
                arg0, arg1
            });
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Error<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (LogLevel > LogLevel.Error)
            {
                return;
            }

            target.Log(LogLevel.Error, prefix, message, new object[]
            {
                arg0, arg1, arg2
            });
        }
#if UNITY_2022_1_OR_NEWER
        [HideInStackTrace(true)]
#endif

        public void Error(string message, params object[] args)
        {
            if (LogLevel > LogLevel.Error)
            {
                return;
            }

            target.Log(LogLevel.Error, prefix, message, args);
        }

        public ILog SubLog(string debugPrefix)
        {
            return new Log(target, LogLevel, prefix != "" ? prefix + "/" + debugPrefix : debugPrefix);
        }

        public LogLevel LogLevel { get; set; }
    }
}