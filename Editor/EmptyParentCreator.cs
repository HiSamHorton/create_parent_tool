using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EmptyParentCreator
{
	[MenuItem("GameObject/Create Parent For Selected", priority = 0)]
	private static void CreateEmptyParent(MenuCommand command)
	{
		// This happens when this button is clicked via hierarchy's right click context menu
		// and is called once for each object in the selection. We don't want that, we want
		// the function to be called only once so that there aren't multiple empty parents 
		// generated in one call
		if (command.context)
		{
			EditorApplication.update -= CallCreateEmptyParentOnce;
			EditorApplication.update += CallCreateEmptyParentOnce;

			return;
		}

		Transform[] selection = Selection.transforms;
		if (selection.Length == 0)
			return;

		List<Renderer> renderers = new List<Renderer>(8);
		Bounds bounds = new Bounds();
		bool boundsInitialized = false;
		for (int i = 0; i < selection.Length; i++)
		{
			if (AssetDatabase.Contains(selection[i].gameObject))
				continue;

			renderers.Clear();
			selection[i].GetComponentsInChildren(renderers);

			for (int j = renderers.Count - 1; j >= 0; j--)
			{
				if (boundsInitialized)
					bounds.Encapsulate(renderers[j].bounds);
				else
				{
					bounds = renderers[j].bounds;
					boundsInitialized = true;
				}
			}
		}

		Transform newParent = new GameObject().transform;
		newParent.position = bounds.center;
		//newParent.position -= new Vector3( 0f, bounds.extents.y, 0f ); // Move pivot to the bottom

		Undo.RegisterCreatedObjectUndo(newParent.gameObject, "Parent Selected");
		for (int i = 0; i < selection.Length; i++)
		{
			if (AssetDatabase.Contains(selection[i].gameObject))
				continue;

			Undo.SetTransformParent(selection[i], newParent, "Parent Selected");
		}

		Selection.activeTransform = newParent;
	}

	private static void CallCreateEmptyParentOnce()
	{
		EditorApplication.update -= CallCreateEmptyParentOnce;
		CreateEmptyParent(new MenuCommand(null));
	}
}
