using System.Collections;
using UnityEngine;

namespace Framework.Runtime
{
    public class GameAppShell : MonoBehaviour
    {
        private GameObject m_DriverGameObject;
      
        private void Awake()
        {
            //if (GameApplication.Instance != null)
            //{
            //    Destroy(gameObject);
            //    return;
            //}
            //Application.wantsToQuit += QuitApp;
            //Instance = this;
            //name = "GameAppShell";
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
           
        }
        //private bool QuitApp()
        //{
        //    Debug.Log("退出应用");
        //    if (GameApplication.Instance != null)
        //    {
        //        GameApplication.Instance.StopApplication();
        //    }
        //    //Instance = null;
        //    return true;
        //}

        private void Start()
        {
            //StartCoroutine(StartEngineDelay());
        }

        //private IEnumerator StartEngineDelay()
        //{
        //    yield return new WaitForSeconds(0.1f);
        //    GameApplication.CreateInstance();
        //    GameApplication.Instance.StartApplication();
        //}
    }
}