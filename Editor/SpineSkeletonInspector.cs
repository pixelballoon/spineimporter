using UnityEngine;
using System.Collections;
using UnityEditor;

namespace SpineImporter
{

	[CustomEditor(typeof(SpineSkeleton))]
	public class LevelScriptEditor : Editor 
	{
	    public override void OnInspectorGUI()
	    {
		    SpineSkeleton skeleton = (SpineSkeleton)target;

		    if (GUILayout.Button("Refresh"))
		    {
			    if (skeleton)
			    {
				    skeleton.Refresh();
					return;
			    }
		    }

		    DrawDefaultInspector();
	    }
	}

}
