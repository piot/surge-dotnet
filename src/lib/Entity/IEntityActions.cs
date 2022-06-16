/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge
{
    public interface IEntityActions
    {
        IAction[] Actions { get; }
    }

    public interface IEntityActionsDoUnDo
    {
        void UnDoAction(IAction action);
        void DoAction(IAction action);
    }
}