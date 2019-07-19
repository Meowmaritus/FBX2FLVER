using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBX2FLVER
{
    public class FBX2FLVERGenericEventArgs<T> : EventArgs
    {
        public T Parameter;
        public FBX2FLVERGenericEventArgs(T Parameter)
        {
            this.Parameter = Parameter;
        }
    }
}
