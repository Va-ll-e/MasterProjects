# Master Projects

[![Unity]([https://img.shields.io/badge/unity-2022.3%2B-black.svg?style=flat&logo=unity](https://img.shields.io/badge/Unity-100000?style=for-the-badge&logo=unity&logoColor=white))](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

This repository serves as the central hub for all the individual experiments, prototypes, and sub-projects that will eventually be combined into my final Master's project.

It contains a collection of small-to-medium sized Unity projects focused on Mixed Reality (XR), robotics integration, hand tracking interaction, hardware interfacing, and various proof-of-concept features. Each folder is designed to be mostly self-contained so I can test ideas quickly, iterate, and later cherry-pick the best parts for the final application.

## Main Goals of this Repository
- Rapid prototyping of XR interactions in Unity
- Testing hardware integrations (Hololens 2, Franka Emika FR3 robot, etc.)
- Experimenting with hand-tracking UI, collision safety systems, digital twins, etc.
- Keeping everything organized and reusable for the final master thesis project

## Highlighted Projects

### Hand Menu (XR Interaction Toolkit based)
A lightweight, cleaned-up version of the official Unity XR Interaction Toolkit hand menu example.

**Features**
- Show the palm of your left hand → radial menu appears near the hand
- Configurable behavior:
  - **Follow mode**: menu follows hand movement
  - **Toggle mode**: menu stays in place after activation
- Reduced complexity and scene overhead while preserving important functionality
- Works reliably on Meta Quest 3, Hololens 2 and in Unity Editor (OpenXR / Windows MR)

**Folder**: `/UnityProjects/Hand Menu`
**Quick Access**: `/QuickAssets/Hand_UI`

### Franka FR3 Digital Twin & Safety System
Real-time bidirectional connection between a physical **Franka Emika Research 3** robot and its digital twin inside Unity.

**Features**
- Real robot joint positions → digital twin updates in Unity (via `libfranka` + ROS2 TCP interface)
- Unity digital twin → real robot (optional position streaming)
- Collision detection in Unity → automatically sends **emergency stop** to the real robot
- Tested and working on:
  - Unity Editor (Windows)
  - Microsoft Hololens 2 (deployed standalone build)

**Folder**: `/UnityProjects/Franka FR3`


## Requirements
- Depends on the project, but in general:
- Unity 6 or newer
- XR Plugin Management + OpenXR
- XR Interaction Toolkit
- For Franka projects: libfranka + custom server running on the robot's control PC


## How to Run a Project
1. Clone the repository
2. Open the desired subfolder with Unity Hub (each contains its own `ProjectSettings`)
3. Switch platform to **Windows → x86_64** (for PC) or **UWP → ARM64** (for Hololens 2)
4. Enter Play mode or build & deploy



## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

Feel free to open issues if you have suggestions or find bugs in any of the experiments. Contributions are welcome (especially clean-ups and performance improvements)!
