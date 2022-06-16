/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Linq;

namespace Piot.Surge.FastTypeInformation
{
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