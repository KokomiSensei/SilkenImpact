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
        public static Color Pyro => FromHexString("#FFBB28");
        public static Color Anemo => FromHexString("#24FFD3");
        public static Color Geo => FromHexString("#FFE064");
        public static Color Electro => FromHexString("#E5AAFF");
        public static Color Dendro => FromHexString("#00C94E");
        public static Color Physical => FromHexString("#FFFFFF");
    }

}