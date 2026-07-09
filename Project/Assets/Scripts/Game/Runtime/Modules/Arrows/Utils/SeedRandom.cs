using System;
using Framework.Runtime.LogSystem;

public class SeedRandom
{
    public int Seed { get; private set; }
    private Random m_Prng;

    public void InitSeed(int seed)
    {
        Seed = seed;
        m_Prng = new Random(seed);
        Log.Info($"初始化种子，使用固定种子{Seed}");
    }
    public void InitSeed()
    {
        Seed = Environment.TickCount ^ Guid.NewGuid().GetHashCode();
        m_Prng = new Random(Seed);
        Log.Info($"初始化种子，使用随机种子{Seed}");
    }

    public float Range(float min, float max)
    {
        if (m_Prng == null)
        {
            InitSeed(Environment.TickCount);
        }

        float sample = (float)m_Prng.NextDouble();
        return min + sample * (max - min);
    }

    public int Range(int min, int max)
    {
        if (m_Prng == null)
        {
            InitSeed(Environment.TickCount);
        }

        if (min >= max)
        {
            return min;
        }

        return m_Prng.Next(min, max);
    }
}