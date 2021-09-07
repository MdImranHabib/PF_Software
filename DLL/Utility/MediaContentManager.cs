using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.Utility
{
    public class MediaContentManager
    {
        public static string FileType(string fileExtension)
        {
            switch (fileExtension.ToLower())
            {

                case ".jpeg":
                case ".jpg":
                case ".png":
                case ".gif":
                case ".bmp":
                    return "IMAGE";
                case ".docx":
                case ".doc":
                    return "DOC";
                case ".pdf":
                    return "PDF";
                default:
                    return "";
            }
        }
    }
}