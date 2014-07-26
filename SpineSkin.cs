using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace SpineImporter
{

	[Serializable]
	public class SpineSkin
	{

		[Serializable]
		public class Slot
		{
			[SerializeField] private string _name;
			[SerializeField] private List<SpineAttachment> _attachments;

			public string Name
			{
				get { return _name; }
				set { _name = value; }
			}

			public Slot(string name)
			{
				_name = name;
			}

			public SpineAttachment GetAttachment(string name)
			{
				if (_attachments == null)
				{
					return null;
				}

				foreach (SpineAttachment attachment in _attachments)
				{
					if (attachment.Name == name)
					{
						return attachment;
					}
				}

				return null;
			}

			public SpineAttachment GetOrCreateAttachment(string name, SpineAttachment.AttachmentType type)
			{
				if (_attachments == null)
				{
					_attachments = new List<SpineAttachment>();
				}

				foreach (SpineAttachment attachment in _attachments)
				{
					if (attachment.Name == name)
					{
						return attachment;
					}
				}

				SpineAttachment spineAttachment = new SpineAttachment(name, type);
				_attachments.Add(spineAttachment);

				return spineAttachment;
			}
		}

		[SerializeField] private string _name;
		[SerializeField] private List<Slot> _slots;

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public List<Slot> Slots
		{
			get { return _slots; }
		}

		public SpineSkin(string name)
		{
			_name = name;
		}

		public Slot GetSlot(string name)
		{
			if (_slots == null)
			{
				_slots = new List<Slot>();
			}

			foreach (Slot slot in _slots)
			{
				if (slot.Name == name)
				{
					return slot;
				}
			}
			
			Slot s = new Slot(name);
			_slots.Add(s);
			return s;
		}

	}

}
