string ImageUrl = "http://opensimulator.org/skins/osmonobook/images/headerLogo.png";
string FontName = "Courier New";
integer FontSize1 = 30;
integer FontSize2 = 20;
integer listen_handle;
string TEXTURE_TRANSPARENT = "8dcd4a48-2d37-4909-9f78-f7a9eb4ef903"; 

list colorNames = [
    "AliceBlue", "AntiqueWhite", "Aqua", "Aquamarine", "Azure", "Beige", "Bisque",
    "Black", "BlanchedAlmond", "Blue", "BlueViolet", "Brown", "BurlyWood",
    "CadetBlue", "Chartreuse", "Chocolate", "Coral", "CornflowerBlue", "Cornsilk",
    "Crimson", "Cyan", "DarkBlue", "DarkCyan", "DarkGoldenrod", "DarkGrey",
    "DarkGreen", "DarkKhaki", "DarkMagenta", "DarkOliveGreen", "DarkOrange",
    "DarkOrchid", "DarkRed", "DarkSalmon", "DarkSeaGreen", "DarkSlateBlue",
    "DarkSlateGrey", "DarkTurquoise", "DarkViolet", "DeepPink", "DeepSkyBlue",
    "DimGrey", "DodgerBlue", "FireBrick", "FloralWhite", "ForestGreen", "Fuchsia",
    "Gainsboro", "GhostWhite", "Gold", "Goldenrod", "Grey", "Green", "GreenYellow",
    "Honeydew", "HotPink", "IndianRed", "Indigo", "Ivory", "Khaki", "Lavender",
    "LavenderBlush", "LawnGreen", "LemonChiffon", "LightBlue", "LightCoral",
    "LightCyan", "LightGoldenrodYellow", "LightGreen", "LightGrey", "LightPink",
    "LightSalmon", "LightSeaGreen", "LightSkyBlue", "LightSlateGrey",
    "LightSteelBlue", "LightYellow", "Lime", "LimeGreen", "Linen", "Magenta",
    "Maroon", "MediumAquamarine", "MediumBlue", "MediumOrchid", "MediumPurple",
    "MediumSeaGreen", "MediumSlateBlue", "MediumSpringGreen", "MediumTurquoise",
    "MediumVioletRed", "MidnightBlue", "MintCream", "MistyRose", "Moccasin",
    "NavajoWhite", "Navy", "OldLace", "Olive", "OliveDrab", "Orange", "OrangeRed",
    "Orchid", "PaleGoldenrod", "PaleGreen", "PaleTurquoise", "PaleVioletRed",
    "PapayaWhip", "PeachPuff", "Peru", "Pink", "Plum", "PowderBlue", "Purple",
    "Red", "RosyBrown", "RoyalBlue", "SaddleBrown", "Salmon", "SandyBrown",
    "SeaGreen", "Seashell", "Sienna", "Silver", "SkyBlue", "SlateBlue", "SlateGrey",
    "Snow", "SpringGreen", "SteelBlue", "Tan", "Teal", "Thistle", "Tomato",
    "Turquoise", "Violet", "Wheat", "White", "WhiteSmoke", "Yellow", "YellowGreen"
];


integer calculateHorizontalOffset(string str,string fontName, integer size){
    return 256 - llRound(osGetDrawStringSize( "vector", str, fontName, size).x/2);
}

integer calculateVerticalOffset(string str,string fontName, integer size){
    return 128 - llRound(osGetDrawStringSize( "vector", str, fontName, size).y/2);
}

default
{
    state_entry()
    {
        draw_init();
    }
}

draw_init(){
    string CommandList = ""; // Storage; for our drawing commands
    string text = "Right";
    integer BigFont = 80;

    CommandList = osSetFontName(CommandList, FontName);
    CommandList = osSetFontSize(CommandList, BigFont);
    CommandList = osSetPenColor(CommandList, "FFFFFFAA");

    CommandList = osSetFontSize(CommandList, BigFont);
    CommandList = osMovePen( CommandList, calculateHorizontalOffset(text, FontName, BigFont), calculateVerticalOffset(text, FontName, BigFont) );           // Upper left corner at <10,10>
    CommandList = osDrawText( CommandList, text ); // Place some text
    osSetDynamicTextureDataFace("", "vector", CommandList, "width:512,height:256,bgColor:000000FF,setalpha:0", 0, 4 );
    osSetDynamicTextureDataFace("", "vector", CommandList, "width:512,height:256,bgColor:000000FF,setalpha:0", 0, 2 );
}