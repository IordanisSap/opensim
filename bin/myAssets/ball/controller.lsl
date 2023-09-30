
default
{
    touch(integer num_detected)
    {
        setTimeFactor(0.25);
        if (mazeHasStarted())
            return;
        generateMaze(5, llGetPos()-<20,-3,3>, llDetectedKey(0));
    }
}

