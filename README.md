# 小白T9输入法

> **可能是 PC 上唯一的小键盘九宫格输入法**

基于 [Rime / 小狼毫](https://github.com/rime/weasel) 深度定制的 Windows 九宫格输入法，支持小键盘打字、软键盘、语音输入等功能。

官网：https://t9.xiaobai.pro/

QQ 群：387170746

---

## 功能特性

- **小键盘九宫格输入** — 用 NumPad 1-9 数字键拼音打字，位置与手机九宫格一致
- **T9 软键盘** — 可视化屏幕键盘，支持拖动缩放，适合平板/触屏设备
- **语音输入** — 按住 `Ctrl+Alt+D` 说话，自动识别转文字（基于 [Sherpa-ONNX](https://github.com/k2-fsa/sherpa-onnx)）
- **NumLock 关时改键** — NumLock 关闭状态下，小键盘可自定义为各种功能键
- **细节设置面板** — 可视化配置输入法参数、候选词展示样式等
- **词库扩展** — 支持自定义词库，兼容 Rime 词库格式
- **皮肤主题** — 支持自定义输入法外观

---

## 操作说明

### 小键盘输入

| 按键 | 功能 |
|------|------|
| NumPad 1-9 | 输入拼音编码（位置同手机九宫格） |
| `/` `*` | 翻页 |
| `-` `+` | 翻选 |
| `0` | 确认选字 |
| `Ctrl` + NumPad 数字 | 快速选字 |
| `Enter` | 直接上屏数字 |
| `.` | 退格 |
| `7` | 开启标点符号（如 `7douhao` → `，`） |
| `77` | 开启字母输入（如 `7781` → `a`，`7792` → `e`） |

#### 筛选功能

输入编码后按 `7` 开启筛选，依次按 `8` 切换筛选结果：

```
186       → 原始编码
1867      → 开启筛选
18678     → pan
186788    → pao
1867888   → ran
18678888  → rao
...
```

### 语音输入

按住 `Ctrl+Alt+D` 开始录音，松开自动识别并输入文字。

### T9 软键盘

- 打开方式：开始菜单 或 右键托盘「中」字 → T9软键盘
- 拖动边角可放大/缩小
- 双击悬浮窗可隐藏/显示
- 部分按键右键有特殊功能（平板上为长按）
- 右键悬浮窗可设置或退出

### NumPad 改键

NumLock 关闭时，小键盘按键可自定义为：
`ESC`、`Shift`、`Tab`、`Ctrl`、复制、粘贴、剪切、全选、撤回、退格、空格、逗号、句号、打开我的电脑/计算器/浏览器/邮件、切换输入法、微信截图、切换鼠标键、语音输入 等。

---

## 项目结构

```
weasel/
├── WeaselTSF/            # TSF 输入法框架（C++）— 基于小狼毫
├── WeaselServer/         # 输入法服务进程（C++）
├── WeaselUI/             # 候选词 UI 渲染（C++）
├── WeaselIPC/            # 进程间通信
├── WeaselIME/            # IME 兼容层
├── WeaselSetup/          # 安装程序
├── WeaselDeployer/       # 部署工具
├── librime/              # Rime 引擎核心（Git submodule）
│
├── t9keyboard/           # T9 软键盘（C# WinForms）
│   └── help.cs           #   设置与改键面板
│
├── numkeyboard/          # 小键盘改键驱动（C# WinForms）
│
├── t9configui/           # 细节设置面板（C# WinForms）
│
├── t9s2t/                # 语音识别（C# WinForms）
│   └── Engines/          #   识别引擎（Sherpa-ONNX）
│
├── ceui/                 # 候选词弹窗（C++）
├── helpme/               # 一键修复工具（C# WinForms，自动拉起 t9s2t）
├── t9skin/               # 皮肤工具
├── resource/             # 图标、图片等资源
├── output/               # 编译输出 & NSIS 安装脚本
└── plum/                 # Rime 词库管理工具
```

### 技术栈

| 组件 | 技术 |
|------|------|
| 输入法核心 | C++ / Rime (librime) / TSF |
| 软键盘 / 改键 / 设置 / 语音 | C# / WinForms (.NET Framework) |
| 语音引擎 | [Sherpa-ONNX](https://github.com/k2-fsa/sherpa-onnx)（本地离线识别） |
| 候选词弹窗 | C++ (Win32) |
| 安装程序 | NSIS |

---

## 编译

### 环境要求

- Visual Studio 2019/2022（含 C++ 桌面开发 & .NET 桌面开发）
- CMake 3.x（编译 librime）
- Boost C++ Libraries

### 编译步骤

```batch
# 1. 克隆（含子模块）
git clone --recursive https://github.com/xiaobai9978/xiaobai-t9.git

# 2. 配置环境
copy env.bat.template env.bat
# 编辑 env.bat 设置 Boost 路径等

# 3. 编译
build.bat
```

C# 子项目（t9keyboard、numkeyboard、t9configui、t9s2t、helpme）可用 Visual Studio 直接打开 `.sln` 编译。

---

## 安装

适用于 **Windows 8.1 ~ Windows 11**。

下载最新安装包：https://t9.xiaobai.pro/

安装后在输入法列表中选择「小白T9输入法」即可使用。

### 添加自定义词库

1. 右键托盘图标 → 程序文件夹 → `data/xiaobai.dict.yaml`
2. 添加词库文件路径（文件名需与 `name` 字段一致，加 `.dict.yaml` 后缀）
3. 词库格式：

```yaml
name: my_dict
version: "1.0"
sort: by_weight
词语	ci yu	2000
```

格式说明：`汉字` + `Tab` + `编码（空格分隔）` + `Tab` + `词频`

---

## 致谢

- [Rime / 中州韵输入法引擎](https://rime.im/) — 提供核心输入法引擎
- [小狼毫 Weasel](https://github.com/rime/weasel) — 基础框架
- [Sherpa-ONNX](https://github.com/k2-fsa/sherpa-onnx) — 本地语音识别引擎
- [NAudio](https://github.com/naudio/NAudio) — 音频采集

## 许可证

基于 [GPLv3](LICENSE.txt) 开源

---

*有问题？来 QQ 群：387170746*
