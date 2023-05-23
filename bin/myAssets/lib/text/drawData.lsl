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
        //listen_handle = llListen(getArenaCommChannel(), "", "", "");
    }
    listen(integer channel, string name, key id, string message)
    {
        list names = llParseStringKeepNulls(message, "+", "");
        string teamName1 = llKey2Name(llList2String(names, 0));
        string teamName2 = llKey2Name(llList2String(names, 1));
        llOwnerSay("Drawing: " + teamName1 + " vs " + teamName2);
        llOwnerSay("Message: " + message);
        draw(teamName1, teamName2);
    }
}

draw(string name1,string name2)
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
    CommandList = osSetPenColor(CommandList, "FFBC544B");
    CommandList = osMovePen( CommandList, calculateHorizontalOffset("vs", FontName, FontSize2), calculateVerticalOffset("vs", FontName, FontSize2) );           // Upper left corner at <10,10>
    CommandList = osDrawText( CommandList, "vs" ); // Place some text
    CommandList = osSetPenColor(CommandList, "FFAAAAAA");
     
    CommandList = osSetFontSize(CommandList, FontSize1);
    CommandList = osMovePen( CommandList, calculateHorizontalOffset(name2,FontName,FontSize1), 256-68 );           // Upper left corner at <10,10>
    CommandList = osDrawText( CommandList, name2 ); // Place some textaw

    // Now draw the image
    osSetDynamicTextureData("", "vector", CommandList, "width:512,height:256", 0 );
}

draw_init(){
    string CommandList = ""; // Storage; for our drawing commands
    string text = "Setting up...";
    CommandList = osSetFontName(CommandList, FontName);
    CommandList = osSetFontSize(CommandList, FontSize1);

    CommandList = osSetPenColor(CommandList, "00000000");
    CommandList = osDrawFilledRectangle( CommandList, 512, 256 ); // 200 pixels by 100 pixels
    CommandList = osSetPenColor(CommandList, "FFAAAAAA");

    CommandList = osSetFontSize(CommandList, FontSize1);
    CommandList = osMovePen( CommandList, calculateHorizontalOffset(text, FontName, FontSize1), calculateVerticalOffset("vs", FontName, FontSize1) );           // Upper left corner at <10,10>
    CommandList = osDrawText( CommandList, text ); // Place some text
    osSetDynamicTextureDataFace("", "vector", CommandList, "width:512,height:256", 0, 4 );
    //osSetDynamicTextureDataBlendFace("", "vector", CommandList, "width:512,height:256,Alpha:0", TRUE, 2, 0, 255, 4);

}