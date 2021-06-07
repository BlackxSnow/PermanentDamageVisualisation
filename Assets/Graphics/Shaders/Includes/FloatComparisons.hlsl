static const float Epsilon = 0.00001;
static const float HEpsilon = 0.001; 
float gtF(float a, float b, float E){
    if( b > a + E ) { return false; }
    if( b > a - E ) { return false; }
    return true;
}
float ltF(float a, float b, float E){
    if( b > a + E ) { return true; }
    if( b > a - E ) { return false; }
    return false;
}
float eqF(float a, float b, float E){
    if( b > a + E ) { return false; }
    if( b > a - E ) { return true; }
    return false;
}
float gteqF(float a, float b, float E){
    if( b > a + E ) { return false; }
    if( b > a - E ) { return true; }
    return true;
}
float lteqF(float a, float b, float E){
    if( b > a + E ) { return true; }
    if( b > a - E ) { return true; }
    return false;
}