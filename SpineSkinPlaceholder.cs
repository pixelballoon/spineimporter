using UnityEngine;
using System.Collections;

public class SpineSkinPlaceholder : MonoBehaviour
{

	public void SetSkin(string name)
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform skin = transform.GetChild(i);
			skin.gameObject.SetActive(skin.name == name);
		}
	}

}
