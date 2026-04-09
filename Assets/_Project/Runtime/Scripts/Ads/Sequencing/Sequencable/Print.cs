using UnityEngine;

namespace Metroma
{
    public class Print : Sequencable
    {
        [SerializeField] private string _text;
        
        public override async Awaitable ExecuteAsync() {
            Debug.Log(_text);
        }
    }
}
