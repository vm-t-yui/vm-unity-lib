﻿using UnityEngine;
using System.Collections;

namespace VMUnityLib {
	public enum PlayDirection {
		Reverse = -1,
		Toggle = 0,
		Forward = 1
	}

	public enum Trigger {
		OnPointerEnter,
		OnPointerDown,
		OnPointerClick,
		OnPointerUp,
		OnPointerExit,
	}
}