using CrossStitch.Core.Utility.Extensions;
using CrossStitch.Stitch.BuiltInClassV1;
using System;
using System.Collections.Generic;

namespace CrossStitch.Core.Modules.Stitches.Adaptors.BuiltInClassV1
{
    public class BuiltInClassV1Parameters
    {
        public BuiltInClassV1Parameters(Dictionary<string, string> parameters)
        {
            var typeName = parameters.GetOrDefault(Parameters.TypeName);
            if (string.IsNullOrEmpty(typeName))
                throw new Exception("Type name is not specified");

            StitchType = Type.GetType(typeName);
            if (StitchType == null)
                throw new Exception("Type name is not found");
        }

        public Type StitchType { get; }
    }
}
