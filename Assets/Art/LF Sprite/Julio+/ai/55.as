int ego(){
//armored
if(self.weapon_type == 2)
{
	//jump attack
	if((self.frame == 208 || self.frame == 209 || self.frame == 212) && self.y_velocity < 0 && abs(self.z-target.z) < 15 && self_facing()) A();
	//running
	else if(abs(self.x-target.x) > 80 && self.state == 2 && abs(self.x-target.x) < 400)
	{
		run();
		//teleport
		if(abs(self.x-target.x) > 150 && abs(self.z-target.z) > 50 && self_facing()) D();
		return 1;
	}
	//combo
	else if(target.state == 12 && (self.frame == 57 ||self.frame == 58)) J();
	//teleport
	else if(abs(self.x-target.x) > 400 && self.mp > 50)
	{
		if(self.frame == 22 || self.frame == 208) J();
		else if(self.state != 2) D();
		else J();
		return 1;
	}
	//counter
	else if(abs(self.x-target.x) < 80 && abs(self.z-target.z) < 15 && target.state == 3) D();
	//followup
	else if(target.state == 16 && self.frame == 22 && self.mp > 100) A();
	else if(target.state == 16 && self.frame == 22 && self.mp > 50) J();
	else run();
}
//unarmored
else
{
	if(self.frame == 215 || self.frame == 99 || self.frame == 107 || self.frame == 212 || self.frame == 209) return 1;
	else if(self.hp < target.dark_hp && abs(self.x-target.x) < 350) flee();
	else if(self.state != 12 && self.hp < target.dark_hp && abs(self.x-target.x) >= 350) {J();A();}
	else if(self.frame == 177 || self.frame == 178 || self.frame == 179 || self.frame == 197 || self.frame == 198 || self.frame == 199) {turn_self(); return 1;}
}
return 0;
}

void run(){
	if(self.x-target.x < -200) right();
	else if(self.x-target.x > 200) left();
}
void flee(){
	if(self.state != 12) J();
	if(self.x-target.x > 0 && bg_width-self.x > 100) right();
	else if(self.x-target.x < 0 && self.x > 100) left();
}

void turn_self(){
   //turn around
   if(self.facing){right(1,0);}else{left(1,0);}
}
int facing_distance(){
   //positive: target distance to the front
   return -xdistance()*(2*(self.facing?1:0)-1);
}
bool self_facing(){
   //true if self facing target
   return (facing_distance()>0)?true:false;
}
int xdistance(){
   //positive: target distance to the right
   return target.x-self.x;
}