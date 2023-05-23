key trackedAvatar = NULL_KEY;
vector pos = ZERO_VECTOR;
default
{
    state_entry()
    {
        llRequestPermissions(llGetOwner(), PERMISSION_CONTROL_CAMERA);
    }
    attach(key av)
    {
        if (NULL_KEY != av)
            llResetScript();
    }
    run_time_permissions(integer perm)
    {
        if (PERMISSION_CONTROL_CAMERA & perm)
            llSetTimerEvent(0.1);   // just to start
    }
    timer()
    {
        vector avSize = llGetAgentSize(trackedAvatar);
        if (avSize == ZERO_VECTOR)
        {
            llClearCameraParams();
            trackedAvatar = NULL_KEY;
            list regionAVs = llGetAgentList(AGENT_LIST_REGION, []);
            integer avIdx = llGetListLength(regionAVs);
            while ((0 <= --avIdx) && (NULL_KEY == trackedAvatar))
            {
                key testAV = llList2Key(regionAVs, avIdx);
                if (llGetOwner() != testAV)
                    trackedAvatar = testAV;
            }
            if (NULL_KEY == trackedAvatar)
            {
                llOwnerSay("No other agents in region");
                llSetTimerEvent(15.0);  // suspend a while before resampling region
                return;
            }
            llSetTimerEvent(0.3);
            avSize = llGetAgentSize(trackedAvatar);
        }
        list avData = llGetObjectDetails(trackedAvatar, [OBJECT_POS, OBJECT_NAME]);
        vector targetPos = llList2Vector(avData, 0) + <0.0, 0.0, avSize.z*0.4>;
        // llOwnerSay("tracking "+llList2String(avData, 1)+" at position "+(string)targetPos);
        llSetCameraParams([
                CAMERA_ACTIVE, TRUE,
                CAMERA_FOCUS, targetPos,
                CAMERA_FOCUS_LOCKED, TRUE,
                CAMERA_POSITION, targetPos + llVecNorm(llGetPos()-targetPos) + <-5,0,8>,
                CAMERA_POSITION_LOCKED, TRUE
                ]);
    }
}


moveCameraSmooth(){

}