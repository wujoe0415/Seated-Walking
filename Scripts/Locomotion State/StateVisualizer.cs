
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace LocomotionStateMachine
{
    public class StateVisualizer : MonoBehaviour
    {
        [System.Serializable]
        public class VisualizeData {
            public string LocomotionStateName;
            public RawImage Icon;
        }

        public List<VisualizeData> Icons = new List<VisualizeData>();
        public float FadeDuration = 1.0f;
        public float StayDuration = 2.0f;

        private RawImage _currentIcon;
        private IEnumerator _coroutine;

        private void OnEnable()
        {
            HideAll();
        }
        private void OnDisable()
        {
            HideAll();
        }
        public void ShowIcon(string stataName)
        {
            _currentIcon = Icons.Find(x => x.LocomotionStateName == stataName).Icon;
            _currentIcon.gameObject.SetActive(true);
            if(_coroutine != null)
                StopCoroutine(_coroutine);
            _coroutine = Show();
            StartCoroutine(_coroutine);
        }
        public void HideIcon()
        {
            if(_coroutine != null)
                StopCoroutine(_coroutine);
            _coroutine = null;
            if (_currentIcon == null)
                return;
            Color color = _currentIcon.GetComponent<RawImage>().color;
            _currentIcon.GetComponent<RawImage>().color = new Color(color.r, color.b, color.g, 1);
            _currentIcon.gameObject.SetActive(false);
        }
        private IEnumerator Show()
        {
            RawImage rawImage = _currentIcon.GetComponent<RawImage>();
            Color initColor = rawImage.color;
            Color targetColor = new Color(initColor.r, initColor.g, initColor.b, 0);

            for (float i = 0; i < FadeDuration; i += Time.deltaTime)
            {
                rawImage.color = Color.Lerp(initColor, targetColor, i);
                yield return null;
            }
            rawImage.color = targetColor;
            yield return new WaitForSeconds(StayDuration);
            for (float i = 0; i < FadeDuration; i += Time.deltaTime)
            {
                rawImage.color = Color.Lerp(targetColor, initColor, i);
                yield return null;
            }
        }

        private void HideAll()
        {
            for (int i = 0; i < Icons.Count; i++)
            {
                Color color = Icons[i].Icon.GetComponent<RawImage>().color;
                Icons[i].Icon.color = new Color(color.r, color.b, color.g, 1);
                Icons[i].Icon.gameObject.SetActive(false);
            }
        }
        private void OnValidate()
        {
            if (FindObjectOfType<LocomotionParser>() == null)
            {
                Icons.Clear();
                return;
            }
            HashSet<string> existingKeys = new HashSet<string>();
            foreach (var locomotionKey in Icons)
                existingKeys.Add(locomotionKey.LocomotionStateName);
            foreach (LocomotionParser.StateMap state in FindObjectOfType<LocomotionParser>().StateMaps)
            {
                if(existingKeys.Contains(state.key))
                    continue;
                Icons.Add(new VisualizeData
                {
                    LocomotionStateName = state.key,
                    Icon = null
                });
            }
        }
    }
}
