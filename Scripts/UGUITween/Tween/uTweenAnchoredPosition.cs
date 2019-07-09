using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace VMUnityLib {
	[AddComponentMenu("uTools/Tween/Tween Anchored Position(uTools)")]
	public class uTweenAnchoredPosition : uTweener {
		
		public Vector2 from;
		public Vector2 to;
		
		RectTransform mRectTransform;
		
		public RectTransform cachedRectTransform { get { if (mRectTransform == null) mRectTransform = GetComponent<RectTransform>(); return mRectTransform;}}
		public Vector2 value {
			get { return cachedRectTransform.anchoredPosition;}
            set { cachedRectTransform.anchoredPosition = value; }
		}
		
		protected override void OnUpdate (float factor, bool isFinished)
		{
			value = from + factor * (to - from);
		}

        public static uTweenAnchoredPosition Begin (GameObject go, Vector2 from, Vector2 to, float duration = 1f, float delay = 0f)
        {
            uTweenAnchoredPosition comp = uTweener.Begin<uTweenAnchoredPosition> (go, duration);
			comp.from = from;
			comp.to = to;
			comp.duration = duration;
			comp.delay = delay;
			if (duration <= 0) {
				comp.Sample(1, true);
				comp.enabled = false;
			}
			return comp;
		}

		[ContextMenu("Set 'From' to current value")]
		public override void SetStartToCurrentValue () { from = value; }
		
		[ContextMenu("Set 'To' to current value")]
		public override void SetEndToCurrentValue () { to = value; }
		
		[ContextMenu("Assume value of 'From'")]
		public override void SetCurrentValueToStart () { value = from; }
		
		[ContextMenu("Assume value of 'To'")]
		public override void SetCurrentValueToEnd () { value = to; }

	}
}
