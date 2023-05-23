#include "lib/assert.lsl"

key weapon = NULL_KEY;
string WEAPON_NAME = "";
integer shooting = FALSE;
float reloadTime;
float lastShoot;


initWeapon(key weaponKey, string weaponName, float reload)
{
    weapon = weaponKey;
    WEAPON_NAME = weaponName;
    reloadTime = reload;
}

integer hasWeapon()
{
    return weapon != NULL_KEY && WEAPON_NAME != "";
}

integer canShoot()
{
    assert(hasWeapon(), "Weapon not set");
    if (llGetTime() - lastShoot < reloadTime) return FALSE;
    return TRUE;
}

shoot()
{
    if (!canShoot()) return;
    lastShoot = llGetTime();
    osNpcTouch(npcKey, weapon, LINK_THIS); //Shoot
}
