/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Event
{
    /// <summary>
    ///     An ID that indicates which class or struct to be created
    /// </summary>
    public readonly struct EventArchetypeId
    {
        public readonly byte archetype;

        public EventArchetypeId(byte archetype)
        {
            this.archetype = archetype;
        }

        public override string ToString()
        {
            return $"[EventArchetype {archetype}]";
        }
    }
}