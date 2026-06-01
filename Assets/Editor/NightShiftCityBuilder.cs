// NightShiftCityBuilder.cs — v0.2
// Place this file in Assets/Editor/ so Unity only compiles it in the editor.
// Menu: Tools > NightShift City > Build Basic Scene

using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class NightShiftCityBuilder
{
    private const string RootName = "NightShift City [Generated]";

    // Road grid constants — PotholeSpawner.cs uses these same values.
    private const float MapHalf      = 22f;
    private const float RoadMainW    = 3.5f;
    private const float RoadSecW     = 2.5f;
    private const float RoadLength   = 44f;

    // =========================================================================
    // MENU ITEMS
    // =========================================================================

    [MenuItem("Tools/NightShift City/Build Basic Scene", false, 10)]
    public static void BuildBasicScene()
    {
        bool ok = EditorUtility.DisplayDialog(
            "Build NightShift City v0.2",
            "This will create (or replace) the full city scene.\n\n" +
            "Previously generated objects will be removed.\nContinue?",
            "Build", "Cancel");
        if (!ok) return;

        Debug.Log("[Builder] Starting v0.2 build...");

        EnsureTagExists("Trash");
        EnsureTagExists("Pothole");
        RemoveGeneratedObjects();

        // ---- Materials -------------------------------------------------------
        Material matGround   = Make("Mat_Ground",   new Color(0.22f, 0.20f, 0.18f));
        Material matGrass    = Make("Mat_Grass",    new Color(0.10f, 0.26f, 0.10f));
        Material matRoad     = Make("Mat_Road",     new Color(0.09f, 0.09f, 0.09f));
        Material matLane     = Make("Mat_Lane",     new Color(0.92f, 0.90f, 0.75f));
        Material matSidewalk = Make("Mat_Sidewalk", new Color(0.40f, 0.38f, 0.35f));
        Material matPole     = Make("Mat_Pole",     new Color(0.18f, 0.18f, 0.20f));
        Material matBulb     = Emissive("Mat_Bulb", new Color(1f, 0.95f, 0.7f), new Color(1.5f, 1.0f, 0.3f));
        Material matTrunk    = Make("Mat_Trunk",    new Color(0.26f, 0.16f, 0.07f));
        Material matLeaf     = Make("Mat_Leaf",     new Color(0.08f, 0.22f, 0.08f));
        Material matWin      = Emissive("Mat_Win",  new Color(1f, 0.92f, 0.65f), new Color(0.55f, 0.42f, 0.08f));
        Material matTrash    = Make("Mat_Trash",    new Color(1.00f, 0.85f, 0.00f));
        Material matPothole  = Make("Mat_Pothole",  new Color(0.13f, 0.10f, 0.08f));
        Material matPotWarn  = Emissive("Mat_PotWarn", new Color(0.9f, 0.45f, 0.0f), new Color(0.5f, 0.22f, 0.0f));

        // Five building colour variants
        Material[] bMats = {
            Make("Mat_Bldg_A", new Color(0.38f, 0.44f, 0.50f)),
            Make("Mat_Bldg_B", new Color(0.48f, 0.44f, 0.38f)),
            Make("Mat_Bldg_C", new Color(0.22f, 0.24f, 0.27f)),
            Make("Mat_Bldg_D", new Color(0.30f, 0.40f, 0.35f)),
            Make("Mat_Bldg_E", new Color(0.22f, 0.27f, 0.42f)),
        };

        // Bot materials
        Material matCleanBody  = Make("Mat_CleanBody",  new Color(0.12f, 0.32f, 0.60f));
        Material matRepairBody = Make("Mat_RepairBody", new Color(0.72f, 0.35f, 0.04f));
        Material matWheel      = Make("Mat_Wheel",      new Color(0.08f, 0.08f, 0.08f));
        Material matCyanEye    = Emissive("Mat_CyanEye",   new Color(0f, 1f, 1f),    new Color(0f, 0.55f, 0.55f));
        Material matOrangeEye  = Emissive("Mat_OrangeEye", new Color(1f, 0.5f, 0f),  new Color(0.55f, 0.22f, 0f));
        Material matBotLight   = Emissive("Mat_BotLight",  new Color(0.5f, 1f, 0.2f),new Color(0.18f, 0.48f, 0.04f));

        // ---- Build scene -----------------------------------------------------
        GameObject root = new GameObject(RootName);

        SetupNightLighting();
        BuildGround(root, matGround);
        BuildGrassPatches(root, matGrass);
        BuildRoads(root, matRoad, matLane, matSidewalk);
        BuildBuildings(root, bMats, matWin);
        BuildStreetLights(root, matPole, matBulb);
        BuildTrees(root, matTrunk, matLeaf);
        PositionCamera();

        GameObject trashPrefab   = CreateTrashPrefab(matTrash);
        GameObject potholePrefab = CreatePotholePrefab(matPothole, matPotWarn);

        BuildCityManager(root);
        BuildTrashSpawner(root, trashPrefab);
        BuildPotholeSpawner(root, potholePrefab);
        BuildCleanerBots(root, matCleanBody, matWheel, matCyanEye, matBotLight);
        BuildRepairBots(root, matRepairBody, matWheel, matOrangeEye, matBotLight);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[Builder] v0.2 build complete.");

        EditorUtility.DisplayDialog("Done — NightShift City v0.2",
            "Scene built successfully!\n\n" +
            "  Blue CleanerBots  → clean trash\n" +
            "  Orange RepairBots → repair potholes\n\n" +
            "Press Play to start the simulation.", "OK");
    }

    [MenuItem("Tools/NightShift City/Clear Generated Objects", false, 11)]
    public static void ClearGeneratedObjects()
    {
        if (EditorUtility.DisplayDialog("Clear Scene",
            "Remove all NightShift-generated objects?", "Clear", "Cancel"))
        {
            RemoveGeneratedObjects();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    // =========================================================================
    // HELPERS
    // =========================================================================

    static void RemoveGeneratedObjects()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null) Object.DestroyImmediate(existing);
    }

    static void EnsureTagExists(string tag)
    {
        SerializedObject mgr = new SerializedObject(
            AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty tags = mgr.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        mgr.ApplyModifiedProperties();
    }

    static Material Make(string name, Color color)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material m = new Material(sh) { name = name };
        m.color = color;
        return m;
    }

    static Material Emissive(string name, Color baseCol, Color glow)
    {
        Material m = Make(name, baseCol);
        m.EnableKeyword("_EMISSION");
        if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", glow);
        return m;
    }

    // Creates a primitive, parents it, sets local transform, assigns material,
    // and removes its collider (visual children don't need physics).
    static GameObject AddPart(GameObject parent, string partName, PrimitiveType type,
        Vector3 localPos, Vector3 localScale, Material mat)
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

    // =========================================================================
    // NIGHT LIGHTING
    // =========================================================================

    static void SetupNightLighting()
    {
        // Nearly black ambient — street lights do all the real work.
        RenderSettings.ambientMode  = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.04f, 0.04f, 0.09f);

        // Low-intensity moonlight (cool blue-white).
        Light[] all = Object.FindObjectsByType<Light>(FindObjectsInactive.Include);
        Light dir = null;
        foreach (Light l in all)
            if (l.type == LightType.Directional) { dir = l; break; }

        if (dir == null)
        {
            dir      = new GameObject("Directional Light").AddComponent<Light>();
            dir.type = LightType.Directional;
        }
        dir.intensity = 0.18f;
        dir.color     = new Color(0.68f, 0.73f, 0.95f);
        dir.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    // =========================================================================
    // GROUND AND GRASS
    // =========================================================================

    static void BuildGround(GameObject root, Material mat)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Plane);
        g.name = "Ground";
        g.transform.SetParent(root.transform, false);
        g.transform.localScale = new Vector3(5f, 1f, 5f); // 50×50 world units
        g.GetComponent<Renderer>().material = mat;
    }

    static void BuildGrassPatches(GameObject root, Material mat)
    {
        GameObject c = new GameObject("GrassPatches");
        c.transform.SetParent(root.transform, false);

        // Small green planes at the far corners of the city.
        (float x, float z, float w, float d)[] patches = {
            ( 19f,  19f, 5f, 5f), (-19f,  19f, 5f, 5f),
            ( 19f, -19f, 5f, 5f), (-19f, -19f, 5f, 5f),
        };

        foreach (var (x, z, w, d) in patches)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Plane);
            p.name = "Grass";
            p.transform.SetParent(c.transform, false);
            p.transform.position   = new Vector3(x, 0.01f, z);
            p.transform.localScale = new Vector3(w / 10f, 1f, d / 10f);
            p.GetComponent<Renderer>().material = mat;
            Object.DestroyImmediate(p.GetComponent<MeshCollider>());
        }
    }

    // =========================================================================
    // ROADS — surface cubes + dashed center lines + sidewalks
    // =========================================================================

    static void BuildRoads(GameObject root, Material matRoad, Material matLane, Material matSidewalk)
    {
        GameObject c = new GameObject("Roads");
        c.transform.SetParent(root.transform, false);

        // (isEW, center along perpendicular axis, road width)
        (bool ew, float center, float w)[] roads = {
            (true,    0f, RoadMainW), // EW main
            (false,   0f, RoadMainW), // NS main
            (true,   15f, RoadSecW),  // EW north
            (true,  -15f, RoadSecW),  // EW south
            (false,  15f, RoadSecW),  // NS east
            (false, -15f, RoadSecW),  // NS west
        };

        foreach (var (ew, center, w) in roads)
        {
            // Road surface
            Vector3 pos   = ew ? new Vector3(0f, 0.02f, center) : new Vector3(center, 0.02f, 0f);
            Vector3 scale = ew ? new Vector3(RoadLength, 0.05f, w) : new Vector3(w, 0.05f, RoadLength);

            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "Road";
            road.transform.SetParent(c.transform, false);
            road.transform.position   = pos;
            road.transform.localScale = scale;
            road.GetComponent<Renderer>().material = matRoad;
            Object.DestroyImmediate(road.GetComponent<BoxCollider>());

            // Dashed center line
            BuildDashedLine(c, ew, center, RoadLength, matLane);

            // Sidewalks on both sides
            BuildSidewalks(c, ew, center, w, RoadLength, matSidewalk);
        }
    }

    static void BuildDashedLine(GameObject parent, bool isEW, float center, float length, Material mat)
    {
        float dashLen = 1.5f, gap = 1.5f, step = dashLen + gap;
        int count = Mathf.FloorToInt(length / step);
        float start = -(count * step) * 0.5f + dashLen * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float offset = start + i * step;
            Vector3 pos   = isEW ? new Vector3(offset, 0.04f, center)  : new Vector3(center, 0.04f, offset);
            Vector3 scale = isEW ? new Vector3(dashLen, 0.02f, 0.12f) : new Vector3(0.12f, 0.02f, dashLen);

            GameObject dash = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dash.name = "Dash";
            dash.transform.SetParent(parent.transform, false);
            dash.transform.position   = pos;
            dash.transform.localScale = scale;
            dash.GetComponent<Renderer>().material = mat;
            Object.DestroyImmediate(dash.GetComponent<BoxCollider>());
        }
    }

    static void BuildSidewalks(GameObject parent, bool isEW, float center,
        float roadW, float length, Material mat)
    {
        float sideOff = roadW * 0.5f + 0.6f; // offset from road center to sidewalk center
        float sw = 1.1f, sh = 0.09f;

        for (int side = -1; side <= 1; side += 2)
        {
            Vector3 pos, scale;
            if (isEW)
            {
                pos   = new Vector3(0f, sh * 0.5f, center + side * sideOff);
                scale = new Vector3(length, sh, sw);
            }
            else
            {
                pos   = new Vector3(center + side * sideOff, sh * 0.5f, 0f);
                scale = new Vector3(sw, sh, length);
            }

            GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cube);
            s.name = "Sidewalk";
            s.transform.SetParent(parent.transform, false);
            s.transform.position   = pos;
            s.transform.localScale = scale;
            s.GetComponent<Renderer>().material = mat;
            Object.DestroyImmediate(s.GetComponent<BoxCollider>());
        }
    }

    // =========================================================================
    // BUILDINGS — with emissive windows and rooftop details
    // =========================================================================

    // Pre-baked tint values so each build looks the same.
    static readonly float[] BldgTints = {
        1.00f, 0.87f, 1.13f, 0.92f, 1.08f, 0.82f, 1.18f, 0.95f,
        1.05f, 0.86f, 1.14f, 0.90f, 1.10f, 0.84f, 1.16f, 0.93f,
    };

    static void BuildBuildings(GameObject root, Material[] bMats, Material winMat)
    {
        GameObject c = new GameObject("Buildings");
        c.transform.SetParent(root.transform, false);

        // (centerX, centerZ, height, widthX, depthZ, material index)
        (float x, float z, float h, float w, float d, int m)[] specs = {
            // NE quadrant
            ( 8f,  8f,  8f, 3.0f, 3.0f, 0), (12f,  7f,  5f, 2.5f, 3.0f, 1),
            ( 8f, 12f,  6f, 2.0f, 2.0f, 2), (18f, 18f, 10f, 4.0f, 3.5f, 3),
            (11f, 18f,  4f, 3.0f, 2.0f, 0),
            // NW quadrant
            (-8f,  8f,  7f, 3.0f, 3.0f, 1), (-12f, 10f, 4f, 2.0f, 2.5f, 2),
            (-18f, 18f, 9f, 3.5f, 4.0f, 4), ( -8f, 18f, 5f, 2.5f, 2.0f, 0),
            // SE quadrant
            ( 8f, -8f,  6f, 2.5f, 2.5f, 3), (11f, -12f, 8f, 3.0f, 3.0f, 0),
            (18f,-18f,  5f, 2.0f, 2.0f, 1), ( 7f, -18f, 7f, 2.0f, 3.0f, 2),
            // SW quadrant
            (-8f, -8f,  9f, 3.0f, 3.0f, 4), (-12f, -7f, 5f, 2.0f, 2.0f, 0),
            (-18f,-18f, 7f, 3.5f, 3.0f, 1),
        };

        for (int i = 0; i < specs.Length; i++)
        {
            var (x, z, h, w, d, m) = specs[i];
            float tint = BldgTints[i % BldgTints.Length];

            // Each building gets its own material copy with a slight brightness tint.
            Material bm = new Material(bMats[m % bMats.Length]);
            Color c0 = bm.color;
            bm.color = new Color(
                Mathf.Clamp01(c0.r * tint),
                Mathf.Clamp01(c0.g * tint),
                Mathf.Clamp01(c0.b * tint));

            GameObject bldg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bldg.name = "Building_" + (i + 1).ToString("D2");
            bldg.transform.SetParent(c.transform, false);
            bldg.transform.position   = new Vector3(x, h * 0.5f, z);
            bldg.transform.localScale = new Vector3(w, h, d);
            bldg.GetComponent<Renderer>().material = bm;

            // Emissive windows on all four faces.
            AddWindows(bldg, w, h, d, winMat);

            // Rooftop detail: slightly smaller cube in a darker tone.
            Material roofMat = new Material(bm);
            roofMat.color = bm.color * 0.60f;
            AddPart(bldg, "Roof", PrimitiveType.Cube,
                new Vector3(0f, 0.5f + 0.20f / h, 0f),
                new Vector3(0.88f, 0.40f / h, 0.88f),
                roofMat);
        }
    }

    // Adds emissive window panes as children of a building cube.
    // Because the building has non-unit scale, all child positions and scales
    // must be divided by the building's scale to get the correct world size.
    static void AddWindows(GameObject bldg, float bw, float bh, float bd, Material mat)
    {
        if (bh < 2.5f) return; // very short buildings look fine without windows

        int rows  = Mathf.Clamp(Mathf.FloorToInt(bh / 2.0f), 1, 5);
        int colsX = Mathf.Clamp(Mathf.FloorToInt(bw / 1.3f), 1, 4);
        int colsZ = Mathf.Clamp(Mathf.FloorToInt(bd / 1.3f), 1, 4);

        for (int r = 0; r < rows; r++)
        {
            // localY: distribute rows evenly between -0.38 and +0.38 of building height
            float ly = Mathf.Lerp(-0.38f, 0.38f, (r + 0.5f) / rows);

            // Front (+Z) and back (-Z) faces
            for (int c = 0; c < colsX; c++)
            {
                float lx = Mathf.Lerp(-0.38f, 0.38f, (c + 0.5f) / colsX);
                PlaceWindow(bldg, new Vector3(lx, ly,  0.52f), bw, bh, bd, true,  mat);
                PlaceWindow(bldg, new Vector3(lx, ly, -0.52f), bw, bh, bd, true,  mat);
            }

            // Right (+X) and left (-X) faces
            for (int c = 0; c < colsZ; c++)
            {
                float lz = Mathf.Lerp(-0.38f, 0.38f, (c + 0.5f) / colsZ);
                PlaceWindow(bldg, new Vector3( 0.52f, ly, lz), bw, bh, bd, false, mat);
                PlaceWindow(bldg, new Vector3(-0.52f, ly, lz), bw, bh, bd, false, mat);
            }
        }
    }

    // Creates one window pane as a child of a building.
    // isFrontBack = true  → depth is in the Z direction (front/back face)
    // isFrontBack = false → depth is in the X direction (side face)
    static void PlaceWindow(GameObject bldg, Vector3 lp,
        float bw, float bh, float bd, bool isFrontBack, Material mat)
    {
        // Desired world size of the window pane
        float ww = 0.30f, wh = 0.25f, wd = 0.07f;

        // Divide by building scale so the child's world scale equals the desired size.
        Vector3 ls = isFrontBack
            ? new Vector3(ww / bw, wh / bh, wd / bd)
            : new Vector3(wd / bw, wh / bh, ww / bd);

        GameObject win = GameObject.CreatePrimitive(PrimitiveType.Cube);
        win.name = "Win";
        win.transform.SetParent(bldg.transform, false);
        win.transform.localPosition = lp;
        win.transform.localScale    = ls;
        win.GetComponent<Renderer>().material = mat;
        Object.DestroyImmediate(win.GetComponent<BoxCollider>());
    }

    // =========================================================================
    // STREET PROPS — lights and trees
    // =========================================================================

    static void BuildStreetLights(GameObject root, Material poleMat, Material bulbMat)
    {
        GameObject c = new GameObject("StreetLights");
        c.transform.SetParent(root.transform, false);

        // Lights at all nine road intersections.
        Vector3[] positions = {
            new Vector3(  0f, 0f,   0f), new Vector3( 15f, 0f,  15f),
            new Vector3(-15f, 0f,  15f), new Vector3( 15f, 0f, -15f),
            new Vector3(-15f, 0f, -15f), new Vector3(  0f, 0f,  15f),
            new Vector3(  0f, 0f, -15f), new Vector3( 15f, 0f,   0f),
            new Vector3(-15f, 0f,   0f),
        };

        foreach (var pos in positions)
            BuildOneLight(c, pos, poleMat, bulbMat);
    }

    static void BuildOneLight(GameObject parent, Vector3 worldPos, Material poleMat, Material bulbMat)
    {
        GameObject lr = new GameObject("StreetLight");
        lr.transform.SetParent(parent.transform, false);
        lr.transform.position = worldPos;

        // Vertical post
        AddPart(lr, "Post", PrimitiveType.Cylinder,
            new Vector3(0f, 2.0f, 0f), new Vector3(0.08f, 2.0f, 0.08f), poleMat);

        // Horizontal arm at top
        AddPart(lr, "Arm", PrimitiveType.Cube,
            new Vector3(0.55f, 4.0f, 0f), new Vector3(1.1f, 0.08f, 0.08f), poleMat);

        // Lamp bulb sphere
        AddPart(lr, "Bulb", PrimitiveType.Sphere,
            new Vector3(1.1f, 3.95f, 0f), new Vector3(0.28f, 0.18f, 0.28f), bulbMat);

        // Point light — warm yellow, range 12 units
        GameObject lo = new GameObject("PointLight");
        lo.transform.SetParent(lr.transform, false);
        lo.transform.localPosition = new Vector3(1.1f, 3.9f, 0f);
        Light pl  = lo.AddComponent<Light>();
        pl.type      = LightType.Point;
        pl.color     = new Color(1.0f, 0.88f, 0.52f);
        pl.intensity = 3.0f;
        pl.range     = 12f;
    }

    static void BuildTrees(GameObject root, Material trunkMat, Material leafMat)
    {
        GameObject c = new GameObject("Trees");
        c.transform.SetParent(root.transform, false);

        Vector3[] positions = {
            new Vector3( 20f, 0f,  20f), new Vector3(-20f, 0f,  20f),
            new Vector3( 20f, 0f, -20f), new Vector3(-20f, 0f, -20f),
            new Vector3(  5f, 0f,  20f), new Vector3( -5f, 0f,  20f),
            new Vector3(  5f, 0f, -20f), new Vector3( -5f, 0f, -20f),
        };

        foreach (var pos in positions)
        {
            GameObject tr = new GameObject("Tree");
            tr.transform.SetParent(c.transform, false);
            tr.transform.position = pos;

            AddPart(tr, "Trunk",   PrimitiveType.Cylinder,
                new Vector3(0f, 0.6f, 0f), new Vector3(0.15f, 0.6f, 0.15f), trunkMat);
            AddPart(tr, "Foliage", PrimitiveType.Sphere,
                new Vector3(0f, 1.9f, 0f), new Vector3(1.3f,  1.5f, 1.3f),  leafMat);
        }
    }

    // =========================================================================
    // CAMERA
    // =========================================================================

    static void PositionCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject co = new GameObject("Main Camera");
            co.tag = "MainCamera";
            cam    = co.AddComponent<Camera>();
            co.AddComponent<AudioListener>();
        }
        cam.transform.position = new Vector3(0f, 30f, -24f);
        cam.transform.rotation = Quaternion.Euler(48f, 0f, 0f);
        cam.farClipPlane       = 300f;
    }

    // =========================================================================
    // PREFABS
    // =========================================================================

    static GameObject CreateTrashPrefab(Material mat)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        GameObject root = new GameObject("Trash");
        root.tag = "Trash";
        root.AddComponent<TrashItem>();

        // Main bright yellow chunk
        GameObject c1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c1.name = "Chunk_Main"; c1.transform.SetParent(root.transform, false);
        c1.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        c1.transform.localScale    = new Vector3(0.55f, 0.50f, 0.55f);
        c1.GetComponent<Renderer>().material = mat;
        Object.DestroyImmediate(c1.GetComponent<BoxCollider>());

        // Small tilted second chunk for a pile look
        Material mat2 = new Material(mat); mat2.color = new Color(0.95f, 0.58f, 0.04f);
        GameObject c2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c2.name = "Chunk_Top"; c2.transform.SetParent(root.transform, false);
        c2.transform.localPosition    = new Vector3(0.08f, 0.60f, 0.05f);
        c2.transform.localScale       = new Vector3(0.35f, 0.30f, 0.30f);
        c2.transform.localEulerAngles = new Vector3(8f, 28f, 12f);
        c2.GetComponent<Renderer>().material = mat2;
        Object.DestroyImmediate(c2.GetComponent<BoxCollider>());

        const string path = "Assets/Prefabs/Trash.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    static GameObject CreatePotholePrefab(Material baseMat, Material warnMat)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        GameObject root = new GameObject("Pothole");
        root.tag = "Pothole";
        root.AddComponent<PotholeItem>();

        // Dark flat disc — damaged road surface
        GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disc.name = "Base"; disc.transform.SetParent(root.transform, false);
        disc.transform.localScale = new Vector3(1.3f, 0.04f, 1.3f);
        disc.GetComponent<Renderer>().material = baseMat;
        Object.DestroyImmediate(disc.GetComponent<CapsuleCollider>());

        // Slightly smaller emissive warning ring on top — visible at night
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Warning"; ring.transform.SetParent(root.transform, false);
        ring.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        ring.transform.localScale    = new Vector3(1.0f, 0.05f, 1.0f);
        ring.GetComponent<Renderer>().material = warnMat;
        Object.DestroyImmediate(ring.GetComponent<CapsuleCollider>());

        const string path = "Assets/Prefabs/Pothole.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    // =========================================================================
    // GAME LOGIC OBJECTS
    // =========================================================================

    static void BuildCityManager(GameObject root)
    {
        GameObject obj = new GameObject("CityManager");
        obj.transform.SetParent(root.transform, false);
        obj.AddComponent<CityManager>();
        obj.AddComponent<CityUI>(); // HUD builds itself at runtime
    }

    static void BuildTrashSpawner(GameObject root, GameObject prefab)
    {
        GameObject obj  = new GameObject("TrashSpawner");
        obj.transform.SetParent(root.transform, false);
        TrashSpawner ts = obj.AddComponent<TrashSpawner>();
        ts.trashPrefab  = prefab;
        ts.spawnInterval= 5f;
        ts.mapRange     = 18f;
        ts.spawnY       = 0.5f;
    }

    static void BuildPotholeSpawner(GameObject root, GameObject prefab)
    {
        GameObject obj    = new GameObject("PotholeSpawner");
        obj.transform.SetParent(root.transform, false);
        PotholeSpawner ps = obj.AddComponent<PotholeSpawner>();
        ps.potholePrefab  = prefab;
        ps.spawnInterval  = 10f;
        ps.spawnY         = 0.04f;
    }

    // =========================================================================
    // ROBOTS
    // =========================================================================

    static void BuildCleanerBots(GameObject root,
        Material body, Material wheel, Material eye, Material light)
    {
        GameObject c = new GameObject("CleanerBots");
        c.transform.SetParent(root.transform, false);

        Vector3[] starts = {
            new Vector3(-5f, 0f, -5f),
            new Vector3( 5f, 0f, -5f),
            new Vector3( 0f, 0f,  5f),
        };

        for (int i = 0; i < starts.Length; i++)
        {
            GameObject bot = BuildBotModel(i + 1, false, body, wheel, eye, light);
            bot.transform.SetParent(c.transform, false);
            bot.transform.position = starts[i];
        }
    }

    static void BuildRepairBots(GameObject root,
        Material body, Material wheel, Material eye, Material light)
    {
        GameObject c = new GameObject("RepairBots");
        c.transform.SetParent(root.transform, false);

        Vector3[] starts = {
            new Vector3(-8f, 0f,  8f),
            new Vector3( 8f, 0f,  8f),
            new Vector3( 0f, 0f, -8f),
        };

        for (int i = 0; i < starts.Length; i++)
        {
            GameObject bot = BuildBotModel(i + 1, true, body, wheel, eye, light);
            bot.transform.SetParent(c.transform, false);
            bot.transform.position = starts[i];
        }
    }

    // Builds a multi-part bot model.
    // isRepair = false → blue CleanerBot with RobotController
    // isRepair = true  → orange RepairBot with RepairBotController
    static GameObject BuildBotModel(int index, bool isRepair,
        Material body, Material wheel, Material eye, Material light)
    {
        string name = isRepair
            ? "RepairBot_" + index.ToString("D2")
            : "CleanerBot_" + index.ToString("D2");

        GameObject root = new GameObject(name);

        // RobotController / RepairBotController is on the PARENT.
        // Moving the parent moves the whole robot; all visual parts follow.
        if (isRepair) root.AddComponent<RepairBotController>();
        else          root.AddComponent<RobotController>();

        // ---- Body parts (local positions/scales) ----------------------------
        AddPart(root, "Base",    PrimitiveType.Cube,
            new Vector3(0f, 0.12f, 0f),    new Vector3(1.00f, 0.25f, 1.30f), body);
        AddPart(root, "Body",    PrimitiveType.Cube,
            new Vector3(0f, 0.70f, 0f),    new Vector3(0.80f, 0.90f, 0.85f), body);
        AddPart(root, "Head",    PrimitiveType.Sphere,
            new Vector3(0f, 1.35f, 0f),    new Vector3(0.65f, 0.65f, 0.65f), body);
        AddPart(root, "Eye",     PrimitiveType.Sphere,
            new Vector3(0f, 1.35f, 0.34f), new Vector3(0.18f, 0.18f, 0.18f), eye);
        AddPart(root, "Antenna", PrimitiveType.Cylinder,
            new Vector3(0f, 1.75f, 0f),    new Vector3(0.05f, 0.20f, 0.05f), body);
        AddPart(root, "Light",   PrimitiveType.Sphere,
            new Vector3(0f, 1.98f, 0f),    new Vector3(0.13f, 0.13f, 0.13f), light);

        // ---- Wheels (cylinders rotated 90° on Z to look like side wheels) ---
        (string n, Vector3 p)[] wheels = {
            ("Wheel_FL", new Vector3( 0.58f, 0.25f,  0.44f)),
            ("Wheel_FR", new Vector3(-0.58f, 0.25f,  0.44f)),
            ("Wheel_BL", new Vector3( 0.58f, 0.25f, -0.44f)),
            ("Wheel_BR", new Vector3(-0.58f, 0.25f, -0.44f)),
        };
        foreach (var (n, p) in wheels)
        {
            GameObject w = AddPart(root, n, PrimitiveType.Cylinder,
                p, new Vector3(0.55f, 0.10f, 0.55f), wheel);
            w.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
        }

        // ---- Billboard name label above the bot -----------------------------
        GameObject lo = new GameObject("Label");
        lo.transform.SetParent(root.transform, false);
        lo.transform.localPosition = new Vector3(0f, 2.5f, 0f);

        TextMeshPro tmp = lo.AddComponent<TextMeshPro>();
        tmp.text        = isRepair ? "RBot_" + index.ToString("D2") : "Bot_" + index.ToString("D2");
        tmp.fontSize    = 2f;
        tmp.alignment   = TextAlignmentOptions.Center;
        tmp.color       = isRepair ? new Color(1f, 0.70f, 0.25f) : Color.white;
        lo.AddComponent<RobotLabel>();

        return root;
    }
}
