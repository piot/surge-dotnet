/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class LogicAttribute : Attribute
    {
        public bool generate { get; set; }
    }

    [AttributeUsage(AttributeTargets.Struct)]
    public class InputAttribute : Attribute
    {
        public bool generate { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class InputFetchAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class SimulatedAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public class ShortLivedEventsAttribute : Attribute
    {
    }
}