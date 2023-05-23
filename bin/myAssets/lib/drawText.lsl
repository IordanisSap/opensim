string ImageUrl = "http://opensimulator.org/skins/osmonobook/images/headerLogo.png";
string FontName = "Courier New";
integer FontSize1 = 45;
integer FontSize2 = 25;
string name1 = "SaΒΒιδης";
string name2 = "npc";
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
        name1 = llToUpper(name1);
        name2 = llToUpper(name2);
        
        string CommandList = ""; // Storage for our drawing commands
        CommandList = osSetFontName(CommandList, FontName);
        CommandList = osSetFontSize(CommandList, FontSize1);

        CommandList = osSetPenColor(CommandList, "FF0000FF");
        CommandList = osDrawFilledRectangle( CommandList, 512, 256 ); // 200 pixels by 100 pixels
        CommandList = osSetPenColor(CommandList, "FFAAAAAA");


        CommandList = osMovePen( CommandList, calculateHorizontalOffset(name1,FontName,FontSize1), 25 );  // Upper left corner at <10,10>
        CommandList = osDrawText( CommandList, name1 ); // Place some text
        
        CommandList = osSetFontSize(CommandList, FontSize2);
        CommandList = osMovePen( CommandList, calculateHorizontalOffset("vs", FontName, FontSize2), calculateVerticalOffset("vs", FontName, FontSize2) );           // Upper left corner at <10,10>
        CommandList = osDrawText( CommandList, "vs" ); // Place some text
                
        CommandList = osSetFontSize(CommandList, FontSize1);
        CommandList = osMovePen( CommandList, calculateHorizontalOffset(name2,FontName,FontSize1), 256-98 );           // Upper left corner at <10,10>
        CommandList = osDrawText( CommandList, name2 ); // Place some text
 
        // Now draw the image
        osSetDynamicTextureData( "", "vector", CommandList, "width:512,height:256", 0 );
    }
}