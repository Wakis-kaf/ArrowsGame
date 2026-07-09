using System;
using static Framework.Utils.Utility;

namespace Framework.Utils
{
    public static partial class Utility
    {
        private class SnowflakeIdGenerator
        {
            private const long Epoch = 1704067200000L;
            private const int WorkerIdBits = 10;
            private const int SequenceBits = 12;

            private const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);
            private const int WorkerIdShift = SequenceBits;
            private const int TimestampShift = SequenceBits + WorkerIdBits;
            private const long SequenceMask = -1L ^ (-1L << SequenceBits);

            private readonly long _workerId;
            private long _sequence = 0L;
            private long _lastTimestamp = -1L;

       

            private readonly object _lock = new object();

            public SnowflakeIdGenerator(long workerId)
            {
                if (workerId > MaxWorkerId || workerId < 0)
                    throw new ArgumentException($"Worker ID must be between 0 and {MaxWorkerId}");
                _workerId = workerId;
            }

            public long NextId()
            {
                lock (_lock)
                {
                    long timestamp = GetCurrentTimestamp();

                    if (timestamp < _lastTimestamp)
                        throw new Exception("Clock moved backwards.");

                    if (_lastTimestamp == timestamp)
                    {
                        _sequence = (_sequence + 1) & SequenceMask;
                        if (_sequence == 0)
                        {
                            timestamp = WaitNextMillis(_lastTimestamp);
                        }
                    }
                    else
                    {
                        _sequence = 0L;
                    }

                    _lastTimestamp = timestamp;

                    return ((timestamp - Epoch) << TimestampShift) |
                           (_workerId << WorkerIdShift) |
                           _sequence;
                }
            }

            private long WaitNextMillis(long lastTimestamp)
            {
                long timestamp = GetCurrentTimestamp();
                while (timestamp <= lastTimestamp)
                {
                    timestamp = GetCurrentTimestamp();
                }
                return timestamp;
            }

            private long GetCurrentTimestamp()
            {
                return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }
        public static class IDGenerator
        {
            private static readonly SnowflakeIdGenerator m_SnowflakeIdGenerator = new SnowflakeIdGenerator(1);
            public static long GetSnowflakeID()
            {
                return m_SnowflakeIdGenerator.NextId();
            }
            public static int GetIntGuidID()
            {
                return Guid.NewGuid().GetHashCode() & int.MaxValue;
            }

            /// <summary>
            /// 方法一 使用随机抽取数组index中的数，填充在新的数组array中，使数组array中的数是随机的
            /// 方法一思路：用一个数组来保存索引号，先随机生成一个数组位置，然后把随机抽取到的位置的索引号取出来，
            /// 并把最后一个索引号复制到当前的数组位置，然后使随机数的上限减一，具体如：先把这100个数放在一个数组内， 每次随机取一个位置（第一次是1-100，第二次是1-99，...），将该位置的数用最后的数代替。
            /// </summary>
            public static int[] GetRandomIDArrayByDoubleArray(int length)
            {
                int seed = Guid.NewGuid().GetHashCode();
                Random radom = new Random(seed);
                int[] index = new int[length];
                for (int i = 0; i < length; i++)
                {
                    index[i] = i + 1;
                }

                int[] array = new int[length]; // 用来保存随机生成的不重复的数
                int site = length; // 设置上限
                int idx; // 获取index数组中索引为idx位置的数据，赋给结果数组array的j索引位置
                for (int j = 0; j < length; j++)
                {
                    idx = radom.Next(0, site - 1); // 生成随机索引数
                    array[j] = index[idx]; // 在随机索引位置取出一个数，保存到结果数组
                    index[idx] = index[site - 1]; // 作废当前索引位置数据，并用数组的最后一个数据代替之
                    site--; // 索引位置的上限减一（弃置最后一个数据）
                }

                return array;
            }

            public static int GetRandomIDByDoubleArray(int length)
            {
                return GetRandomIDArrayByDoubleArray(length).GetHashCode();
            }

            public static string GetRandomIDStrByDoubleArray(int length)
            {
                return GetRandomIDByDoubleArray(length).ToString();
            }

            public static string GetStrGuidID()
            {
                return Guid.NewGuid().ToString().Replace("-", "_");
            }
        }
    }
}