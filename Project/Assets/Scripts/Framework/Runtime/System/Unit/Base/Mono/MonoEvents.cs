using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Framework.Runtime.UnitSystem.MonoBase
{
    public class MonoEvents : MonoBehaviour
    {
        private Action m_OnDestroy;
        private Action m_OnHide;
        private Action m_OnShow;
        private Action m_OnMouseDown;
        private Action m_OnMouseUp;
        private Action m_OnMouseEnter;
        private Action m_OnMouseExit;

        public void SetMouseDown(Action onMouseDown)
        {
            m_OnMouseDown = onMouseDown;
        }
        public void SetMouseUp(Action onMouseUp)
        {
            m_OnMouseUp = onMouseUp;
        }
        public void SetMouseEnter(Action onMouseEnter)
        {
            m_OnMouseEnter = onMouseEnter;
        }
        public void SetMouseExit(Action onMouseExit)
        {
            m_OnMouseExit = onMouseExit;
        }
        public void SetOnDestroy(Action onDestroy)
        {
            m_OnDestroy = onDestroy;
        }

        public void SetOnHide(Action onHide)
        {
            m_OnHide = onHide;
        }

        public void SetOnShow(Action onShow)
        {
            m_OnShow = onShow;
        }

        private void OnDestroy()
        {
            if (!GameApp.IsAppRunning()) return;
            m_OnDestroy?.Invoke();
        }

        private void OnDisable()
        {
            if (!GameApp.IsAppRunning()) return;
            m_OnHide?.Invoke();
        }

        private void OnEnable()
        {
            if (!GameApp.IsAppRunning()) return;
            m_OnShow?.Invoke();
        }
        private void OnMouseDown()
        {
            m_OnMouseDown?.Invoke();
        }
        private void OnMouseUp()
        {
            m_OnMouseUp?.Invoke();
        }
        private void OnMouseEnter()
        {
            m_OnMouseEnter?.Invoke();
        }
        private void OnMouseExit()
        {
            m_OnMouseExit?.Invoke();
        }

    }
}
