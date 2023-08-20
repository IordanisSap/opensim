
default
{
    state_entry()
    {
        generateMaze2D(6);
        llSetTimerEvent(350);
    }
    timer()
    {
        resetMaze();
    }
    touch(integer num_detected)
    {
        llOwnerSay("Resetting maze");
        resetMaze();
    }
}

