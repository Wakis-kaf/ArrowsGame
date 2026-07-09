using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Runtime;

namespace Framework.Library
{
    public class LightCoroutine<T> where T : ILightCoroutineTaskData
    {
        public GameObject coroutineExecutor { get; private set; }
        public LightCoroutineMono lightCoroutineExecutingMono { get; private set; }

        public LightCoroutine(GameObject coroutineExecutor)
        {
            this.coroutineExecutor = coroutineExecutor;
            lightCoroutineExecutingMono = coroutineExecutor.AddComponent<LightCoroutineMono>();
        }

        private Dictionary<string, Queue<Coroutine>> _coroutines = new Dictionary<string, Queue<Coroutine>>();
        private Dictionary<string, List<T>> m_Name2CoroutineTaskMap = new Dictionary<string, List<T>>(100);
        private Dictionary<string, List<Action<T>>> _Name2CoroutineCBMap = new Dictionary<string, List<Action<T>>>(100);
        private Dictionary<string, int> _busyCoroutinesCount = new Dictionary<string, int>();
        private Dictionary<string, int> _freeCoroutinesCount = new Dictionary<string, int>();
        private Dictionary<float, WaitForSeconds> _waitForSecondsCache = new Dictionary<float, WaitForSeconds>();

        /// <summary>
        /// 执行协程任务
        /// </summary>
        /// <param name="coroutineTaskName"></param>
        /// <param name="coroutineExecutor"></param>
        public void StartCoroutineTasks(string coroutineTaskName, T coroutineTaskData, Action<T> cb)
        {
            if (string.IsNullOrEmpty(coroutineTaskName)) return;

            if (!m_Name2CoroutineTaskMap.ContainsKey((coroutineTaskName)))
            {
                m_Name2CoroutineTaskMap.Add(coroutineTaskName, new List<T>());
                _Name2CoroutineCBMap.Add(coroutineTaskName, new List<Action<T>>());
            }

            m_Name2CoroutineTaskMap[coroutineTaskName].Add(coroutineTaskData);
            _Name2CoroutineCBMap[coroutineTaskName].Add(cb);

            if (!_coroutines.ContainsKey(coroutineTaskName))
            {
                // 保存协程
                _coroutines.Add(coroutineTaskName, new Queue<Coroutine>(20));
                _busyCoroutinesCount.Add(coroutineTaskName, 0);
                _freeCoroutinesCount.Add(coroutineTaskName, 0);
                _coroutines[coroutineTaskName].Enqueue(
                    lightCoroutineExecutingMono.StartCoroutine(LightTaskExecutor_Coroutine(coroutineTaskName)));
            //     Log.Info(
            //         $"当前协程数{_coroutines.Count},正在工作的协程任务{_busyCoroutinesCount[coroutineTaskName]},正在等待的协程任务{_freeCoroutinesCount[coroutineTaskName]}");
            }
        }

        IEnumerator LightTaskExecutor_Coroutine(string coroutineTaskName)
        {
            while (true)
            {
                for (int i = m_Name2CoroutineTaskMap[coroutineTaskName].Count - 1; i >= 0; i--)
                {
                    T task = m_Name2CoroutineTaskMap[coroutineTaskName][i];
                    float timeGoes = m_Name2CoroutineTaskMap[coroutineTaskName][i].createTime +
                                     m_Name2CoroutineTaskMap[coroutineTaskName][i].waitSeconds;
                    if (timeGoes <= Time.time)
                    {
                        _Name2CoroutineCBMap[coroutineTaskName][i]?.Invoke(task);
                        m_Name2CoroutineTaskMap[coroutineTaskName].RemoveAt(i);
                    }
                }

                yield return null;
            }
        }
    }

    public interface ILightCoroutineTaskData
    {
        public float createTime { get; }
        public float waitSeconds { get; }
    }
}