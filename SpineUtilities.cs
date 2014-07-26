using UnityEngine;
using System.Collections;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SpineImporter
{

	public class SpineUtilities : MonoBehaviour
	{

#if UNITY_EDITOR
		public static void SetAnimationSettings(AnimationClip clip, AnimationClipSettings settings)
		{
			MethodInfo methodInfo = typeof(AnimationUtility).GetMethod("SetAnimationClipSettings", BindingFlags.Static | BindingFlags.NonPublic);
			methodInfo.Invoke(null, new object[] { clip, settings });
		}
#endif

	}

}
