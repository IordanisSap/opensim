
command(){
    // Commands start here.




    move_fwd(1);
    sleep(1);


    


    // integer i; 
    // for (i=0;i < 14; ++i)
    // {
    //     move_fwd(1);
    //     sleep(1);
    //     move_back(1);
    //     sleep(1);
    // }


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
    llSleep(seconds*getTimeFactor());
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

level_up(){
    activate_powerup("LevelUp",[]);
}

move_fwd(integer dist){
    movePlayer(<0,dist,0>);
}

move_back(integer dist){
    movePlayer(<0,-dist,0>);
}

move_left(integer dist){
    movePlayer(<-dist,0,0>);
}

move_right(integer dist){
    movePlayer(<dist,0,0>);
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