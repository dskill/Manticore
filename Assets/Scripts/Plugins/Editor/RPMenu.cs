using UnityEngine;
using UnityEditor;
using System.Collections;

public class RPMenu : MonoBehaviour {
	
	[MenuItem("RP/Assets/Build AssetBundle From Selection - Track dependencies")]
    static void ExportResource () {
        // Note: BuildAssetBundle API is deprecated in Unity 5+
        // Use AssetBundle workflow with AssetBundleBuild instead
        Debug.LogWarning("BuildAssetBundle is deprecated. Please use the AssetBundle Browser or AssetBundleBuild workflow.");

        // Legacy code commented out for Unity 2019+ compatibility
        /*
        string path = EditorUtility.SaveFilePanel ("Save Resource", "", "New Resource", "unity3d");
        if (path.Length != 0) {
            Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets);
            Selection.objects = selection;
        }
        */
    }
    [MenuItem("RP/Assets/Build AssetBundle From Selection - No dependency tracking")]
    static void ExportResourceNoTrack () {
        // Note: BuildAssetBundle API is deprecated in Unity 5+
        Debug.LogWarning("BuildAssetBundle is deprecated. Please use the AssetBundle Browser or AssetBundleBuild workflow.");

        // Legacy code commented out for Unity 2019+ compatibility
        /*
        string path = EditorUtility.SaveFilePanel ("Save Resource", "", "New Resource", "unity3d");
        if (path.Length != 0) {
            BuildPipeline.BuildAssetBundle(Selection.activeObject, Selection.objects, path);
        }
        */
    }
	
	// Note: AudioImporter properties (threeD, format, loadType) are deprecated in Unity 2019+
	// Use AudioImporterSampleSettings for platform-specific settings instead
	// These menu items are commented out for Unity 2019+ compatibility

	/*
	[MenuItem ("RP/Audio/Disable 3D Sound")]
	static void Disable3DSound()
	{
		Object[] audioclips = GetSelectedAudioclips();
        foreach (AudioClip audioclip in audioclips) {
			string path = AssetDatabase.GetAssetPath(audioclip);
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
			if(audioImporter.threeD)
			{
				audioImporter.threeD = false;
				AssetDatabase.ImportAsset(path);
			}
		}
	}

	[MenuItem ("RP/Audio/Compress")]
	static void Compress()
	{
		Object[] audioclips = GetSelectedAudioclips();
        foreach (AudioClip audioclip in audioclips) {
			string path = AssetDatabase.GetAssetPath(audioclip);
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
			if(audioImporter.format == AudioImporterFormat.Native)
			{
				audioImporter.format = AudioImporterFormat.Compressed;
				AssetDatabase.ImportAsset(path);
			}
		}
	}

	[MenuItem ("RP/Audio/Compress")]
	static void Uncompress()
	{
		Object[] audioclips = GetSelectedAudioclips();
        foreach (AudioClip audioclip in audioclips) {
			string path = AssetDatabase.GetAssetPath(audioclip);
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
			if(audioImporter.format == AudioImporterFormat.Compressed)
			{
				audioImporter.format = AudioImporterFormat.Native;
				AssetDatabase.ImportAsset(path);
			}
		}
	}

	[MenuItem ("RP/Audio/Set Music Import Settings")]
	static void SetMusicImportSettings()
	{
		Object[] audioclips = GetSelectedAudioclips();
        foreach (AudioClip audioclip in audioclips) {
			bool changed = false;
			string path = AssetDatabase.GetAssetPath(audioclip);
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
			if(audioImporter.format == AudioImporterFormat.Native)
			{
				audioImporter.format = AudioImporterFormat.Compressed;
				changed = true;
			}
			if(audioImporter.threeD)
			{
				audioImporter.threeD = false;
				changed = true;
			}
			if(audioImporter.loadType != AudioClipLoadType.Streaming)
			{
				audioImporter.loadType = AudioClipLoadType.Streaming;
				changed = true;
			}

			if(changed)
				AssetDatabase.ImportAsset(path);
		}
	}

	[MenuItem ("RP/Audio/Set SFX Import Settings")]
	static void SetSFXImportSettings()
	{
		Object[] audioclips = GetSelectedAudioclips();
        foreach (AudioClip audioclip in audioclips) {
			bool changed = false;
			string path = AssetDatabase.GetAssetPath(audioclip);
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
			if(audioImporter.threeD)
			{
				audioImporter.threeD = false;
				changed = true;
			}
			if(audioImporter.loadType != AudioClipLoadType.CompressedInMemory)
			{
				audioImporter.loadType = AudioClipLoadType.CompressedInMemory;
				changed = true;
			}

			if(changed)
				AssetDatabase.ImportAsset(path);
		}
	}

	[MenuItem ("RP/Audio/Set Dialogue Settings")]
	static void SetDialogueSettings()
	{
		Object[] audioclips = GetSelectedAudioclips();
        foreach (AudioClip audioclip in audioclips) {
			bool changed = false;
			string path = AssetDatabase.GetAssetPath(audioclip);
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
			if(audioImporter.threeD)
			{
				audioImporter.threeD = false;
				changed = true;
			}
			if(audioImporter.loadType != AudioClipLoadType.CompressedInMemory)
			{
				audioImporter.loadType = AudioClipLoadType.CompressedInMemory;
				changed = true;
			}
			if(audioImporter.format == AudioImporterFormat.Native)
			{
				audioImporter.format = AudioImporterFormat.Compressed;
				changed = true;
			}

			if(changed)
				AssetDatabase.ImportAsset(path);
		}
	}
	*/
	
	static Object[] GetSelectedAudioclips()
	{
		return Selection.GetFiltered(typeof(AudioClip), SelectionMode.DeepAssets);
	}
}
