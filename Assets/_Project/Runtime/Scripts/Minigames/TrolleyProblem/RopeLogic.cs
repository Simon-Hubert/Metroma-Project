using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;
using Shapes;

namespace Metroma
{
    public class RopeLogic : MonoBehaviour
    {
        [SerializeField] private GameObject _ropePartsParent;
        
        [Header("Trolley Choice")]
        [SerializeField] public TrolleyChoiceLogic choice;
        [SerializeField] private Vector3 _choiceOffset;
        [SerializeField] private Vector3 _choiceHookingOffset;

        [Header("Hook part")]
        [SerializeField] private GameObject _hook;
        [SerializeField] private Vector3 _hookOffset;
        [SerializeField, HideInInspector] private Transform _hookTransform;
        [SerializeField, ReadOnly] private Rigidbody2D _hookRB2D;
        [Space(7)]
        [SerializeField] private Vector2 _respawnForce = Vector2.zero; 
        [SerializeField] private bool _randomizeRespawnForce = true;
        [SerializeField] private Vector2 _respawnMaxForce = Vector2.zero;
        
        public Rigidbody2D GetHookingPart => _hookRB2D;
        
        [Header("Rope Prefab")]
        [SerializeField] private GameObject _ropePartPrefab;
        [SerializeField, Min(0)] private int _nbOfParts;
        [SerializeField, ReadOnly] private List<GameObject> _ropePartList = new List<GameObject>();
        [SerializeField, HideInInspector] private List<Rigidbody2D> _ropePartRB2DList = new List<Rigidbody2D>();
        [SerializeField, HideInInspector] private List<SpringJoint2D> _ropePartSJ2DList = new List<SpringJoint2D>();
        
        [Header("Parts RigidBody Options")]
        [SerializeField, Min(0)] private float _rbMass = 1f;
        [SerializeField, Min(0)] private float _rbLinearDamping = 0f;
        [SerializeField, Min(0)] private float _rbAngularDamping = 0.05f;
        [SerializeField, Min(0)] private float _rbGravityScale = 1f;
        [Space(7)]
        [SerializeField, Min(0)] private float _rbRopeEndMass = 1f;
        [SerializeField, Min(0)] private float _rbRopeEndGravityScale = 1f;
        
        [Header("Parts SpringJoints Oprions")]
        [SerializeField, Min(0)] private float _sjDampingRatio = 0f;
        [SerializeField, Min(0)] private float _sjFrequency = 3f;
        [SerializeField] private JointBreakAction2D _sjBreakAction = JointBreakAction2D.Disable;

        [Header("Parts Visual Options")] 
        [SerializeField] private Color _vColor = Color.saddleBrown;
        [SerializeField] private Rectangle.RectangleType _vRectangleType = Rectangle.RectangleType.RoundedSolid;
        [SerializeField, Min(0)] private Vector2 _vSize = new Vector2(0.15f, 1f);
        [SerializeField] private float _vVerticalOffset = -0.1f;
        [SerializeField, Min(0)] private float _vCornerRadius = 0.05f;
        
        private float GetVOffset => _vSize.y + _vVerticalOffset;
        private float GetDemiVOffset => (_vSize.y + _vVerticalOffset)/2;
        
        
        [Button]
        public bool GenerateRope() {
            ClearRopeParts();

            if (_ropePartsParent == null || _ropePartPrefab == null) {
                return false;
            }
            
            Transform lastPart = _ropePartsParent.transform;
            Rigidbody2D lastRB2D = lastPart.GetComponent<Rigidbody2D>();
            lastRB2D.simulated = true;
            
            #region Creating Rope Parts
            if (_nbOfParts > 0) {
                for (int i = 0; i < _nbOfParts; i++) {
                    GameObject newPart = Instantiate(_ropePartPrefab, _ropePartsParent.transform);
                    _ropePartList.Add(newPart);

                    newPart.name = $"[{i}] RopePart";
                    newPart.transform.localPosition = new Vector3(0, -GetDemiVOffset - GetVOffset * i, 0);

                    if (!newPart.TryGetComponent<Rigidbody2D>(out Rigidbody2D newPartRB2D)) {
                        Debug.LogError($"{name}'s RopeLogic : cannot find Rigidbody2D in prefab", this);
                        ClearRopeParts();
                        return false;
                    }
                    RigidBodyDefaultValues(ref newPartRB2D);
                    _ropePartRB2DList.Add(newPartRB2D);
                    
                    if (!newPart.TryGetComponent<SpringJoint2D>(out SpringJoint2D newPartSJ2D)) {
                        Debug.LogError($"{name}'s RopeLogic : cannot find SpringJoint2D in prefab", this);
                        ClearRopeParts();
                        return false;
                    }
                    newPartSJ2D.connectedBody = lastRB2D;
                    SpringJointsDefaultValues(ref newPartSJ2D);
                    _ropePartSJ2DList.Add(newPartSJ2D);
                    
                    if (!newPart.TryGetComponent<Rectangle>(out Rectangle newPartVisual)) {
                        Debug.LogError($"{name}'s RopeLogic : cannot find Rigidbody2D in prefab", this);
                        ClearRopeParts();
                        return false;
                    }
                    VisualDefaultValues(ref newPartVisual);

                    lastPart = newPart.transform;
                    lastRB2D = newPartRB2D;
                }
            }
            #endregion

            #region Hook
            if (_hook != null) {
                _hook.transform.parent = lastPart;
                
                _hookTransform = _hook.transform;
                _hookTransform.localPosition = new Vector3(0, -GetDemiVOffset, 0) + _hookOffset;
            }
            else {
                Debug.LogWarning($"{name}'s RopeLogic : No Hook was referenced, taking last rope part as hook", this);
                
                _hookTransform = lastPart;
                _hookTransform.localPosition = new Vector3(0, -GetDemiVOffset - GetVOffset * (_nbOfParts-1), 0);
            }
            
            _hookRB2D = lastRB2D;
            _hookRB2D.simulated = true;
            _hookRB2D.mass = _rbRopeEndMass;
            _hookRB2D.gravityScale = _rbRopeEndGravityScale;
            #endregion
            
            #region Choice
            if (choice) {
                choice.SetHingeConnectedRB2D = _hookRB2D;
                ChoiceDefaultValues();
            }
            else Debug.LogWarning($"{name}'s RopeLogic : No Choice was referenced", this);
            #endregion
            
            Debug.Log($"{name}'s RopeLogic : Rope was successfully generated", this);
            return true;
        }

        [Button]
        private void ClearRopeParts() {
            if (_hookTransform != null || _ropePartsParent.transform == _hookTransform) {
                _hook.transform.parent = transform;
                _hookTransform.parent = transform;
                _hookRB2D.simulated = false;
            }

            if (choice) {
                choice.SetHingeConnectedRB2D = null;
            }
            
            foreach (GameObject ropePart in _ropePartList) {
                DestroyImmediate(ropePart.gameObject);
            }
            
            _ropePartList.Clear();
            _ropePartRB2DList.Clear();
            _ropePartSJ2DList.Clear();
        }

        private void RigidBodyDefaultValues(ref Rigidbody2D rb2D) {
            rb2D.mass = _rbMass;
            rb2D.linearDamping = _rbLinearDamping;
            rb2D.angularDamping = _rbAngularDamping;
            rb2D.gravityScale = _rbGravityScale;
        }
        private void SpringJointsDefaultValues(ref SpringJoint2D sj2D) {
            sj2D.dampingRatio = _sjDampingRatio;
            sj2D.frequency = _sjFrequency;
            sj2D.breakAction = _sjBreakAction;
            sj2D.anchor = new Vector2(0, GetDemiVOffset);
            if (sj2D.connectedBody.transform != _ropePartsParent.transform) sj2D.connectedAnchor = new Vector2(0, -GetDemiVOffset);
        }
        private void VisualDefaultValues(ref Rectangle visual) {
            visual.Color = _vColor;
            visual.Width = _vSize.x;
            visual.Height = _vSize.y;
            visual.CornerRadius = _vCornerRadius;
        }
        private void ChoiceDefaultValues()
        {
            choice.transform.position = _hookTransform.position + _choiceOffset;
            choice.SetHingeAnchorOffset = _choiceHookingOffset;
            choice.SetHingeEnabled = true;
        }
        
        
        [Button]
        public void ResetRopePosition() {
            _ropePartsParent.transform.localPosition = Vector3.zero;
            
            // Rigidbody2D rb2D = _ropePartsParent.GetComponent<Rigidbody2D>();
            // rb2D.linearVelocity = Vector2.zero;
            // rb2D.angularVelocity = 0;
            
            for (int i = 0; i < _ropePartList.Count; i++) {
                _ropePartList[i].transform.localPosition = new Vector3(0, -GetDemiVOffset - GetVOffset * i, 0);
                _ropePartList[i].transform.localRotation = Quaternion.Euler(0, 0, 0);
                
                _ropePartRB2DList[i].linearVelocity = Vector2.zero;
                _ropePartRB2DList[i].angularVelocity = 0;
                _ropePartRB2DList[i].mass = _rbMass;
                _ropePartRB2DList[i].gravityScale = _rbGravityScale;

                _ropePartSJ2DList[i].enabled = true;
            }
            
            _hookRB2D.mass = _rbRopeEndMass;
            _hookRB2D.gravityScale = _rbRopeEndGravityScale;
            
            ChoiceDefaultValues();
            if (_randomizeRespawnForce) choice.ResetPhysicsValues(_respawnForce, _respawnMaxForce);
            else choice.ResetPhysicsValues(_respawnForce);
        }
        [Button]
        public void CutRopeAtEnd() => CutRopeAtIndex(_ropePartList.Count);
        public void CutRopeAtIndex(int index) {
            if (index < 0 || index > _ropePartList.Count) return;

            if (index == _ropePartList.Count) {
                choice.SetHingeEnabled = false;
            }
            else {
                if (_ropePartSJ2DList[index].enabled == false) return;
                _ropePartSJ2DList[index].enabled = false;
                
                _ropePartRB2DList[index].mass = _rbRopeEndMass;
                _ropePartRB2DList[index].gravityScale = _rbRopeEndGravityScale;
            }
            
            if (index > 0) {
                _ropePartRB2DList[index-1].mass = _rbRopeEndMass;
                _ropePartRB2DList[index-1].gravityScale = _rbRopeEndGravityScale;
            }
        }

        public void CutRopeAtPosition(Vector2 position) {
            int index;
            for (index = 0; index < _ropePartList.Count; index++)
            {
                if (_ropePartList[index].transform.position.y < position.y) {
                    index--;
                    break;
                }
            }
            
            Mathf.Clamp(index, 0, _ropePartList.Count);
            if (index == _ropePartList.Count - 1 && _hookTransform.position.y > position.y) index = _ropePartList.Count;
            
            CutRopeAtIndex(index);
        }
    }
}
