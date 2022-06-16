/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Linq;

namespace Piot.Surge
{
    public interface IGeneratedEntity : IEntitySerializer, IEntityDeserializer, IEntityChanges, IEntityOverwrite,
        ISimpleLogic, IEntityFireChanges, IEntityActions, IEntityActionsDoUnDo
    {
        public ILogic Logic { get; }

        public ArchetypeId ArchetypeId { get; }

        public TypeInformation TypeInformation { get; }
    }

    public struct TypeInformationField
    {
        public ulong mask;

        public Type type;

        public FieldName name;
    }

    public struct FieldName
    {
        public string name;

        public FieldName(string name)
        {
            this.name = name;
        }
    }

    public class TypeInformation
    {
        public TypeInformation(TypeInformationField[] fields)
        {
            this.fields = fields;
        }

        public TypeInformationField[] fields { get; }

        public override string ToString()
        {
            return fields.Aggregate("",
                (current, field) => current + $"{field.mask:X16} {field.type.Name} {field.name.name} ");
        }
    }
}