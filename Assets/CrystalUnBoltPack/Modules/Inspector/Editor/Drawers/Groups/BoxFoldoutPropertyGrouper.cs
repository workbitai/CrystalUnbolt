namespace CrystalUnbolt
{
    [PropertyGrouper(typeof(BoxFoldoutAttribute))]
    public class BoxFoldoutPropertyGrouper : PropertyGrouper
    {
        public override void BeginGroup(CustomInspector editor, string groupID, string label)
        {
            EditorFoldoutBool foldoutBool = editor.GetFoldout(groupID);

            foldoutBool.Value = CrystalEditorGUILayoutCustom.BeginExpandBoxGroup(!string.IsNullOrEmpty(label) ? label : groupID, foldoutBool.Value);
        }

        public override void EndGroup()
        {
            CrystalEditorGUILayoutCustom.EndBoxGroup();
        }

        public override bool DrawRenderers(CustomInspector editor, string groupID) => editor.GetFoldout(groupID).Value;
    }
}
