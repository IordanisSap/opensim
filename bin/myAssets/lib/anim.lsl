list    TextureList;
integer index = 0;
default
{
    state_entry()
    {
        integer count = llGetInventoryNumber(INVENTORY_TEXTURE);
        while (count--)
        {
            TextureList += llGetInventoryName(INVENTORY_TEXTURE, count);
        }
        llSetTexture(llGetInventoryKey(llList2String(TextureList, index)), ALL_SIDES);
        //llSetTimerEvent(0.1);
        llSetTextureAnim(ANIM_ON | SMOOTH | LOOP , ALL_SIDES, 1, 1, 1.0, 1.0, 1.0);
    }

    timer(){
        index++;
        if(index >= llGetListLength(TextureList)){
            index = 0;
        }
        llSetTexture(llGetInventoryKey(llList2String(TextureList, index)), ALL_SIDES);
    }
}