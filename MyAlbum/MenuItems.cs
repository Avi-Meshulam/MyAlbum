using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlbum
{
    static class MenuItems
    {
        public static class Main
        {
            public const string Albums = "Albums";
            public const string Photo = "Photo";
            public const string Exit = "Exit";
        }

        public static class Albums
        {
            public const string NewAlbum = "New Album...";
        }

        public static class Album
        {
            public const string AddPhotos = "Add Photos...";
            public const string CapturePhoto = "Capture";
        }

        public static class Photo
        {
            public const string Details = "Details";
            public const string Move = "Move To...";
            public const string Delete = "Delete";
        }
    }
}
