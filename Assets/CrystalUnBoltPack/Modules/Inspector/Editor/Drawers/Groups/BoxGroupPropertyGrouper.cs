using UnityEngine;

namespace CrystalUnbolt
{
    [PropertyGrouper(typeof(BoxGroupAttribute))]
    public class BoxGroupPropertyGrouper : PropertyGrouper
    {
        public override void BeginGroup(CustomInspector editor, string groupID, string label)
        {
            CrystalEditorGUILayoutCustom.BeginBoxGroup(label);
        }

        public override void EndGroup()
        {
            CrystalEditorGUILayoutCustom.EndBoxGroup();
        }
    }
}
