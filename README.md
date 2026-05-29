# Calculator in the Air | 悬浮计算器

[English](#calculator-in-the-air) | [简体中文](#简体中文-chinese)

---

## Calculator in the Air

`Calculator in the Air` is a lightweight, modern, macOS Spotlight-style floating mathematical calculator for Windows. It runs persistently in the background, triggers instantly via a global hotkey, calculates expression values in real-time, copies the result on Enter, and stays tucked away in the system tray. 

It is packaged as a single portable `.exe` file that requires **no installation**—just double-click and run!

### Features

- **Spotlight-like Overlay**:
  - Centers on the screen 20% from the top.
  - **Multi-Monitor Aware**: Automatically centers on the active monitor where the mouse cursor is located.
  - Custom drag support: Drag it anywhere if it blocks your view.
- **Glassmorphic Design**:
  - Frameless, borderless window with translucent dark backing.
  - Glowing, luminous border using a purple-to-cyan gradient edge.
  - Soft, hardware-accelerated drop shadows to float above other windows.
- **Real-time Math Evaluation**:
  - Dynamically tokenizes and parses expressions as you type.
  - **Implicit Multiplication**: Treats expressions like `2pi` or `2(3+4)` naturally as multiplication.
  - Collapses result panel automatically on incomplete inputs (silent error reporting).
- **Instant Actions**:
  - Press `Alt + Space` (default) to toggle the window.
  - Press `Enter` to copy the calculated result to your clipboard and close the window instantly.
  - Press `Esc` to hide the window.
  - Runs in a single instance; launching the app again simply wakes up the running background process.
- **Interactive System Tray**:
  - Custom-drawn tray icon (a purple-cyan gradient block with a white `=` sign) built dynamically in memory at runtime.
  - Right-click tray menu to show, open settings, or exit.
- **Modern Settings Dialog**:
  - **Interactive Hotkey Recorder**: Click to record any custom global hotkey combination (supports Ctrl, Alt, Shift, Win).
  - Set calculation precision (decimal places or Auto).
  - Toggle behaviors like focus-loss hiding (`Hide on focus lost`) and Enter-copying.
- **Localization**:
  - Full support for **Simplified Chinese (简体中文)** and **British English (en-GB)**.
  - Automatically matches the system culture or can be set manually.
- **Portable & Dependency Free**:
  - Built with .NET 10.0 WPF.
  - Bundled as a self-contained single `.exe` file (~50MB) containing the runtime. Works on any 64-bit Windows machine.

### Supported Math Expressions

#### Arithmetic & Operators
- `+`, `-`, `*`, `/`, `%` (modulo), `^` (power)
- Parentheses: `( )` for grouping
- Unary signs: `-5`, `+3`
- Implicit multiplication: `2pi (3 + sqrt(25))`

#### Constants
- `pi` / `π` : Ratio of a circle's circumference to its diameter (`3.14159265...`)
- `e` : Euler's number (`2.71828182...`)
- `tau` : Turn constant (`2 * pi` = `6.28318530...`)

#### Functions
- Trigonometric: `sin(x)`, `cos(x)`, `tan(x)` (parameters in radians)
- Inverse Trig: `asin(x)` / `arcsin(x)`, `acos(x)` / `arccos(x)`, `atan(x)` / `arctan(x)`
- Roots & Powers: `sqrt(x)` (square root), `cbrt(x)` (cube root), `exp(x)` ($e^x$)
- Logarithms: `log(x)` (base-10), `log(x, base)` (custom base), `ln(x)` (natural log)
- Miscellaneous: `abs(x)` (absolute value), `floor(x)`, `ceil(x)`, `round(x)` (round to nearest integer), `round(x, decimals)` (round to specific precision)

##### Examples
- `2pi * 5` $\rightarrow$ `31.4159265359`
- `sqrt(3^2 + 4^2)` $\rightarrow$ `5`
- `sin(pi/2) + log(100)` $\rightarrow$ `3`
- `round(2.71828, 2)` $\rightarrow$ `2.72`

### How to Get It

This project uses **GitHub Actions** to build the application automatically. You do not need to compile it yourself:
1. Go to the **GitHub Repository** page.
2. Click on **Releases** on the right side, or click on the **Actions** tab.
3. Download the latest `CalculatorInAir.exe` from the Release assets or workflow build artifacts.
4. Move `CalculatorInAir.exe` to any folder on your computer (e.g. your User folder or Desktop).
5. Double-click it. It will start running in the background and sit in your taskbar system tray.

### Configuration File Location

Settings are saved in JSON format under the user's local application data folder:
`%LOCALAPPDATA%\CalculatorInAir\settings.json`

If you ever wish to reset settings to default, simply delete this file or click `Exit` in the tray menu and delete it before relaunching.

### Building Locally

If you wish to build the executable from source code, make sure you have the [.NET 10.0 SDK](https://dotnet.microsoft.com/download) installed on your system.

Open a command prompt (cmd/PowerShell) in the project directory and run:

```bash
# Restore project dependencies
dotnet restore src/CalculatorInAir/CalculatorInAir.csproj

# Compile and package as a single self-contained executable
dotnet publish src/CalculatorInAir/CalculatorInAir.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish
```

The output portable binary `CalculatorInAir.exe` will be located inside the `./publish` directory.

---

## 简体中文 (Chinese)

`Calculator in the Air`（悬浮计算器）是一款轻量级、现代化、类 macOS Spotlight 的 Windows 悬浮数学计算器。它常驻后台运行，支持通过全局快捷键瞬间呼出，实时计算数学表达式的值，按 Enter 键即可自动复制结果并隐藏，且安静地收纳在系统托盘中。

它被打包为单个便携式 `.exe` 文件，**无需安装**——双击即可运行！

### 功能特性

- **类 Spotlight 悬浮窗**：
  - 屏幕水平居中，垂直方向位于屏幕顶部向下 20% 处。
  - **多显示器感知**：自动在鼠标光标所在的活动显示器中心弹出。
  - 支持自定义拖拽：如果挡住了视线，可以按住任意空白处将其拖动到任意位置。
- **现代化毛玻璃美学**：
  - 无边框设计，配有优雅的半透明暗色毛玻璃背景。
  - 采用紫青渐变边缘的流光发光边框。
  - 带有柔和的、硬件加速的窗口投影，使其立体浮现于其他窗口之上。
- **实时数学计算**：
  - 随着您的输入动态分词并实时解析、计算表达式。
  - **隐式乘法**：自然支持类似 `2pi` 或 `2(3+4)` 的隐式乘法运算。
  - 输入未完成或有误时自动收起结果面板（无干扰的静默错误处理）。
- **便捷操作**：
  - 按 `Alt + Space`（默认）即可全局快速呼出/隐藏窗口。
  - 按 `Enter` 键即可将计算结果复制到剪贴板，并立即隐藏窗口。
  - 按 `Esc` 键隐藏窗口。
  - 单实例运行：再次启动应用只会唤醒已在后台运行的进程，不会重复启动。
- **系统托盘常驻**：
  - 运行时在内存中动态构建的自定义托盘图标（带白色 `=` 号的紫青渐变方块）。
  - 右击托盘图标可显示菜单，用于快速显示窗口、打开设置或退出程序。
- **现代化设置面板**：
  - **交互式快捷键录制器**：点击即可录制任意自定义全局快捷键组合（支持 Ctrl、Alt、Shift、Win 等修饰键）。
  - 设置计算结果的精度（自定义保留的小数位数，或设置为自动）。
  - 切换失去焦点时隐藏 (`Hide on focus lost`) 以及回车自动复制等行为。
- **双语界面**：
  - 完整支持**简体中文**与**英国英语 (en-GB)**。
  - 自动匹配系统语言，亦可在设置面板中手动进行切换。
- **纯绿色免安装**：
  - 基于 .NET 10.0 WPF 构建。
  - 打包为包含完整运行时的独立单 `.exe` 文件（约 50MB），在任何 64 位 Windows 电脑上均可即开即用。

### 支持的数学表达式

#### 基础运算与符号
- `+`, `-`, `*`, `/`, `%` (取模), `^` (幂运算)
- 括号：`( )` 用于分组与改变优先级
- 单目符号：`-5`, `+3`
- 隐式乘法：`2pi (3 + sqrt(25))`

#### 常数
- `pi` / `π`：圆周率 (`3.14159265...`)
- `e`：自然常数 (`2.71828182...`)
- `tau`：双倍圆周率 (`2 * pi` = `6.28318530...`)

#### 函数
- 三角函数：`sin(x)`, `cos(x)`, `tan(x)`（参数单位为弧度）
- 反三角函数：`asin(x)` / `arcsin(x)`, `acos(x)` / `arccos(x)`, `atan(x)` / `arctan(x)`
- 开方与指数：`sqrt(x)`（平方根）, `cbrt(x)`（立方根）, `exp(x)`（$e^x$）
- 对数：`log(x)`（以10为底）, `log(x, base)`（自定义底数）, `ln(x)`（自然对数）
- 其他函数：`abs(x)`（绝对值）, `floor(x)`（向下取整）, `ceil(x)`（向上取整）, `round(x)`（四舍五入到最近的整数）, `round(x, decimals)`（四舍五入到指定的小数位数）

##### 示例
- `2pi * 5` $\rightarrow$ `31.4159265359`
- `sqrt(3^2 + 4^2)` $\rightarrow$ `5`
- `sin(pi/2) + log(100)` $\rightarrow$ `3`
- `round(2.71828, 2)` $\rightarrow$ `2.72`

### 如何获取运行

本工程配置了 **GitHub Actions** 自动构建发布。您不需要自己编译它：
1. 前往 **GitHub 仓库** 页面。
2. 点击右侧的 **Releases** 链接，或点击 **Actions** 选项卡。
3. 从 Release 附件中或 workflow 运行结果的 Artifacts 中下载最新的 `CalculatorInAir.exe`。
4. 将 `CalculatorInAir.exe` 移动到电脑的任意文件夹中（例如个人用户文件夹或桌面）。
5. 双击运行即可。它将在后台启动并静默驻留在任务栏系统托盘中。

### 配置文件路径

所有的用户设置都以 JSON 格式保存在用户的本地应用数据文件夹下：
`%LOCALAPPDATA%\CalculatorInAir\settings.json`

如果您想将所有设置重置为默认值，只需退出程序，删除该文件，然后重新运行即可。

### 本地编译

如果您希望从源码编译该可执行文件，请确保系统已安装 [.NET 10.0 SDK](https://dotnet.microsoft.com/download)。

在项目根目录下打开命令行（cmd/PowerShell）并运行：

```bash
# 还原项目依赖项
dotnet restore src/CalculatorInAir/CalculatorInAir.csproj

# 编译并打包为独立的单文件可执行文件
dotnet publish src/CalculatorInAir/CalculatorInAir.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish
```

输出的绿色便携二进制文件 `CalculatorInAir.exe` 将被保存在 `./publish` 目录下。
