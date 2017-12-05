using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlbum
{
    public enum FileTypes
    {
        Images,
        Music,
        Documents
    }

    public enum ViewType
    {
        GridView,
        FlipView
    }

    public enum CustomContentDialogResult
    {
        None = 0,
        Primary = 1,
        Secondary = 2,
        Cancel = 3
    }
}
