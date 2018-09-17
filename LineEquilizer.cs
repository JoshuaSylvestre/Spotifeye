using UnityEngine;
using System.Collections;

public class LineEquilizer : MonoBehaviour
{
    private const int SAMPLE_SIZE = 1024;

    private AudioSource source;
    private float[] samples;
    private float[] spectrum;
    private float samplerate;

    public float visualModifier = 50f;
    public float smootheningSpeed = 10f;
    public float scaleDownPercent = 0.02f;
    public float maxVisualScale = 25f;
    public float keepPercentage = 0.25f;

    private Transform[] visualList;
    private float[] visualScale;
    public int amtVisual = 64;

    public float rmsValue;
    public float dbValue;
    public float pitchValue;

    void Start()
    {
        source = GetComponent<AudioSource>();
        samples = new float[SAMPLE_SIZE];
        spectrum = new float[SAMPLE_SIZE];
        samplerate = AudioSettings.outputSampleRate;
        SpawnLine();
    }

    void Update()
    {
        AnalyzeSound();
        UpdateVisual();
    }

    private void SpawnLine()
    {
        visualScale = new float[amtVisual];
        visualList = new Transform[amtVisual];

        for (int i = 0; i < amtVisual; i++)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube) as GameObject;
            visualList[i] = go.transform;
            visualList[i].position = new Vector3(visualList[i].position.x + i * scaleDownPercent, visualList[i].position.y, visualList[i].position.z + 3.0f);
        }
    }

    private void UpdateVisual()
    {
        int visualIndex = 0;
        int spectrumIndex = 0;
        int averageSize = (int)((SAMPLE_SIZE * keepPercentage) / amtVisual);

        while (visualIndex < amtVisual)
        {
            int j = 0;
            float sum = 0;
            while (j < averageSize)
            {
                sum += spectrum[spectrumIndex];
                spectrumIndex++;
                j++;
            }

            float scaleY = sum / averageSize * visualModifier;
            visualScale[visualIndex] -= Time.deltaTime * smootheningSpeed;

            if (visualScale[visualIndex] < scaleY)
            {
                visualScale[visualIndex] = scaleY;
            }

            if (visualScale[visualIndex] > maxVisualScale)
            {
                visualScale[visualIndex] = maxVisualScale;
            }

            visualList[visualIndex].localScale = Vector3.one + Vector3.up * visualScale[visualIndex];
            visualList[visualIndex].localScale *= scaleDownPercent;
            visualIndex++;
        }
    }

    private void AnalyzeSound()
    {
        float sum = 0;

        source.GetOutputData(samples, 0);

        for (int i = 0; i < SAMPLE_SIZE; i++)
        {
            sum += samples[i] * samples[i];
        }

        rmsValue = Mathf.Sqrt(sum / SAMPLE_SIZE);

        dbValue = 20 * Mathf.Log10(rmsValue / 1.0f);

        source.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        // Pitch
        float maxVal = 0;
        var maxN = 0;
        for (int i = 0; i < SAMPLE_SIZE; i++)
        {
            if (!(spectrum[i] > maxVal) || !(spectrum[i] > 0f))
            {
                continue;
            }
            maxVal = spectrum[i];
            maxN = i;
        }

        float freqN = maxN;

        if (maxN > 0 && maxN < SAMPLE_SIZE - 1)
        {
            var dL = spectrum[maxN - 1] / spectrum[maxN];
            var dR = spectrum[maxN + 1] / spectrum[maxN];
            freqN += 0.5f * (dR * dR - dL * dL);
        }

        pitchValue = freqN * (samplerate / 2) / SAMPLE_SIZE;
    }
}