integer PIN=123321;

default
{
    state_entry()
    {
        generateMaze2D(5);
        //llRemoteLoadScriptPin( llGetLinkKey(2), "some script", PIN, TRUE, 0xBEEEEEEF );
        llSleep(10);
        resetMaze();
    }
}
