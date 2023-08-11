default
{
    state_entry()
    {
       llTargetOmega(llRot2Up(llGetLocalRot()), PI/2, 1);           
    }
        touch_start(integer num_detected)
    {
        osForceAttachToAvatar(ATTACH_HUD_BOTTOM);
        llWhisper(0, "Pos clicked: " + (string)llDetectedTouchPos(0));
    }
    attach(key id)
    {
        if (llGetAttached()){
            llSetScale(<0.05,0.05,0.05>); 
            llSetPos(<0,0.5,0.1>);
            llTargetOmega(llRot2Up(llGetLocalRot()), PI/4, 1);           

        }
        else{
            llSetScale(<1,1,1>); 
        }
    }

}
