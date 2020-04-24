using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DynamicAppIconRenderer))]
public class DynamicAppIconRendererEditor : UnityEditor.Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUILayout.Button("Render icons")) ((DynamicAppIconRenderer) target).RenderIcons();
	}
}
