int stage_lock=current_phase;
int ego(){
  if(self.mp<1000){
   int t = target.num;
   int count = 0;
   int shot = 0;
   for (int i=0;i<400;++i){
      if(abs(target.x-self.x)>100&&(loadTarget(i)==0||loadTarget(i)==3) && target.team!=self.team && target.x-self.x==abs(target.x-self.x)*((self.facing)?-1:1) && abs(target.x-self.x)<700){
	   count++;
	  }
      if(loadTarget(i)>0 && target.team==self.team && target.x-self.x==abs(target.x-self.x)*((self.facing)?-1:1)){
	   shot++;
	  }
   }
   loadTarget(t);
   if(self.frame==111||self.frame==112||self.frame==180||self.frame==186||self.frame==220||self.frame==222||self.frame==224||self.frame==226){DJA();return 1;}
   if((count>10&&shot==0) || (target.x-self.x==abs(target.x-self.x)*((self.facing)?-1:1) && target.y<0 && target.state!=12 && target.state!=6)){DuJ();return 1;}
   if(abs(target.x-self.x)<700 && abs(target.z-self.z)<2&&shot==0){
    if (self.x-target.x > 0){DlJ();}
    else if (self.x-target.x < 0){DrJ();}
   }
   else if(target.team!=self.team && target.x-self.x==abs(target.x-self.x)*((self.facing)?-1:1) && abs(target.x-self.x)>400&&(self.frame==239||self.frame==244)&&shot<=2){A();}
   else if(abs(target.x-self.x)>350&&shot==0){
    if (self.x-target.x > 0){DlA();}
    else if (self.x-target.x < 0){DrA();}
   }
  }
  if(current_phase>stage_lock){A();return 1;}
  return 0;
}