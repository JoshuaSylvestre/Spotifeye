using UnityEngine;
using System.Collections;

public class Equalizer : MonoBehaviour
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
        spawnCircle();
    }

    void Update()
    {
        AnalyzeSound();
        UpdateVisual();
    }

    private void spawnCircle()
    {
        visualScale = new float[amtVisual];
        visualList = new Transform[amtVisual];

        Vector3 center = Vector3.zero;
        float radius = 10f;

        for(int i = 0; i < amtVisual; i++)
        {
            float angle = i * 1.0f / amtVisual;
            angle *= Mathf.PI * 2;

            float x = center.x + Mathf.Cos(angle) * radius;
            float y = center.y + Mathf.Sin(angle) * radius;

            Vector3 position = center + new Vector3(x, y, 0);
            GameObject cubeSpawn = GameObject.CreatePrimitive(PrimitiveType.Cube) as GameObject;

            cubeSpawn.transform.position = position;
            cubeSpawn.transform.rotation = Quaternion.LookRotation(Vector3.forward, position);

            visualList[i] = cubeSpawn.transform;
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

            if(visualScale[visualIndex] > maxVisualScale)
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