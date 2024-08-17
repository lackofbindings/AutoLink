using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using AudioLink;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class AutoLinkV0 : UdonSharpBehaviour
{
    [Space(10)]
    public AudioLink.AudioLink audioLink;
    public AudioLinkControllerV0 audioLinkController;
    public Material audioSpectrumDisplay;

    [Space(10)]
    [UdonSynced] public bool isActive = true;
    public float minimumActivityThreshold = 0.0001f;

    [Space(10)]
    public bool adjustBandThresholds = true;
    public float thresholdRiseSpeed = 0.5f;
    public float thresholdFallSpeed = 0.125f;
    public float thresholdRiseMargin = 0.05f;
    public float thresholdLowerLimit = 0.25f;
    public float heightRampIntensity = 1f;
    public float sideRampIntensity = 0.5f;

    [Space(10)]
    public bool adjustGain = true;
    public float autoGainLowLimit = 0.8f;
    public float autoGainHighLimit = 1.33f;
    public float autoGainRiseSpeed = 0.1f;
    public float autoGainFallSpeed = 0.15f;
    public bool overrideMaxGain = false;
    public float maxGain = 300f;

    [Space(10)]
    public Text statusText;
    public Text toggleText;

    private int bassIndex = 0;
    private int lowMidIndex = 128;
    private int highMidIndex = 256;
    private int trebleIndex = 384;

    void Start()
    {
        UpdateDisplay();

        if (overrideMaxGain && audioLinkController)
        {
            audioLinkController.gainSlider.maxValue = maxGain / 100f;
        }
    }

    public void TakeOwnership()
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public void Toggle()
    {
        isActive = !isActive;
        TakeOwnership();
        RequestSerialization();
        UpdateDisplay();
    }

    public override void OnDeserialization()
    {
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (isActive)
        {
            statusText.text = "AutoLink is: ON";
            toggleText.text = "Turn Off";
        }
        else
        {
            statusText.text = "AutoLink is: OFF";
            toggleText.text = "Turn On";
        }

        /*
        audioLinkController.gainSlider.gameObject.SetActive(!isActive);
        audioLinkController.threshold0Slider.gameObject.SetActive(!isActive);
        audioLinkController.threshold1Slider.gameObject.SetActive(!isActive);
        audioLinkController.threshold2Slider.gameObject.SetActive(!isActive);
        audioLinkController.threshold3Slider.gameObject.SetActive(!isActive);
        */
    }

    void Update()
    {
        if (!audioLink || !isActive)
            return;

        Color[] audioData = (Color[])audioLink.GetProgramVariable("audioData");
        if (audioData.Length == 0)
            return;

        float intensity = (audioData[(22 * 128) + 8].r + audioData[(22 * 128) + 8].b) / 2.0f;
        if (intensity < minimumActivityThreshold)
            return;

        // Auto-gain for overall levels
        if (adjustGain)
        {
            intensity *= audioLink.gain;
            intensity *= 0.5f * (audioLink.bass + audioLink.treble);

            if (intensity < autoGainLowLimit)
            {
                audioLink.gain += autoGainRiseSpeed * Time.deltaTime;
            }
            else if (intensity > autoGainHighLimit)
            {
                audioLink.gain -= autoGainFallSpeed * Time.deltaTime;
            }
        }

        // Adjust each band threshold individually
        if (adjustBandThresholds)
        {
            float bassValue = audioData[bassIndex].grayscale;
            float lowMidValue = audioData[lowMidIndex].grayscale;
            float highMidValue = audioData[highMidIndex].grayscale;
            float trebleValue = audioData[trebleIndex].grayscale;
            float total = bassValue + lowMidValue + highMidValue + trebleValue;

            float sideRamp = 1f;
            if (bassValue > audioLink.threshold0 + thresholdRiseMargin)
            {
                float riseSpeed = thresholdRiseSpeed * sideRamp;
                audioLink.threshold0 += riseSpeed * Time.deltaTime;
            }
            else if (audioLink.threshold0 > thresholdLowerLimit)
            {
                float fallSpeed = thresholdFallSpeed * (heightRampIntensity * audioLink.threshold0);
                fallSpeed *= sideRamp;
                audioLink.threshold0 -= fallSpeed * Time.deltaTime;
            }

            sideRamp = 1f - (sideRampIntensity * 0.25f);
            if (lowMidValue > audioLink.threshold1 + thresholdRiseMargin)
            {
                float riseSpeed = thresholdRiseSpeed * sideRamp;
                audioLink.threshold1 += riseSpeed * Time.deltaTime;
            }
            else if (audioLink.threshold1 > thresholdLowerLimit)
            {
                float fallSpeed = thresholdFallSpeed * (heightRampIntensity * audioLink.threshold1);
                fallSpeed *= sideRamp;
                audioLink.threshold1 -= fallSpeed * Time.deltaTime;
            }

            sideRamp = 1f - (sideRampIntensity * 0.5f);
            if (highMidValue > audioLink.threshold2 + thresholdRiseMargin)
            {
                float riseSpeed = thresholdRiseSpeed * sideRamp;
                audioLink.threshold2 += thresholdRiseSpeed * Time.deltaTime;
            }
            else if (audioLink.threshold2 > thresholdLowerLimit)
            {
                float fallSpeed = thresholdFallSpeed * (heightRampIntensity * audioLink.threshold2);
                fallSpeed *= sideRamp;
                audioLink.threshold2 -= fallSpeed * Time.deltaTime;
            }

            sideRamp = 1f - (sideRampIntensity * 0.75f);
            if (trebleValue > audioLink.threshold3 + thresholdRiseMargin)
            {
                float riseSpeed = thresholdRiseSpeed * sideRamp;
                audioLink.threshold3 += thresholdRiseSpeed * Time.deltaTime;
            }
            else if (audioLink.threshold3 > thresholdLowerLimit)
            {
                float fallSpeed = thresholdFallSpeed * (heightRampIntensity * audioLink.threshold3);
                fallSpeed *= sideRamp;
                audioLink.threshold3 -= fallSpeed * Time.deltaTime;
            }
        }

        // Update displays
        if (audioLinkController)
        {
            audioLinkController.gainLabel.text = "Gain: " + (audioLink.gain * 100f).ToString("F1") + "%";
        }

        if (audioSpectrumDisplay)
        {
            audioSpectrumDisplay.SetFloat("_Threshold0", audioLink.threshold0);
            audioSpectrumDisplay.SetFloat("_Threshold1", audioLink.threshold1);
            audioSpectrumDisplay.SetFloat("_Threshold2", audioLink.threshold2);
            audioSpectrumDisplay.SetFloat("_Threshold3", audioLink.threshold3);
        }

        audioLink.UpdateSettings();
    }
}
