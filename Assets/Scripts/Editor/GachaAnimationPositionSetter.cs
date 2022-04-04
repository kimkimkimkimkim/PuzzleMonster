using UnityEditor;
using UnityEngine;

public class GachaAnimationPositionSetter : EditorWindow
{
    private GameObject startPositionObject;
    private GameObject endPositionObject;
    private GameObject cameraObject;
    private GameObject rockObject;
    private GameObject portalObject;
    private float portalDistance = 2.3f;
    private float endPositionDistance = 3.5f;

    [MenuItem("Tools/GachaAnimationPositionSetter")]
    static void Init()
    {
        GetWindow<GachaAnimationPositionSetter>("GachaAnimationPositionSetter");
    }

    private void OnGUI()
    {
        startPositionObject = (GameObject)EditorGUILayout.ObjectField("出発地点", startPositionObject, typeof(GameObject), true);
        endPositionObject = (GameObject)EditorGUILayout.ObjectField("目的地", endPositionObject, typeof(GameObject), true);
        cameraObject = (GameObject)EditorGUILayout.ObjectField("カメラ", cameraObject, typeof(GameObject), true);
        portalObject = (GameObject)EditorGUILayout.ObjectField("ポータル", portalObject, typeof(GameObject), true);
        rockObject = (GameObject)EditorGUILayout.ObjectField("石", rockObject, typeof(GameObject), true);
        portalDistance = EditorGUILayout.FloatField("カメラとポータルの距離", portalDistance);
        endPositionDistance = EditorGUILayout.FloatField("カメラの最終地点と石の距離", endPositionDistance);

        if (GUILayout.Button("配置"))
        {
            if(startPositionObject == null || endPositionObject == null || cameraObject == null || rockObject == null || portalObject == null)
            {
                EditorUtility.DisplayDialog("エラー", "アタッチされていないオブジェクトがあります", "とじる");
                return;
            }

            // オブジェクトの向きを取得
            var direction = rockObject.transform.position - startPositionObject.transform.position;
            var look = Quaternion.LookRotation(direction, Vector3.up);

            // カメラとポータルを取得した向きに
            cameraObject.transform.rotation = look;
            portalObject.transform.rotation = look;

            // カメラ、ポータル、カメラの最終地点を設定した距離に配置
            var portalPosition = startPositionObject.transform.position + direction.normalized * portalDistance;
            var endPosition = rockObject.transform.position - direction.normalized * endPositionDistance;
            portalObject.transform.position = portalPosition;
            endPositionObject.transform.position = endPosition;
            cameraObject.transform.position = startPositionObject.transform.position;

            EditorUtility.DisplayDialog("完了", $"配置が完了しました", "とじる");
        }
    }
}
