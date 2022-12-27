/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;

namespace Piot.Clog
{
    public static class Utils
    {
        public static string ArgumentValueToString(object arg)
        {
            switch (arg)
            {
                case null:
                    return "null";
                case string:
                    return arg.ToString()!;
            }

            if (arg is not IEnumerable enumerable)
            {
                return arg.ToString()!;
            }

            var stringArray = new List<string>();
            var x = enumerable.GetEnumerator();
            while (x.MoveNext())
            {
                stringArray.Add(x.Current.ToString()!);
            }

            var result = $"[ {string.Join(",", stringArray)} ]";
            if (stringArray.Count == 0)
            {
                result = "[]";
            }

            return result;
        }
    }
}