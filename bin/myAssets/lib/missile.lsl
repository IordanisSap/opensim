default
{
    state_entry()
    {
        vector v = llRot2Fwd( llGetRot() );
        llShout(0,v);

        llParticleSystem([
            PSYS_PART_FLAGS,     PSYS_PART_EMISSIVE_MASK | PSYS_PART_INTERP_COLOR_MASK,
            PSYS_PART_START_COLOR, <0.95, 0.8, 0.1>,
            PSYS_PART_END_COLOR,   <0.95, 0.95, 0.95>,
            PSYS_PART_START_SCALE, <0.15, 0.15, 0.15>,
            PSYS_PART_END_SCALE,  <8,8,8>,
            PSYS_SRC_ACCEL,       -v*3,
            PSYS_SRC_BURST_SPEED_MIN,    0.1,
            PSYS_SRC_BURST_SPEED_MAX,    0.2,
            PSYS_SRC_ANGLE_BEGIN,       PI*0.4,
            PSYS_SRC_ANGLE_END,        PI*0.6,
            PSYS_SRC_BURST_RADIUS,      0.02,
            PSYS_SRC_PATTERN,     PSYS_SRC_PATTERN_ANGLE_CONE,
            PSYS_SRC_BURST_RATE,  0.05,
            PSYS_SRC_BURST_PART_COUNT,  10,
            //PSYS_SRC_MAX_AGE,     35, 
            PSYS_PART_MAX_AGE,    1.5,
            PSYS_PART_START_ALPHA, 0.4,
            PSYS_PART_END_ALPHA, 0.0
            
        ]);
    }
    on_rez(integer start_param)
    {
        // Restarts the script every time the object is rezzed
        llResetScript(); 
    }
}