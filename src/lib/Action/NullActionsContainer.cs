/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge
{
    public class NullActionsContainer : IActionsContainer
    {

        public IAction[] Actions { get; } = Array.Empty<IAction>();
        public void Add(IAction action)
        {
        }

        public void Spawn(ILogic data)
        {
        }
    }
}
