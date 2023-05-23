default
{
     touch(integer num_detected)
     {
          key copiedNPC = osNpcCreate("copiedNPC", "copiedNPC", llGetPos(), llGetOwner());
          osNpcSaveAppearance(copiedNPC, "copiedNPC", TRUE);
     }
}

