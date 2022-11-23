/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge
{
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class LogicAttribute : Attribute
    {
        public bool generate { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class BitSerializerAttribute : Attribute
    {
        public bool generate { get; set; }
    }


    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class GhostAttribute : Attribute
    {
        public bool generate { get; set; }
    }

    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class InputAttribute : Attribute
    {
        public bool generate { get; set; }
    }

    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class ShortLivedEventAttribute : Attribute
    {
    }
}