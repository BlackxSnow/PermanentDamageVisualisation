using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Utility
{
    public static class Vector
    {
        public static float Max(this Vector3 v)
        {
            return Mathf.Max(v.x, v.y, v.z);
        }

        public static Vector4 ToVector4(this Vector3 v)
        {
            return new Vector4(v.x, v.y, v.z, 1);
        }
    }
}
