using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Crea/abre una escena independiente para el primer juego jugable.
/// Mantiene intacta la escena de diagnostico y reutiliza BiofitBLEMonitor.
/// </summary>
public static class BiofitPrepararDash
{
    private const string ScenePath = "Assets/BIOFIT/Scenes/BiofitDash.unity";

    [MenuItem("BIOFIT/Crear o abrir BIOFIT DASH (primer juego)")]
    public static void Preparar()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        Scene scene = asset == null ?
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single) :
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject cameraObject = GameObject.Find("Main Camera");
        if (cameraObject == null)
        {
            cameraObject = new GameObject("Main Camera");
            UnityEngine.Camera camera = cameraObject.AddComponent<UnityEngine.Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.10f, 0.18f);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        }

        GameObject root = GameObject.Find("BIOFIT_DASH");
        if (root == null) root = new GameObject("BIOFIT_DASH");

        if (root.GetComponent<BiofitBLEMonitor>() == null)
            root.AddComponent<BiofitBLEMonitor>();
        if (root.GetComponent<BiofitDashGame>() == null)
            root.AddComponent<BiofitDashGame>();

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            EditorUtility.DisplayDialog("BIOFIT DASH",
                "No se pudo guardar la escena.", "Aceptar");
            return;
        }

        List<EditorBuildSettingsScene> scenes =
            new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool exists = false;
        foreach (EditorBuildSettingsScene existing in scenes)
            if (existing.path == ScenePath) exists = true;
        if (!exists)
        {
            // El diagnostico conserva el indice 0; Dash se agrega como
            // segunda escena, sin duplicar el receptor BLE en la misma.
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        Selection.activeGameObject = root;
        EditorUtility.DisplayDialog("BIOFIT DASH",
            "Escena creada/abierta: BiofitDash.\n\n" +
            "Presiona PLAY. El cubo avanzara automaticamente. " +
            "Presiona ESPACIO para saltar, R para reiniciar, P para pausa.\n\n" +
            "Para una primera prueba no conectes sensores ni generes APK. " +
            "El salto por BLE esta DESACTIVADO por defecto.",
            "Entendido");
    }
}
