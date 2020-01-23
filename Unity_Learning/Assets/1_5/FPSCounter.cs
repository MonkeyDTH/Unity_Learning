using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public int FPS { get; private set; }
    public int AverageFPS { get; private set; }
    public int HighestFPS { get; private set; }
    public int LowestFPS { get; private set; }
    public int frameRange = 60;

    int[] fpsBufffer;
    int fpsBufferIndex;

    void InitializeBuffer ()
    {
        if (frameRange <= 0)
        {
            frameRange = 1;
        }
        fpsBufffer = new int[frameRange];
        fpsBufferIndex = 0;
    }

    void UpdateBuffer()
    {
        fpsBufffer[fpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);
        if (fpsBufferIndex >= frameRange) {
            fpsBufferIndex = 0;
        }
    }

    void CalculateFPS ()
    {
        int sum = 0;
        int highest = 0;
        int lowest = int.MaxValue;
        for (int i = 0; i < frameRange; i++)
        {
            int fps = fpsBufffer[i];
            sum += fps;
            if (fps > highest)
            {
                highest = fps;
            }
            if (fps < lowest)
            {
                lowest = fps;
            }
        }
        AverageFPS = sum / frameRange;
        HighestFPS = highest;
        LowestFPS = lowest;
    }

    // Update is called once per frame
    void Update()
    {
        if (fpsBufffer == null || fpsBufffer.Length != frameRange)
        {
            InitializeBuffer();
        }
        UpdateBuffer();
        CalculateFPS();
        //FPS = (int)(1f / Time.unscaledDeltaTime);
    }
}
