default
{
    state_entry()
    {
        llSensorRepeat("Ball", NULL_KEY, ( ACTIVE ), 1.5, PI , 0.15);
    }
    link_message(integer sender_num, integer num, string str, key id)
    {
        //if (str == llGetObjectName()) {
            llBreakLink(llGetLinkNumber());
            llDie();
        //}
    }
    sensor( integer detected )
    {
        while(detected--)
        {
            llMessageLinked(1, 0, llGetScriptName(), llDetectedKey(detected));
        }
    }
}