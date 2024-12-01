using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Converters;

public class Options : MonoBehaviour {
    public static MapType SelectedMapType = MapType.Normal;
    private List<string> resses;
    Dictionary<string, CustomResolution> resolutions;
    public AudioMixer mixer;
    public Slider masterVolume;
    public Slider soundEffectVolume;
    public Dropdown resolutionDropDown;
    public Dropdown fullscreenDropDown;
    public Dropdown mapType;
    private string fileName = "options.ini";

    void Start() {
        CustomResolution currRes = new CustomResolution(Screen.currentResolution);

        List<string> fullscreenoptions = new List<string>();
        foreach (FullScreenMode fsm in typeof(FullScreenMode).GetEnumValues()) {
            fullscreenoptions.Add(fsm + "");
        }
        fullscreenDropDown.ClearOptions();
        fullscreenDropDown.AddOptions(fullscreenoptions);
        fullscreenDropDown.onValueChanged.AddListener(SetFullscreen);
        fullscreenDropDown.value = (int)currRes.fullScreenMode;
        fullscreenDropDown.RefreshShownValue();
        resses = new List<string>();
        resolutions = new Dictionary<string, CustomResolution>();
        foreach (Resolution res in Screen.resolutions) {
            CustomResolution cres = new CustomResolution(res);
            if (resolutions.ContainsKey(cres.ToString())) {
                continue;
            }
            string resstring = cres.ToString();
            resolutions[resstring] = cres;
            resses.Add(resstring);
        }
        resolutionDropDown.ClearOptions();
        resolutionDropDown.AddOptions(resses);
        resolutionDropDown.value = resses.IndexOf(currRes.ToString());
        resolutionDropDown.RefreshShownValue();
        resolutionDropDown.onValueChanged.AddListener(OnResolutionChange);
        masterVolume.onValueChanged.AddListener(MasterVolumeChange);
        soundEffectVolume.onValueChanged.AddListener(SoundEffectVolumeChange);

        mapType.onValueChanged.AddListener(OnMaptypeChange);
        MasterVolumeChange(50);
        SoundEffectVolumeChange(50);
        Load();
    }

    private void Load() {
        string filePath = System.IO.Path.Combine(Application.dataPath.Replace("/Assets", ""), fileName);
        if (File.Exists(filePath) == false) {
            return;
        }
        OptionSave save = JsonConvert.DeserializeObject<OptionSave>(File.ReadAllText(filePath));
        masterVolume.value = save.MasterVolume;
        MasterVolumeChange(save.MasterVolume);
        soundEffectVolume.value = save.SoundEffectVolume;
        SoundEffectVolumeChange(save.SoundEffectVolume);

        resolutionDropDown.value = resses.IndexOf(save.resolution.ToString());
        OnResolutionChange(resolutionDropDown.value);
        fullscreenDropDown.value = (int)save.resolution.fullScreenMode;
        SetFullscreen(fullscreenDropDown.value);

        mapType.value = (int)save.MapType;
        mapType.RefreshShownValue();
    }

    private void Save() {
        string path = Application.dataPath.Replace("/Assets", "");
        if (Directory.Exists(path) == false) {
            // NOTE: This can throw an exception if we can't create the folder,
            // but why would this ever happen? We should, by definition, have the ability
            // to write to our persistent data folder unless something is REALLY broken
            // with the computer/device we're running on.
            Directory.CreateDirectory(path);
        }
        OptionSave save = new OptionSave {
            MasterVolume = masterVolume.value,
            SoundEffectVolume = soundEffectVolume.value,
            resolution = new CustomResolution(Screen.currentResolution),
            MapType = SelectedMapType
        };
        string filePath = System.IO.Path.Combine(path, fileName);
        File.WriteAllText(filePath, JsonConvert.SerializeObject(save,new JsonSerializerSettings { }));
    }

    private void SoundEffectVolumeChange(float value) {
        mixer.SetFloat("SoundEffectVolume", ConvertToDecibel(value));
    }

    private void MasterVolumeChange(float value) {
        mixer.SetFloat("MasterVolume", ConvertToDecibel(value));
    }

    void Update() {
    }
    public void OnResolutionChange(int val) {
        CustomResolution cr = resolutions[resses[val]];
        Screen.SetResolution(cr.width, cr.height, Screen.fullScreenMode, cr.refreshRate);
    }
    void SetFullscreen(int value) {
        string res = resolutionDropDown.options[value].text;
        resolutions[res].fullScreenMode = (FullScreenMode)value;
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height,
            (FullScreenMode)value, Screen.currentResolution.refreshRate);
    }

    private void OnMaptypeChange(int selected) {
        SelectedMapType = (MapType)selected;
    }

    /**
     * Convert the value coming from our sliders to a decibel value we can
     * feed into the audio mixer.
     */
    public static float ConvertToDecibel(float value) {
        // Log(0) is undefined so we just set it by default to -80 decibels
        // which is 0 volume in the audio mixer.
        float decibel = -80f;

        // I think the correct formula is Mathf.Log10(value / 100f) * 20f.
        // Using that yields -6dB at 50% on the slider which is I think is half
        // volume, but I don't feel like it sounds like half volume. :p And I also
        // felt this homemade formula sounds more natural/linear when you go towards 0.
        // Note: We use 0-100 for our volume sliders in the menu, hence the
        // divide by 100 in the equation. If you use 0-1 instead you would remove that.
        if (value > 0) {
            decibel = Mathf.Log(value / 100f) * 17f;
        }

        return decibel;
    }
    [JsonObject]
    public class CustomResolution {
        public int width;
        public int height;
        public int refreshRate;
        [JsonConverter(typeof(StringEnumConverter))]
        public FullScreenMode fullScreenMode;

        public CustomResolution() {
        }
        public CustomResolution(Resolution res) {
            width = res.width;
            height = res.height;
            refreshRate = res.refreshRate;
        }
        public override string ToString() {
            return string.Format(width + " x " + height + " @ " + refreshRate);
        }

    }
    [JsonObject]
    private class OptionSave {
        public MapType MapType;
        public float MasterVolume;
        public float SoundEffectVolume;
        public CustomResolution resolution;
    }
    private void OnDisable() {
        Save();
    }
}
