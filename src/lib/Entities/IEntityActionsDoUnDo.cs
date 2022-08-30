/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.LogicAction;

namespace Piot.Surge.Entities
{
    public interface IEntityActionsDoUnDo
    {
        void UnDoAction(IAction action);
        void DoAction(IAction action);
    }
}