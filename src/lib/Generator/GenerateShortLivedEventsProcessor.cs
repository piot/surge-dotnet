/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text;

namespace Piot.Surge.Generator
{
    public static class GenerateShortLivedEventsProcessor
    {
        public static void AddEventProcessor(StringBuilder sb, ShortLivedEventInterface messageInterface, int indent)
        {
            Generator.AddClassDeclaration(sb, "GeneratedEventProcessor", "IEventProcessor", indent);
            sb.Append(@"
    private readonly Tests.Surge.ExampleGame.IShortEvents target;
    public GeneratedEventProcessor(Tests.Surge.ExampleGame.IShortEvents target)
    {
        this.target = target;
    }

    public void ReadAndApply(IBitReader reader)
    {
#if DEBUG
        BitMarker.AssertMarker(reader, 0x5, 3);
#endif
        var archetypeValue = reader.ReadBits(7);

        switch (archetypeValue)
        {
");
            foreach (var methodInfo in messageInterface.methodInfos)
            {
                sb.Append($@"
        case EventArchetypeConstants.{methodInfo.Name}:
            target.{methodInfo.Name}(
");
                var paramIndex = 0;
                foreach (var parameter in methodInfo.GetParameters())
                {
                    if (paramIndex != 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(GenerateSerializers.BitDeSerializeMethod(parameter.ParameterType));
                    paramIndex++;
                }

                sb.Append(@");
            break;
");
            }

            sb.Append(@"
          default:
               throw new Exception($""Unknown event {archetypeValue}"");
        }
    }
");

            sb.Append(@"
    public void SkipOneEvent(IBitReader reader)
    {
#if DEBUG
        BitMarker.AssertMarker(reader, 0x5, 3);
#endif
        var archetypeValue = reader.ReadBits(7);

        switch (archetypeValue)
        {
");

            foreach (var methodInfo in messageInterface.methodInfos)
            {
                sb.Append($@"
        case EventArchetypeConstants.{methodInfo.Name}:
            
");
                foreach (var parameter in methodInfo.GetParameters())
                {
                    sb.Append($@"{GenerateSerializers.BitDeSerializeMethod(parameter.ParameterType, false)};
");
                }

                sb.Append(@"
            break;
");
            }

            sb.Append(@"
          default:
               throw new Exception($""Unknown event to skip {archetypeValue}"");
        }
    }
");


            Generator.AddEndDeclaration(sb, indent);
        }
    }
}