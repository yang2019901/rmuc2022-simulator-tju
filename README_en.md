# rmuc2022-simulator-tju

The rmuc2022-simulator-tju project (denoted as "simulator" below) is an open-source RMUC2022 simulator developed with Unity Engine by Yang Ming, who serves Beiyang Mecha Clan, which is an active team of TianJin University participating in RoboMaster Series Competition organized by DJI every year. The project is inspired by DJI Referee System Client as well as SimulatorX of South China Tigers Clan and is released under Mozilla Public License Version 2.0

## 1 Guidance of Project

The project is developed by Unity Editor 2021.3.8f1c1. Open the parent folder of `Assets/` in UnityHub to view and modify it.

Note 1: It usually takes longer when project is first opened by Unity Editor because of compilation. The process is expected to take 5-15 minutes, depending on your computer's hardware configuration.

Note 2: There are two scenes in the project, which are `Lobby.unity` for choosing play mode and create/join lobby for multiplayer, and `BattleField.unity` that simulates the 7-min real game, both of which are located in `Assets/Scenes/`. Double click one in Unity Editor to see hierarchy of GameObjects and more details.

Note 3: Build Settings, e.g. target platform of built executable, can be changed by pressing Ctrl-Shift-B.

Note 4: Feel free to file an issue or ask the author (most importantly :)

Note 5: log files are stored in `<simulator_folder>\rmuc_simu_preview_Data\YOUR_LOGS`.

## 2 Guidance of Release

The simulator can be either stand-alone or online. When online, the room creator's computer is a server (denoted as server PC), and other players (denoted as client PC) can join the room through LAN connection and ipv6 connection. (If it is not in the same LAN and no ipv6 address, it can be connected by means of internal network penetration). 

### 2.1 Network speed requirements

The network bandwidth of server PC and client PC is recommended to be no less than 1Mb/s, and the theoretical value of network speed occupation is about 10Kb/s to 20Kb/s. The network latency should be as low as possible to ensure the consistent behavior of the robots on the client PC and the server PC. (The simulator data is summarized in the server PC operation, and all data are subject to the operation results on the server PC)

### 2.2 Hardware Requirements

There is no strict limitation on computer hardware (RTX or not does NOT matter). But apparently, good hardware can provides you higher FPS and thus better experience.

At present, simulator does not apply LOD, so the number of triangular faces in the scene is large, which is demanding for CPU and graphics card. However,  the 9th generation i5+GTX1650 laptop can work well under 90 frames.

### 2.3 Operation Guidance

After entering the BattleField scene, keys of each robot are as follows:

Note 1: Press the `~` key to open the Settings screen, select Quit to exit the current competition, and return to the Lobby.

Note 2: The operation in the project bracket is in the gripper mode, such as: `R` key => extend/retracting rescue card; `Lshift-R` key => Gripper on/off suction.

| key             | infantry                                                     | hero                                                         | engineer                                                     | drone                                                        |
| --------------- | ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ |
| `Esc`           | lock/unlock the mouse                                        | lock/unlock the mouse                                        | lock/unlock the mouse                                        | lock/unlock the mouse                                        |
| `~`             | settings window                                              | settings window                                              | settings window                                              | settings window                                              |
| `O`             | ammunition window                                            | ammunition window                                            | ammunition window                                            | ammunition window                                            |
| `W`/`A`/`S`/`D` | forward/left/right/backward                                  | forward/left/right/backward                                  | forward/left/right/backward of robot (or of claws)           | forward/left/right/backward                                  |
| `R`             | \                                                            | \                                                            | extend/retract revive card（or turn on/off the claws suction） | call air support                                             |
| `C`             | turn on/off capacitance                                      | turn on/off capacitance                                      | none  (flip claws 90 degree out)                             | \                                                            |
| `Z`             | \                                                            | \                                                            | none (flip claws 90 degree in)                               | \                                                            |
| `X`             | brake                                                        | brake                                                        | brake                                                        | \                                                            |
| `E`/`Q`         | \                                                            | \                                                            | turn left/right                                              | rise/fall                                                    |
| `Lshift`        | spin                                                         | spin                                                         | switch mode between manipulating robot and claws             | \                                                            |
| Mouse Left      | shoot                                                        | shoot                                                        | \                                                            | shoot                                                        |
| Mouse Right     | auto aim of robot armor if `R` is not pressed and of rune if `R` is pressed | auto aim of robot armor if `R` is not pressed and of rune if `R` is pressed | \                                                            | auto aim of robot armor if `R` is not pressed and of rune if `R` is pressed |
| Move Mouse      | look around                                                  | look around                                                  | look around                                                  | look around                                                  |

## 3 Demonstration

### 3.1 Login

The user fills in the nickname (i.e. Player Name) here.

No more than six half-corner characters are recommended, if you don't want the nickname to be incomplete.

Every time you start simulator, the default is the name of your computer.

<img src="README.assets\log_in.png" alt="log_in" style="zoom: 67%;" />

### 3.2 Play mode selection

`Single Player`: play alone.

`Multi Player`: creates rooms and allows other players to join.

`Back`: Returns to the login screen.

<img src="README.assets\select_mode.png" alt="select_mode" style="zoom:67%;" />

### 3.3 Robot Type Selection

`Start Game` is only available for homeowners. Click to start the game and switch the scene to BattleField.

Click the name region (on the left of index if red clan, on the right of index if blue clan) to select the robot. When selected, the non-lobbyowner's `Start Game` is displayed as `Cancel Ready`, which is used to inform the lobbyowner whether you are ready, but does not affect the `Start Game` execution (this is to prevent malicious not-starting-game).

<img src="README.assets\select_robot.png" alt="select_robot" style="zoom:67%;" />

### 3.4 Battlefield

The UI simulates the operation interface of the real competition (but does not support customization, unless you modify the source code of project). At the same time, the capacitance and robot experience indicator bar are added in the lower left corner.

<img src="README.assets\in_game1.png" alt="in_game1" style="zoom: 33%;" />

drive safe :)

<img src="README.assets\in_game2.png" alt="in_game2" style="zoom: 33%;" />

## 4 Acknowledgement

Thanks to DJI Innovation for creating the RoboMaster series competition, whose competition mechanism is awesome and challenging, and the competition field is beautiful and full of technology. The UI and CG related to the Battlefield of simulator are made according to its style. Without the elaborate competition mechanism designed by DJI, simulator would be pale and boring.

Thanks to the SimulatorX released by the South China Tigers Clan in 2021, it was the first RMUC simulator I've seen, and it also motivates me to develop a simulator from scratch. Without their pioneering idea, the will and effort wouldn't be made to build a simulator.

Thanks to every member of the Beiyang Mech Clan, who selflessly provided high-precision 3D models of all the robots in the 2022 season, and made important contributions to improving the visual and physical fidelity of simulator.

Special thanks to Zhang Chenrui and Cao Hongyu, who actively participated in the validating of simulator's network connection and multi-player confrontation during the development and pre-release process. They also provided a lot of valuable suggestions in terms of robot control simulation and triangular surface optimization, which directly contributed to the optimization and iteration of simulator.

## 5 Feedback

Feel free to contact the author if you have any questions or suggestions, including but not limited to mechanism bugs, model overlapping, operational difficulties, UI beautification, etc.

QQ: 1308592371; Email: mingyang2601@outlook.com.

Enjoy it!
