using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace VMUnityLib {
	[AddComponentMenu("uTools/Tween/Tween Alpha(uTools)")]
	public class uTweenAlpha : uTweenValue {

		public bool includeChilds = false;
        public bool overrideOriginalAlpha = true;

        float mAlpha = 0f;

        Dictionary<int, Color> orgColorDic = new Dictionary<int, Color> ();

		public float alpha {
			get {
				return mAlpha;
			}
			set {
				SetAlpha(transform, value);
				mAlpha = value;
			}
		}

		protected override void ValueUpdate (float value, bool isFinished)
		{
			alpha = value;
		}

        public static uTweenAlpha Begin (GameObject go, float duration, float delay, float from, float to)
        {
            uTweenAlpha comp = uTweener.Begin<uTweenAlpha> (go, duration);
            comp.from = from;
            comp.to = to;
            comp.delay = delay;

            if (duration <= 0) {
                comp.Sample (1, true);
                comp.enabled = false;
            }
            return comp;
        }

		void SetAlpha(Transform _transform, float _alpha) {
			var canvasGroup = _transform.GetComponent<CanvasGroup> ();
            if (canvasGroup != null) {
                canvasGroup.alpha = _alpha;
                return;
            }
            Color c = Color.white;
            int key = _transform.gameObject.GetInstanceID();
			var text = _transform.GetComponent<Text> ();
			if (text != null){
                if (orgColorDic.ContainsKey (key) == false) {
                    orgColorDic[key] = text.color;
                }
				c = text.color;
                c.a = overrideOriginalAlpha ? _alpha : _alpha * orgColorDic[key].a;
				text.color = c;
			}
			var light = _transform.GetComponent<Light>();
			if (light != null){
                if (orgColorDic.ContainsKey (key) == false) {
                    orgColorDic[key] = light.color;
                }
				c = light.color;
                c.a = overrideOriginalAlpha ? _alpha : _alpha * orgColorDic[key].a;
				light.color = c;
			}
			var image = _transform.GetComponent<Image> ();
			if (image != null) {
                if (orgColorDic.ContainsKey (key) == false) {
                    orgColorDic[key] = image.color;
                }
				c = image.color;
                c.a = overrideOriginalAlpha ? _alpha : _alpha * orgColorDic[key].a;
				image.color = c;
			}
            var rawImage = _transform.GetComponent<RawImage> ();
            if (rawImage != null) {
                if (orgColorDic.ContainsKey (key) == false) {
                    orgColorDic[key] = rawImage.color;
                }
                c = rawImage.color;
                c.a = overrideOriginalAlpha ? _alpha : _alpha * orgColorDic[key].a;
                rawImage.color = c;
            }
			var spriteRender = _transform.GetComponent<SpriteRenderer> ();
			if (spriteRender != null) {
                if (orgColorDic.ContainsKey (key) == false) {
                    orgColorDic[key] = spriteRender.color;
                }
				c = spriteRender.color;
                c.a = overrideOriginalAlpha ? _alpha : _alpha * orgColorDic[key].a;
				spriteRender.color = c;
			}
			if (_transform.GetComponent<Renderer>() != null) {
				var mat = _transform.GetComponent<Renderer>().material;
				if (mat != null) {
                    if (orgColorDic.ContainsKey (key) == false) {
                        orgColorDic[key] = mat.color;
                    }
					c = mat.color;
                    c.a = overrideOriginalAlpha ? _alpha : _alpha * orgColorDic[key].a;
					mat.color = c;
				}
			}
			if (includeChilds) {
				for (int i = 0; i < _transform.childCount; ++i) {
					Transform child = _transform.GetChild(i);
					SetAlpha(child, _alpha);
				}
			}
		}
	}
}