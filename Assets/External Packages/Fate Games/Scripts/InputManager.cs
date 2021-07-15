using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FateGames
{
    public class InputManager : MonoBehaviour
    {
        private static InputManager instance = null;

        private static List<Swerve2D> swerve2Ds = null;
        private static List<Swerve1D> swerve1Ds = null;

        private void Awake()
        {
            if (instance)
            {
                DestroyImmediate(gameObject);
                return;
            }
            instance = this;
        }

        private void Update()
        {
            if (swerve2Ds != null && swerve2Ds.Count > 0)
            {
                for (int i = 0; i < swerve2Ds.Count; i++)
                {
                    CheckSwerve2D(swerve2Ds[i]);
                }
            }
            if (swerve1Ds != null && swerve1Ds.Count > 0)
            {
                for (int i = 0; i < swerve1Ds.Count; i++)
                {
                    CheckSwerve1D(swerve1Ds[i]);
                }
            }

        }

        public static void Clear()
        {
            if (swerve2Ds != null)
                swerve2Ds.Clear();
            if (swerve1Ds != null)
                swerve1Ds.Clear();
        }
        private void CheckSwerve2D(Swerve2D swerve)
        {
            if (swerve != null && swerve.enabled)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    swerve.Reset();
                }
                else if (Input.GetMouseButton(0))
                {
                    Vector2 difference = (Vector2)Input.mousePosition - swerve.Anchor;
                    Vector2 anchor = swerve.Anchor;
                    if (difference.magnitude > swerve.Range)
                    {
                        anchor = (Vector2)Input.mousePosition - difference.normalized * swerve.Range;
                        difference = (Vector2)Input.mousePosition - anchor;
                    }
                    swerve.Anchor = anchor;
                    swerve.Difference = difference;
                    swerve.Rate = Mathf.Clamp(difference.magnitude / swerve.Range, 0, 1);
                }
                else
                {
                    if (Input.GetMouseButtonUp(0) && swerve.OnRelease != null)
                        swerve.OnRelease();
                    swerve.Active = false;
                }
            }

        }

        private void CheckSwerve1D(Swerve1D swerve)
        {
            if (swerve != null && swerve.enabled)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    swerve.Reset();
                }
                else if (Input.GetMouseButton(0))
                {
                    Vector2 mousePosition = Input.mousePosition;
                    Vector2 differenceVector = mousePosition * swerve.Dimension - swerve.Anchor * swerve.Dimension;
                    float differenceSign = ((differenceVector.x * swerve.Dimension.x >= 0) ? 1 : -1) * ((differenceVector.y * swerve.Dimension.y >= 0) ? 1 : -1);
                    float difference = differenceVector.magnitude * differenceSign;
                    float anchor = swerve.Anchor;
                    if (Mathf.Abs(difference) > swerve.Range)
                    {
                        Vector2 anchorVector = mousePosition * swerve.Dimension + swerve.Range * swerve.Dimension * (difference > swerve.Range ? -1 : 1);
                        float anchorSign = ((anchorVector.x * swerve.Dimension.x >= 0) ? 1 : -1) * ((anchorVector.y * swerve.Dimension.y >= 0) ? 1 : -1);
                        anchor = anchorVector.magnitude * anchorSign;
                        differenceVector = mousePosition * swerve.Dimension - anchor * swerve.Dimension;
                        differenceSign = ((differenceVector.x * swerve.Dimension.x >= 0) ? 1 : -1) * ((differenceVector.y * swerve.Dimension.y >= 0) ? 1 : -1);
                        difference = differenceVector.magnitude * differenceSign;
                    }
                    swerve.Anchor = anchor;
                    swerve.Difference = difference;
                    swerve.Rate = Mathf.Clamp(difference / swerve.Range, -1, 1);
                    swerve.OnSwerve?.Invoke();
                }
                else
                {
                    if (Input.GetMouseButtonUp(0) && swerve.OnRelease != null)
                        swerve.OnRelease();
                    swerve.Active = false;
                }
            }
        }

        public static Swerve2D CreateSwerve2D()
        {
            Swerve2D swerve = new Swerve2D();
            if (swerve2Ds == null)
                swerve2Ds = new List<Swerve2D>();
            swerve2Ds.Add(swerve);
            return swerve;
        }

        public static Swerve2D CreateSwerve2D(float range)
        {
            Swerve2D swerve = new Swerve2D
            {
                Range = range
            };
            if (swerve2Ds == null)
                swerve2Ds = new List<Swerve2D>();
            swerve2Ds.Add(swerve);
            return swerve;
        }

        public static Swerve1D CreateSwerve1D(Vector2 dimension)
        {
            Swerve1D swerve = new Swerve1D(dimension);
            if (swerve1Ds == null)
                swerve1Ds = new List<Swerve1D>();
            swerve1Ds.Add(swerve);
            return swerve;
        }

        public static Swerve1D CreateSwerve1D(Vector2 dimension, float range)
        {
            Swerve1D swerve = new Swerve1D(dimension)
            {
                Range = range
            };
            if (swerve1Ds == null)
                swerve1Ds = new List<Swerve1D>();
            swerve1Ds.Add(swerve);
            return swerve;
        }
    }

    public class Swerve2D
    {
        public Vector2 Anchor = Vector2.zero;
        public Vector2 Difference = Vector2.zero;
        public float Rate = 0;
        public bool Active = false;
        public bool enabled = true;
        public float Range = 100;

        public delegate void Callback();

        public Callback OnStart = null;
        public Callback OnSwerve = null;
        public Callback OnRelease = null;

        public void Reset()
        {
            Active = true;
            Anchor = Input.mousePosition;
            Rate = 0;
            Difference = Vector2.zero;
        }
    }

    public class Swerve1D
    {
        public float Anchor = 0;
        public float Difference = 0;
        public float Rate = 0;
        public bool Active = false;
        public bool enabled = true;
        public float Range = 100;
        public Vector2 Dimension { get; } = Vector2.right;

        public delegate void Callback();

        public Callback OnStart = null;
        public Callback OnSwerve = null;
        public Callback OnRelease = null;

        public Swerve1D(Vector2 dimension)
        {
            Dimension = dimension;
        }

        public void Reset()
        {
            Active = true;
            Vector2 mousePosition = Input.mousePosition;
            Vector2 anchorVector = (mousePosition * Dimension);
            float anchorSign = ((anchorVector.x * Dimension.x >= 0) ? 1 : -1) * ((anchorVector.y * Dimension.y >= 0) ? 1 : -1);
            Anchor = anchorVector.magnitude * anchorSign;
            OnStart?.Invoke();
            Rate = 0;
            Difference = 0;
        }
    }
}