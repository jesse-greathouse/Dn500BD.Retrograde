# DN 500BD Retrograde

**DN 500BD Retrograde** is a Windows application written in C# (.NET 8 & WinUI 3) for controlling the **Denon DN-500BD Blu-ray Disc Player** via its RS232 serial port. It features a GTK-based graphical interface with command buttons mapped to Denon's documented control protocol.

---

## 🚀 Getting Started

### ✅ Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Windows App Runtime (see [Latest Windows App SDK]https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads))
- Windows 10 or 11

## 🏗️ Windows Build & Deployment (WinUI 3)

```bash
dotnet restore
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained
```

### To sideload
- Enable Developer Mode in Windows Settings → Privacy & Security → For Developers
- Run the generated .msixbundle or install using PowerShell:

```bash
Add-AppxPackage -Path .\path\to\your.msixbundle
```
---

## 🧩 Hardware Requirements

### 🔌 Recommended RS232 Adapter

To communicate with the Denon DN-500BD device, you need a **USB to RS232 serial adapter**. The recommended model:

> **USB Serial Adapter with FTDI Chipset (USB 2.0 to DB9 Male RS232)**
> Compatible with Windows 11/10/8/7/Vista/XP, Linux, and macOS.

- Cable length: 6 feet (1.8 meters)
- Chipset: FTDI (preferred for stability and compatibility)
- Connector: USB-A to DB9 Male (RS232)

---

### 🖥️ Connecting the Cable

To connect your PC to the **Denon DN-500BD**:

- **PC Side:** Use the USB-A plug to connect to your PC.
- **DN-500BD Side:** Use a **null-modem (crossed)** DB9-to-DB9 cable (if the serial adapter is DB9 Male, and Denon device also uses DB9 Female).
- Make sure the COM port is visible in Device Manager under "Ports (COM & LPT)".

---

## 🕹️ How to Use the Application

1. **Launch the Application**
   - Open a terminal and run `dotnet run`
   - The main window will appear.

2. **Select COM Port**
   - Use the dropdown at the top to select the COM port of your USB-to-RS232 adapter.

3. **Connect**
   - Click the **Connect** button to open the serial connection.

4. **Control the Player**
   - Use the directional buttons (Up, Down, Left, Right, Enter, Home) to send commands to the player.
   - Commands are immediately dispatched over RS232 and logged in the console.
   - Response data from the player is read and logged (currently via standard output).

> More buttons and features are under development, including playback control, subtitle switching, and advanced menu navigation.

## 🛠️ Utilities

### `audit.ps1`

The `audit.ps1` script is a PowerShell utility for auditing the current project structure and contents. It recursively prints out the contents of all source files matching a given mask (e.g. `*.cs`, `*.sln`, etc.) for quick code inspection, documentation, or review purposes.

**Usage:**

```powershell
.\audit.ps1 -Mask "*.cs"
```

This will display all .cs files in the repository, grouped by filename and directory.

> Tip: You can use multiple masks by separating them with a pipe (|), e.g. *.cs|*.sln.

---

## 📖 Denon DN-500BD Command Reference

The Denon DN-500BD accepts commands over RS232 using a proprietary ASCII protocol. Below is a human-readable summary of common commands.

| Function                | Command         | Description                             |
|------------------------|-----------------|-----------------------------------------|
| Power On               | `@0PW00\r`      | Turns the device on                     |
| Power Off              | `@0PW01\r`      | Turns the device off                    |
| Stop                   | `@02354\r`      | Stops playback                          |
| Play                   | `@02353\r`      | Starts playback                         |
| Pause                  | `@02348\r`      | Pauses playback                         |
| Next Track             | `@02332\r`      | Skips to the next track/chapter         |
| Previous Track         | `@02333\r`      | Skips to the previous track/chapter     |
| Up                     | `@0PCCUSR3\r`   | Cursor Up                               |
| Down                   | `@0PCCUSR4\r`   | Cursor Down                             |
| Left                   | `@0PCCUSR1\r`   | Cursor Left                             |
| Right                  | `@0PCCUSR2\r`   | Cursor Right                            |
| Enter                  | `@0PCENTR\r`    | Executes selected option                |
| Home                   | `@0PCHM\r`      | Returns to the HOME menu                |
| Setup Menu             | `@0PCSU\r`      | Opens the Setup menu                    |
| Top Menu (Disc Menu)   | `@0DVTP\r`      | Shows the title menu from the disc      |
| Pop-up Menu            | `@0DVPU\r`      | Displays the pop-up menu                |
| Return                 | `@0PCRTN\r`     | Return to previous menu                 |
| Lock Panel             | `@023KL\r`      | Disables physical panel buttons         |
| Unlock Panel           | `@023KU\r`      | Enables physical panel buttons          |
| Tray Open              | `@0PCDTRYOP\r`  | Opens the disc tray                     |
| Tray Close             | `@0PCDTRYCL\r`  | Closes the disc tray                    |
| Set A for A-B Repeat   | `@0PCRPAF\r`    | Marks point A                           |
| Set B & Start Repeat   | `@0PCRPBF\r`    | Marks point B and starts repeat         |
| Exit A-B Repeat        | `@0PCEXRP\r`    | Exits repeat mode                       |
| Subtitle Select        | `@0DVSBTL1\r`   | Selects primary subtitle                |
| Audio Dialog Primary   | `@0DVADLG+\r`   | Primary audio dialog                    |
| Angle Next             | `@0DVANGL+\r`   | Changes angle on supported discs        |
| Display Info           | `@0DVDSIF\r`    | Shows information on screen             |
| Red/Green/Blue/Yellow  | `@0DVFCLR1\r`–`\r4` | Function/color button (red to yellow)   |
| Mute On                | `@0mt00\r`      | Mutes the audio                         |
| Mute Off               | `@0mt01\r`      | Unmutes the audio                       |
| Program Mode On        | `@0PCPMP00\r`   | Enables program playback                |
| Program Mode Off       | `@0PCPMP01\r`   | Disables program playback               |
| Random Mode            | `@0PCPMR\r`     | Enables random/shuffle playback         |
| Hide OSD On            | `@0DVHOSD01\r`  | Hides all OSD elements                  |
| Hide OSD Off           | `@0DVHOSD00\r`  | Shows OSD                               |
| Disc Tray Open         | `@0PCDTRYOP\r`  | Ejects disc                             |
| Disc Tray Close        | `@0PCDTRYCL\r`  | Closes tray                             |
| Display Timecode Mode  | `@0PCTMDTL\r`   | Switches to total elapsed time          |

> For full specifications, refer to the official [Denon RS232 Protocol documentation for the DN-500BD](https://raw.githubusercontent.com/jesse-greathouse/dn-retrograde/refs/heads/main/Denon%20DN-500BD_MKII_Protocol_Guide_1.0.pdf).

---

## 📌 Status

- ✅ Serial port enumeration
- ✅ Connect/disconnect support
- ✅ Basic directional controls (Up/Down/Left/Right/Enter/Home)
- ⏳ Playback and menu commands (coming soon)
- ⏳ Timecode entry and response logging (in progress)
- 🛠️ Cross-platform GTK compatibility (requires validation)
