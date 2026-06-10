using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace Metroma.UI.Effects
{
    [RequireComponent(typeof(RectTransform))]
    public class UIRippleEffect : MonoBehaviour, IPointerDownHandler
    {

        // --- Settings ---
        
        [Header("Global Settings")]
        
        [SerializeField] 
        private Color RippleColor = new Color(1f, 1f, 1f, 0.5f);
        
        [SerializeField] 
        private float Speed = 2f;
        
        [SerializeField] 
        private AnimationCurve AlphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Circle Ripple (Background)")]
        
        [SerializeField] 
        private float CircleMaxScale = 4f;
        
        [SerializeField] 
        private float CircleStartSize = 100f;

        [Header("Shape Ripple (Buttons/Elements)")]
        
        [SerializeField] 
        private float ShapeMaxScale = 1.3f;


        // --- Static Pooling ---
        
        private class RippleInstance
        {
            public GameObject Obj;
            public RectTransform RectTrans;
            public Image RippleImage;
        }

        private static Queue<RippleInstance> RipplePool = new Queue<RippleInstance>();
        
        private static Transform PoolContainer = null;
        
        private static Sprite CircleSpriteCache = null;

        // --- Instance Tracking ---
        
        private List<RippleInstance> ActiveRipples = new List<RippleInstance>();


        // --- Lifecycle ---

        private void Awake()
        {
            if (CircleSpriteCache == null)
                CircleSpriteCache = GenerateCircleSprite(128);
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            RipplePool.Clear();
            PoolContainer = null;
            CircleSpriteCache = null;
        }
#endif

        private void OnDisable()
        {
            foreach (RippleInstance inst in ActiveRipples)
            {
                if (inst.Obj != null)
                {
                    inst.Obj.SetActive(false);
                    RipplePool.Enqueue(inst);
                }
            }
            ActiveRipples.Clear();
        }

        // --- Input Handling ---

        public void OnPointerDown(PointerEventData eventData)
        {
            GameObject ClickedObj = eventData.pointerCurrentRaycast.gameObject;
            Selectable ClickedSelectable = ClickedObj != null ? ClickedObj.GetComponentInParent<Selectable>() : null;

            if (ClickedSelectable != null)
            {
                // Clic sur un élément interactif -> Effet de forme

                Image TargetImage = ClickedSelectable.GetComponent<Image>();
                Sprite TargetSprite = (TargetImage != null && TargetImage.sprite != null) ? TargetImage.sprite : CircleSpriteCache;
                RectTransform TargetRect = ClickedSelectable.GetComponent<RectTransform>();

                Vector2 StartSize = TargetRect.rect.size;
                Vector3 SpawnPos = transform.InverseTransformPoint(TargetRect.position);

                SpawnRipple((Vector2)SpawnPos, StartSize, TargetSprite, true, TargetImage != null ? TargetImage.type : Image.Type.Simple);
            }
            else
            {
                // Clic sur un fond vide -> Effet circulaire
                Vector2 LocalPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform)transform, 
                    eventData.position, 
                    eventData.pressEventCamera, 
                    out LocalPoint
                );

                SpawnRipple(LocalPoint, new Vector2(CircleStartSize, CircleStartSize), CircleSpriteCache, false, Image.Type.Simple);
            }
        }


        // --- Pooling Logic ---

        private static void EnsurePoolContainer()
        {
            if (PoolContainer == null)
            {
                GameObject container = new GameObject("[UI_Ripple_Pool]");
                DontDestroyOnLoad(container);
                PoolContainer = container.transform;
            }
        }


        private RippleInstance GetRipple()
        {
            EnsurePoolContainer();

            while (RipplePool.Count > 0 && RipplePool.Peek().Obj == null)
                RipplePool.Dequeue();

            if (RipplePool.Count > 0)
            {
                RippleInstance inst = RipplePool.Dequeue();
                inst.Obj.SetActive(true);
                inst.Obj.transform.SetParent(transform, false);
                inst.Obj.transform.SetAsLastSibling();
                return inst;
            }
            else
            {
                RippleInstance inst = new RippleInstance();
                inst.Obj = new GameObject("Ripple");
                inst.Obj.transform.SetParent(transform, false);
                inst.RectTrans = inst.Obj.AddComponent<RectTransform>();
                inst.RippleImage = inst.Obj.AddComponent<Image>();
                inst.RippleImage.raycastTarget = false;
                return inst;
            }
        }


        private void ReturnRipple(RippleInstance inst)
        {
            if (inst.Obj != null)
            {
                EnsurePoolContainer();
                inst.Obj.SetActive(false);
                inst.Obj.transform.SetParent(PoolContainer, false); 
                RipplePool.Enqueue(inst);
            }
        }


        // --- Core Logic ---

        private void SpawnRipple(Vector2 LocalPosition, Vector2 StartSize, Sprite RippleSprite, bool IsShapeRipple, Image.Type ImageType)
        {
            RippleInstance inst = GetRipple();
            ActiveRipples.Add(inst);

            inst.RectTrans.anchoredPosition = LocalPosition;
            inst.RectTrans.sizeDelta = StartSize;

            inst.RippleImage.sprite = RippleSprite;
            inst.RippleImage.color = RippleColor;
            inst.RippleImage.type = ImageType; 
            
            StartCoroutine(RippleRoutine(inst, IsShapeRipple));
        }


        private IEnumerator RippleRoutine(RippleInstance inst, bool IsShapeRipple)
        {
            float Elapsed = 0f;
            float Duration = 1f / Speed;
            Color StartColor = inst.RippleImage.color;
            
            Vector3 StartScale = IsShapeRipple ? Vector3.one : Vector3.zero;
            Vector3 EndScale = IsShapeRipple ? Vector3.one * ShapeMaxScale : Vector3.one * CircleMaxScale;

            while (Elapsed < Duration)
            {
                Elapsed += Time.unscaledDeltaTime;
                float t = Elapsed / Duration;

                inst.RectTrans.localScale = Vector3.LerpUnclamped(StartScale, EndScale, t);
                
                Color NewColor = StartColor;
                NewColor.a = StartColor.a * AlphaCurve.Evaluate(t);
                inst.RippleImage.color = NewColor;

                yield return null;
            }

            ActiveRipples.Remove(inst);
            ReturnRipple(inst);
        }


        // --- Utility ---

        private Sprite GenerateCircleSprite(int Resolution)
        {
            Texture2D Tex = new Texture2D(Resolution, Resolution, TextureFormat.RGBA32, false);
            float Radius = Resolution / 2f;
            Vector2 Center = new Vector2(Radius, Radius);

            for (int y = 0; y < Resolution; y++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    float Dist = Vector2.Distance(new Vector2(x, y), Center);
                    float Alpha = Mathf.Clamp01(Radius - Dist); 
                    Tex.SetPixel(x, y, new Color(1f, 1f, 1f, Alpha));
                }
            }
            
            Tex.Apply();
            return Sprite.Create(Tex, new Rect(0, 0, Resolution, Resolution), new Vector2(0.5f, 0.5f));
        }
    }
}
