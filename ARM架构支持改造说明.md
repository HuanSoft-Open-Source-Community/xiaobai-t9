# 小白T9 / 小狼毫 fork —— ARM 架构支持改造说明

> 改造执行：2026-10-14
> 关联文档：`ARM架构支持可行性分析.md`（2026-10-10，静态分析）
> 备份位置：`C:\weasel_backup_arm64_20261014\`（18 个文件 + MD5 清单 + 一键恢复脚本）
> 改动规模：11 个文件、+68 行 / -1 行（**未动任何 C++/C# 源码**）

---

## 一、结论速览

| # | 项目 | 状态 |
|---|---|---|
| 1 | 改动文件 | 6×`.csproj` + `install.nsi` + `build.bat` + 3×env 文件 |
| 2 | 源码改动 | **零**。C++ 侧 ARM64 逻辑（TSF/IME、ARM64X、WeaselSetup 部署）上游已写好，本次只补"构建 → 打包"链路 |
| 3 | 已完成验证 | ✅ install.nsi 全量出包（退出码 0）✅ 6 个 C# 工程编译验证 ✅ build.bat 逻辑实测 ✅ 备份/恢复 |
| 4 | 需你完成 | ① 装 VS2022 ARM64/ARM64EC 工具链（唯一硬前置）② `build.bat arm64` 产出 ARM64 二进制 ③ ARM64 实机验证 |
| 5 | 回滚 | 一键：`restore.ps1`；或 `git checkout -- <文件>` |

**为什么以前在 ARM64 上装不上**：安装时 `WeaselSetup.exe` 的部署逻辑（`imesetup.cpp`，源码早已支持 ARM64）会把 6 个 ARM64/ARM32 输入法组件复制到系统目录并注册；但本 fork **从未构建过这些文件、安装包也从未投递过它们** → ARM64 系统上安装**必然中途失败**。本次改造把这 6 个文件的「构建产出 → 打包投递 → 释放安装」整条链路补齐。

---

## 二、改动清单（逐文件）

### 2.1 `output/install.nsi`（核心改动，+16 行）

在 ARM64 运行时条件（`${If} ${IsNativeARM64}`）下追加投递 6 个输入法组件（放在原 `weasel.dll` / `weasel.ime` 投递之后）：

```nsis
${If} ${IsNativeARM64}
  File /nonfatal "weaselARM.dll"        ; ARM32（仅 Win11 24H2 之前的 ARM32 WOW 需要）
  File /nonfatal "weaselARM64.dll"      ; ARM64 原生
  File /nonfatal "weaselARM64X.dll"     ; ARM64X 重定向器（必需）
${EndIf}
...（.ime 三个同理：weaselARM.ime / weaselARM64.ime / weaselARM64X.ime）
```

- `File /nonfatal`：文件缺失时**仅警告**、不阻断编译（与上游 rime/weasel 完全一致的做法）
- 这 6 个文件平时**不释放**（只在 ARM64 系统上解压到安装目录），非 ARM64 用户不受影响
- 改动前备份：`output/install.nsi.bak-20261014-arm64`

### 2.2 6 个 C# 工程 csproj（编译选项，各 +2 行）

| 工程 | Debug\|AnyCPU | Release\|AnyCPU |
|---|---|---|
| `t9keyboard` / `t9configui` / `numkeyboard` / `t9skin` / `helpme` / `update` | 加 `<Prefer32Bit>false</Prefer32Bit>` | 加 `<Prefer32Bit>false</Prefer32Bit>` |
| `t9s2t` | ——（原本已设置，未改动） | —— |

**效果**：编译产物从 `anycpu32bitpreferred`（强制 32 位进程）变为 **AnyCPU 64 位优先**：

- Windows 11 ARM64：以 **ARM64 原生**运行（系统 .NET Framework 4.8.1+ 含 ARM64 CLR）
- Windows 10 ARM64 等旧系统：以 x64 模拟运行（仍可用）
- x64 Windows：32 位进程 → 64 位进程（**行为变化，见 §5.5 回归验证**）

### 2.3 `build.bat`（构建脚本，+18 行）

新增 `:check_arm64` 子过程，在 `arm64` 构建第一步（Boost 之前）执行工具链预检：

- 找到 ARM64 工具链 → 正常继续
- 未找到 → 输出明确的组件安装指引后退出（原先要到编译中途才报 `MSB8020 无法找到 v142 的生成工具` 这类难懂错误）
- 找不到 `vswhere.exe`（很老的 VS 环境）→ 跳过检查，保持原行为

### 2.4 环境文件注释（`env.vs2019.bat` / `env.vs2022.bat` / `env.bat.template`，+16 行）

增加 ARM64 构建所需 VS 组件说明（纯注释，无行为变化）。

---

## 三、设计说明（为什么这样改）

### 3.1 上游的 ARM64 策略（继承，未改）

| 组件 | ARM64 策略 | 原因 |
|---|---|---|
| `WeaselTSF` → **weaselARM64.dll** | **原生 ARM64** | TSF 加载进系统/应用进程，架构必须匹配 |
| `WeaselIME` → **weaselARM64.ime** | **原生 ARM64** | 同上（传统 IME 路径） |
| `WeaselUI` / `WeaselIPC` | 随 ARM64 构建（静态库） | 被 TSF 链接 |
| `weaselARM64X.dll/.ime` | **ARM64X 重定向器** | 单文件同时含 ARM64+x64 转发（见 3.2） |
| `WeaselServer` / `WeaselDeployer` / `WeaselSetup` / `rime.dll` | **保持 x86 模拟** | 独立进程无需原生；本 fork 因 x64 rime.dll 崩溃 bug 本就全用 32 位核心组件 |
| `librime` / `Boost` 库 | 无需 ARM64 库 | TSF/IME 不链接 librime（已核实 ProjectReference 仅 WeaselIPC+WeaselUI） |

对应到 `weasel.sln`：ARM64 平台只构建 TSF/UI/IPC/IME 四个工程（与上游一致，**故未改动 sln**）。`xbuild.bat`（xmake 路线）的 arm64 分支此前已具备，未改。

### 3.2 ARM64X 重定向机制（已就绪，只差产物）

ARM64 系统上 `C:\Windows\System32\weasel.dll` 是一个 **ARM64X 容器**（由 `weaselARM64X.dll` 释放改名而来），由系统按加载进程的架构自动路由：

```
ARM64 进程加载 weasel.dll ──→ 转发 weaselARM64.dll（真实代码）
x64   进程加载 weasel.dll ──→ 转发 weaselx64.dll  （真实代码）
```

同一机制适用于 `weasel.ime`。注册表由 `Register.cpp` 的 `#ifdef _M_ARM64` 分支处理（已就绪）。

### 3.3 安装时的文件流向（源码已就绪，本次补齐投递）

`WeaselSetup.exe`（32 位进程）在 ARM64 系统上执行 `imesetup.cpp` 的既有逻辑：

| 源（安装目录内，本次新增投递） | → 目标 |
|---|---|
| `weaselARM64.dll` / `.ime` | `C:\Windows\System32\` |
| `weaselx64.dll` / `.ime`（已有） | `C:\Windows\System32\` |
| `weaselARM64X.dll` / `.ime`（本次新增） | `C:\Windows\System32\weasel.dll` / `weasel.ime`（ARM64X 容器） |
| `weaselARM.dll` / `.ime`（本次新增） | `C:\Windows\SysArm32\`（仅系统支持 ARM32 WOW 时，即 Win11 24H2 之前） |

---

## 四、验证记录（本次已执行，全部通过）

| # | 验证项 | 方法 | 结果 |
|---|---|---|---|
| 1 | csproj 结构 | XML 解析 + 逐配置组检查 | ✅ 6 文件各恰 +2 行，位置正确，BOM/CRLF 保持 |
| 2 | C# 编译验证 | `dotnet build -t:ResolveAssemblyReferences;CoreCompile` | ✅ 5/6 通过（仅既有 CS0649/CS0414 警告）；`t9configui` 的 CS0246（COM 引用）经**原始文件对照实验**确认是预先存在的命令行限制，与本次改动无关 |
| 3 | install.nsi 编码 | 写入后 GBK 解码完整复核 | ✅ 26976 → 27754 字节，中文注释可读 |
| 4 | **install.nsi 全量出包** | `makensis -V4`（测试版本号 2026.10.99） | ✅ **退出码 0**，产出 45.4MB 安装包（测试包已清理）；警告 7 条 = **6 条新 ARM64 缺件（/nonfatal 预期行为）** + 1 条原有的 `data\*.gram` |
| 5 | build.bat 逻辑 | 桩程序实测「缺 vswhere / 缺组件 / 有组件」3 分支 | ✅ 全部 PASS |
| 6 | 备份完整性 | 18 文件 + MD5 + 一键恢复脚本 | ✅ 见 `C:\weasel_backup_arm64_20261014\` |

> 说明 #4：警告中的 6 条「weaselARM*.dll/.ime 文件未找到」是**当前本机尚未构建 ARM64 产物的正常现象**。装好工具链并执行 `build.bat arm64` 后重新出包，这 6 条警告会全部消失，仅剩 1 条原有的 `data\*.gram`。

---

## 五、需要你完成的部分

### 5.1 安装 ARM64 工具链（唯一硬前置）

**推荐（一步到位）**：VS2022 **17.4 或更新版本**，安装组件：

> **MSVC v143 - VS 2022 C++ ARM64/ARM64EC 生成工具**

其中 ARM64EC 部分是 `arm64x_wrapper` 生成 `weaselARM64X.dll/.ime` 的**必要条件**，而 ARM64X 文件在所有 ARM64 系统安装时都是必需的。

备选：VS2019 + 「MSVC v142 - VS 2019 C++ ARM64 生成工具」——能构建 `weaselARM64.dll/.ime` 等 4 个文件，但**没有 ARM64EC → 无法生成 ARM64X 重定向器**，ARM64 安装仍不完整，**不建议单独使用**。

### 5.2 执行 ARM64 构建

**第 1 步：把工具集切到 VS2022 的 v143**（编辑现有 `env.bat`，保留原有 BOOST_ROOT 行不动）：

```bat
rem 在原 env.bat 中追加或修改以下两行：
set BJAM_TOOLSET=msvc-14.3
set PLATFORM_TOOLSET=v143
```

> ⚠️ 不要直接拿 `env.vs2022.bat` 覆盖 `env.bat`——它的默认 `BOOST_ROOT` 指向不存在的 `deps\boost_1_78_0`；若要用它，务必同时把 `BOOST_ROOT` 改回 `C:\Libraries\boost_1_84_0`。

**第 2 步：构建**（在对应 VS 的 **Developer Command Prompt** / x64 Native Tools 里）：

```bat
cd C:\weasel
build.bat arm64 release
```

> 说明：`build.bat arm64` 会按脚本既有流程重建全部平台（Win32/x64/ARM/ARM64，均使用 v143），x86/x64 产物被一并刷新属正常现象。若 boost 构建报「找不到 msvc-14.3」，到 `C:\Libraries\boost_1_84_0` 下重新运行一次 `bootstrap.bat` 再重试。

**预期产出**（`output\` 目录下应新增 6 个二进制文件）：

| 文件 | 来源 | 校验（可选） |
|---|---|---|
| `weaselARM.dll` / `weaselARM.ime` | weasel.sln `ARM` 平台构建 | `dumpbin /headers` → ARM |
| `weaselARM64.dll` / `weaselARM64.ime` | weasel.sln `ARM64` 平台构建 | `dumpbin /headers` → ARM64 |
| `weaselARM64X.dll` / `weaselARM64X.ime` | `arm64x_wrapper\build.bat`（构建末尾自动执行） | `dumpbin /headers` → ARM64X |

> 同时会新增若干 `.lib/.exp/.pdb` 中间文件（不参与打包，无需处理）。

### 5.3 重新出安装包

```bat
build.bat arm64 installer
```

检查 makensis 输出：**应只剩 1 条警告**（`data\*.gram`，原有），不再出现 6 条 `weaselARM*` 缺件警告。

### 5.4 ARM64 实机验证清单（装好包后逐项过）

1. **安装过程**：双击安装包正常完成，无「安装失败」弹窗
2. **系统目录检查**（`C:\Windows\`）：
   - `System32\` 下存在：`weasel.dll`、`weaselx64.dll`、`weaselARM64.dll` 及对应 `.ime`
   - Win11 24H2 之前系统额外检查：`SysArm32\weasel.dll`、`weasel.ime`
3. **输入功能**：
   - ARM64 原生应用（记事本、设置）中能打字、候选窗正常
   - x64 应用（VS Code、微信等）中能打字（走 ARM64X → weaselx64.dll 分支）
   - 语言栏切换/中英切换正常
4. **周边工具**（6 个，应能启动 + 基本功能正常）：`t9keyboard`（软键盘 + 悬浮窗 + 托盘）、`numkeyboard`（热键）、`t9configui`、`t9skin`、`helpme`、`update`
5. **t9s2t 语音输入**：录音与识别可用（引擎为 x64，在 x64 模拟下运行）
6. **卸载**：能干净卸载（检查系统目录里 weasel* 文件与注册表项已清理）

### 5.5 x64 / x86 设备回归验证（因 C# 工具位数变化）

本次 6 个 C# 工具从 32 位进程变为 64 位进程，**普通 x64 Windows 上也需要回归一遍**（重点）：

| 工具 | 重点验证 |
|---|---|
| `t9keyboard` | 软键盘打字、悬浮窗拖动/贴边收起、托盘菜单、双击行为 |
| `numkeyboard` | 热键注册、与键盘驱动通信（000 三连按等） |
| `t9configui` | 打开设置界面（用到 COM 组件 IWshRuntimeLibrary） |
| `t9skin` / `helpme` / `update` | 正常启动与基本操作 |

如个别工具有问题：删掉对应 csproj 里的 `<Prefer32Bit>false</Prefer32Bit>` 重新构建即可单独回退（或从备份恢复该文件）。

---

## 六、回滚方法

### 方式一：文件级一键恢复（推荐）

```powershell
& "C:\weasel_backup_arm64_20261014\restore.ps1"
```

### 方式二：git

```bat
cd C:\weasel
git checkout -- build.bat env.vs2019.bat env.vs2022.bat env.bat.template output\install.nsi ^
  t9keyboard\t9keyboard\t9keyboard.csproj t9configui\t9configui\t9configui.csproj ^
  numkeyboard\numkeyboard\numkeyboard.csproj t9skin\t9skin\t9skin\t9skin.csproj ^
  helpme\helpme\helpme.csproj update\update\update.csproj
```

改动前 git 快照（HEAD `a584a4d`，分支 master）记录在备份目录 `GIT_SNAPSHOT.txt`。

### 单点回滚速查

| 想撤销什么 | 操作 |
|---|---|
| ARM64 文件投递 | 恢复 `output/install.nsi`（或删 §2.1 的 16 行） |
| 某工具 64 位化 | 删除该 csproj 中 2 处 `<Prefer32Bit>false</Prefer32Bit>` |
| 构建预检 | 恢复 `build.bat` |

---

## 七、后续可选路线（本次未做，与可行性分析报告的 P2/P3 对应）

| 项 | 内容 | 前置 |
|---|---|---|
| t9s2t 引擎 ARM64 原生化 | 服务端补 win-arm64 版 `onnxruntime.dll` + `sherpa-onnx-c-api.dll`，按架构分发 | 现可先以 x64 模拟运行 |
| WeaselServer 等核心服务 ARM64 原生化 | 需 librime + 全套第三方依赖（glib/leveldb/marisa/opencc/yaml-cpp）ARM64 重建 | 工作量较大（数人天） |
| ARM32 兼容补全（Win10 ARM） | 监控 ARM32 WOW 场景的反馈；产物已随构建自动生成 | —— |
