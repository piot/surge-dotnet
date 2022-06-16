/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Clog
{
    public interface ILog
    {
        public void Info(string s);
        public void Info<T>(string s, T arg);
        public void Info<T0, T1>(string s, T0 arg0, T1 arg1);
        public void Info<T0, T1, T2>(string s, T0 arg0, T1 arg1, T2 arg2);
        public void Info<T0, T1, T2>(string s, object[] args);
    }
}