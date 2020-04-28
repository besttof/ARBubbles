using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(fileName = "AppIconRenderer")]
public sealed class DynamicAppIconRenderer : ScriptableObject
{
	[Serializable]
	private class Target
	{
		public Color Background = Color.black;
		public Texture2D TargetTexture = default;
	}

	[Serializable]
	private class CameraSettings
	{
		public float Near = 0.01f;
		public float Far = 10f;
		public Vector3 Position = Vector3.zero;
		public Vector3 Rotation = Vector3.zero;
	}

	[SerializeField] private bool _generateNewIconsOnBuild;
	[Space]
	[SerializeField] private GameObject _prefab;
	[SerializeField] private CameraSettings _cameraSettings;

	[SerializeField] private Target[] _targets;

	private Material _tex2rgbMaterial
	{
		get
		{
			_material.Value.SetFloat("_ManualTex2SRGB", QualitySettings.activeColorSpace == ColorSpace.Linear ? 1f : 0.0f);
			return _material.Value;
		}
	}

	private Lazy<Material> _material = new Lazy<Material>(
		() =>
		{
			var material = new Material(EditorGUIUtility.LoadRequired("SceneView/GUITextureBlit2SRGB.shader") as Shader);
			material.hideFlags |= HideFlags.DontSaveInEditor;

			return material;
		});

	public void RenderIcons()
	{
		var previewRenderer = new PreviewRenderUtility(true);

		foreach (var target in _targets)
		{
			RenderIcon(previewRenderer, target);
		}

		previewRenderer.Cleanup();
	}

	private void RenderIcon(PreviewRenderUtility previewRenderer, Target target)
	{
		const int w = 1024;
		const int h = 1024;
		var rect = new Rect(0f, 0f, w, h);

		previewRenderer.BeginPreview(rect, null);

		previewRenderer.InstantiatePrefabInScene(_prefab);

		previewRenderer.camera.clearFlags = CameraClearFlags.SolidColor;
		previewRenderer.camera.backgroundColor = target.Background;
		previewRenderer.camera.transform.position = _cameraSettings.Position;
		previewRenderer.camera.transform.rotation = Quaternion.Euler(_cameraSettings.Rotation);
		previewRenderer.camera.nearClipPlane = _cameraSettings.Near;
		previewRenderer.camera.farClipPlane = _cameraSettings.Far;
		previewRenderer.Render(true);

		var rt = previewRenderer.EndPreview();

		var temporary = RenderTexture.GetTemporary(w, h, 0, GraphicsFormat.R8G8B8A8_UNorm);
		Graphics.Blit(rt, temporary, _tex2rgbMaterial);
		RenderTexture.active = temporary;
		var texture = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
		texture.ReadPixels(rect, 0, 0);
		texture.Apply();

		RenderTexture.ReleaseTemporary(temporary);

		try
		{
			var path = AssetDatabase.GetAssetPath(target.TargetTexture);
			File.WriteAllBytes(path, texture.EncodeToPNG());
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
		}
		catch (Exception e)
		{
			Debug.LogError($"Unable to write to icon: {e.Message}");
		}
	}

	public sealed class Processor : IPreprocessBuildWithReport
	{
		public int callbackOrder => -1;

		public void OnPreprocessBuild(BuildReport report)
		{
			var settings = Resources.Load<DynamicAppIconRenderer>("Editor/AppIconRenderer");

			if (settings != null && settings._generateNewIconsOnBuild) settings.RenderIcons();
		}
	}
}
