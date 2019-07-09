using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace VMUnityLib {
	[AddComponentMenu("uTools/Tween/Tween Color(uTools)")]
	public class uTweenColor : uTweener {

		public Color from = Color.white;
		public Color to = Color.white;
		public bool includeChilds = false;
        public bool overrideOriginalColor = true;
        
		Color mColor = Color.white;

        Dictionary<int, Color> orgColorDic = new Dictionary<int, Color> ();

		public Color colorValue {
			get {
				return mColor;
			}
			set {
				SetColor(transform, value);
				mColor = value;
			}
		}

		protected override void OnUpdate (float factor, bool isFinished)
		{
			colorValue = Color.Lerp(from, to, factor);
		}

		public static uTweenColor Begin(GameObject go, float duration, float delay, Color from, Color to) {
			uTweenColor comp = uTweener.Begin<uTweenColor>(go, duration);
			comp.from = from;
			comp.to = to;
			comp.delay = delay;
			
			if (duration <=0) {
				comp.Sample(1, true);
				comp.enabled = false;
			}
			return comp;
		}

		void SetColor(Transform _transform, Color _color) {
            int key = _transform.gameObject.GetInstanceID ();
			var text = _transform.GetComponent<Text> ();
			if (text != null){
                if (orgColorDic.ContainsKey (key) == false) {
                    orgColorDic[key] = text.color;
                }
                text.color = overrideOriginalColor ? _color : _color * orgColorDic[key];
			}
			var light = _transform.GetComponent<Light>();
			if (light != null){
                if (orgColorDic.ContainsKey (key) == false) {
                    orgColorDic[key] = light.color;
                }
                light.color = overrideOriginalColor ? _color : _color * orgColorDic[key];
			}
			var image = _transform.GetComponent<Image> ();
			if (image != null) {
                if (orgColorDic.ContainsKey (key) == false) {
                    orgColorDic[key] = image.color;
                }
                image.color = overrideOriginalColor ? _color : _color * orgColorDic[key];
			}
            var rawImage = _transform.GetComponent<RawImage> ();
            if (rawImage != null) {
                if (orgColorDic.ContainsKey (key) == false) {
                    orgColorDic[key] = rawImage.color;
                }
                rawImage.color = overrideOriginalColor ? _color : _color * orgColorDic[key];
            }
			var spriteRender = _transform.GetComponent<SpriteRenderer> ();
			if (spriteRender != null) {
                if (orgColorDic.ContainsKey (key) == false) {
                    orgColorDic[key] = spriteRender.color;
                }
                spriteRender.color = overrideOriginalColor ? _color : _color * orgColorDic[key];
			}
			if (_transform.GetComponent<Renderer>() != null) {
				var mat = _transform.GetComponent<Renderer>().material;
				if (mat != null) {
                    if (orgColorDic.ContainsKey (key) == false) {
                        orgColorDic[key] = mat.color;
                    }
                    mat.color = overrideOriginalColor ? _color : _color * orgColorDic[key];
				}
			}
			if (includeChilds) {
				for (int i = 0; i < _transform.childCount; ++i) {
					Transform child = _transform.GetChild(i);
					SetColor(child, _color);
				}
			}			
		}
	}
}
