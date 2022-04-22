using System.Collections.Generic;

namespace General.Shaders
{
    interface IReferenceHost
    {
        HashSet<Declaration> References { get; }
        void AppendReference(Declaration reference);
    }
}
