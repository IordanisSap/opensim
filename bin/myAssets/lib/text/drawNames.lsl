string ImageUrl = "http://opensimulator.org/skins/osmonobook/images/headerLogo.png";
string FontName = "Courier New";
integer FontSize1 = 25;
integer FontSize2 = 20;
integer listen_handle;

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
        llOwnerSay("default");
        draw_init();
        listen_handle = llListen(getArenaCommChannel() + 1, "", "", "");
    }
    listen(integer channel, string name, key id, string message)
    {
        llOwnerSay(message);
        draw_msg(message);
    }
}

draw_msg(string message){
    list names = llParseStringKeepNulls(message, "+", "");
    integer size = llGetListLength(names);
    if (size == 1) draw_single(message);
    else draw_multiple(llKey2Name(llList2String(names, 0)), llKey2Name(llList2String(names, 1)));
}

draw_multiple(string name1,string name2)
{
    name1 = llToUpper(name1);
    name2 = llToUpper(name2);
    llOwnerSay("Drawing: " + name1 + " vs " + name2);
    
    string CommandList = ""; // Storage for our drawing commands
    CommandList = osSetFontName(CommandList, FontName);
    CommandList = osSetFontSize(CommandList, FontSize1);

    CommandList = osSetPenColor(CommandList, "FF000000");
    CommandList = osDrawFilledRectangle( CommandList, 512, 256 ); // 200 pixels by 100 pixels
    CommandList = osSetPenColor(CommandList, "FFFFFFFF");


    CommandList = osMovePen( CommandList, calculateHorizontalOffset(name1,FontName,FontSize1), 25 );  // Upper left corner at <10,10>
    CommandList = osDrawText( CommandList, name1 ); // Place some text
    
    CommandList = osSetFontSize(CommandList, FontSize2);
    CommandList = osSetPenColor(CommandList, "FFBC544B");
    CommandList = osMovePen( CommandList, calculateHorizontalOffset("vs", FontName, FontSize2), calculateVerticalOffset("vs", FontName, FontSize2) );           // Upper left corner at <10,10>
    CommandList = osDrawText( CommandList, "vs" ); // Place some text
    CommandList = osSetPenColor(CommandList, "FFFFFFFF");
     
    CommandList = osSetFontSize(CommandList, FontSize1);
    CommandList = osMovePen( CommandList, calculateHorizontalOffset(name2,FontName,FontSize1), 256-68 );           // Upper left corner at <10,10>
    CommandList = osDrawText( CommandList, name2 ); // Place some textaw

    // Now draw the image
    osSetDynamicTextureDataFace("", "vector", CommandList, "width:512,height:256", 0,1 );
}

draw_single(string textt)
{

    string text = llToUpper(textt);
    string CommandList = ""; // Storage for our drawing commands
    CommandList = osSetFontName(CommandList, FontName);
    CommandList = osSetFontSize(CommandList, FontSize2);

    CommandList = osSetPenColor(CommandList, "FF000000");
    CommandList = osDrawFilledRectangle( CommandList, 512, 256 ); // 200 pixels by 100 pixels


    CommandList = osSetPenColor(CommandList, "FFBC544B");
    CommandList = osMovePen( CommandList, calculateHorizontalOffset(text, FontName, FontSize2), calculateVerticalOffset(text, FontName, FontSize2) );           // Upper left corner at <10,10>
    CommandList = osDrawText( CommandList, text ); // Place some text
     
    // Now draw the image
    osSetDynamicTextureDataFace("", "vector", CommandList, "width:512,height:256", 0,1 );
}

draw_init(){
    string CommandList = ""; // Storage; for our drawing commands
    string text = "Waiting for players...";
    CommandList = osSetFontName(CommandList, FontName);
    CommandList = osSetFontSize(CommandList, FontSize2+5);

    

    CommandList = osSetPenColor(CommandList, "FF000000");
    CommandList = osDrawFilledRectangle( CommandList, 512, 256 ); // 200 pixels by 100 pixels
    CommandList = osSetPenColor(CommandList, "FFAAAAAA");

    CommandList = osMovePen( CommandList, calculateHorizontalOffset(text, FontName, FontSize2+5), calculateVerticalOffset("vs", FontName, FontSize2+5) );           // Upper left corner at <10,10>
    CommandList = osDrawText( CommandList, text ); // Place some text
    osSetDynamicTextureDataFace("", "vector", CommandList, "width:512,height:256", 0,1 );
}