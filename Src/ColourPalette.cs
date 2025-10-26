using System.Collections.Generic;
using UnityEngine;
namespace SilkenImpact {
    public static class ColourPalette {
        public static Color FromHexString(string hex) {
            if (hex.Length != 7 || hex[0] != '#') {
                throw new System.ArgumentException("Invalid hex color format. Expected format: #RRGGBB");
            }

            return new Color(
            int.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber) / 255.0f,
            int.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber) / 255.0f,
            int.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber) / 255.0f);
        }

        public static Color Hydro => FromHexString("#1EC5E3");
        public static Color Cryo => FromHexString("#78FDFF");
        public static Color Pyro => FromHexString("#FF6400");
        public static Color Anemo => FromHexString("#24FFD3");
        public static Color Geo => FromHexString("#FFE064");
        public static Color Electro => FromHexString("#D272FF");
        public static Color Dendro => FromHexString("#00C94E");
        public static Color Physical => FromHexString("#FFFFFF");
        public static Color HornetDress => FromHexString("#A83448");
        public static Color HealTextColor => FromHexString("#BDFF37");

        public static List<Color> AllElementColors => new List<Color> {
            Hydro,
            Cryo,
            Pyro,
            Anemo,
            Geo,
            Electro,
            Dendro,
            Physical,
            HornetDress
        };
    }

}