// Place this file in: Assets/Editor/NightShiftCityBuilder.cs
// Unity only compiles files inside an "Editor" folder for the editor itself,
// so this script will never be included in a game build.

using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NightShiftCityBuilder
{
    // All generated objects are placed under one root with this name,
    // which makes it easy to find and delete them on the next build.
    private const string RootName = "NightShift City [Generated]";

    // -------------------------------------------------------------------------
    // Menu: Tools > NightShift City > Build Basic Scene
    // -------------------------------------------------------------------------

    [MenuItem("Tools/NightShift City/Build Basic Scene", false, 10)]
    public static void BuildBasicScene()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Build NightShift City",
            "This will create (or replace) the basic city scene.\n\n" +
            "Any previously generated objects will be removed first.\n\n" +
            "Continue?",
            "Build", "Cancel");

        if (!confirmed) return;

        Debug.Log("[NightShift Builder] Starting...");

        // 1. Make sure the Trash tag exists in the project.
        EnsureTagExists("Trash");

        // 2. Remove any objects left from a previous build.
        RemoveGeneratedObjects();

        // 3. Create all materials upfront so they can be reused across objects.
        Material matGround   = MakeMaterial("Mat_Ground",    new Color(0.36f, 0.32f, 0.27f));
        Material matRoad     = MakeMaterial("Mat_Road",      new Color(0.18f, 0.18f, 0.18f));
        Material matBuilding = MakeMaterial("Mat_Building",  new Color(0.42f, 0.48f, 0.54f));
        Material matBody     = MakeMaterial("Mat_RobotBody", new Color(0.15f, 0.35f, 0.62f));
        Material matWheel    = MakeMaterial("Mat_Wheel",     new Color(0.12f, 0.12f, 0.12f));
        Material matEye      = MakeEmissive("Mat_Eye",       new Color(0f, 1f, 1f),         new Color(0f, 0.6f, 0.6f));
        Material matBotLight = MakeEmissive("Mat_BotLight",  new Color(0.5f, 1f, 0.2f),     new Color(0.2f, 0.5f, 0.05f));
        Material matTrash    = MakeMaterial("Mat_Trash",     new Color(1.00f, 0.85f, 0.00f)); // bright yellow — easy to spot

        // 4. Create a root container — everything goes inside it.
        GameObject root = new GameObject(RootName);

        // 5. Build the city geometry.
        BuildGround(root, matGround);
        BuildRoads(root, matRoad);
        BuildBuildings(root, matBuilding);

        // 6. Lighting and camera.
        EnsureDirectionalLight();
        PositionCamera();

        // 7. Create the Trash prefab and wire up logic objects.
        GameObject trashPrefab = CreateTrashPrefab(matTrash);
        BuildCityManager(root);
        BuildTrashSpawner(root, trashPrefab);
        BuildRobots(root, matBody, matWheel, matEye, matBotLight);

        // Mark the scene dirty so Unity prompts you to save.
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log("[NightShift Builder] Done.");

        EditorUtility.DisplayDialog(
            "Build Complete!",
            "NightShift City scene is ready.\n\n" +
            "• Trash prefab → Assets/Prefabs/Trash.prefab\n" +
            "• Press Play to start the simulation.",
            "OK");
    }

    // -------------------------------------------------------------------------
    // Menu: Tools > NightShift City > Clear Generated Objects
    // -------------------------------------------------------------------------

    [MenuItem("Tools/NightShift City/Clear Generated Objects", false, 11)]
    public static void ClearGeneratedObjects()
    {
        if (EditorUtility.DisplayDialog(
            "Clear Generated Objects",
            "Remove all objects created by the NightShift City Builder?",
            "Clear", "Cancel"))
        {
            RemoveGeneratedObjects();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[NightShift Builder] Generated objects removed.");
        }
    }

    // -------------------------------------------------------------------------
    // Cleanup
    // -------------------------------------------------------------------------

    static void RemoveGeneratedObjects()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null)
            Object.DestroyImmediate(existing);
    }

    // -------------------------------------------------------------------------
    // Tag helper — adds a tag to the project if it doesn't already exist
    // -------------------------------------------------------------------------

    static void EnsureTagExists(string tag)
    {
        // Tags are stored in ProjectSettings/TagManager.asset as a serialized array.
        SerializedObject manager = new SerializedObject(
            AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));

        SerializedProperty tagList = manager.FindProperty("tags");

        // Check if tag already exists.
        for (int i = 0; i < tagList.arraySize; i++)
        {
            if (tagList.GetArrayElementAtIndex(i).stringValue == tag)
                return;
        }

        // Add it.
        tagList.InsertArrayElementAtIndex(tagList.arraySize);
        tagList.GetArrayElementAtIndex(tagList.arraySize - 1).stringValue = tag;
        manager.ApplyModifiedProperties();

        Debug.Log("[NightShift Builder] Created project tag: " + tag);
    }

    // -------------------------------------------------------------------------
    // Material helpers
    // -------------------------------------------------------------------------

    // Creates a basic lit material. Tries the URP shader first; falls back to
    // Standard if URP is not installed.
    static Material MakeMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                     ?? Shader.Find("Standard");

        Material mat = new Material(shader) { name = name };
        mat.color = color;
        return mat;
    }

    // Creates a material with an emission color (for glowing parts).
    static Material MakeEmissive(string name, Color baseColor, Color glowColor)
    {
        Material mat = MakeMaterial(name, baseColor);
        mat.EnableKeyword("_EMISSION");

        if (mat.HasProperty("_EmissionColor"))
            mat.SetColor("_EmissionColor", glowColor);

        return mat;
    }

    // -------------------------------------------------------------------------
    // Ground — a single scaled plane
    // -------------------------------------------------------------------------

    static void BuildGround(GameObject root, Material mat)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Plane);
        g.name = "Ground";
        g.transform.SetParent(root.transform, false);
        g.transform.localScale = new Vector3(5f, 1f, 5f); // Plane is 10×10 in local space → 50×50 world units
        g.GetComponent<Renderer>().material = mat;
    }

    // -------------------------------------------------------------------------
    // Roads — flat cubes laid out in a cross-grid pattern
    // -------------------------------------------------------------------------

    static void BuildRoads(GameObject root, Material mat)
    {
        GameObject container = new GameObject("Roads");
        container.transform.SetParent(root.transform, false);

        // (name, world position, scale)
        (string n, Vector3 pos, Vector3 size)[] roads =
        {
            // Main cross roads through the city centre
            ("Road_EW_Center", new Vector3(  0f, 0.02f,   0f), new Vector3(50f, 0.05f,  3f)),
            ("Road_NS_Center", new Vector3(  0f, 0.02f,   0f), new Vector3( 3f, 0.05f, 50f)),
            // Secondary streets
            ("Road_EW_North",  new Vector3(  0f, 0.02f,  15f), new Vector3(50f, 0.05f,  2f)),
            ("Road_EW_South",  new Vector3(  0f, 0.02f, -15f), new Vector3(50f, 0.05f,  2f)),
            ("Road_NS_East",   new Vector3( 15f, 0.02f,   0f), new Vector3( 2f, 0.05f, 50f)),
            ("Road_NS_West",   new Vector3(-15f, 0.02f,   0f), new Vector3( 2f, 0.05f, 50f)),
        };

        foreach (var (n, pos, size) in roads)
        {
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = n;
            road.transform.SetParent(container.transform, false);
            road.transform.position   = pos;
            road.transform.localScale = size;
            road.GetComponent<Renderer>().material = mat;
            // Roads are purely visual — no physics needed.
            Object.DestroyImmediate(road.GetComponent<BoxCollider>());
        }
    }

    // -------------------------------------------------------------------------
    // Buildings — cubes placed in the four quadrants between the roads
    // -------------------------------------------------------------------------

    static void BuildBuildings(GameObject root, Material baseMat)
    {
        GameObject container = new GameObject("Buildings");
        container.transform.SetParent(root.transform, false);

        // (centre X, centre Z, height, footprint width)
        // Using fixed values so the scene looks the same every time you rebuild.
        (float x, float z, float h, float w)[] specs =
        {
            // NE quadrant
            ( 8f,  8f, 8f, 3.0f), (12f,  7f, 5f, 2.5f), ( 8f, 12f, 6f, 2.0f), (18f, 18f, 10f, 4.0f),
            // NW quadrant
            (-8f,  8f, 7f, 3.0f), (-12f, 10f, 4f, 2.0f), (-18f, 18f, 9f, 3.5f),
            // SE quadrant
            ( 8f, -8f, 6f, 2.5f), (11f, -12f, 8f, 3.0f), (18f, -18f, 5f, 2.0f),
            // SW quadrant
            (-8f, -8f, 9f, 3.0f), (-12f, -7f, 5f, 2.0f), (-18f, -18f, 7f, 3.5f),
        };

        // Pre-baked tint values so the buildings have slight colour variety
        // without using Random (which would differ every rebuild).
        float[] tints = { 1.00f, 0.85f, 1.15f, 0.90f, 1.10f, 0.80f, 1.20f,
                          0.95f, 1.05f, 0.88f, 1.12f, 0.82f, 1.08f };

        for (int i = 0; i < specs.Length; i++)
        {
            var (x, z, h, w) = specs[i];
            float t = tints[i % tints.Length];

            // Slightly tint each building differently.
            Material m = new Material(baseMat);
            m.color = new Color(
                Mathf.Clamp01(baseMat.color.r * t),
                Mathf.Clamp01(baseMat.color.g * t),
                Mathf.Clamp01(baseMat.color.b * t));

            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
            b.name = "Building_" + (i + 1).ToString("D2");
            b.transform.SetParent(container.transform, false);
            // Position Y at half the height so the base sits on the ground.
            b.transform.position   = new Vector3(x, h * 0.5f, z);
            b.transform.localScale = new Vector3(w, h, w);
            b.GetComponent<Renderer>().material = m;
        }
    }

    // -------------------------------------------------------------------------
    // Directional light — only creates one if the scene has none
    // -------------------------------------------------------------------------

    static void EnsureDirectionalLight()
    {
        // Search all lights (including inactive) to avoid duplicates.
        Light[] all = Object.FindObjectsByType<Light>(FindObjectsInactive.Include);

        foreach (Light l in all)
            if (l.type == LightType.Directional) return; // Already present.

        GameObject obj = new GameObject("Directional Light");
        Light light = obj.AddComponent<Light>();
        light.type      = LightType.Directional;
        light.intensity = 1.2f;
        light.color     = new Color(1f, 0.95f, 0.85f);
        obj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    // -------------------------------------------------------------------------
    // Camera — uses the existing Main Camera if there is one
    // -------------------------------------------------------------------------

    static void PositionCamera()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            GameObject obj = new GameObject("Main Camera");
            obj.tag = "MainCamera";
            cam = obj.AddComponent<Camera>();
            obj.AddComponent<AudioListener>();
        }

        // Bird's-eye-ish angle overlooking the city.
        cam.transform.position = new Vector3(0f, 25f, -20f);
        cam.transform.rotation = Quaternion.Euler(50f, 0f, 0f);
        cam.farClipPlane       = 300f;
    }

    // -------------------------------------------------------------------------
    // Trash prefab — saved to Assets/Prefabs/Trash.prefab
    //
    // PrefabUtility.SaveAsPrefabAsset requires a temporary scene object:
    //   1. Create the object in the scene.
    //   2. Save it as a prefab asset on disk.
    //   3. Delete the temporary scene object.
    // -------------------------------------------------------------------------

    static GameObject CreateTrashPrefab(Material mat)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // Root — holds the tag, TrashItem component, and visual children.
        // Using an empty root means the logic components stay on the root
        // while visual pieces are children (safe to change looks without
        // breaking the robot/trash system).
        GameObject root = new GameObject("Trash");
        root.tag = "Trash";
        root.AddComponent<TrashItem>();

        // --- Main chunk (large bright yellow cube) ---
        GameObject chunk1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chunk1.name = "Chunk_Main";
        chunk1.transform.SetParent(root.transform, false);
        chunk1.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        chunk1.transform.localScale    = new Vector3(0.55f, 0.50f, 0.55f);
        chunk1.GetComponent<Renderer>().material = mat;
        Object.DestroyImmediate(chunk1.GetComponent<BoxCollider>());

        // --- Second chunk (smaller, rotated — gives a "pile of junk" look) ---
        Material mat2 = new Material(mat);
        mat2.color = new Color(0.95f, 0.60f, 0.05f); // slightly orange variant
        GameObject chunk2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chunk2.name = "Chunk_Top";
        chunk2.transform.SetParent(root.transform, false);
        chunk2.transform.localPosition    = new Vector3(0.08f, 0.60f, 0.05f);
        chunk2.transform.localScale       = new Vector3(0.35f, 0.30f, 0.30f);
        chunk2.transform.localEulerAngles = new Vector3(8f, 28f, 12f);
        chunk2.GetComponent<Renderer>().material = mat2;
        Object.DestroyImmediate(chunk2.GetComponent<BoxCollider>());

        const string path = "Assets/Prefabs/Trash.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);

        Debug.Log("[NightShift Builder] Trash prefab saved → " + path);
        return prefab;
    }

    // -------------------------------------------------------------------------
    // CityManager
    // -------------------------------------------------------------------------

    static void BuildCityManager(GameObject root)
    {
        GameObject obj = new GameObject("CityManager");
        obj.transform.SetParent(root.transform, false);
        obj.AddComponent<CityManager>();
        // CityUI creates its own Canvas at runtime — just attach it here.
        obj.AddComponent<CityUI>();
    }

    // -------------------------------------------------------------------------
    // TrashSpawner — receives the prefab reference directly in code
    // -------------------------------------------------------------------------

    static void BuildTrashSpawner(GameObject root, GameObject trashPrefab)
    {
        GameObject obj = new GameObject("TrashSpawner");
        obj.transform.SetParent(root.transform, false);

        TrashSpawner spawner   = obj.AddComponent<TrashSpawner>();
        spawner.trashPrefab    = trashPrefab;  // Assigned here — no manual drag needed.
        spawner.spawnInterval  = 5f;
        spawner.mapRange       = 18f;
        spawner.spawnY         = 0.5f;
    }

    // -------------------------------------------------------------------------
    // Robots — three CleanerBot models, each with RobotController on the root
    // -------------------------------------------------------------------------

    static void BuildRobots(GameObject root,
        Material bodyMat, Material wheelMat, Material eyeMat, Material lightMat)
    {
        GameObject container = new GameObject("Robots");
        container.transform.SetParent(root.transform, false);

        // Spread the robots around the city centre so they don't all start
        // in the same spot.
        Vector3[] starts =
        {
            new Vector3(-5f, 0f, -5f),
            new Vector3( 5f, 0f, -5f),
            new Vector3( 0f, 0f,  5f),
        };

        for (int i = 0; i < starts.Length; i++)
        {
            GameObject robot = BuildRobotModel(i + 1, bodyMat, wheelMat, eyeMat, lightMat);
            robot.transform.SetParent(container.transform, false);
            robot.transform.position = starts[i];
        }
    }

    // Builds the multi-part CleanerBot model under a single empty parent.
    // RobotController is on the PARENT — moving the parent moves the whole robot.
    static GameObject BuildRobotModel(int index,
        Material bodyMat, Material wheelMat, Material eyeMat, Material lightMat)
    {
        GameObject root = new GameObject("Robot_" + index.ToString("D2"));
        root.AddComponent<RobotController>();

        // ---- Visual parts — all positions/scales are LOCAL to the robot root ----
        // Parts are ~1.4× larger than the original for better camera visibility.

        AddPart(root, "Base",    PrimitiveType.Cube,
            new Vector3(0f, 0.12f, 0f),    new Vector3(1.00f, 0.25f, 1.30f), bodyMat);

        AddPart(root, "Body",    PrimitiveType.Cube,
            new Vector3(0f, 0.70f, 0f),    new Vector3(0.80f, 0.90f, 0.85f), bodyMat);

        AddPart(root, "Head",    PrimitiveType.Sphere,
            new Vector3(0f, 1.35f, 0f),    new Vector3(0.65f, 0.65f, 0.65f), bodyMat);

        // Cyan glowing eye on the front of the head.
        AddPart(root, "Eye",     PrimitiveType.Sphere,
            new Vector3(0f, 1.35f, 0.34f), new Vector3(0.18f, 0.18f, 0.18f), eyeMat);

        AddPart(root, "Antenna", PrimitiveType.Cylinder,
            new Vector3(0f, 1.75f, 0f),    new Vector3(0.05f, 0.20f, 0.05f), bodyMat);

        // Green glowing indicator light on top of the antenna.
        AddPart(root, "Light",   PrimitiveType.Sphere,
            new Vector3(0f, 1.98f, 0f),    new Vector3(0.13f, 0.13f, 0.13f), lightMat);

        // ---- Wheels ----------------------------------------------------------
        // Cylinders stand upright by default (height along Y).
        // Rotating 90° on the Z axis lays them on their side → wheel disc shape.
        (string name, Vector3 pos)[] wheels =
        {
            ("Wheel_FL", new Vector3( 0.58f, 0.25f,  0.44f)),
            ("Wheel_FR", new Vector3(-0.58f, 0.25f,  0.44f)),
            ("Wheel_BL", new Vector3( 0.58f, 0.25f, -0.44f)),
            ("Wheel_BR", new Vector3(-0.58f, 0.25f, -0.44f)),
        };

        foreach (var (name, pos) in wheels)
        {
            GameObject w = AddPart(root, name, PrimitiveType.Cylinder,
                pos, new Vector3(0.55f, 0.10f, 0.55f), wheelMat);
            w.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
        }

        // ---- Name label above the robot -------------------------------------
        // TextMeshPro (3D) renders text as a world-space mesh.
        // RobotLabel.cs rotates it every frame to face the camera.
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(root.transform, false);
        labelObj.transform.localPosition = new Vector3(0f, 2.5f, 0f);

        TextMeshPro label   = labelObj.AddComponent<TextMeshPro>();
        label.text          = "Bot_" + index.ToString("D2");
        label.fontSize      = 2f;    // 2 world units — readable from the game camera
        label.alignment     = TextAlignmentOptions.Center;
        label.color         = Color.white;

        labelObj.AddComponent<RobotLabel>(); // keeps label facing the camera

        return root;
    }

    // Creates a primitive child, parents it, sets its local transform,
    // assigns a material, and removes its collider (visual parts don't need physics).
    static GameObject AddPart(GameObject parent, string partName,
        PrimitiveType type, Vector3 localPos, Vector3 localScale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = partName;
        obj.transform.SetParent(parent.transform, false);
        obj.transform.localPosition = localPos;
        obj.transform.localScale    = localScale;
        obj.GetComponent<Renderer>().material = mat;
        Object.DestroyImmediate(obj.GetComponent<Collider>());
        return obj;
    }
}
