/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge
{
    public class LogicAttribute : Attribute
    {
        public bool generate { get; set; }
    }

    public class InputAttribute : Attribute
    {
        public bool generate { get; set; }
    }

    public class InputSourceAttribute : Attribute
    {
        public string BindName { get; set; } = string.Empty;
    }
}