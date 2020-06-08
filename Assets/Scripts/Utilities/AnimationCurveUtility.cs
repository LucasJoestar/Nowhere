using UnityEngine;

namespace Nowhere
{
    public static class AnimationCurveUtility 
    {
        #region Methods
        /// <summary>
        /// Increase a value following an animation curve
        /// according to a given timer.
        /// </summary>
        /// <param name="_curve">Refrence curve to evaluate.</param>
        /// <param name="_value">Value to increase.</param>
        /// <param name="_time">Animation curve reference time.</param>
        public static float IncreaseValue(AnimationCurve _curve, float _value, ref float _time)
        {
            if (_value < _curve[_curve.length - 1].value)
            {
                _value = _curve.Evaluate(_time);
                _time = Mathf.Min(_time + Time.deltaTime, _curve[_curve.length - 1].time);
            }

            return _value;
        }

        /// <summary>
        /// Increase a value following an animation curve
        /// according to a given timer.
        /// </summary>
        /// <param name="_curve">Refrence curve to evaluate.</param>
        /// <param name="_value">Value to increase.</param>
        /// <param name="_time">Animation curve reference time.</param>
        /// <param name="_increaseCoef">Coefficient applied to time increase.</param>
        public static float IncreaseValue(AnimationCurve _curve, float _value, ref float _time, float _increaseCoef)
        {
            if (_value < _curve[_curve.length - 1].value)
            {
                _value = _curve.Evaluate(_time);
                _time = Mathf.Min(_time + (Time.deltaTime * _increaseCoef), _curve[_curve.length - 1].time);
            }

            return _value;
        }
        #endregion
    }
}
