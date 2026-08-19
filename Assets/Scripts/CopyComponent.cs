using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CopiarComponentes : MonoBehaviour
{
    public GameObject origen; // Aquí arrastras tu cápsula

    public void Copiar()
    {
        if (origen == null)
        {
            Debug.LogWarning("Falta asignar el objeto origen (la cápsula).");
            return;
        }

        Component[] componentes = origen.GetComponents<Component>();

        foreach (Component c in componentes)
        {
            // Saltar el Transform (no se puede copiar, ya existe)
            if (c is Transform) continue;

            UnityEditorInternal.ComponentUtility.CopyComponent(c);
            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(this.gameObject);
        }

        Debug.Log("Componentes copiados desde " + origen.name);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CopiarComponentes))]
public class CopiarComponentesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CopiarComponentes script = (CopiarComponentes)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Copiar Componentes"))
        {
            script.Copiar();
        }
    }
}
#endif