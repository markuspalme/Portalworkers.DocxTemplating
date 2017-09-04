using System.Xml.Linq;

namespace Portalworkers.DocxTemplating
{
    internal static class DocxConstants
    {
        public static XNamespace Namespace =
            "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        public static XName Body = Namespace + "body";
        public static XName Footer = Namespace + "ftr";
        public static XName Header = Namespace + "hdr";
        public static XName Sdt = Namespace + "sdt";
        public static XName SdtPr = Namespace + "sdtPr";
        public static XName Tag = Namespace + "tag";
        public static XName Val = Namespace + "val";
        public static XName SdtContent = Namespace + "sdtContent";
        public static XName Tbl = Namespace + "tbl";
        public static XName Tr = Namespace + "tr";
        public static XName Tc = Namespace + "tc";
        public static XName P = Namespace + "p";
        public static XName R = Namespace + "r";
        public static XName T = Namespace + "t";
        public static XName RPr = Namespace + "rPr";
        public static XName Highlight = Namespace + "highlight";
        public static XName PPr = Namespace + "pPr";
        public static XName Color = Namespace + "color";
        public static XName Sz = Namespace + "sz";
        public static XName SzCs = Namespace + "szCs";
    }
}
