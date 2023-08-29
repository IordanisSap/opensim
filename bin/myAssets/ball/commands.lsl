
command(){
    // Commands start here.
    move_fwd(1);
    sleep(1);
    move_right(2);
    sleep(2);
    move_fwd(2);
    sleep(2);
    move_fwd(1);

    //activate_powerup("Jump",[]);

    //activate_powerup("Build",["front"]);
    // Commands end here.
 }
 
 example_command(){
    move_fwd(2);  
    sleep(2);
    build_fwd();
}
 
 
 
 // Do not edit beyond this point.
 integer channel = -13572468;

 sleep(integer seconds){
    llSleep(seconds);
 }

 shield(){
    activate_powerup("Shield",[]);
 }

 build_fwd(){
    activate_powerup("Build",["forward"]);
 }

build_left(){
    activate_powerup("Build",["left"]);
}

build_right(){
    activate_powerup("Build",["right"]);
}

build_back(){
    activate_powerup("Build",["back"]);
}

integer powerup_num(string searchPowerup){
    list powerups = getPowerUps(llGetKey());
    integer count = 0;
    integer totalPowerups = llGetListLength(powerups);
    for (integer i = 0; i < totalPowerups; i++)
    {
        string powerup = llList2String(powerups, i);
        if (powerup == searchPowerup)
        {
            count++;
        }
    }
    return count;
}

 default
 {
     state_entry()
     {
         command();
     }
 }