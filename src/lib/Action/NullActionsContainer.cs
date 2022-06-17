/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge
{
    public class NullActionsContainer : IActionsContainer
    {
        public void Add(IAction action)
        {
        }

        public IAction[] Actions { get; } = Array.Empty<IAction>();

        public void Spawn(ILogic data)
        {
        }
    }
}