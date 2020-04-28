using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DynamicAppIconRenderer))]
public class DynamicAppIconRendererEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		EditorGUILayout.Space();

		using (new EditorGUILayout.HorizontalScope())
		{
			EditorGUILayout.PrefixLabel(" "); // if the label is empty, it is ignored amd the button is drawn across (almost) the full width of the inspector.
			if (GUILayout.Button("Render New Icons")) ((DynamicAppIconRenderer) target).RenderIcons();
		}
	}
}
