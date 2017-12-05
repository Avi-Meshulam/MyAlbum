using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyAlbum
{
    public interface IVariableDisplayProperty
    {
        PropertyInfo VariableDisplayPropertyInfo { get; }
    }
}
