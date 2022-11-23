/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using Piot.Clog;
using Piot.Surge.Tick;

namespace Piot.Surge.DeltaSnapshot.ComponentFieldMask
{
    public sealed class AllEntitiesChangesEachTickHistory
    {
        readonly Queue<AllEntitiesChangesThisTick> masksQueue = new();

        public AllEntitiesChangesUnionImmutable Fetch(TickIdRange range, ILog log)
        {
            log.Debug("finding out component field mask changes changes for {Range}", range);
            var allEntitiesChangesInRange = masksQueue.Where(mask => range.Contains(mask.TickId)).ToArray();
            var entityChangesMutable = new Dictionary<uint, ComponentFieldMasksMutable>();

            foreach (var allEntitiesChangesThisTick in allEntitiesChangesInRange)
            {
                log.Debug("all entities changes this {TickId} {Count}", allEntitiesChangesThisTick.TickId, allEntitiesChangesThisTick.EntitiesComponentChanges.Count);
                foreach (var componentChangesForOneEntity in allEntitiesChangesThisTick.EntitiesComponentChanges.Values)
                {
                    log.Debug("changes for {EntityId} {Count}", componentChangesForOneEntity.entityId, componentChangesForOneEntity.componentChanges.Count);
                    var wasFound = entityChangesMutable.TryGetValue(componentChangesForOneEntity.entityId.Value, out var found);
                    if (!wasFound || found is null)
                    {
                        found = new(range);
                        entityChangesMutable.Add(componentChangesForOneEntity.entityId.Value, found);
                    }

                    foreach (var component in componentChangesForOneEntity.componentChanges)
                    {
                        log.Debug("merge {ComponentTypeId} {Mask}", component.Key, component.Value);
                        found.MergeComponentFields(component.Key, component.Value);
                    }
                }
            }

            var dict = new Dictionary<uint, ComponentChangesForOneEntityImmutable>();
            foreach (var x in entityChangesMutable)
            {
                var immutableChanges =
                    new ComponentChangesForOneEntityImmutable(new((ushort)x.Key), x.Value.FieldMasks);

                dict.Add(x.Key, immutableChanges);
            }

            return new(range, dict);
        }

        public void DiscardUpTo(TickId tickId)
        {
            while (masksQueue.Count > 0)
            {
                if (masksQueue.Peek().TickId.tickId >= tickId.tickId)
                {
                    break;
                }

                masksQueue.Dequeue();
            }
        }

        public void Enqueue(AllEntitiesChangesThisTick componentFieldMasksForTick)
        {
            masksQueue.Enqueue(componentFieldMasksForTick);
        }
    }
}