using CapyCSS.Script;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace CbVS.Script.Lib
{
    [ScriptClass("Graphics", true, true)]
    public static class Graphics
    {
        public enum BrushColors
        {
            None,
            Black,
            DarkGreen,
            Maroon,
            DarkRed,
            Olive,
            Green,
            Teal,
            DarkCyan,
            Navy,
            DarkBlue,
            Indigo,
            Purple,
            DarkMagenta,
            MidnightBlue,
            DarkSlateGray,
            SaddleBrown,
            DarkOliveGreen,
            OliveDrab,
            ForestGreen,
            DarkTurquoise,
            MediumBlue,
            DarkGoldenrod,
            SeaGreen,
            DarkViolet,
            MediumVioletRed,
            Firebrick,
            LightSeaGreen,
            Brown,
            Sienna,
            DarkSlateBlue,
            DimGray,
            Red,
            OrangeRed,
            DarkOrange,
            Orange,
            Gold,
            Yellow,
            LawnGreen,
            Chartreuse,
            Lime,
            SpringGreen,
            MediumSpringGreen,
            Aqua,
            Cyan,
            DeepSkyBlue,
            Blue,
            Magenta,
            Fuchsia,
            Crimson,
            Chocolate,
            Goldenrod,
            DeepPink,
            BlueViolet,
            Peru,
            YellowGreen,
            LimeGreen,
            DarkOrchid,
            MediumSeaGreen,
            SteelBlue,
            CadetBlue,
            SlateGray,
            Gray,
            GreenYellow,
            DodgerBlue,
            Turquoise,
            RoyalBlue,
            MediumTurquoise,
            LightSlateGray,
            MediumOrchid,
            IndianRed,
            Tomato,
            MediumAquamarine,
            SlateBlue,
            DarkKhaki,
            Coral,
            CornflowerBlue,
            MediumPurple,
            Orchid,
            PaleVioletRed,
            RosyBrown,
            DarkSeaGreen,
            DarkGray,
            LightSalmon,
            HotPink,
            SandyBrown,
            MediumSlateBlue,
            Salmon,
            DarkSalmon,
            BurlyWood,
            Tan,
            Aquamarine,
            PaleGreen,
            LightSkyBlue,
            LightCoral,
            Khaki,
            LightGreen,
            Violet,
            SkyBlue,
            Plum,
            Silver,
            PaleGoldenrod,
            PaleTurquoise,
            PowderBlue,
            LightBlue,
            LightSteelBlue,
            Thistle,
            PeachPuff,
            NavajoWhite,
            Bisque,
            Moccasin,
            LightPink,
            Wheat,
            Gainsboro,
            LightGray,
            MistyRose,
            BlanchedAlmond,
            LemonChiffon,
            Pink,
            PapayaWhip,
            LightYellow,
            AntiqueWhite,
            LightGoldenrodYellow,
            Snow,
            SeaShell,
            FloralWhite,
            Cornsilk,
            Beige,
            Ivory,
            Honeydew,
            LightCyan,
            Azure,
            LavenderBlush,
            Linen,
            Lavender,
            MintCream,
            AliceBlue,
            GhostWhite,
            OldLace,
            WhiteSmoke,
            White,
            Transparent
        }

        public static BrushColors CreateBrushColors(BrushColors colors)
        {
            return colors;
        }

        public static SolidColorBrush CreateBrushes(BrushColors colors)
        {
            switch (colors)
            {
                case BrushColors.None:
                    return null;
                case BrushColors.Black:
                    return Brushes.Black;
                case BrushColors.DarkGreen:
                    return Brushes.DarkGreen;
                case BrushColors.Maroon:
                    return Brushes.Maroon;
                case BrushColors.DarkRed:
                    return Brushes.DarkRed;
                case BrushColors.Olive:
                    return Brushes.Olive;
                case BrushColors.Green:
                    return Brushes.Green;
                case BrushColors.Teal:
                    return Brushes.Teal;
                case BrushColors.DarkCyan:
                    return Brushes.DarkCyan;
                case BrushColors.Navy:
                    return Brushes.Navy;
                case BrushColors.DarkBlue:
                    return Brushes.DarkBlue;
                case BrushColors.Indigo:
                    return Brushes.Indigo;
                case BrushColors.Purple:
                    return Brushes.Purple;
                case BrushColors.DarkMagenta:
                    return Brushes.DarkMagenta;
                case BrushColors.MidnightBlue:
                    return Brushes.MidnightBlue;
                case BrushColors.DarkSlateGray:
                    return Brushes.DarkSlateGray;
                case BrushColors.SaddleBrown:
                    return Brushes.SaddleBrown;
                case BrushColors.DarkOliveGreen:
                    return Brushes.DarkOliveGreen;
                case BrushColors.OliveDrab:
                    return Brushes.OliveDrab;
                case BrushColors.ForestGreen:
                    return Brushes.ForestGreen;
                case BrushColors.DarkTurquoise:
                    return Brushes.DarkTurquoise;
                case BrushColors.MediumBlue:
                    return Brushes.MediumBlue;
                case BrushColors.DarkGoldenrod:
                    return Brushes.DarkGoldenrod;
                case BrushColors.SeaGreen:
                    return Brushes.SeaGreen;
                case BrushColors.DarkViolet:
                    return Brushes.DarkViolet;
                case BrushColors.MediumVioletRed:
                    return Brushes.MediumVioletRed;
                case BrushColors.Firebrick:
                    return Brushes.Firebrick;
                case BrushColors.LightSeaGreen:
                    return Brushes.LightSeaGreen;
                case BrushColors.Brown:
                    return Brushes.Brown;
                case BrushColors.Sienna:
                    return Brushes.Sienna;
                case BrushColors.DarkSlateBlue:
                    return Brushes.DarkSlateBlue;
                case BrushColors.DimGray:
                    return Brushes.DimGray;
                case BrushColors.Red:
                    return Brushes.Red;
                case BrushColors.OrangeRed:
                    return Brushes.OrangeRed;
                case BrushColors.DarkOrange:
                    return Brushes.DarkOrange;
                case BrushColors.Orange:
                    return Brushes.Orange;
                case BrushColors.Gold:
                    return Brushes.Gold;
                case BrushColors.Yellow:
                    return Brushes.Yellow;
                case BrushColors.LawnGreen:
                    return Brushes.LawnGreen;
                case BrushColors.Chartreuse:
                    return Brushes.Chartreuse;
                case BrushColors.Lime:
                    return Brushes.Lime;
                case BrushColors.SpringGreen:
                    return Brushes.SpringGreen;
                case BrushColors.MediumSpringGreen:
                    return Brushes.MediumSpringGreen;
                case BrushColors.Aqua:
                    return Brushes.Aqua;
                case BrushColors.Cyan:
                    return Brushes.Cyan;
                case BrushColors.DeepSkyBlue:
                    return Brushes.DeepSkyBlue;
                case BrushColors.Blue:
                    return Brushes.Blue;
                case BrushColors.Magenta:
                    return Brushes.Magenta;
                case BrushColors.Fuchsia:
                    return Brushes.Fuchsia;
                case BrushColors.Crimson:
                    return Brushes.Crimson;
                case BrushColors.Chocolate:
                    return Brushes.Chocolate;
                case BrushColors.Goldenrod:
                    return Brushes.Goldenrod;
                case BrushColors.DeepPink:
                    return Brushes.DeepPink;
                case BrushColors.BlueViolet:
                    return Brushes.BlueViolet;
                case BrushColors.Peru:
                    return Brushes.Peru;
                case BrushColors.YellowGreen:
                    return Brushes.YellowGreen;
                case BrushColors.LimeGreen:
                    return Brushes.LimeGreen;
                case BrushColors.DarkOrchid:
                    return Brushes.DarkOrchid;
                case BrushColors.MediumSeaGreen:
                    return Brushes.MediumSeaGreen;
                case BrushColors.SteelBlue:
                    return Brushes.SteelBlue;
                case BrushColors.CadetBlue:
                    return Brushes.CadetBlue;
                case BrushColors.SlateGray:
                    return Brushes.SlateGray;
                case BrushColors.Gray:
                    return Brushes.Gray;
                case BrushColors.GreenYellow:
                    return Brushes.GreenYellow;
                case BrushColors.DodgerBlue:
                    return Brushes.DodgerBlue;
                case BrushColors.Turquoise:
                    return Brushes.Turquoise;
                case BrushColors.RoyalBlue:
                    return Brushes.RoyalBlue;
                case BrushColors.MediumTurquoise:
                    return Brushes.MediumTurquoise;
                case BrushColors.LightSlateGray:
                    return Brushes.LightSlateGray;
                case BrushColors.MediumOrchid:
                    return Brushes.MediumOrchid;
                case BrushColors.IndianRed:
                    return Brushes.IndianRed;
                case BrushColors.Tomato:
                    return Brushes.Tomato;
                case BrushColors.MediumAquamarine:
                    return Brushes.MediumAquamarine;
                case BrushColors.SlateBlue:
                    return Brushes.SlateBlue;
                case BrushColors.DarkKhaki:
                    return Brushes.DarkKhaki;
                case BrushColors.Coral:
                    return Brushes.Coral;
                case BrushColors.CornflowerBlue:
                    return Brushes.CornflowerBlue;
                case BrushColors.MediumPurple:
                    return Brushes.MediumPurple;
                case BrushColors.Orchid:
                    return Brushes.Orchid;
                case BrushColors.PaleVioletRed:
                    return Brushes.PaleVioletRed;
                case BrushColors.RosyBrown:
                    return Brushes.RosyBrown;
                case BrushColors.DarkSeaGreen:
                    return Brushes.DarkSeaGreen;
                case BrushColors.DarkGray:
                    return Brushes.DarkGray;
                case BrushColors.LightSalmon:
                    return Brushes.LightSalmon;
                case BrushColors.HotPink:
                    return Brushes.HotPink;
                case BrushColors.SandyBrown:
                    return Brushes.SandyBrown;
                case BrushColors.MediumSlateBlue:
                    return Brushes.MediumSlateBlue;
                case BrushColors.Salmon:
                    return Brushes.Salmon;
                case BrushColors.DarkSalmon:
                    return Brushes.DarkSalmon;
                case BrushColors.BurlyWood:
                    return Brushes.BurlyWood;
                case BrushColors.Tan:
                    return Brushes.Tan;
                case BrushColors.Aquamarine:
                    return Brushes.Aquamarine;
                case BrushColors.PaleGreen:
                    return Brushes.PaleGreen;
                case BrushColors.LightSkyBlue:
                    return Brushes.LightSkyBlue;
                case BrushColors.LightCoral:
                    return Brushes.LightCoral;
                case BrushColors.Khaki:
                    return Brushes.Khaki;
                case BrushColors.LightGreen:
                    return Brushes.LightGreen;
                case BrushColors.Violet:
                    return Brushes.Violet;
                case BrushColors.SkyBlue:
                    return Brushes.SkyBlue;
                case BrushColors.Plum:
                    return Brushes.Plum;
                case BrushColors.Silver:
                    return Brushes.Silver;
                case BrushColors.PaleGoldenrod:
                    return Brushes.PaleGoldenrod;
                case BrushColors.PaleTurquoise:
                    return Brushes.PaleTurquoise;
                case BrushColors.PowderBlue:
                    return Brushes.PowderBlue;
                case BrushColors.LightBlue:
                    return Brushes.LightBlue;
                case BrushColors.LightSteelBlue:
                    return Brushes.LightSteelBlue;
                case BrushColors.Thistle:
                    return Brushes.Thistle;
                case BrushColors.PeachPuff:
                    return Brushes.PeachPuff;
                case BrushColors.NavajoWhite:
                    return Brushes.NavajoWhite;
                case BrushColors.Bisque:
                    return Brushes.Bisque;
                case BrushColors.Moccasin:
                    return Brushes.Moccasin;
                case BrushColors.LightPink:
                    return Brushes.LightPink;
                case BrushColors.Wheat:
                    return Brushes.Wheat;
                case BrushColors.Gainsboro:
                    return Brushes.Gainsboro;
                case BrushColors.LightGray:
                    return Brushes.LightGray;
                case BrushColors.MistyRose:
                    return Brushes.MistyRose;
                case BrushColors.BlanchedAlmond:
                    return Brushes.BlanchedAlmond;
                case BrushColors.LemonChiffon:
                    return Brushes.LemonChiffon;
                case BrushColors.Pink:
                    return Brushes.Pink;
                case BrushColors.PapayaWhip:
                    return Brushes.PapayaWhip;
                case BrushColors.LightYellow:
                    return Brushes.LightYellow;
                case BrushColors.AntiqueWhite:
                    return Brushes.AntiqueWhite;
                case BrushColors.LightGoldenrodYellow:
                    return Brushes.LightGoldenrodYellow;
                case BrushColors.Snow:
                    return Brushes.Snow;
                case BrushColors.SeaShell:
                    return Brushes.SeaShell;
                case BrushColors.FloralWhite:
                    return Brushes.FloralWhite;
                case BrushColors.Cornsilk:
                    return Brushes.Cornsilk;
                case BrushColors.Beige:
                    return Brushes.Beige;
                case BrushColors.Ivory:
                    return Brushes.Ivory;
                case BrushColors.Honeydew:
                    return Brushes.Honeydew;
                case BrushColors.LightCyan:
                    return Brushes.LightCyan;
                case BrushColors.Azure:
                    return Brushes.Azure;
                case BrushColors.LavenderBlush:
                    return Brushes.LavenderBlush;
                case BrushColors.Linen:
                    return Brushes.Linen;
                case BrushColors.Lavender:
                    return Brushes.Lavender;
                case BrushColors.MintCream:
                    return Brushes.MintCream;
                case BrushColors.AliceBlue:
                    return Brushes.AliceBlue;
                case BrushColors.GhostWhite:
                    return Brushes.GhostWhite;
                case BrushColors.OldLace:
                    return Brushes.OldLace;
                case BrushColors.WhiteSmoke:
                    return Brushes.WhiteSmoke;
                case BrushColors.White:
                    return Brushes.White;
                case BrushColors.Transparent:
                    return Brushes.Transparent;
            }
            return null;
        }
    }
}
