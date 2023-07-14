key BLANK_TEXTURE = "5748decc-f629-461c-9a36-a35a221fe21f";


reset_texture(){
    llSetTextureAnim(FALSE, ALL_SIDES, 0, 0, 0.0, 0.0, 1.0);
    llSetPrimitiveParams( [ PRIM_GLOW, ALL_SIDES, .04 ] ) ;
    llSetTexture(BLANK_TEXTURE, ALL_SIDES);
}
