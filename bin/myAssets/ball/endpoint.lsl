//Child Prim PIN setter
integer PIN=123321;

default 
{
    state_entry() {
        llParticleSystem([
            PSYS_PART_FLAGS,    PSYS_PART_EMISSIVE_MASK | PSYS_PART_INTERP_COLOR_MASK |
                                PSYS_PART_INTERP_SCALE_MASK |
                                PSYS_PART_FOLLOW_VELOCITY_MASK,
            PSYS_PART_START_COLOR, <0.95, 0.8, 0>,
            PSYS_PART_END_COLOR, <0.95, 0.95, 0.6>,
            PSYS_PART_START_SCALE, <0.4, 0.4, 0.4>,
            PSYS_PART_END_SCALE, <1, 1, 1>,
            PSYS_SRC_PATTERN, PSYS_SRC_PATTERN_ANGLE_CONE,
            PSYS_SRC_ANGLE_BEGIN, 0.1,
            PSYS_SRC_ANGLE_END, 0.1,
            PSYS_PART_MAX_AGE, 3.0,
            PSYS_SRC_BURST_RATE, 0.2,
            PSYS_SRC_BURST_PART_COUNT, 12,
            PSYS_SRC_BURST_RADIUS, 0.0,
            PSYS_SRC_BURST_SPEED_MIN, 1.0,
            PSYS_SRC_BURST_SPEED_MAX, 2.0,
            PSYS_SRC_ACCEL, <0.0, 0.0, 1.0>,
            PSYS_SRC_OMEGA, <0.0, 0.0, 1.0>
        ]);
    }
    collision(integer num_detected)
    {
        integer finished = endPointCollision(llDetectedKey(0));
        if (finished == 1){
            llParticleSystem([
                PSYS_PART_FLAGS,    PSYS_PART_EMISSIVE_MASK | PSYS_PART_INTERP_COLOR_MASK |
                                    PSYS_PART_INTERP_SCALE_MASK |
                                    PSYS_PART_FOLLOW_VELOCITY_MASK,
                PSYS_PART_START_COLOR, <0.1, 0.6, 1>,
                PSYS_PART_END_COLOR, <0.4, 0.8, 1>,
                PSYS_PART_START_SCALE, <0.8, 0.8, 0.8>,
                PSYS_PART_END_SCALE, <2, 2, 2>,
                PSYS_SRC_PATTERN, PSYS_SRC_PATTERN_ANGLE_CONE,
                PSYS_SRC_ANGLE_BEGIN, 0.1,
                PSYS_SRC_ANGLE_END, 0.1,
                PSYS_PART_MAX_AGE, 3.0,
                PSYS_SRC_BURST_RATE, 0.2,
                PSYS_SRC_BURST_PART_COUNT, 6,
                PSYS_SRC_BURST_RADIUS, 0.0,
                PSYS_SRC_BURST_SPEED_MIN, 0.5,
                PSYS_SRC_BURST_SPEED_MAX, 1.0,
                PSYS_SRC_ACCEL, <0.0, 0.0, 1.0>,
                PSYS_SRC_OMEGA, <0.0, 0.0, 1.0>
            ]);
        }

    }
}