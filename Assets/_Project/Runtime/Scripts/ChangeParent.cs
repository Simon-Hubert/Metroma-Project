using UnityEngine;

namespace Metroma
{
    public class ChangeParent : MonoBehaviour
    {
        [SerializeField] public Transform _parent;
        [SerializeField] public Transform _child;
        
        public void Change() {
            if (!_child) Debug.LogWarning("ChangeParent : _child is not set", this);
            else {
                _child.SetParent(_parent);
            }
        }

        public void SetNoParent() {
            if (!_child) Debug.LogWarning("ChangeParent : _child is not set", this);
            else {
                _child.SetParent(null);
            }
        }
    }
}
