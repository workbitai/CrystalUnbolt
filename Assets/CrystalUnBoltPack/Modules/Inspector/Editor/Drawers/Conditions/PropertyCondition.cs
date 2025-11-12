using System.Collections.Generic;
using System.Reflection;
using System;

namespace CrystalUnbolt
{
    public abstract class PropertyCondition
    {
        public abstract bool CanBeDrawn(CustomInspector editor, FieldInfo fieldInfo, object targetObject, List<Type> nestedTypes);
    }
}
