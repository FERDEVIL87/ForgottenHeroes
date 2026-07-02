int ego(){ 
//Sonata Throw
if(frame(110,110)&&xdst(0,500)&&self.mp>=180){J();}

//MagicStar Arrow
else if(frame(0,11)&&!xdst(0,400)&&self.mp>=300){DfJ();}

//Flute Wind
else if(frame(0,11)&&xdst(400,800)&&zdst(15)&&self.mp>=150){DvA();}

//Sonata Throw Start
else if(frame(0,11)&&xdst(0,500)&&self.mp>=180){D();return 1;}

//(Cha)Flute Arrow
else if(frame(0,11)&&!xdst(0,300)&&self.mp>=65){DfA();}

return 0;
}

//true if within z range
bool zdst(int range){
if(abs(self.z-target.z)<=range){return true;}
else{return false;}
}

//true if within x range
bool xdst(int close, int far){
if(abs(self.x-target.x)>=close && abs(self.x-target.x)<=far){return true;}
else{return false;}
}

//true if within frame range
bool frame(int first, int last){
if(self.frame>=first && self.frame<=last){return true;}
else{return false;}
}

//perform D<>J towards target
void DfJ(){
if((self.x-target.x)<0){DrJ();}
else{DlJ();}
}

//perform DdA towards target
void DvA(){
face_target();
DdA();
}

//perform D<>A towards target
void DfA(){
if((self.x-target.x)<0){DrA();}
else{DlA();}
}

//face target
void face_target(){
if((self.x-target.x)<0){right();}
else{left();}
}