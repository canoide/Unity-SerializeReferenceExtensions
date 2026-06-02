using System;
using UnityEngine;

/// <summary>
/// Attribute to specify the type of the field serialized by the SerializeReference attribute in the inspector.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class SubclassSelectorAttribute : PropertyAttribute
{

#if UNITY_2021_3_OR_NEWER
    // NOTE: Use managedReferenceValue getter to invoke instance method in SubclassSelectorDrawer.
    public bool UseToStringAsLabel { get; set; }
#endif

	/// <summary>
	/// If true, the full namespace will be shown in the popup.
	/// </summary>
	public bool ShowFullNamespace { get; set; }

	public SubclassSelectorAttribute (bool showFullNamespace = false) {
		ShowFullNamespace = showFullNamespace;
	}

}