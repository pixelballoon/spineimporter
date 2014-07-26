using UnityEngine;
using System.Collections;

namespace SpineImporter
{

	public class SpineBone : MonoBehaviour
	{

		[SerializeField] private float _length;

		public float Length
		{
			get { return _length; }
			set { _length = value; }
		}

		void OnDrawGizmos()
		{
			Gizmos.DrawLine(transform.position, transform.TransformPoint(new Vector3(_length, 0, 0)));
		}

	}

}
