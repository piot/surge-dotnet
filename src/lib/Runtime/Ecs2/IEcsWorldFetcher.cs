/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;

namespace Piot.Surge.Ecs2
{
    public interface IEcsWorldFetcher
    {
        public IEnumerable<object> Components { get; }
        public bool HasComponent<T>(uint entityId) where T : struct;
        public T? Get<T>(uint entityId) where T : struct;
        public T Grab<T>(uint entityId) where T : struct;
    }

}