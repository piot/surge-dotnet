/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.FastTypeInformation;

namespace Piot.Surge.Entities
{
    /// <summary>
    ///     Interfaces that a complete entity must support. Usually implementation is produced from a source code generator.
    /// </summary>
    public interface ICompleteEntity : IEntityBothSerializer, IEntityDeserializer,
        IEntityBitDeserializer, IEntityChanges, IEntityClearChanges,
        ISimpleLogic, IMovementSimulation, IEntityFireChanges, IEntityActions, IEntityActionsDoUnDo,
        IAuthoritativeEntityCaptureSnapshot
    {
        public ILogic Logic { get; }

        public EntityRollMode RollMode { get; set; }

        public ArchetypeId ArchetypeId { get; }

        public TypeInformation TypeInformation { get; }
    }
}