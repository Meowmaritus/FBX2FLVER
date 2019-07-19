using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBX2FLVER
{
    public static class ExtensionMethods
    {
        public static System.Numerics.Vector3 ToNumerics(this Microsoft.Xna.Framework.Vector3 v)
        {
            return new System.Numerics.Vector3(v.X, v.Y, v.Z);
        }
    }
}
