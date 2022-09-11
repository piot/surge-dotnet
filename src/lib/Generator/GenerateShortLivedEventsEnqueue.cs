/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text;

namespace Piot.Surge.Generator
{
    public static class GenerateShortLivedEventsEnqueue
    {
        public static void AddEventEnqueue(StringBuilder sb, ShortLivedEventInterface messageInterface, int indent)
        {
            Generator.AddClassDeclaration(sb, "GeneratedEventEnqueue",
                Generator.FullName(messageInterface.eventInterface), indent);

            sb.Append(@"
    private readonly EventStreamPackQueue eventStream;

    public GeneratedEventEnqueue(EventStreamPackQueue eventStream)
    {
        this.eventStream = eventStream;
    }
");
            indent++;

            foreach (var eventMethod in messageInterface.methodInfos)
            {
                sb.Append($"{Generator.Indent(indent)}public void {eventMethod.Name}(");

                var index = 0;
                foreach (var param in eventMethod.GetParameters())
                {
                    if (index != 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append($"{param.ParameterType} {param.Name}");
                    index++;
                }

                sb.Append($@")

{Generator.Indent(indent)}{{
{Generator.Indent(indent + 1)}var writer = eventStream.BitWriter;
#if DEBUG
{Generator.Indent(indent + 1)}BitMarker.WriteMarker(writer, 0x5, 3);
#endif
{Generator.Indent(indent + 1)}writer.WriteBits(EventArchetypeConstants.{eventMethod.Name}, 7);

");

                foreach (var param in eventMethod.GetParameters())
                {
                    var fieldName = param.Name!;
                    sb.Append(
                        $@"{Generator.Indent(indent + 1)}{GenerateSerializers.BitSerializeMethod(param.ParameterType, fieldName)};
");
                }

                sb.Append($@"
{Generator.Indent(indent)}}}

");
            }

            indent--;
            Generator.AddEndDeclaration(sb, indent);
        }
    }
}