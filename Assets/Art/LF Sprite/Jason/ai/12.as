int ego(){

//bottle slam
if(self.weapon_type == 6 && self.frame == 121)
{
 if (self.x-target.x > 0)right();
 else if (self.x-target.x < 0)left();
A();
return 1;
}

//combos
if(self.frame >= 74 && self.frame <= 75)
{
 if (self.x-target.x > 0)right();
 else if (self.x-target.x < 0)left();
 }
else if(self.frame == 77) A();
else if(self.frame >= 236 && self.frame <= 239)
{
if(self.mp > 150)
{
 if (self.frame == 239)DdA();
 else return 1;
 }
else if(self.mp > 75)
{
 if (self.x-target.x > 0)right();
 else if (self.x-target.x < 0)left();
 }
 else
 A();
 }
else if(self.frame == 241) A();
else if(self.frame >= 260 && self.frame <= 264 && self.mp > 75) return 1;

//DfA
 if ((abs(100*(self.z-target.z)/((self.x-target.x)*((self.facing?1:0)*2-1))) == 15
 || abs(100*(self.z-target.z)/((self.x-target.x)*((self.facing?1:0)*2-1))) < 2)
 && abs(self.x-target.x) > 200){
 if (self.x-target.x > 0)DlA();
 else if (self.x-target.x < 0)DrA();
 }
//DfJ
else if (target.y == 0 && abs(target.z-self.z) < 15
&& abs(self.x-target.x) > 60){
 if (self.x-target.x > 0)DlJ();
 else if (self.x-target.x < 0)DrJ();
}
//DuA
else if (target.y == 0 && abs(target.z-self.z) < 15
&& (self.x-target.x)*((self.facing?1:0)*2-1) > 0
&& (self.x-target.x)*((self.facing?1:0)*2-1) < 60)
DuA();
//DuJ
else if (target.state == 3)DuJ();

return 0;
}