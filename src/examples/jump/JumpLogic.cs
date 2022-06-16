/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.OctetSerialize;
using Piot.Surge.Types;
using Piot.Surge.TypeSerialization;

namespace Jump;

public class JumperLogic
{
    public Position3 aiming = new();
    public bool isFiring;
    public Action? onIsFiringChanged;
    public Action? onPositionChanged;
    public Position3 position;

    public JumperLogic()
    {
        onPositionChanged += OnPositionChanged;
    }

    private void OnPositionChanged()
    {
        Console.WriteLine("Position was changed");
    }

    public void Tick()
    {
        position = position with { x = position.x + 10 };
        //if (isFiring && isSimulator) commands.Spawn(new RocketLogic { position = position, damage = 120 });
    }


    public void Serialize(ulong mode, IOctetWriter writer)
    {
        if ((mode & 0x01) != 0)
        {
            // Ownership transfers
        }

        if ((mode & 0x02) != 0) Position3Writer.Write(position, writer);

        if ((mode & 0x04) != 0) writer.WriteUInt8(isFiring ? (byte)0xff : (byte)0x00);

        if ((mode & 0x80) != 0)
        {
            writer.WriteUInt8((byte)((mode >> 8) & 0xff));
            if ((mode & 0x01) != 0) Position3Writer.Write(aiming, writer);
        }
    }
}

public class RocketLogic
{
    public int damage;
    public Position3 position = new();


    public void Tick()
    {
        throw new NotImplementedException();
    }
}