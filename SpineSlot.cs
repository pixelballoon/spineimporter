using UnityEngine;
using System.Collections;

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

		public void Clear()
		{
			DestroyImmediate(GetComponent<SpriteRenderer>());
			DestroyImmediate(GetComponent<SkinnedMeshRenderer>());
		}
		
		public void SetAttachment(SpineAttachment attachment)
		{
			Clear();

			switch (attachment.Type)
			{
				case SpineAttachment.AttachmentType.Region:
				{
					SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
					spriteRenderer.sprite = attachment.Sprite;
					transform.localPosition = new Vector3(attachment.PositionOffset.x, attachment.PositionOffset.y, _drawOrder);
					transform.localEulerAngles = new Vector3(0, 0, attachment.RotationOffset);
					break;
				}
			}
		}

	}

}
