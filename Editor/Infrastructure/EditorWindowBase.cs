using System;
using UnityEditor;
using UnityEngine;

namespace WhReality.Stargazer.Infrastructure
{
    public abstract class EditorWindowBase : EditorWindow
    {
        protected void OnPropertyChanged(ref bool field, bool newValue)
        {
            if (field == newValue) return;

            field = newValue;
            OnPropertyChanged();
        }

        protected void OnPropertyChanged(ref int field, int newValue)
        {
            if (field == newValue) return;

            field = newValue;
            OnPropertyChanged();
        }

        protected void OnPropertyChanged(ref float field, float newValue, float tolerance = 0.1f)
        {
            if (Math.Abs(field - newValue) < tolerance) return;

            field = newValue;
            OnPropertyChanged();
        }

        protected void OnVectorChanged(ref Vector3 field, Vector3 value)
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }

        protected void OnPropertyChanged<T>(ref T field, T newValue) where T : class
        {
            if (field == newValue) return;
            field = newValue;
            OnPropertyChanged();
        }

        protected virtual void OnPropertyChanged()
        {
        }
    }
}