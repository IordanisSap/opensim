default
{
    state_entry()
    {
        llSensorRepeat("Ball", NULL_KEY, ( ACTIVE ), 1.5, PI , 0.15);
        llSetText(14, <1, 1, 1>, 1);
        llParticleSystem([
            PSYS_PART_FLAGS,
                PSYS_PART_INTERP_COLOR_MASK |
                PSYS_PART_INTERP_COLOR_MASK |
                PSYS_PART_EMISSIVE_MASK,
            PSYS_PART_START_COLOR,
                <1.0, 0.5, 0.0>,
            PSYS_PART_END_COLOR,
                <1.0, 1.0, 1.0>,
            PSYS_PART_START_SCALE,
                <0.35, 0.35, 0.0>,
            PSYS_PART_END_SCALE,
                <2, 2, 0.0>,
            PSYS_SRC_BURST_PART_COUNT,
                85,
            PSYS_SRC_BURST_RADIUS,
                0.35,
            PSYS_SRC_BURST_SPEED_MIN,
                0.1,
            PSYS_SRC_BURST_SPEED_MAX,
                0.4,
            PSYS_SRC_PATTERN,
                PSYS_SRC_PATTERN_EXPLODE,
            PSYS_SRC_ACCEL,
                <0.0, 0.0, 1.5>,
            PSYS_SRC_BURST_RATE,
                0.1,
            PSYS_PART_MAX_AGE,
                2.0,
            PSYS_PART_START_ALPHA,
                1,
            PSYS_PART_END_ALPHA,
                0.1
        ]);
    }
    link_message(integer sender_num, integer num, string str, key id)
    {
        llSetText(num, <1, 1, 1>, 1);
        if (num == 0) {
            llParticleSystem([]);
            llSensorRemove();
            llSetText("", ZERO_VECTOR, 0);
            return;
        }
        llParticleSystem([
            PSYS_PART_FLAGS,
                PSYS_PART_INTERP_COLOR_MASK |
                PSYS_PART_INTERP_COLOR_MASK |
                PSYS_PART_EMISSIVE_MASK,
            PSYS_PART_START_COLOR,
                <1.0, 0.5, 0.0>,
            PSYS_PART_END_COLOR,
                <1.0, 1.0, 1.0>,
            PSYS_PART_START_SCALE,
                <0.35, 0.35, 0.0>,
            PSYS_PART_END_SCALE,
                <2, 2, 0.0>,
            PSYS_SRC_BURST_PART_COUNT,
                (integer) num * 4 + 20,
            PSYS_SRC_BURST_RADIUS,
                0.35,
            PSYS_SRC_BURST_SPEED_MIN,
                0.1,
            PSYS_SRC_BURST_SPEED_MAX,
                0.4,
            PSYS_SRC_PATTERN,
                PSYS_SRC_PATTERN_EXPLODE,
            PSYS_SRC_ACCEL,
                <0.0, 0.0, 1.5>,
            PSYS_SRC_BURST_RATE,
                0.1,
            PSYS_PART_MAX_AGE,
                num / 8 + 1,
            PSYS_PART_START_ALPHA,
                1,
            PSYS_PART_END_ALPHA,
                0.1
        ]);
    }
    sensor( integer detected )
    {
        while(detected--)
        {
            llMessageLinked(1, 0, llGetScriptName(), llDetectedKey(detected));
        }
    }
}