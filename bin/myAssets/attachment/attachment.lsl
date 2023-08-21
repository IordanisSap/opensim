default
{
    state_entry()
    {
       llTargetOmega(llRot2Up(llGetLocalRot()), PI/4, 1);           
    }
    attach(key id)
    {
        if (llGetAttached()){
            llSetScale(<0.05,0.05,0.05>); 
        }
        else{
            llSetScale(<1,1,1>); 
        }
    }

}
