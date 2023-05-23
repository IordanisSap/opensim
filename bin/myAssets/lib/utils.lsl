integer random_integer(integer min, integer max)
{
    return min + (integer)(llFrand(max - min + 1));
}
