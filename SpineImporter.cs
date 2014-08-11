using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using Object = UnityEngine.Object;

namespace SpineImporter
{

	public class SpineImporter
	{

		#if UNITY_EDITOR
		[MenuItem("Assets/Spine/Import")]
		public static void Import()
		{
			Object file = Selection.activeObject;
			TextAsset text = Selection.activeObject as TextAsset;

			if (!text)
				return;

			string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(file));

			string prefabPath = EditorUtility.SaveFilePanel("Choose Prefab Location", assetPath, file.name, "prefab");
			prefabPath = "Assets" + prefabPath.Substring(Application.dataPath.Length);

			string name = Path.GetFileNameWithoutExtension(prefabPath);

			string imagePath = assetPath + "/images/";
			string dataPath = Path.GetDirectoryName(prefabPath) + "/" + name + "_data/";

			if (!Directory.Exists(Application.dataPath + dataPath.Substring(6)))
			{
				AssetDatabase.CreateFolder(Path.GetDirectoryName(prefabPath), name + "_data");
			}

			GameObject rootGo = new GameObject(file.name);

			Animator animator = rootGo.AddComponent<Animator>();

			string animatorPath = dataPath + name + ".controller";
			UnityEditorInternal.AnimatorController controller = AssetDatabase.LoadAssetAtPath(animatorPath, typeof(RuntimeAnimatorController)) as UnityEditorInternal.AnimatorController;
			if (!controller)
			{
				controller = UnityEditorInternal.AnimatorController.CreateAnimatorControllerAtPath(animatorPath);
				UnityEditorInternal.AnimatorController.SetAnimatorController(animator, controller);
			}

			UnityEditorInternal.AnimatorController.SetAnimatorController(animator, controller);

			SpineSkeleton skeleton = rootGo.AddComponent<SpineSkeleton>();
			skeleton.ImagePath = imagePath;
			skeleton.DataPath = dataPath;
			skeleton.SourceData = text;
			skeleton.Refresh();

			GameObject prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof (GameObject)) as GameObject;

			if (prefab)
			{
				PrefabUtility.ReplacePrefab(rootGo, prefab);
			}
			else
			{
				PrefabUtility.CreatePrefab(prefabPath, rootGo);
			}
			
			Object.DestroyImmediate(rootGo);
		}

		#endif

	}

}
