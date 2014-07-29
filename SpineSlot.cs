using UnityEngine;

namespace SpineImporter
{

	public class SpineSlot : MonoBehaviour
	{

		[SerializeField] private string _defaultAttachment;
		[SerializeField] private bool _additiveBlending;
		[SerializeField] private Color _color;
		[SerializeField] private float _drawOrder;

		public string DefaultAttachment
		{
			get { return _defaultAttachment; }
			set { _defaultAttachment = value; }
		}

		public bool AdditiveBlending
		{
			get { return _additiveBlending; }
			set { _additiveBlending = value; }
		}

		public Color Color
		{
			get { return _color; }
			set { _color = value; }
		}

		public float DrawOrder
		{
			get { return _drawOrder; }
			set { _drawOrder = value; }
		}

		public void SetAttachment(string name)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform attachment = transform.GetChild(i);
				attachment.gameObject.SetActive(attachment.name == name);
			}
		}
	}

}
