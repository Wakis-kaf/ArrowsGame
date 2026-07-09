using UnityEngine;

public class ValueOscillator
{
    private float elapsedTime;

    // 可动态调整的参数
    public float StartValue { get; set; }
    public float EndValue { get; set; }
    public float Duration { get; set; }
    public bool IsPaused { get; set; }

    // 默认构造函数
    public ValueOscillator()
    {
        StartValue = 0f;
        EndValue = 1f;
        Duration = 1f;
        elapsedTime = 0f;
        IsPaused = false;
    }

    // 带初始值的构造函数（可选）
    public ValueOscillator(float start = 0f, float end = 1f, float duration = 1f)
    {
        StartValue = start;
        EndValue = end;
        Duration = duration;
        elapsedTime = 0f;
        IsPaused = false;
    }
 
    public float Update(float deltaTime)
    {
        if (IsPaused)
            return GetCurrentValue();

        elapsedTime += deltaTime;
        return GetCurrentValue();
    }

    public float GetCurrentValue()
    {
        if (Duration <= 0f)
            return StartValue;

        float t = Mathf.PingPong(elapsedTime, Duration) / Duration;
        return Mathf.Lerp(StartValue, EndValue, t);
    }

    // 动态调整方法
    public void SetRange(float start, float end)
    {
        StartValue = start;
        EndValue = end;
    }

    public void SetDuration(float duration)
    {
        Duration = Mathf.Max(0.001f, duration); // 避免除零
    }

    public void SetSpeed(float speed)
    {
        // 速度是持续时间的倒数
        Duration = Mathf.Max(0.001f, 1f / speed);
    }

    public void Reset()
    {
        elapsedTime = 0f;
    }

    public void SetProgress(float normalizedTime)
    {
        elapsedTime = normalizedTime * Duration;
    }
}