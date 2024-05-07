# MccDev260.Logger
## Features
- Generates a text log file that can be used for builds or toggled for use with the editor.
- Logs the following basic session data:
    - FPS data (Average, median, highest and lowest).
    - Device specs.
    - Play session length.
- Configurable within the editor and some settings can be changed in builds via an optionally generated config.json file.
- Log can be written to via static methods.
- Designed to have as minimal overhead as possible.

## How to use:
Drag and drop *Logger/====Logger====.prefab* into the scene, configure settings detailed below as needed.

## Example Output file
    06/05/2024 20:31:56
    Example Project v1.2.3
    Note: Something Noteworthy
    Unique System Identifier: 123abc

    ===!#! EDITOR !#!===

    # System Info...
    OS: Windows 11  (10.0.22631) 64bit
    Graphics Driver: Direct3D 11.0 [level 11.1]
    Battery Status: Discharging

    ## Hardware...
    - CPU -
    Model: AMD Ryzen 7 7735HS with Radeon Graphics 
    Hardware Threads: 16
    Frequency: 3194 MHz
    - GPU -
    Device Vendor: NVIDIA
    Device Vendor ID: 4318
    Model: NVIDIA GeForce RTX 4060 Laptop GPU
    Device ID: 10400
    - Memory -
    RAM: 15610 MB
    VRAM: 7957 MB

    ====== Update Loop Start ======
    [20:31:56] -> FPSCounter: loaded!
    ====== Update Loop End   ======

    Session Length: 0 hours : 0 mins : 7 secs

    # FPS...
    Average: 189.2042
    Median: 221.3109
    Highest: 245.2371
    Lowest: 118.6466

## Output Path
By default the log is generated in the *Logger* directory which can be found at the following paths:

### Editor (If enabled)
...\YourProjectDir\Assets\Logger

### Windows Build
....\YourBuildDir\YourProjectName_Data\Logger

## Components
### Logger
![Logger inspector view.](/docs/imgs/Logger-Inspector.png)
#### Inspector Options

| **Setting** | **Effect** |
|:---:|:---:|
| Global Settings | Scriptable Object that holds shared data between the logger and Fps counter. If missing, can be created at *Assets/Create/MccDev260/LoggerSettings*. |
| Log File Name | Desired name of the output text file. |
| Log Note | Note thats printed in the header of the file. |
| Include note in file name? | Weather or not the note should be included in the file name eg. LogFileName-LogNote.txt |
| Overwrite Output? | If false, will generate a new file each time the game is run with the following naming structure: LogFileName[-LogNote?]_CreationDate&Time.txt |
| Output In Unique Id folder? | If true, Log file output path will be *.../Logger/[YourDeviceId]/LogFile.txt* |
| Include Hardware Info? | If true, will include system specs of the device game is  running on. |
| Generate Config Json In Build? | (Builds Only) If true, will generate a config json in the root *Logger* directory. |

#### Static Members
| **Method** | **Result** |
|:---:|:---:|
| bool Write(string) | Writes a single line to the log, preceded by current time, returns false if write is unsuccessful. |
| bool Write(string[]) | Write multiple lines from an array to the log file. More efficient than multiple calls to Write(string). Each element is treated as a new line preceded by current time, Empty strings are treated as blank lines. returns false if  write is unsuccessful. |
| event LogSessionData | Event used to signal when performance data should be collected. Is invoked on application quit before results are logged.  |

---

### FPS Counter
![FPS Counter inspector view.](/docs/imgs/FpsCounter-Inspector.png)
#### Inspector Options

| Setting | Effect |
|:---:|:---:|
| Global Settings | Scriptable Object that holds shared data between the logger and Fps counter. If missing, can be created at *Assets/Create/MccDev260/LoggerSettings*. |
| FPS Update Interval | Time in seconds fps data is collected. |
| Max Recorded FPS Count | Max amount of recent fps data that will be recorded to keep memory in check. Once the recorded data goes over this amount the oldest values are removed. Don't set too low or this will skew the average fps value. |

### Global Settings
![Global settings inspector view.](/docs/imgs/GlobalSettings-Inspector.png)
#### Inspector Options
| Setting | Effect |
|:---:|:---:|
| Record In Editor? | if true, will enable logger to be used in the editor. See [Output Path](#output-path). |