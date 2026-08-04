# Calculator in the Air | 悬浮计算器

[English](#calculator-in-the-air) | [简体中文](#简体中文-chinese)

---

## Calculator in the Air

`Calculator in the Air` is a lightweight, modern, macOS Spotlight-style floating mathematical calculator for Windows. It runs persistently in the background, triggers instantly via a global hotkey, calculates expression values in real-time, copies the result on Enter, and stays tucked away in the system tray. 

It is packaged as a single portable `.exe` file that is **completely self-contained**—no installation and no .NET runtime required! Just run it!

### Features

- **Spotlight-like Overlay & Fluent Animations**:
  - Centers on the screen 20% from the top (600px width).
  - **Multi-Monitor Aware**: Automatically centers on the active monitor where the mouse cursor is located.
  - **Windows Fluent Transition Animations**: Ultra-lightweight 120ms Fade & Scale In/Out animations when triggering or hiding.
  - Custom drag support: Drag it anywhere if it blocks your view.
- **Glassmorphic Design & Theme Adaptability**:
  - Frameless, borderless window utilizing native Windows 11 Acrylic backdrop (`DwmSetWindowAttribute`), with graceful fallbacks for older Windows versions.
  - Soft, hardware-accelerated drop shadows to float above other windows.
  - **Dynamic Theme Adaptation**: Automatically adapts to Windows native light/dark mode settings, or can be overridden manually (powered by WPF XAML ResourceDictionaries).
  - **Monospace Font Option**: Choose between standard system fonts and monospace tabular figures (Cascadia Code / Consolas) to eliminate text jitter.
- **Real-time Math Evaluation & Multi-Format Conversions**:
  - Dynamically tokenizes and parses expressions as you type.
  - **Implicit Multiplication**: Treats expressions like `2pi` or `2(3+4)` naturally as multiplication.
  - **Unit Conversion**: Built-in support for length, weight/mass, and temperature conversions (e.g., `10 m to cm`, `98.6 f in c`).
  - **Thousands Separator & Format Selector**: Toggle thousands separator (`1,234,567.89`) and use `Up`/`Down` arrow keys on results to cycle formats (Standard, Raw, Ten-thousand `123.46万`, and Chinese Upper-case RMB).
  - **Result History (`ans`)**: Reuse the last calculated result easily with the `ans` constant.
  - **Input History**: Use the `Up` and `Down` arrow keys to navigate through your previous calculations when result format switching is inactive.
  - **Error Feedback**: Silent error handling while typing, but provides a visual shake animation and red text if you attempt to submit an invalid expression.
- **Instant Actions & Visual Guidance**:
  - Press `Alt + Space` (default) to toggle the window.
  - **Multi-Mode Copying**: Press `Enter` to copy result, or `Shift + Enter` to copy full formula & result (`12 * 8 = 96`) with instant on-screen Toast feedback.
  - **Smart Esc Key**: Single press Esc clears text; double-press Esc (within 350ms) or Esc on empty input hides the calculator.
  - **Window Pinning (`Ctrl + P`)**: Pin button on top right to temporarily keep window open on focus loss.
  - **Smart Clipboard Hint**: Automatically detects valid math formulas on clipboard when waking up, offering one-click paste.
- **Interactive System Tray**:
  - Custom-drawn tray icon (a beautiful violet-to-blue gradient block with a white `=` sign) built dynamically in memory at runtime.
  - Right-click tray menu to show, open settings, or exit.
- **Modern Settings Dialog**:
  - **Interactive Hotkey Recorder**: Click to record any custom global hotkey combination with instant system shortcut conflict warnings (`Win+E`, `Alt+F4`, etc.).
  - **Theme Selection**: Choose between "Follow System (Auto)", "Dark Mode", or "Light Mode".
  - Set calculation precision (Auto or 0 to 10 decimal places).
  - Toggle behaviors like focus-loss hiding, monospace font, thousands separator, and Enter-copying.
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

All user preferences are stored in JSON format at your local app data directory:
`%LOCALAPPDATA%\CalculatorInAir\settings.json`

If you ever wish to reset all settings to default, simply exit the app, delete this file, and run the app again.

---

## 简体中文 (Chinese)

`Calculator in the Air (悬浮计算器)` 是一款轻量、现代、macOS Spotlight 风格的 Windows 悬浮数学计算器。它静默驻留在后台，可通过全局快捷键瞬间唤醒，实时计算输入算式的值，按回车一键复制结果，不用时优雅隐藏回系统托盘。

程序打包为独立的便携 `.exe` 文件，**完全自包含 (Self-Contained)**——无需安装，无需预装 .NET 运行时，双击即可运行！

### 核心特性

- **类 Spotlight 悬浮窗口与 Fluent 动画**：
  - 屏幕水平居中、距顶部 20% 位置呈现（标准宽度 600px）。
  - **多显示器感应**：自动跟随鼠标所在的当前活动显示器居中弹出。
  - **Fluent 极轻量过渡动画**：唤醒与隐藏时带 120ms 淡入微缩放 (Fade & Scale In/Out) 动画。
  - 支持拖拽：按住窗口任意位置即可轻松移动。
- **毛玻璃质感与主题自适应**：
  - 无边框设计，原生支持 Windows 11 Acrylic 亚克力背景效果（`DwmSetWindowAttribute`），在旧版 Windows 上自动平滑降级。
  - 软硬件加速阴影，优雅悬浮于其他窗口之上。
  - **动态主题跟随**：自动适应 Windows 深色/浅色主题，或在设置中手动固定。
  - **数字等宽字体**：提供 Segoe UI 与 Cascadia Code / Consolas 数字等宽字体选项，杜绝频繁计算时的字符抖动。
- **实时数学求值与多格式转换**：
  - 随打随算，实时解析。
  - **隐式乘法**：自然支持 `2pi` 或 `2(3+4)` 等隐式乘法书写。
  - **单位换算**：内置长度、重量/质量、温度换算（如 `10 m to cm`, `98.6 f in c`）。
  - **千位分隔符与大数格式切换**：支持千位分隔符 (`1,234,567.89`)，按键盘 `上下方向键` 在标准、纯数字、**万元 (xx.xx万)** 与 **大写人民币 (RMB)** 格式间实时切换。
  - **结果历史 (`ans`)**：轻松使用 `ans` 常量引用上一次的计算结果。
  - **历史记录导航**：在未触发格式切换时按 `Up` / `Down` 键快速浏览历史计算公式。
  - **错误反馈**：输入时静默处理语法错误，回车时若算式非法则播放震动动画与红色提示。
- **快捷操作与可视化引导 Toast**：
  - 按 `Alt + Space`（默认）随时唤醒/隐藏窗口。
  - **多模式快捷复制**：按 `Enter` 复制计算结果，按 `Shift + Enter` 复制完整算式与结果 (`12 * 8 = 96`)，伴有 Toast 轻提示。
  - **Esc 分级逻辑**：单击 Esc 清空输入框；350ms 内连击双击 Esc 或空框时隐藏窗口。
  - **窗口置顶固定 (`Ctrl + P`)**：右上角一键图钉，固定时临时禁用离焦隐藏，方便连续多笔对照计算。
  - **剪贴板算式智能提醒**：唤醒时检测到剪贴板包含合法公式自动弹出轻提示，支持一键粘贴。
- **交互式系统托盘**：
  - 运行时动态绘制系统托盘图标（紫色到蓝色渐变背景与白色 `=` 符号）。
  - 右键托盘菜单支持显示、打开设置或退出。
- **现代设置对话框**：
  - **交互式快捷键录制**：支持录制任意全局快捷键，录制 `Win+E`, `Alt+F4` 等系统快捷键时实时给出风险提示。
  - **主题与精度控制**：自由选择主题与保留小数位数 (自动或 0~10 位)。
  - 丰富行为开关：失去焦点隐藏、数字等宽字体、千位分隔符等。
- **多语言支持**：
  - 原生支持 **简体中文** 与 **英国英语 (en-GB)**。
- **便携开箱即用**：
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

---
