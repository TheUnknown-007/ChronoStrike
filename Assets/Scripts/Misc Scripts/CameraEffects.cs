using UnityEngine;

namespace DitzeGames.Effects
{
    public class CameraEffects : MonoBehaviour
    {
        public Vector3 Amount = new Vector3(1f, 1f, 0);
        public float Duration = 1;
        public float Speed = 10;
        public AnimationCurve Curve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        public bool DeltaMovement = true;

        protected Camera Camera;
        protected float time = 0;
        protected Vector3 lastPos;
        protected Vector3 nextPos;
        protected float lastFoV;
        protected float nextFoV;
        protected bool destroyAfterPlay;

        private void Awake()
        {
            Camera = GetComponent<Camera>();
        }

        public static void ShakeOnce(float duration = 1f, float speed = 10f, Vector3? amount = null, Camera camera = null, bool deltaMovement = true, AnimationCurve curve = null)
        {
            var instance = ((camera != null) ? camera : Camera.main).gameObject.AddComponent<CameraEffects>();
            instance.Duration = duration;
            instance.Speed = speed;
            if (amount != null)
                instance.Amount = (Vector3)amount;
            if (curve != null)
                instance.Curve = curve;
            instance.DeltaMovement = deltaMovement;

            instance.destroyAfterPlay = true;
            instance.Shake();
        }

        public void Shake()
        {
            ResetCam();
            time = Duration;
        }

        private void LateUpdate()
        {
            if (time > 0)
            {
                time -= Time.deltaTime;
                if (time > 0)
                {
                    nextPos = (Mathf.PerlinNoise(time * Speed, time * Speed * 2) - 0.5f) * Amount.x * transform.right * Curve.Evaluate(1f - time / Duration) +
                              (Mathf.PerlinNoise(time * Speed * 2, time * Speed) - 0.5f) * Amount.y * transform.up * Curve.Evaluate(1f - time / Duration);
                    nextFoV = (Mathf.PerlinNoise(time * Speed * 2, time * Speed * 2) - 0.5f) * Amount.z * Curve.Evaluate(1f - time / Duration);

                    Camera.fieldOfView += (nextFoV - lastFoV);
                    Camera.transform.Translate(DeltaMovement ? (nextPos - lastPos) : nextPos);

                    lastPos = nextPos;
                    lastFoV = nextFoV;
                }
                else
                {
                    ResetCam();
                    if (destroyAfterPlay)
                        Destroy(this);
                }
            }
        }

        private void ResetCam()
        {
            Camera.transform.Translate(DeltaMovement ? -lastPos : Vector3.zero);
            Camera.fieldOfView -= lastFoV;

            lastPos = nextPos = Vector3.zero;
            lastFoV = nextFoV = 0f;
        }
    }

}