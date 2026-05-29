# Calculator in the Air | 悬浮计算器

[English](#calculator-in-the-air) | [简体中文](#简体中文-chinese)

---

## Calculator in the Air

`Calculator in the Air` is a lightweight, modern, macOS Spotlight-style floating mathematical calculator for Windows. It runs persistently in the background, triggers instantly via a global hotkey, calculates expression values in real-time, copies the result on Enter, and stays tucked away in the system tray. 

It is packaged as a single portable `.exe` file that is **completely self-contained**—no installation and no .NET runtime required! Just run it!

### Features

- **Spotlight-like Overlay**:
  - Centers on the screen 20% from the top (600px width).
  - **Multi-Monitor Aware**: Automatically centers on the active monitor where the mouse cursor is located.
  - Custom drag support: Drag it anywhere if it blocks your view.
- **Glassmorphic Design & Theme Adaptability**:
  - Frameless, borderless window utilizing native Windows 11 Acrylic backdrop (`DwmSetWindowAttribute`), with graceful fallbacks for older Windows versions.
  - Soft, hardware-accelerated drop shadows to float above other windows.
  - **Dynamic Theme Adaptation**: Automatically adapts to Windows native light/dark mode settings, or can be overridden manually (powered by WPF XAML ResourceDictionaries).
- **Real-time Math Evaluation & Conversions**:
  - Dynamically tokenizes and parses expressions as you type.
  - **Implicit Multiplication**: Treats expressions like `2pi` or `2(3+4)` naturally as multiplication.
  - **Unit Conversion**: Built-in support for length, weight/mass, and temperature conversions (e.g., `10 m to cm`, `98.6 f in c`).
  - **Result History (`ans`)**: Reuse the last calculated result easily with the `ans` constant.
  - **Input History**: Use the `Up` and `Down` arrow keys to navigate through your previous calculations.
  - **Error Feedback**: Silent error handling while typing, but provides a visual shake animation and red text if you attempt to submit an invalid expression.
- **Instant Actions**:
  - Press `Alt + Space` (default) to toggle the window.
  - Press `Enter` to copy the calculated result to your clipboard and close the window instantly.
  - Press `Esc` to hide the window.
  - Runs in a single instance; launching the app again simply wakes up the running background process.
- **Interactive System Tray**:
  - Custom-drawn tray icon (a beautiful violet-to-blue gradient block with a white `=` sign) built dynamically in memory at runtime.
  - Right-click tray menu to show, open settings, or exit.
- **Modern Settings Dialog**:
  - **Interactive Hotkey Recorder**: Click to record any custom global hotkey combination (supports Ctrl, Alt, Shift, Win).
  - **Theme Selection**: Choose between "Follow System (Auto)", "Dark Mode", or "Light Mode".
  - Set calculation precision (Auto or 0 to 10 decimal places).
  - Toggle behaviors like focus-loss hiding (`Hide when focus is lost`) and Enter-copying.
- **Localization**:
  - Full support for **Simplified Chinese (简体中文)** and **British English (en-GB)** using dynamically loaded XAML dictionaries.
  - Automatically matches the system culture or can be set manually.
- **Portable & Ready-to-Run**:
  - Built with .NET 10.0 WPF.
  - Published as an optimized, `ReadyToRun`, **self-contained** single `.exe` file. It includes the necessary .NET runtime components, so it runs out-of-the-box on any supported Windows x64 machine.

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
- `ans` : The result of the last successful calculation

#### Functions
- Trigonometric: `sin(x)`, `cos(x)`, `tan(x)` (parameters in radians)
- Inverse Trig: `asin(x)` / `arcsin(x)`, `acos(x)` / `arccos(x)`, `atan(x)` / `arctan(x)`
- Roots & Powers: `sqrt(x)` (square root), `cbrt(x)` (cube root), `exp(x)` ($e^x$)
- Logarithms: `log(x)` (base-10), `log(x, base)` (custom base), `ln(x)` (natural log)
- Miscellaneous: `abs(x)` (absolute value), `floor(x)`, `ceil(x)`, `round(x)` (round to nearest integer), `round(x, decimals)` (round to specific precision)

#### Unit Conversion
Convert values between different units using the syntax `<expression> <unit> to <target>` or `<expression> <unit> in <target>`.

- **Length**: `m` (`meter`/`meters`/`米`), `cm` (`centimeter`/`centimeters`/`厘米`), `mm` (`millimeter`/`millimeters`/`毫米`), `km` (`kilometer`/`kilometers`/`千米`/`公里`), `in` (`inch`/`inches`/`英寸`), `ft` (`foot`/`feet`/`英尺`), `yd` (`yard`/`yards`/`码`), `mi` (`mile`/`miles`/`英里`)
- **Weight/Mass**: `kg` (`kilogram`/`kilograms`/`千克`/`公斤`), `g` (`gram`/`grams`/`克`), `mg` (`milligram`/`milligrams`/`毫克`), `lb` (`lbs`/`pound`/`pounds`/`磅`), `oz` (`ounce`/`ounces`/`盎司`)
- **Temperature**: `c` (`celsius`/`摄氏度`), `f` (`fahrenheit`/`华氏度`), `k` (`kelvin`/`开尔文`)

##### Examples
- `2pi * 5` $\rightarrow$ `31.4159265359`
- `sqrt(3^2 + 4^2)` $\rightarrow$ `5`
- `sin(pi/2) + log(100)` $\rightarrow$ `3`
- `round(2.71828, 2)` $\rightarrow$ `2.72`
- `10 m to cm` $\rightarrow$ `1000`
- `100 f in c` $\rightarrow$ `37.7777777778`
- `5 lb to kg` $\rightarrow$ `2.26796185`
- `ans + 5` $\rightarrow$ `7.26796185` (if last result was `2.26796185`)

### How to Get It

This project uses **GitHub Actions** to build the application automatically. Rolling releases are available for every push to `main`, and stable releases are created for version tags.
1. Go to the **GitHub Repository** page.
2. Click on **Releases** on the right side, or click on the **Actions** tab.
3. Download the latest `CalculatorInAir.exe` from the Release assets or workflow build artifacts.
4. Move `CalculatorInAir.exe` to any folder on your computer.
5. Double-click it. It will run in the background and sit in your taskbar system tray.

> [!NOTE]
> The published executable is built as a self-contained single file. This means the .NET runtime is bundled directly inside it, so you don't need to install any external dependencies!

### Configuration File Location

Settings are saved in JSON format under the user's local application data folder:
`%LOCALAPPDATA%\CalculatorInAir\settings.json`

If you ever wish to reset settings to default, simply delete this file or click `Exit` in the tray menu and delete it before relaunching.

### Building Locally

If you wish to build the executable from source code, make sure you have the [.NET 10.0 SDK](https://dotnet.microsoft.com/download) installed on your system.

Open a command prompt (cmd/PowerShell) in the project directory and run one of the following commands:

#### Running Unit Tests
To execute mathematical parser and unit conversion tests:
```bash
dotnet test tests/CalculatorInAir.Tests/CalculatorInAir.Tests.csproj
```

#### Publishing the Executable
The project is configured to build as a self-contained, single-file executable by default:
```bash
dotnet restore src/CalculatorInAir/CalculatorInAir.csproj
dotnet publish src/CalculatorInAir/CalculatorInAir.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -o ./publish
```

The output portable binary `CalculatorInAir.exe` will be located inside the `./publish` directory.

---

## 简体中文 (Chinese)

`Calculator in the Air`（悬浮计算器）是一款轻量级、现代化、类 macOS Spotlight 的 Windows 悬浮数学计算器。它常驻后台运行，支持通过全局快捷键瞬间呼出，实时计算数学表达式的值，按 Enter 键即可自动复制结果并隐藏，且安静地收纳在系统托盘中。

它被打包为单个便携式 `.exe` 文件，并且是**完全自包含 (Self-Contained) 的**——**无需安装**任何框架和运行时，双击即可运行！

### 功能特性

- **类 Spotlight 悬浮窗**：
  - 屏幕水平居中，垂直方向位于屏幕顶部向下 20% 处（宽度 600px）。
  - **多显示器感知**：自动在鼠标光标所在的活动显示器中心弹出。
  - 支持自定义拖拽：如果挡住了视线，可以按住任意空白处将其拖动到任意位置。
- **现代化玻璃美学与主题适配**：
  - 无边框设计，在 Windows 11 及以上系统原生支持 Acrylic 亚克力背景（旧系统可优雅降级）。
  - 带有柔和的、硬件加速的窗口投影，使其立体浮现于其他窗口之上。
  - **动态主题适配**：完美适配 Windows 原生深色/浅色模式，支持跟随系统自动切换或在设置中手动切换（基于 WPF XAML 资源字典动态加载）。
- **实时数学计算与单位换算**：
  - 随着您的输入动态分词并实时解析、计算表达式。
  - **隐式乘法**：自然支持类似 `2pi` 或 `2(3+4)` 的隐式乘法运算。
  - **单位换算**：支持长度、质量/重量、温度的便捷换算。语法非常直观，如 `10 m to cm` 或 `98.6 f in c` 等。
  - **历史计算引用 (`ans`)**：支持通过 `ans` 变量引用上一次的计算结果，方便进行连续计算。
  - **输入历史记录**：使用 `上 (Up)` 和 `下 (Down)` 方向键可以在之前的计算历史之间快速导航。
  - **错误反馈**：输入未完成时静默处理错误不打扰输入；但若包含错误的表达式被回车提交，窗口会左右抖动并以红色显示错误提示。
- **便捷操作**：
  - 按 `Alt + Space`（默认）即可全局快速呼出/隐藏窗口。
  - 按 `Enter` 键即可将计算结果复制到剪贴板，并立即隐藏窗口。
  - 按 `Esc` 键隐藏窗口。
  - 单实例运行：再次启动应用只会唤醒已在后台运行的进程，不会重复启动。
- **系统托盘常驻**：
  - 运行时在内存中动态构建的自定义托盘图标（带白色 `=` 号的蓝紫渐变方块）。
  - 右击托盘图标可显示菜单，用于快速显示窗口、打开设置或退出程序。
- **现代化设置面板**：
  - **交互式快捷键录制器**：点击即可录制任意自定义全局快捷键组合（支持 Ctrl、Alt、Shift、Win 等修饰键）。
  - **界面主题切换**：支持选择“跟随系统 (Auto)”、“暗黑模式”或“明亮模式”。
  - 设置计算结果的精度（自动或保留 0 到 10 位小数）。
  - 切换失去焦点时自动隐藏 (`Hide when focus is lost`) 以及回车自动复制等行为。
- **双语界面**：
  - 完整支持**简体中文**与**英国英语 (en-GB)**。基于动态 XAML 语言资源。
  - 自动匹配系统语言，亦可在设置面板中手动进行切换。
- **纯绿色免安装**：
  - 基于 .NET 10.0 WPF 构建。
  - 打包为开启 `ReadyToRun` 的**自包含 (Self-Contained)** 独立单 `.exe` 文件。程序已内嵌所需的 .NET 运行时，在支持的 x64 Windows 机器上开箱即用。

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
- `ans`：上一次成功计算的计算结果

#### 函数
- 三角函数：`sin(x)`, `cos(x)`, `tan(x)`（参数单位为弧度）
- 反三角函数：`asin(x)` / `arcsin(x)`, `acos(x)` / `arccos(x)`, `atan(x)` / `arctan(x)`
- 开方与指数：`sqrt(x)`（平方根）, `cbrt(x)`（立方根）, `exp(x)`（$e^x$）
- 对数：`log(x)`（以10为底）, `log(x, base)`（自定义底数）, `ln(x)`（自然对数）
- 其他函数：`abs(x)`（绝对值）, `floor(x)`（向下取整）, `ceil(x)`（向上取整）, `round(x)`（四舍五入到最近的整数）, `round(x, decimals)`（四舍五入到指定的小数位数）

#### 单位换算
使用 `<数值> <单位> to <目标单位>` 或 `<数值> <单位> in <目标单位>` 语法在不同的单位之间进行换算。

- **长度**：`m` (`meter`/`meters`/`米`), `cm` (`centimeter`/`centimeters`/`厘米`), `mm` (`millimeter`/`millimeters`/`毫米`), `km` (`kilometer`/`kilometers`/`千米`/`公里`), `in` (`inch`/`inches`/`英寸`), `ft` (`foot`/`feet`/`英尺`), `yd` (`yard`/`yards`/`码`), `mi` (`mile`/`miles`/`英里`)
- **重量/质量**：`kg` (`kilogram`/`kilograms`/`千克`/`公斤`), `g` (`gram`/`grams`/`克`), `mg` (`milligram`/`milligrams`/`毫克`), `lb` (`lbs`/`pound`/`pounds`/`磅`), `oz` (`ounce`/`ounces`/`盎司`)
- **温度**：`c` (`celsius`/`摄氏度`), `f` (`fahrenheit`/`华氏度`), `k` (`kelvin`/`开尔文`)

##### 示例
- `2pi * 5` $\rightarrow$ `31.4159265359`
- `sqrt(3^2 + 4^2)` $\rightarrow$ `5`
- `sin(pi/2) + log(100)` $\rightarrow$ `3`
- `round(2.71828, 2)` $\rightarrow$ `2.72`
- `10 m to cm` $\rightarrow$ `1000`
- `100 f in c` $\rightarrow$ `37.7777777778`
- `5 lb to kg` $\rightarrow$ `2.26796185`
- `ans + 5` $\rightarrow$ `7.26796185` (假设上次计算结果为 `2.26796185`)

### 如何获取运行

本工程配置了 **GitHub Actions** 自动构建发布。只要代码 push 到 `main` 分支即会自动构建出 rolling release，此外也支持 Tag 发布。
1. 前往 **GitHub 仓库** 页面。
2. 点击右侧的 **Releases** 链接，或点击 **Actions** 选项卡。
3. 从 Release 附件中或 workflow 运行结果的 Artifacts 中下载最新的 `CalculatorInAir.exe`。
4. 将 `CalculatorInAir.exe` 移动到电脑的任意文件夹中。
5. 双击运行即可。它将在后台启动并静默驻留在任务栏系统托盘中。

> [!NOTE]
> 发布的程序包采用了**自包含 (Self-Contained)** 方式构建。这意味着 .NET 运行时已经被打包到了 `.exe` 文件内部，你无需在电脑上预先安装任何依赖，纯绿色开箱即用！

### 配置文件路径

所有的用户设置都以 JSON 格式保存在用户的本地应用数据文件夹下：
`%LOCALAPPDATA%\CalculatorInAir\settings.json`

如果您想将所有设置重置为默认值，只需退出程序，删除该文件，然后重新运行即可。

### 本地编译与测试

如果您希望从源码编译或测试该可执行文件，请确保系统已安装 [.NET 10.0 SDK](https://dotnet.microsoft.com/download)。

在项目根目录下打开命令行（cmd/PowerShell）并运行以下命令之一：

#### 运行测试
若要运行解析器和单位换算的单元测试：
```bash
dotnet test tests/CalculatorInAir.Tests/CalculatorInAir.Tests.csproj
```

#### 发布可执行程序
项目默认配置为生成自包含的单文件应用：
```bash
dotnet restore src/CalculatorInAir/CalculatorInAir.csproj
dotnet publish src/CalculatorInAir/CalculatorInAir.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -o ./publish
```

输出的绿色便携二进制文件 `CalculatorInAir.exe` 将被保存在 `./publish` 目录下。

---
