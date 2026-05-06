using UnityEngine;

namespace Metroma.Utils
{
    public enum Direction
    {
        NONE = 0,
        UP = 1 << 0,
        DOWN = 1 << 1,
        LEFT = 1 << 2,
        RIGHT = 1 << 3,
        FORWARD = 1 << 4,
        BACKWARD = 1 << 5
    }
}
