using UnityEditor;

[CustomEditor(typeof(Bee))]
public class CustomBeeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var bee = (Bee)target;
        bee.SetDefaultMaterial();
    }
}
