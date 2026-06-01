// NightShiftCityBuilder.cs — v0.3
// Place this file in Assets/Editor/ — Unity only compiles it in the editor.
// Menu: Tools > NightShift City > Build Basic Scene

using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class NightShiftCityBuilder
{
    private const string RootName  = "NightShift City [Generated]";
    private const float  RoadMainW = 3.5f;
    private const float  RoadSecW  = 2.5f;
    private const float  RoadLen   = 44f;

    // =========================================================================
    // MENU ITEMS
    // =========================================================================

    [MenuItem("Tools/NightShift City/Build Basic Scene", false, 10)]
    public static void BuildBasicScene()
    {
        bool ok = EditorUtility.DisplayDialog(
            "Build NightShift City v0.3",
            "This will create (or replace) the full city scene.\n\n" +
            "Previously generated objects will be removed.\nContinue?",
            "Build", "Cancel");
        if (!ok) return;

        Debug.Log("[Builder] v0.3 build starting...");

        EnsureTagExists("Trash");
        EnsureTagExists("Pothole");
        RemoveGeneratedObjects();

        // ---- Environment materials ------------------------------------------
        Material matGround    = Make("Mat_Ground",    new Color(0.20f, 0.18f, 0.16f));
        Material matGrass     = Make("Mat_Grass",     new Color(0.09f, 0.24f, 0.09f));
        Material matRoad      = Make("Mat_Road",      new Color(0.09f, 0.09f, 0.09f));
        Material matLane      = Make("Mat_Lane",      new Color(0.90f, 0.88f, 0.72f));
        Material matCrosswalk = Make("Mat_Crosswalk", new Color(0.86f, 0.84f, 0.70f));
        Material matSidewalk  = Make("Mat_Sidewalk",  new Color(0.38f, 0.36f, 0.33f));
        Material matCurb      = Make("Mat_Curb",      new Color(0.48f, 0.46f, 0.43f));
        Material matPole      = Make("Mat_Pole",      new Color(0.17f, 0.17f, 0.19f));
        Material matBulb      = Emissive("Mat_Bulb",  new Color(1f, 0.95f, 0.7f),  new Color(1.6f, 1.1f, 0.3f));
        Material matTrunk     = Make("Mat_Trunk",     new Color(0.24f, 0.15f, 0.06f));
        Material matLeaf      = Make("Mat_Leaf",      new Color(0.07f, 0.21f, 0.07f));

        // ---- Building materials ---------------------------------------------
        Material matWinLit  = Emissive("Mat_WinLit",  new Color(1f, 0.92f, 0.62f), new Color(0.6f, 0.44f, 0.08f));
        Material matWinDark = Make("Mat_WinDark",     new Color(0.10f, 0.13f, 0.20f)); // unlit / empty office
        Material matDoor    = Make("Mat_Door",        new Color(0.10f, 0.08f, 0.07f));
        Material matRoofDet = Make("Mat_RoofDetail",  new Color(0.18f, 0.18f, 0.20f));

        // Five building colour variants
        Material[] bMats = {
            Make("Mat_Bldg_A", new Color(0.36f, 0.43f, 0.50f)),
            Make("Mat_Bldg_B", new Color(0.46f, 0.42f, 0.36f)),
            Make("Mat_Bldg_C", new Color(0.20f, 0.22f, 0.26f)),
            Make("Mat_Bldg_D", new Color(0.28f, 0.38f, 0.33f)),
            Make("Mat_Bldg_E", new Color(0.20f, 0.25f, 0.40f)),
        };

        // ---- Problem object materials ----------------------------------------
        Material matTrash   = Make("Mat_Trash",   new Color(1.00f, 0.85f, 0.00f));
        Material matPothole = Make("Mat_Pothole", new Color(0.12f, 0.09f, 0.07f));
        Material matPotWarn = Emissive("Mat_PotWarn", new Color(0.88f, 0.42f, 0.0f), new Color(0.5f, 0.20f, 0.0f));

        // ---- Robot materials ------------------------------------------------
        Material matCleanBody  = Make("Mat_CleanBody",  new Color(0.11f, 0.30f, 0.58f));
        Material matRepairBody = Make("Mat_RepairBody", new Color(0.70f, 0.33f, 0.04f));
        Material matWheel      = Make("Mat_Wheel",      new Color(0.07f, 0.07f, 0.07f));
        Material matCyanEye    = Emissive("Mat_CyanEye",   new Color(0f, 1f, 1f),    new Color(0f, 0.6f, 0.6f));
        Material matOrangeEye  = Emissive("Mat_OrangeEye", new Color(1f, 0.5f, 0f),  new Color(0.6f, 0.24f, 0f));
        Material matBotLight   = Emissive("Mat_BotLight",  new Color(0.4f, 1f, 0.2f),new Color(0.16f, 0.5f, 0.04f));
        Material matBrush      = Make("Mat_Brush",   new Color(0.60f, 0.60f, 0.65f));
        Material matToolbox    = Make("Mat_Toolbox", new Color(0.68f, 0.38f, 0.07f));

        // ---- Build scene ----------------------------------------------------
        GameObject root = new GameObject(RootName);

        SetupNightLighting();
        BuildGround(root, matGround);
        BuildGrassPatches(root, matGrass);
        BuildRoads(root, matRoad, matLane, matSidewalk, matCurb);
        BuildCrosswalks(root, matCrosswalk);
        BuildBuildings(root, bMats, matWinLit, matWinDark, matDoor, matRoofDet);
        BuildStreetLights(root, matPole, matBulb);
        BuildTrees(root, matTrunk, matLeaf);
        PositionCamera();

        GameObject trashPrefab   = CreateTrashPrefab(matTrash);
        GameObject potholePrefab = CreatePotholePrefab(matPothole, matPotWarn);

        BuildCityManager(root);
        BuildTrashSpawner(root, trashPrefab);
        BuildPotholeSpawner(root, potholePrefab);
        BuildCleanerBots(root, matCleanBody, matWheel, matCyanEye, matBotLight, matBrush);
        BuildRepairBots(root, matRepairBody, matWheel, matOrangeEye, matBotLight, matToolbox);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[Builder] v0.3 build complete.");

        EditorUtility.DisplayDialog("NightShift City v0.3 Built!",
            "Scene is ready.\n\n" +
            "  WASD / Arrows — pan camera\n" +
            "  Scroll / Q-E  — zoom\n\n" +
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
        var existing = GameObject.Find(RootName);
        if (existing != null) Object.DestroyImmediate(existing);
    }

    static void EnsureTagExists(string tag)
    {
        var mgr  = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        var tags = mgr.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        mgr.ApplyModifiedProperties();
    }

    static Material Make(string name, Color color)
    {
        var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m  = new Material(sh) { name = name };
        m.color = color;
        return m;
    }

    static Material Emissive(string name, Color baseCol, Color glow)
    {
        var m = Make(name, baseCol);
        m.EnableKeyword("_EMISSION");
        if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", glow);
        return m;
    }

    // Creates a primitive child with local position/scale, removes its collider.
    static GameObject AddPart(GameObject parent, string partName, PrimitiveType type,
        Vector3 lp, Vector3 ls, Material mat)
    {
        var obj = GameObject.CreatePrimitive(type);
        obj.name = partName;
        obj.transform.SetParent(parent.transform, false);
        obj.transform.localPosition = lp;
        obj.transform.localScale    = ls;
        obj.GetComponent<Renderer>().material = mat;
        Object.DestroyImmediate(obj.GetComponent<Collider>());
        return obj;
    }

    // =========================================================================
    // NIGHT LIGHTING + FOG
    // =========================================================================

    static void SetupNightLighting()
    {
        // Nearly-black ambient so point lights do most of the work.
        RenderSettings.ambientMode  = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.04f, 0.04f, 0.09f);

        // Subtle distance fog deepens the night atmosphere.
        RenderSettings.fog              = true;
        RenderSettings.fogMode          = FogMode.Linear;
        RenderSettings.fogColor         = new Color(0.03f, 0.04f, 0.10f);
        RenderSettings.fogStartDistance = 28f;
        RenderSettings.fogEndDistance   = 58f;

        // Low-intensity moonlight (cool blue-white).
        var all = Object.FindObjectsByType<Light>(FindObjectsInactive.Include);
        Light dir = null;
        foreach (var l in all) if (l.type == LightType.Directional) { dir = l; break; }
        if (dir == null) { dir = new GameObject("Directional Light").AddComponent<Light>(); dir.type = LightType.Directional; }
        dir.intensity = 0.15f;
        dir.color     = new Color(0.65f, 0.70f, 0.95f);
        dir.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    // =========================================================================
    // GROUND AND GRASS
    // =========================================================================

    static void BuildGround(GameObject root, Material mat)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Plane);
        g.name = "Ground";
        g.transform.SetParent(root.transform, false);
        g.transform.localScale = new Vector3(5f, 1f, 5f); // 50×50
        g.GetComponent<Renderer>().material = mat;
    }

    static void BuildGrassPatches(GameObject root, Material mat)
    {
        var c = new GameObject("GrassPatches");
        c.transform.SetParent(root.transform, false);

        (float x, float z, float w, float d)[] patches = {
            ( 19f,  19f, 5f, 5f), (-19f,  19f, 5f, 5f),
            ( 19f, -19f, 5f, 5f), (-19f, -19f, 5f, 5f),
        };
        foreach (var (x, z, w, d) in patches)
        {
            var p = GameObject.CreatePrimitive(PrimitiveType.Plane);
            p.name = "Grass";
            p.transform.SetParent(c.transform, false);
            p.transform.position   = new Vector3(x, 0.01f, z);
            p.transform.localScale = new Vector3(w / 10f, 1f, d / 10f);
            p.GetComponent<Renderer>().material = mat;
            Object.DestroyImmediate(p.GetComponent<MeshCollider>());
        }
    }

    // =========================================================================
    // ROADS — surface + dashed center line + sidewalks + curbs
    // =========================================================================

    static void BuildRoads(GameObject root, Material matRoad, Material matLane,
        Material matSidewalk, Material matCurb)
    {
        var c = new GameObject("Roads");
        c.transform.SetParent(root.transform, false);

        (bool ew, float center, float w)[] roads = {
            (true,    0f, RoadMainW), (false,   0f, RoadMainW),
            (true,   15f, RoadSecW),  (true,  -15f, RoadSecW),
            (false,  15f, RoadSecW),  (false, -15f, RoadSecW),
        };

        foreach (var (ew, center, w) in roads)
        {
            // Road surface
            var pos   = ew ? new Vector3(0f, 0.02f, center) : new Vector3(center, 0.02f, 0f);
            var scale = ew ? new Vector3(RoadLen, 0.05f, w)  : new Vector3(w, 0.05f, RoadLen);
            var road  = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "Road";
            road.transform.SetParent(c.transform, false);
            road.transform.position   = pos;
            road.transform.localScale = scale;
            road.GetComponent<Renderer>().material = matRoad;
            Object.DestroyImmediate(road.GetComponent<BoxCollider>());

            // Dashed center line
            BuildDashes(c, ew, center, RoadLen, matLane);

            // Sidewalks + curbs on both sides
            BuildSidewalks(c, ew, center, w, RoadLen, matSidewalk, matCurb);
        }
    }

    static void BuildDashes(GameObject parent, bool ew, float center, float length, Material mat)
    {
        float dash = 1.5f, gap = 1.5f, step = dash + gap;
        int   count = Mathf.FloorToInt(length / step);
        float start = -(count * step) * 0.5f + dash * 0.5f;
        for (int i = 0; i < count; i++)
        {
            float off   = start + i * step;
            var pos     = ew ? new Vector3(off, 0.04f, center)  : new Vector3(center, 0.04f, off);
            var scale   = ew ? new Vector3(dash, 0.02f, 0.12f)  : new Vector3(0.12f, 0.02f, dash);
            var d = GameObject.CreatePrimitive(PrimitiveType.Cube);
            d.name = "Dash"; d.transform.SetParent(parent.transform, false);
            d.transform.position = pos; d.transform.localScale = scale;
            d.GetComponent<Renderer>().material = mat;
            Object.DestroyImmediate(d.GetComponent<BoxCollider>());
        }
    }

    static void BuildSidewalks(GameObject parent, bool ew, float center, float roadW,
        float length, Material matSW, Material matCurb)
    {
        float sideOff = roadW * 0.5f + 0.65f;
        float sw = 1.1f, sh = 0.09f;
        float curbH = 0.11f, curbW = 0.12f;

        for (int side = -1; side <= 1; side += 2)
        {
            // Sidewalk slab
            Vector3 sPos, sScale, cPos, cScale;
            if (ew)
            {
                sPos   = new Vector3(0f,  sh * 0.5f,  center + side * sideOff);
                sScale = new Vector3(length, sh, sw);
                // Curb between road and sidewalk
                cPos   = new Vector3(0f, curbH * 0.5f, center + side * (roadW * 0.5f + curbW * 0.5f));
                cScale = new Vector3(length, curbH, curbW);
            }
            else
            {
                sPos   = new Vector3(center + side * sideOff, sh * 0.5f, 0f);
                sScale = new Vector3(sw, sh, length);
                cPos   = new Vector3(center + side * (roadW * 0.5f + curbW * 0.5f), curbH * 0.5f, 0f);
                cScale = new Vector3(curbW, curbH, length);
            }

            var sw_obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sw_obj.name = "Sidewalk"; sw_obj.transform.SetParent(parent.transform, false);
            sw_obj.transform.position = sPos; sw_obj.transform.localScale = sScale;
            sw_obj.GetComponent<Renderer>().material = matSW;
            Object.DestroyImmediate(sw_obj.GetComponent<BoxCollider>());

            var curb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curb.name = "Curb"; curb.transform.SetParent(parent.transform, false);
            curb.transform.position = cPos; curb.transform.localScale = cScale;
            curb.GetComponent<Renderer>().material = matCurb;
            Object.DestroyImmediate(curb.GetComponent<BoxCollider>());
        }
    }

    // =========================================================================
    // CROSSWALKS — white stripes at the 5 primary intersections
    // =========================================================================

    static void BuildCrosswalks(GameObject root, Material mat)
    {
        var c = new GameObject("Crosswalks");
        c.transform.SetParent(root.transform, false);

        // (intersection x, intersection z, EW road width, NS road width)
        (float ix, float iz, float ew, float ns)[] ixns = {
            ( 0f,  0f, RoadMainW, RoadMainW),
            ( 0f, 15f, RoadMainW, RoadSecW),
            ( 0f,-15f, RoadMainW, RoadSecW),
            (15f,  0f, RoadSecW,  RoadMainW),
            (-15f, 0f, RoadSecW,  RoadMainW),
        };

        foreach (var (ix, iz, ewW, nsW) in ixns)
        {
            // East + West: stripes on EW road, crossing the NS road
            for (int i = 0; i < 4; i++)
            {
                float xE = ix + nsW * 0.5f + 0.25f + i * 0.55f;
                float xW = ix - nsW * 0.5f - 0.25f - i * 0.55f;
                PutStripe(c, new Vector3(xE, 0.035f, iz), new Vector3(0.42f, 0.02f, ewW), mat);
                PutStripe(c, new Vector3(xW, 0.035f, iz), new Vector3(0.42f, 0.02f, ewW), mat);
            }
            // North + South: stripes on NS road, crossing the EW road
            for (int i = 0; i < 4; i++)
            {
                float zN = iz + ewW * 0.5f + 0.25f + i * 0.55f;
                float zS = iz - ewW * 0.5f - 0.25f - i * 0.55f;
                PutStripe(c, new Vector3(ix, 0.035f, zN), new Vector3(nsW, 0.02f, 0.42f), mat);
                PutStripe(c, new Vector3(ix, 0.035f, zS), new Vector3(nsW, 0.02f, 0.42f), mat);
            }
        }
    }

    static void PutStripe(GameObject parent, Vector3 worldPos, Vector3 worldScale, Material mat)
    {
        var s = GameObject.CreatePrimitive(PrimitiveType.Cube);
        s.name = "Stripe"; s.transform.SetParent(parent.transform, false);
        s.transform.localPosition = worldPos;   // parent has no scale/offset
        s.transform.localScale    = worldScale;
        s.GetComponent<Renderer>().material = mat;
        Object.DestroyImmediate(s.GetComponent<BoxCollider>());
    }

    // =========================================================================
    // BUILDINGS — varied heights, lit+dark windows, entrance doors, rooftop props
    // =========================================================================

    static readonly float[] BldgTints = {
        1.00f, 0.86f, 1.14f, 0.91f, 1.09f, 0.81f, 1.19f, 0.94f,
        1.06f, 0.85f, 1.13f, 0.89f, 1.11f, 0.83f, 1.17f, 0.93f,
    };

    static void BuildBuildings(GameObject root, Material[] bMats,
        Material matWinLit, Material matWinDark, Material matDoor, Material matRoofDet)
    {
        var c = new GameObject("Buildings");
        c.transform.SetParent(root.transform, false);

        // (cx, cz, height, widthX, depthZ, mat index)
        (float x, float z, float h, float w, float d, int m)[] specs = {
            // NE
            ( 8f,  8f, 8.5f, 3.0f, 3.0f, 0), (12f,  7f,  5.0f, 2.5f, 3.0f, 1),
            ( 8f, 12f, 6.5f, 2.0f, 2.0f, 2), (18f, 18f, 11.0f, 4.0f, 3.5f, 3),
            (11f, 18f, 4.5f, 3.0f, 2.0f, 0),
            // NW
            (-8f,  8f, 7.5f, 3.0f, 3.0f, 1), (-12f, 10f, 4.5f, 2.0f, 2.5f, 2),
            (-18f, 18f,9.5f, 3.5f, 4.0f, 4), ( -8f, 18f, 5.5f, 2.5f, 2.0f, 0),
            // SE
            ( 8f, -8f, 6.5f, 2.5f, 2.5f, 3), (11f,-12f,  8.0f, 3.0f, 3.0f, 0),
            (18f,-18f, 5.5f, 2.0f, 2.0f, 1), ( 7f,-18f,  7.5f, 2.0f, 3.0f, 2),
            // SW
            (-8f, -8f, 9.0f, 3.0f, 3.0f, 4), (-12f,-7f, 5.5f, 2.0f, 2.0f, 0),
            (-18f,-18f,7.0f, 3.5f, 3.0f, 1),
        };

        for (int i = 0; i < specs.Length; i++)
        {
            var (x, z, h, w, d, m) = specs[i];
            float t = BldgTints[i % BldgTints.Length];

            var bm = new Material(bMats[m % bMats.Length]);
            var c0 = bm.color;
            bm.color = new Color(Mathf.Clamp01(c0.r * t), Mathf.Clamp01(c0.g * t), Mathf.Clamp01(c0.b * t));

            var bldg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bldg.name = "Building_" + (i + 1).ToString("D2");
            bldg.transform.SetParent(c.transform, false);
            bldg.transform.position   = new Vector3(x, h * 0.5f, z);
            bldg.transform.localScale = new Vector3(w, h, d);
            bldg.GetComponent<Renderer>().material = bm;

            // Windows — mix of lit and unlit
            AddWindows(bldg, w, h, d, matWinLit, matWinDark, i);

            // Entrance door on front (+Z) face
            PlaceDoor(bldg, w, h, d, matDoor);

            // Darker roof cap
            var roofMat = new Material(bm); roofMat.color = bm.color * 0.55f;
            AddPart(bldg, "Roof", PrimitiveType.Cube,
                new Vector3(0f, 0.5f + 0.20f / h, 0f),
                new Vector3(0.90f, 0.40f / h, 0.90f), roofMat);

            // Rooftop prop unique to this building
            AddRooftopProp(c, x, z, h, w, d, i, matRoofDet);
        }
    }

    // Adds a grid of window panes to the front/back/side faces of a building.
    // Buildings with non-unit scale require localScale = worldSize / buildingScale.
    static void AddWindows(GameObject bldg, float bw, float bh, float bd,
        Material matLit, Material matDark, int bldgIdx)
    {
        if (bh < 2.5f) return;

        int rows  = Mathf.Clamp(Mathf.FloorToInt(bh / 2.0f), 1, 5);
        int colsX = Mathf.Clamp(Mathf.FloorToInt(bw / 1.3f), 1, 4);
        int colsZ = Mathf.Clamp(Mathf.FloorToInt(bd / 1.3f), 1, 4);

        for (int r = 0; r < rows; r++)
        {
            float ly = Mathf.Lerp(-0.38f, 0.38f, (r + 0.5f) / rows);

            for (int col = 0; col < colsX; col++)
            {
                float lx  = Mathf.Lerp(-0.38f, 0.38f, (col + 0.5f) / colsX);
                // 75% of windows lit, 25% dark — deterministic per building+position
                bool lit  = (bldgIdx + r * 3 + col) % 4 != 0;
                var  mat  = lit ? matLit : matDark;
                PlaceWindow(bldg, new Vector3(lx, ly,  0.52f), bw, bh, bd, true,  mat);
                PlaceWindow(bldg, new Vector3(lx, ly, -0.52f), bw, bh, bd, true,  mat);
            }
            for (int col = 0; col < colsZ; col++)
            {
                float lz = Mathf.Lerp(-0.38f, 0.38f, (col + 0.5f) / colsZ);
                bool lit = (bldgIdx + r * 3 + col + 2) % 4 != 0;
                var  mat = lit ? matLit : matDark;
                PlaceWindow(bldg, new Vector3( 0.52f, ly, lz), bw, bh, bd, false, mat);
                PlaceWindow(bldg, new Vector3(-0.52f, ly, lz), bw, bh, bd, false, mat);
            }
        }
    }

    // One window pane as a child.
    // isFrontBack=true → pane faces ±Z; false → pane faces ±X.
    static void PlaceWindow(GameObject bldg, Vector3 lp,
        float bw, float bh, float bd, bool isFB, Material mat)
    {
        float ww = 0.30f, wh = 0.25f, wd = 0.07f;
        var ls = isFB ? new Vector3(ww / bw, wh / bh, wd / bd)
                      : new Vector3(wd / bw, wh / bh, ww / bd);
        var win = GameObject.CreatePrimitive(PrimitiveType.Cube);
        win.name = "Win"; win.transform.SetParent(bldg.transform, false);
        win.transform.localPosition = lp;
        win.transform.localScale    = ls;
        win.GetComponent<Renderer>().material = mat;
        Object.DestroyImmediate(win.GetComponent<BoxCollider>());
    }

    // Small dark door at the base of the front face.
    static void PlaceDoor(GameObject bldg, float bw, float bh, float bd, Material mat)
    {
        // localY so world bottom of door = 0 (sits on ground)
        float doorY = -0.5f + 0.25f / bh;
        var door    = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name   = "Door"; door.transform.SetParent(bldg.transform, false);
        door.transform.localPosition = new Vector3(0f, doorY, 0.52f);
        door.transform.localScale    = new Vector3(0.38f / bw, 0.50f / bh, 0.07f / bd);
        door.GetComponent<Renderer>().material = mat;
        Object.DestroyImmediate(door.GetComponent<BoxCollider>());
    }

    // Rooftop prop: 4 types cycling by building index.
    // The props container has no scale, so localPosition == worldPosition.
    static void AddRooftopProp(GameObject container,
        float cx, float cz, float bh, float bw, float bd, int idx, Material mat)
    {
        float top = bh + 0.44f; // world Y just above roof cap

        var g = new GameObject("RoofProp");
        g.transform.SetParent(container.transform, false);
        g.transform.localPosition = new Vector3(cx, top, cz);

        switch (idx % 4)
        {
            case 0: // Water tower
                AddPart(g, "Tank", PrimitiveType.Cylinder,
                    new Vector3(bw * 0.28f, 0.32f, bd * 0.28f), new Vector3(0.30f, 0.32f, 0.30f), mat);
                AddPart(g, "Cap", PrimitiveType.Sphere,
                    new Vector3(bw * 0.28f, 0.70f, bd * 0.28f), new Vector3(0.36f, 0.13f, 0.36f), mat);
                break;

            case 1: // HVAC unit
            {
                float hw = Mathf.Clamp(bw * 0.50f, 0.55f, 1.5f);
                float hd = Mathf.Clamp(bd * 0.35f, 0.45f, 0.90f);
                AddPart(g, "HVAC", PrimitiveType.Cube,
                    new Vector3(0f, 0.20f, 0f), new Vector3(hw, 0.40f, hd), mat);
                AddPart(g, "Exhaust", PrimitiveType.Cylinder,
                    new Vector3(hw * 0.32f, 0.55f, 0f), new Vector3(0.10f, 0.17f, 0.10f), mat);
                break;
            }

            case 2: // Antenna + dish
                AddPart(g, "Mast", PrimitiveType.Cylinder,
                    new Vector3(bw * 0.10f, 0.48f, 0f), new Vector3(0.04f, 0.48f, 0.04f), mat);
                AddPart(g, "Dish", PrimitiveType.Sphere,
                    new Vector3(bw * 0.10f + 0.22f, 1.05f, 0f), new Vector3(0.22f, 0.10f, 0.20f), mat);
                break;

            case 3: // Penthouse / mechanical room
            {
                float rw  = Mathf.Clamp(bw * 0.42f, 0.55f, 1.25f);
                float rd  = Mathf.Clamp(bd * 0.40f, 0.45f, 0.85f);
                var pm    = new Material(mat); pm.color = mat.color * 1.25f;
                AddPart(g, "Room", PrimitiveType.Cube,
                    new Vector3(0f, 0.30f, 0f), new Vector3(rw, 0.60f, rd), pm);
                AddPart(g, "Roof", PrimitiveType.Cube,
                    new Vector3(0f, 0.63f, 0f), new Vector3(rw * 1.08f, 0.07f, rd * 1.08f), mat);
                break;
            }
        }
    }

    // =========================================================================
    // STREET LIGHTS — intersections + mid-road fill lights
    // =========================================================================

    static void BuildStreetLights(GameObject root, Material poleMat, Material bulbMat)
    {
        var c = new GameObject("StreetLights");
        c.transform.SetParent(root.transform, false);

        // Nine road intersections
        Vector3[] ixPositions = {
            new Vector3(  0f, 0f,   0f), new Vector3( 15f, 0f,  15f),
            new Vector3(-15f, 0f,  15f), new Vector3( 15f, 0f, -15f),
            new Vector3(-15f, 0f, -15f), new Vector3(  0f, 0f,  15f),
            new Vector3(  0f, 0f, -15f), new Vector3( 15f, 0f,   0f),
            new Vector3(-15f, 0f,   0f),
        };
        foreach (var p in ixPositions) PlaceLight(c, p, poleMat, bulbMat);

        // Mid-road fill lights (one per road segment, alternating sides)
        Vector3[] midPositions = {
            // EW main
            new Vector3(-7.5f, 0f,  2.0f), new Vector3( 7.5f, 0f, -2.0f),
            // NS main
            new Vector3( 2.0f, 0f, -7.5f), new Vector3(-2.0f, 0f,  7.5f),
            // EW north
            new Vector3(-7.5f, 0f, 17.0f), new Vector3( 7.5f, 0f, 13.0f),
            // EW south
            new Vector3(-7.5f, 0f,-13.0f), new Vector3( 7.5f, 0f,-17.0f),
            // NS east
            new Vector3(17.0f, 0f, -7.5f), new Vector3(13.0f, 0f,  7.5f),
            // NS west
            new Vector3(-13.0f,0f, -7.5f), new Vector3(-17.0f,0f,  7.5f),
        };
        foreach (var p in midPositions) PlaceLight(c, p, poleMat, bulbMat);
    }

    static void PlaceLight(GameObject parent, Vector3 worldPos, Material poleMat, Material bulbMat)
    {
        var lr = new GameObject("StreetLight");
        lr.transform.SetParent(parent.transform, false);
        lr.transform.position = worldPos;

        AddPart(lr, "Post", PrimitiveType.Cylinder,
            new Vector3(0f, 2.0f, 0f), new Vector3(0.08f, 2.0f, 0.08f), poleMat);
        AddPart(lr, "Arm", PrimitiveType.Cube,
            new Vector3(0.55f, 4.0f, 0f), new Vector3(1.1f, 0.08f, 0.08f), poleMat);
        AddPart(lr, "Bulb", PrimitiveType.Sphere,
            new Vector3(1.1f, 3.95f, 0f), new Vector3(0.28f, 0.18f, 0.28f), bulbMat);

        var lo = new GameObject("PL");
        lo.transform.SetParent(lr.transform, false);
        lo.transform.localPosition = new Vector3(1.1f, 3.9f, 0f);
        var pl   = lo.AddComponent<Light>();
        pl.type      = LightType.Point;
        pl.color     = new Color(1.0f, 0.88f, 0.50f);
        pl.intensity = 2.8f;
        pl.range     = 11f;
    }

    // =========================================================================
    // TREES
    // =========================================================================

    static void BuildTrees(GameObject root, Material trunkMat, Material leafMat)
    {
        var c = new GameObject("Trees");
        c.transform.SetParent(root.transform, false);

        Vector3[] positions = {
            new Vector3( 20f, 0f,  20f), new Vector3(-20f, 0f,  20f),
            new Vector3( 20f, 0f, -20f), new Vector3(-20f, 0f, -20f),
            new Vector3(  5f, 0f,  20f), new Vector3( -5f, 0f,  20f),
            new Vector3(  5f, 0f, -20f), new Vector3( -5f, 0f, -20f),
        };

        foreach (var pos in positions)
        {
            var tr = new GameObject("Tree");
            tr.transform.SetParent(c.transform, false);
            tr.transform.position = pos;
            AddPart(tr, "Trunk",   PrimitiveType.Cylinder,
                new Vector3(0f, 0.6f, 0f), new Vector3(0.15f, 0.6f, 0.15f), trunkMat);
            AddPart(tr, "Foliage", PrimitiveType.Sphere,
                new Vector3(0f, 1.9f, 0f), new Vector3(1.3f,  1.5f, 1.3f),  leafMat);
        }
    }

    // =========================================================================
    // CAMERA — improved angle + WASD controller
    // =========================================================================

    static void PositionCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var co = new GameObject("Main Camera");
            co.tag = "MainCamera";
            cam    = co.AddComponent<Camera>();
            co.AddComponent<AudioListener>();
        }
        cam.transform.position = new Vector3(2f, 32f, -26f);
        cam.transform.rotation = Quaternion.Euler(50f, -3f, 0f);
        cam.farClipPlane       = 300f;

        // CameraController enables WASD navigation during Play mode.
        if (cam.GetComponent<CameraController>() == null)
            cam.gameObject.AddComponent<CameraController>();
    }

    // =========================================================================
    // PREFABS
    // =========================================================================

    static GameObject CreateTrashPrefab(Material mat)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        var root = new GameObject("Trash");
        root.tag = "Trash";
        root.AddComponent<TrashItem>();

        // Chunk 1 — main bright yellow block
        var c1  = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c1.name = "Chunk_1"; c1.transform.SetParent(root.transform, false);
        c1.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        c1.transform.localScale    = new Vector3(0.55f, 0.50f, 0.55f);
        c1.GetComponent<Renderer>().material = mat;
        Object.DestroyImmediate(c1.GetComponent<BoxCollider>());

        // Chunk 2 — smaller tilted block (orange variant)
        var m2  = new Material(mat); m2.color = new Color(0.95f, 0.58f, 0.04f);
        var c2  = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c2.name = "Chunk_2"; c2.transform.SetParent(root.transform, false);
        c2.transform.localPosition    = new Vector3(0.12f, 0.60f, 0.05f);
        c2.transform.localScale       = new Vector3(0.34f, 0.28f, 0.28f);
        c2.transform.localEulerAngles = new Vector3(8f, 28f, 12f);
        c2.GetComponent<Renderer>().material = m2;
        Object.DestroyImmediate(c2.GetComponent<BoxCollider>());

        // Chunk 3 — small cylinder "can" for visual variety
        var c3  = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        c3.name = "Chunk_3"; c3.transform.SetParent(root.transform, false);
        c3.transform.localPosition = new Vector3(-0.16f, 0.25f, 0.14f);
        c3.transform.localScale    = new Vector3(0.18f, 0.20f, 0.18f);
        c3.GetComponent<Renderer>().material = mat;
        Object.DestroyImmediate(c3.GetComponent<CapsuleCollider>());

        const string path = "Assets/Prefabs/Trash.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    static GameObject CreatePotholePrefab(Material baseMat, Material warnMat)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        var root = new GameObject("Pothole");
        root.tag = "Pothole";
        root.AddComponent<PotholeItem>();

        // Flat dark base disc
        var disc  = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disc.name = "Base"; disc.transform.SetParent(root.transform, false);
        disc.transform.localScale = new Vector3(1.3f, 0.04f, 1.3f);
        disc.GetComponent<Renderer>().material = baseMat;
        Object.DestroyImmediate(disc.GetComponent<CapsuleCollider>());

        // Emissive warning ring
        var ring  = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Warning"; ring.transform.SetParent(root.transform, false);
        ring.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        ring.transform.localScale    = new Vector3(1.0f, 0.05f, 1.0f);
        ring.GetComponent<Renderer>().material = warnMat;
        Object.DestroyImmediate(ring.GetComponent<CapsuleCollider>());

        // Crack lines — thin flat cubes radiating from center
        var crackMat = new Material(baseMat); crackMat.color = baseMat.color * 0.5f;

        (Vector3 pos, Vector3 scale, float rotY)[] cracks = {
            (new Vector3( 0.10f, 0.02f,  0.10f), new Vector3(0.85f, 0.03f, 0.07f),  18f),
            (new Vector3(-0.10f, 0.02f, -0.05f), new Vector3(0.55f, 0.03f, 0.05f), -35f),
            (new Vector3( 0.05f, 0.02f, -0.15f), new Vector3(0.60f, 0.03f, 0.05f),  65f),
        };
        foreach (var (pos, scale, rotY) in cracks)
        {
            var cr  = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cr.name = "Crack"; cr.transform.SetParent(root.transform, false);
            cr.transform.localPosition    = pos;
            cr.transform.localScale       = scale;
            cr.transform.localEulerAngles = new Vector3(0f, rotY, 0f);
            cr.GetComponent<Renderer>().material = crackMat;
            Object.DestroyImmediate(cr.GetComponent<BoxCollider>());
        }

        const string path = "Assets/Prefabs/Pothole.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    // =========================================================================
    // GAME LOGIC OBJECTS
    // =========================================================================

    static void BuildCityManager(GameObject root)
    {
        var obj = new GameObject("CityManager");
        obj.transform.SetParent(root.transform, false);
        obj.AddComponent<CityManager>();
        obj.AddComponent<CityUI>();
    }

    static void BuildTrashSpawner(GameObject root, GameObject prefab)
    {
        var obj = new GameObject("TrashSpawner");
        obj.transform.SetParent(root.transform, false);
        var ts  = obj.AddComponent<TrashSpawner>();
        ts.trashPrefab    = prefab;
        ts.spawnInterval  = 5f;
        ts.mapRange       = 18f;
        ts.spawnY         = 0.5f;
    }

    static void BuildPotholeSpawner(GameObject root, GameObject prefab)
    {
        var obj = new GameObject("PotholeSpawner");
        obj.transform.SetParent(root.transform, false);
        var ps = obj.AddComponent<PotholeSpawner>();
        ps.potholePrefab = prefab;
        ps.spawnInterval = 10f;
        ps.spawnY        = 0.04f;
    }

    // =========================================================================
    // ROBOTS
    // =========================================================================

    static void BuildCleanerBots(GameObject root, Material body, Material wheel,
        Material eye, Material light, Material brush)
    {
        var c = new GameObject("CleanerBots");
        c.transform.SetParent(root.transform, false);

        Vector3[] starts = {
            new Vector3(-5f, 0f, -5f), new Vector3(5f, 0f, -5f), new Vector3(0f, 0f, 5f),
        };
        for (int i = 0; i < starts.Length; i++)
        {
            var bot = BuildBotModel(i + 1, false, body, wheel, eye, light, brush);
            bot.transform.SetParent(c.transform, false);
            bot.transform.position = starts[i];
        }
    }

    static void BuildRepairBots(GameObject root, Material body, Material wheel,
        Material eye, Material light, Material toolbox)
    {
        var c = new GameObject("RepairBots");
        c.transform.SetParent(root.transform, false);

        Vector3[] starts = {
            new Vector3(-8f, 0f, 8f), new Vector3(8f, 0f, 8f), new Vector3(0f, 0f, -8f),
        };
        for (int i = 0; i < starts.Length; i++)
        {
            var bot = BuildBotModel(i + 1, true, body, wheel, eye, light, toolbox);
            bot.transform.SetParent(c.transform, false);
            bot.transform.position = starts[i];
        }
    }

    // isRepair=false → blue CleanerBot (RobotController) with cleaning brush
    // isRepair=true  → orange RepairBot (RepairBotController) with toolbox
    static GameObject BuildBotModel(int index, bool isRepair,
        Material body, Material wheel, Material eye, Material light, Material tool)
    {
        string name = isRepair
            ? "RepairBot_"  + index.ToString("D2")
            : "CleanerBot_" + index.ToString("D2");

        var root = new GameObject(name);
        if (isRepair) root.AddComponent<RepairBotController>();
        else          root.AddComponent<RobotController>();

        // ---- Body -----------------------------------------------------------
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

        // Indicator light — gets BotPulse for animated glow
        var lightPart = AddPart(root, "Light", PrimitiveType.Sphere,
            new Vector3(0f, 1.98f, 0f), new Vector3(0.13f, 0.13f, 0.13f), light);
        lightPart.AddComponent<BotPulse>();

        // ---- Wheels (cylinder rotated 90° on Z = side-facing disc) ---------
        (string n, Vector3 p)[] wheels = {
            ("Wheel_FL", new Vector3( 0.58f, 0.25f,  0.44f)),
            ("Wheel_FR", new Vector3(-0.58f, 0.25f,  0.44f)),
            ("Wheel_BL", new Vector3( 0.58f, 0.25f, -0.44f)),
            ("Wheel_BR", new Vector3(-0.58f, 0.25f, -0.44f)),
        };
        foreach (var (n, p) in wheels)
        {
            var w = AddPart(root, n, PrimitiveType.Cylinder,
                p, new Vector3(0.55f, 0.10f, 0.55f), wheel);
            w.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
        }

        // ---- Tool attachment ------------------------------------------------
        if (!isRepair)
        {
            // CleanerBot: rotating brush disc at the front base
            AddPart(root, "Brush", PrimitiveType.Cylinder,
                new Vector3(0f, 0.15f, 0.70f), new Vector3(0.65f, 0.06f, 0.65f), tool);
        }
        else
        {
            // RepairBot: toolbox mounted on front of body
            AddPart(root, "Toolbox", PrimitiveType.Cube,
                new Vector3(0f, 0.64f, 0.46f), new Vector3(0.42f, 0.32f, 0.30f), tool);
        }

        // ---- Billboard name label -------------------------------------------
        string labelText = isRepair ? "RBot_"  + index.ToString("D2")
                                    : "Bot_"   + index.ToString("D2");
        var lo  = new GameObject("Label");
        lo.transform.SetParent(root.transform, false);
        lo.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        var tmp = lo.AddComponent<TextMeshPro>();
        tmp.text      = labelText;
        tmp.fontSize  = 2f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = isRepair ? new Color(1f, 0.70f, 0.25f) : Color.white;
        lo.AddComponent<RobotLabel>();

        return root;
    }
}
