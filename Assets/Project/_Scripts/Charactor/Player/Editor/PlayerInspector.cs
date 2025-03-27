using UnityEditor;
using UnityEngine.TextCore.Text;



[CustomEditor(typeof(PlayerMovement), true)]
[CanEditMultipleObjects]
public class PlayerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        PlayerMovement playerMovement = (PlayerMovement)target;

        if (playerMovement.playerState !=  null)
        {
            EditorGUILayout.LabelField("Movement State", playerMovement.movementState.currentState.ToString());
            EditorGUILayout.LabelField("Facing Direction", playerMovement.isFacingRight ? "Right" : "Left");
        }

        DrawDefaultInspector();

        //serializedObject.ApplyModifiedProperties();
    }
}
