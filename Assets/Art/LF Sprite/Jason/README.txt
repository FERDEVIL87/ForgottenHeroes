Jason

by prince_freeza & YinYin
April 2015.


INSTALLATION:
1- move the following into your lf2 directory (the place where the lf2.exe file is located):
   jason (folder)
   ai (folder)
   ddraw.dll

2- paste the "data.txt" file inside the "data" folder and replace the original existing one OR:

   *paste these lines into your data.txt file: 

<object>
id: 12  type: 0  file: jason\jason.dat
id: 13  type: 3  file: jason\jason_ball.dat
</object>


MOVE LIST:
----------
A: attack
D: defend
J: jump
^: up
v: down
>: right
<: left
----------

>> + <A     Charged super punch
description: while running hold <A to charge the regular punch, this will cost you mp.

1) D>A      Energy ball
description: an energy ball which will destroy everything in its path, the ball will get destroyed if it takes too much damage.

2) D^A      Energy uppercut
description: a grounded uppercut which will slam enemies into the air.

3) D^A+A    Energy uppercut + Elbow slam
4) D^A+A+<A Energy uppercut + Elbow slam + Energy punch

5) DvA      Energy Elbow slam
6) DVA+<A   Energy Elbow slam + Energy punch
 
7) D>J      Grab spin
description: Jason dashes extending his arm to grab and enemy and start spinning them, 
Jason will not stop spinning unless he runs out of mp or the player usses one of the next commands:

--------------------------------

*while spinning:
A:                throw enemy
direction key+A:  ground slam
</J/D:            stop spinning

--------------------------------

*while catching:
direction key+A:  ground slam
J:                spin  
D^A:              uppercut
D>A:              super punch

--------------------------------



8) D^J      Power stance
description: Jason assumes a power stance generating energy which will block and repel incoming projectiles from his front side.
--------------------------------

*while in power stance:
when you get hit from front: Jason reacts with a "counter grab"
when you get hit from back: Jason reacts with a turn back elbow slam
A: turn back energy elbow slam
--------------------------------



Credits:

Marti Wong, original creator or character concept
Siegvar, providing base template for the elbow slam
YinYin, data work and AI scripting
prince_freeza, sprite work



______________________________________________________________________

                         www.lf-empire.de

             Little Fighter Empire: The official Fansite
                           ~ all you need ~