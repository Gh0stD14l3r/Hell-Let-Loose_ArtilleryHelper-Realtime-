# Realtime Artillery Calculator

This console application will scan your game screen then translate the image region to text. 
It will then perform a realtime distance calculation automatically while you are on artillery.

Due to the dependencies you would need to run this as a script instead of compiling. When you are
in game just run the script from visual studio. Jump on artillery and direct it to where you want to shell.
Adjust the MIL up or down for distance and the script will output the distance for you.

The current timeout is 500ms you can adjust this from 

```
Thread.Sleep(500);
```




![image](https://user-images.githubusercontent.com/38970826/229940552-74dff522-c154-45f9-9226-4d3d40d11dda.png)
