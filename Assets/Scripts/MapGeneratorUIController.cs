using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ponte entre widgets de UI e MapGenerator / EndlessTerrain.
/// Ligue no Inspector via On Value Changed / On Click:
///
/// MapGenerator:
///   SetDrawMode(int)              -> Dropdown
///   SetNormalizeMode(int)         -> Dropdown
///   SetEditorPreviewLOD(float)    -> Slider (whole numbers)
///   SetNoiseScale(float)          -> Slider
///   SetOctaves(float)             -> Slider (whole numbers)
///   SetPersistance(float)         -> Slider
///   SetLacunarity(float)          -> Slider
///   SetGlobalNormalizeModeOffset(float) -> Slider
///   SetTestIntensity(float)       -> Slider
///   SetTestPeaks(float)           -> Slider
///   SetSeed(int)                  -> InputField (int) via SetSeedFromString(string)
///   RandomizeSeed()               -> Button
///   SetOffsetX(float) / SetOffsetY(float) -> Slider ou InputField
///   SetMeshHeightMultiplier(float)-> Slider
///   OnAutoUpdateChanged(bool)     -> Toggle
///   SetRegionHeight(int,float)    -> Slider com índice via código
///   SetRegion0Height..7(float)    -> Slider (uma linha por região)
///   SetRegionColor(int,Color)     -> color picker / sliders RGB
///
/// Regiões (índice fixo por linha na UI):
///   GetRegionName(int)            -> Text (somente leitura no Start)
///   GetRegionHeight(int)          -> popular Slider
///   GetRegionColor(int)           -> popular cor
///
/// EndlessTerrain detailLevels:
///   SetDetailLevelLod(int,float)  -> Slider com índice via código
///   SetDetailLevel0Lod..3(float)  -> Slider (uma linha por LOD)
///   SetDetailLevelVisibleDst(int,float) -> Slider com índice via código
///   SetDetailLevel0VisibleDst..3(float) -> Slider
///   GetDetailLevelLod(int)        -> popular Slider
///   GetDetailLevelVisibleDst(int) -> popular Slider
///
/// Ações:
///   OnGenerateClicked()           -> Button "Gerar"
///   ToggleUI()                    -> Button opcional (Esc também alterna)
///
/// meshHeightCurve: sem binding — editar só no Inspector.
/// </summary>
public class MapGeneratorUIController : MonoBehaviour
{
    [SerializeField] MapGenerator mapGenerator;
    [SerializeField] EndlessTerrain endlessTerrain;
    [SerializeField] GameObject uiRoot;
    [SerializeField] float regenerateDebounceSeconds = 0.35f;
    [SerializeField] TMP_InputField seedInputField;
    [SerializeField] Toggle autoUpdateToggle;
    [SerializeField] Button generateButton;

    Coroutine debouncedRegenerateCoroutine;

    void Awake()
    {
        if (mapGenerator == null) {
            mapGenerator = FindFirstObjectByType<MapGenerator>();
        }

        if (endlessTerrain == null) {
            endlessTerrain = FindFirstObjectByType<EndlessTerrain>();
        }
    }

    void Start()
    {
        SetUIOpen(true);

        seedInputField.onValueChanged.AddListener(SetSeedFromString);
        generateButton.onClick.AddListener(OnGenerateClicked);
        autoUpdateToggle.onValueChanged.AddListener((bool value) => {
            OnAutoUpdateChanged(value);
            generateButton.interactable = !value;
        });
        generateButton.interactable = !autoUpdateToggle.isOn;
        seedInputField.placeholder.GetComponent<TMP_Text>().text = mapGenerator.seed.ToString();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.C)) {
            ToggleUI();
        }
    }

    public void ToggleUI()
    {
        SetUIOpen(!PlayerInputGate.IsUIOpen);
    }

    public void SetUIOpen(bool open)
    {
        PlayerInputGate.SetUIOpen(open);

        if (uiRoot != null) {
            uiRoot.SetActive(open);
        }

        if (open) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void OnGenerateClicked()
    {
        RegenerateTerrain();
    }

    public void RegenerateTerrain()
    {
        if (endlessTerrain != null) {
            endlessTerrain.RegenerateAllChunks();
        }
    }

    public void OnAutoUpdateChanged(bool value)
    {
        mapGenerator.autoUpdate = value;
    }

    void OnSettingChanged()
    {
        if (mapGenerator.autoUpdate) {
            ScheduleRegenerate();
        }
        seedInputField.placeholder.GetComponent<TMP_Text>().text = mapGenerator.seed.ToString();
    }

    void ScheduleRegenerate()
    {
        if (debouncedRegenerateCoroutine != null) {
            StopCoroutine(debouncedRegenerateCoroutine);
        }

        debouncedRegenerateCoroutine = StartCoroutine(DebouncedRegenerate());
    }

    IEnumerator DebouncedRegenerate()
    {
        yield return new WaitForSeconds(regenerateDebounceSeconds);
        RegenerateTerrain();
        debouncedRegenerateCoroutine = null;
    }

    // --- MapGenerator getters ---

    public int GetDrawMode() => (int)mapGenerator.drawMode;
    public int GetNormalizeMode() => (int)mapGenerator.normalizeMode;
    public int GetEditorPreviewLOD() => mapGenerator.editorPreviewLOD;
    public float GetNoiseScale() => mapGenerator.noiseScale;
    public int GetOctaves() => mapGenerator.octaves;
    public float GetPersistance() => mapGenerator.persistance;
    public float GetLacunarity() => mapGenerator.lacunarity;
    public float GetGlobalNormalizeModeOffset() => mapGenerator.globalNormalizeModeOffset;
    public float GetTestIntensity() => mapGenerator.testIntensity;
    public float GetTestPeaks() => mapGenerator.testPeaks;
    public int GetSeed() => mapGenerator.seed;
    public float GetOffsetX() => mapGenerator.offset.x;
    public float GetOffsetY() => mapGenerator.offset.y;
    public float GetMeshHeightMultiplier() => mapGenerator.meshHeightMultiplier;
    public bool GetAutoUpdate() => mapGenerator.autoUpdate;
    public int GetRegionCount() => mapGenerator.regions != null ? mapGenerator.regions.Length : 0;

    public string GetRegionName(int index)
    {
        return IsValidRegionIndex(index) ? mapGenerator.regions[index].name : string.Empty;
    }

    public float GetRegionHeight(int index)
    {
        return IsValidRegionIndex(index) ? mapGenerator.regions[index].height : 0f;
    }

    public Color GetRegionColor(int index)
    {
        return IsValidRegionIndex(index) ? mapGenerator.regions[index].colour : Color.white;
    }

    public float GetDetailLevelLod(int index)
    {
        return IsValidDetailLevelIndex(index) ? endlessTerrain.detailLevels[index].lod : 0f;
    }

    public float GetDetailLevelVisibleDst(int index)
    {
        return IsValidDetailLevelIndex(index) ? endlessTerrain.detailLevels[index].visibleDstThreshold : 0f;
    }

    public int GetDetailLevelCount() => endlessTerrain != null ? endlessTerrain.DetailLevelCount : 0;

    // --- MapGenerator setters (Unity UI events) ---

    public void SetDrawMode(int value)
    {
        mapGenerator.drawMode = (MapGenerator.DrawMode)value;
        OnSettingChanged();
    }

    public void SetNormalizeMode(int value)
    {
        mapGenerator.normalizeMode = (Noise.NormalizeMode)value;
        OnSettingChanged();
    }

    public void SetEditorPreviewLOD(float value)
    {
        mapGenerator.editorPreviewLOD = Mathf.Clamp(Mathf.RoundToInt(value), 0, 6);
        OnSettingChanged();
    }

    public void SetNoiseScale(float value)
    {
        mapGenerator.noiseScale = Mathf.Max(0.001f, value);
        OnSettingChanged();
    }

    public void SetOctaves(float value)
    {
        mapGenerator.octaves = Mathf.Clamp(Mathf.RoundToInt(value), 1, 10);
        OnSettingChanged();
    }

    public void SetPersistance(float value)
    {
        mapGenerator.persistance = Mathf.Clamp01(value);
        OnSettingChanged();
    }

    public void SetLacunarity(float value)
    {
        mapGenerator.lacunarity = Mathf.Max(1f, value);
        OnSettingChanged();
    }

    public void SetGlobalNormalizeModeOffset(float value)
    {
        mapGenerator.globalNormalizeModeOffset = Mathf.Clamp(value, 0.1f, 10f);
        OnSettingChanged();
    }

    public void SetTestIntensity(float value)
    {
        mapGenerator.testIntensity = Mathf.Clamp(value, 0.1f, 10f);
        OnSettingChanged();
    }

    public void SetTestPeaks(float value)
    {
        mapGenerator.testPeaks = Mathf.Clamp(value, -0.5f, 0.5f);
        OnSettingChanged();
    }

    public void SetSeed(int value)
    {
        mapGenerator.seed = value;
        OnSettingChanged();
    }

    public void SetSeedFromString(string value)
    {
        if (int.TryParse(value, out int seed)) {
            SetSeed(seed);
        }
    }

    public void RandomizeSeed()
    {
        SetSeed(Random.Range(int.MinValue, int.MaxValue));
    }

    public void SetOffsetX(float value)
    {
        mapGenerator.offset = new Vector2(value, mapGenerator.offset.y);
        OnSettingChanged();
    }

    public void SetOffsetY(float value)
    {
        mapGenerator.offset = new Vector2(mapGenerator.offset.x, value);
        OnSettingChanged();
    }

    public void SetMeshHeightMultiplier(float value)
    {
        mapGenerator.meshHeightMultiplier = value;
        OnSettingChanged();
    }

    public void SetRegionHeight(int index, float value)
    {
        if (!IsValidRegionIndex(index)) {
            return;
        }

        TerrainTypes region = mapGenerator.regions[index];
        region.height = Mathf.Clamp01(value);
        mapGenerator.regions[index] = region;
        OnSettingChanged();
    }

    public void SetRegion0Height(float value) => SetRegionHeight(0, value);
    public void SetRegion1Height(float value) => SetRegionHeight(1, value);
    public void SetRegion2Height(float value) => SetRegionHeight(2, value);
    public void SetRegion3Height(float value) => SetRegionHeight(3, value);
    public void SetRegion4Height(float value) => SetRegionHeight(4, value);
    public void SetRegion5Height(float value) => SetRegionHeight(5, value);
    public void SetRegion6Height(float value) => SetRegionHeight(6, value);
    public void SetRegion7Height(float value) => SetRegionHeight(7, value);

    public void SetRegionColor(int index, Color value)
    {
        if (!IsValidRegionIndex(index)) {
            return;
        }

        TerrainTypes region = mapGenerator.regions[index];
        region.colour = value;
        mapGenerator.regions[index] = region;
        OnSettingChanged();
    }

    public void SetDetailLevelLod(int index, float value)
    {
        if (!IsValidDetailLevelIndex(index)) {
            return;
        }

        EndlessTerrain.LODInfo info = endlessTerrain.detailLevels[index];
        info.lod = Mathf.Clamp(Mathf.RoundToInt(value), 0, 6);
        endlessTerrain.detailLevels[index] = info;
        OnSettingChanged();
    }

    public void SetDetailLevel0Lod(float value) => SetDetailLevelLod(0, value);
    public void SetDetailLevel1Lod(float value) => SetDetailLevelLod(1, value);
    public void SetDetailLevel2Lod(float value) => SetDetailLevelLod(2, value);
    public void SetDetailLevel3Lod(float value) => SetDetailLevelLod(3, value);

    public void SetDetailLevelVisibleDst(int index, float value)
    {
        if (!IsValidDetailLevelIndex(index)) {
            return;
        }

        EndlessTerrain.LODInfo info = endlessTerrain.detailLevels[index];
        info.visibleDstThreshold = Mathf.Max(0f, value);
        endlessTerrain.detailLevels[index] = info;
        OnSettingChanged();
    }

    public void SetDetailLevel0VisibleDst(float value) => SetDetailLevelVisibleDst(0, value);
    public void SetDetailLevel1VisibleDst(float value) => SetDetailLevelVisibleDst(1, value);
    public void SetDetailLevel2VisibleDst(float value) => SetDetailLevelVisibleDst(2, value);
    public void SetDetailLevel3VisibleDst(float value) => SetDetailLevelVisibleDst(3, value);

    bool IsValidRegionIndex(int index)
    {
        return mapGenerator.regions != null && index >= 0 && index < mapGenerator.regions.Length;
    }

    bool IsValidDetailLevelIndex(int index)
    {
        return endlessTerrain != null
            && endlessTerrain.detailLevels != null
            && index >= 0
            && index < endlessTerrain.detailLevels.Length;
    }
}
