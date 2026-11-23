using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Builds the demo scene from code and exports a WebGL build into the website folder.
// Run from the command line:
//   Unity.exe -batchmode -quit -projectPath <this project> -buildTarget WebGL -executeMethod BuildScript.BuildWebGL
// Or in the editor: menu "Portfolio > Build WebGL".
public static class BuildScript
{
    const string ScenePath = "Assets/Scenes/Demo.unity";
    const string MaterialDir = "Assets/Materials";

    [MenuItem("Portfolio/Generate Demo Scene")]
    public static void GenerateScene()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory(MaterialDir);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Lighting
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.45f, 0.5f, 0.65f);
        RenderSettings.ambientEquatorColor = new Color(0.3f, 0.3f, 0.35f);
        RenderSettings.ambientGroundColor = new Color(0.15f, 0.12f, 0.12f);

        var lightGo = new GameObject("Sun");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.shadows = LightShadows.Soft;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Camera
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.09f, 0.13f);
        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<OrbitCamera>();

        // Floor
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        floor.name = "Floor";
        floor.transform.position = new Vector3(0f, -0.6f, 0f);
        floor.transform.localScale = new Vector3(10f, 0.1f, 10f);
        floor.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Floor", new Color(0.2f, 0.22f, 0.28f), 0.3f);
        Object.DestroyImmediate(floor.GetComponent<Collider>());

        // Ring of shapes
        PrimitiveType[] shapes = { PrimitiveType.Cube, PrimitiveType.Sphere, PrimitiveType.Capsule, PrimitiveType.Cylinder };
        int count = 8;
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            var go = GameObject.CreatePrimitive(shapes[i % shapes.Length]);
            go.name = "Shape " + i;
            go.transform.position = new Vector3(Mathf.Cos(angle) * 3.2f, 0.8f, Mathf.Sin(angle) * 3.2f);
            var color = Color.HSVToRGB((float)i / count, 0.65f, 0.95f);
            go.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Shape" + i, color, 0.6f);
            var spinner = go.AddComponent<Spinner>();
            spinner.spinSpeed = new Vector3(20f + i * 7f, 40f + i * 5f, 0f);
        }

        // Centerpiece
        var center = GameObject.CreatePrimitive(PrimitiveType.Cube);
        center.name = "Centerpiece";
        center.transform.position = new Vector3(0f, 1.2f, 0f);
        center.transform.rotation = Quaternion.Euler(35f, 0f, 45f);
        center.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Center", new Color(1f, 0.85f, 0.4f), 0.85f);
        var cs = center.AddComponent<Spinner>();
        cs.spinSpeed = new Vector3(0f, 60f, 0f);
        cs.bobHeight = 0.4f;

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
    }

    static Material MakeMaterial(string name, Color color, float smoothness)
    {
        string path = $"{MaterialDir}/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        mat.SetFloat("_Glossiness", smoothness);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    [MenuItem("Portfolio/Build WebGL")]
    public static void BuildWebGL()
    {
        GenerateScene();

        PlayerSettings.productName = "Unity Test";
        PlayerSettings.companyName = "Alexander De Smet";
        // GitHub Pages can't send Content-Encoding headers for .br/.gz files, so ship uncompressed.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.template = "PROJECT:Portfolio"; // Assets/WebGLTemplates/Portfolio: canvas fills its frame
        PlayerSettings.SplashScreen.showUnityLogo = false; // ignored on Personal licenses
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.High);
        PlayerSettings.defaultWebScreenWidth = 960;
        PlayerSettings.defaultWebScreenHeight = 600;

        string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../projects/unity-test"));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = outDir,
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        });

        // The site's root index.html is the same page, pointing at this build's files.
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            string html = File.ReadAllText(Path.Combine(outDir, "index.html"))
                .Replace("\"Build/", "\"projects/unity-test/Build/")
                .Replace("\"StreamingAssets\"", "\"projects/unity-test/StreamingAssets\"")
                .Replace("<title>" + PlayerSettings.productName + "</title>", "<title>Alexander De Smet</title>");
            File.WriteAllText(Path.Combine(outDir, "../../index.html"), html);
        }

        Debug.Log($"[BuildScript] Result: {report.summary.result}, size {report.summary.totalSize / 1024 / 1024} MB -> {outDir}");
        if (Application.isBatchMode)
            EditorApplication.Exit(report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
    }
}
