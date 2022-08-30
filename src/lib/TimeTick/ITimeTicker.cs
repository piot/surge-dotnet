/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.MonotonicTime;

namespace Piot.Surge.TimeTick
{
    public interface ITimeTicker
    {
        public void Update(Milliseconds now);
    }
}