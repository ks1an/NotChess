using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MatchTest))]
public class MatchTest_Editor : Editor
{
    [SerializeField] int manaForAdd = 2, manaForDecrease = 2;
    [SerializeField] int bonesForAdd = 2, bonesForDecrease = 2;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        bool isGameRunning = Application.isPlaying && GameController.Instance != null && GameController.Instance.player != null;

        // --- mana ---
        EditorGUILayout.Space(10);
        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.LabelField("Mana test", EditorStyles.boldLabel);
            manaForAdd = EditorGUILayout.IntField("Mana for add", manaForAdd);
            manaForDecrease = EditorGUILayout.IntField("Mana for decrease", manaForDecrease);

            EditorGUILayout.Space(5);

            if (GUILayout.Button($"Add {manaForAdd} Mana"))
            {
                if (isGameRunning) GameController.Instance.player.IncreaseMana(manaForAdd);
                else ShowWarning();
            }
            if (GUILayout.Button($"Decrease {manaForDecrease} Mana"))
            {
                if (isGameRunning) GameController.Instance.player.DecreaseMana(manaForDecrease);
                else ShowWarning();
            }
        }

        // --- Bones ---
        EditorGUILayout.Space(10);
        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.LabelField("Bones test", EditorStyles.boldLabel);
            bonesForAdd = EditorGUILayout.IntField("bones for add", bonesForAdd);
            bonesForDecrease = EditorGUILayout.IntField("bones for decrease", bonesForDecrease);

            EditorGUILayout.Space(5);

            if (GUILayout.Button($"Add {bonesForAdd} Bones"))
            {
                if (isGameRunning) GameController.Instance.player.IncreaseGraveTokens(bonesForAdd);
                else ShowWarning();
            }
            if (GUILayout.Button($"Decrease {bonesForDecrease} Bones"))
            {
                if (isGameRunning) GameController.Instance.player.DecreaseGraveTokens(bonesForDecrease);
                else ShowWarning();
            }
        }

        // --- MATCH FINISH (WIN / LOSE) ---
        EditorGUILayout.Space(10);
        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.LabelField("Match End Test", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
            if (GUILayout.Button("Force Win Match"))
            {
                if (isGameRunning)
                {
                    GameController.Instance.states.GameEnd(GameController.Instance.player.GetTeam(), 0, 0);
                }
                else ShowWarning();
            }

            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
            if (GUILayout.Button("Force Lose Match"))
            {
                if (isGameRunning)
                {
                    GameController.Instance.states.GameEnd(GameController.Instance.enemy.GetTeam(), 0, 0);

                }
                else ShowWarning();
            }
            GUI.backgroundColor = Color.white;
        }
    }

    private void ShowWarning()
    {
        Debug.LogWarning("Play mode not running or GameController is null");
    }
}
