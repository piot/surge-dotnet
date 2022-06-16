/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;

namespace Piot.Surge
{
    public class ActionsContainer : IActionsContainer
    {
        private readonly List<IAction> actions = new();

        public void Add(IAction action)
        {
            actions.Add(action);
        }

        public IAction[] Actions => actions.ToArray();

        public void Clear()
        {
            actions.Clear();
        }
    }
}