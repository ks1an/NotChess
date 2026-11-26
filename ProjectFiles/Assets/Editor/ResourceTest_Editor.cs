using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ResourceTest))]
public class ResourceTest_Editor : Editor
{
    [SerializeField] int MagicCrystalsForAdd = 125, magicCrystalsForDecrease = 125;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        bool isGameRunning = Application.isPlaying && GameController.Instance != null && GameController.Instance.player != null;

        EditorGUILayout.Space(10);
        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.LabelField("magicCrystals test", EditorStyles.boldLabel);
            MagicCrystalsForAdd = EditorGUILayout.IntField("magicCrystals for add", MagicCrystalsForAdd);
            magicCrystalsForDecrease = EditorGUILayout.IntField("magicCrystals for decrease", magicCrystalsForDecrease);

            EditorGUILayout.Space(5);

            if (GUILayout.Button($"Add {MagicCrystalsForAdd} magicCrystals"))
            {
                if (isGameRunning) GameController.Instance.playerData.MagicCrystals.Value += MagicCrystalsForAdd;
                else ShowWarning();
            }
            if (GUILayout.Button($"Decrease {magicCrystalsForDecrease} magicCrystals"))
            {
                if (isGameRunning) GameController.Instance.playerData.MagicCrystals.Value -= magicCrystalsForDecrease;
                else ShowWarning();
            }
        }
    }

    private void ShowWarning()
    {
        Debug.LogWarning("Play mode not running or GameController is null");
    }
}
