# arm64-patches — librime 子模块补丁

`librime/` 是 git 子模块，remote 指向上游 [`rime/librime`](https://github.com/rime/librime)，
本仓库**无法推送**对它的修改。但构建 ARM64 版本前，子模块里有三个文件必须先改，
否则构建会失败。这里保存改好的版本，配一个脚本一键还原。

> 只影响 ARM64 / ARM32 构建链，x64 / Win32 用**未打补丁**的原版子模块也能正常构建。

## 三个补丁

| 文件 | 改了什么 | 不打会怎样 |
|---|---|---|
| `env.bat` | 工具集切到 `msvc-14.3` / `v143`，生成器 `Visual Studio 17 2022`，并指定 `CMAKE_GENERATOR_INSTANCE=C:/BuildTools2022` | 沿用 VS2019 的 `msvc-14.2` / `v142`，ARM64 目标无法编译（v142 工具集在本机不含 ARM64） |
| `build.bat` | 把 `CMAKE_GENERATOR_INSTANCE` 透传给 cmake | cmake 只按名字找 "Visual Studio 17 2022"，本机同时装了 VS2019 时会选错实例，报 `VCTargetsPath` 找不到 / generator mismatch |
| `deps/opencc/data/CMakeLists.txt` | 开头按 `CMAKE_GENERATOR_PLATFORM` 判断，ARM/ARM64 时 `return()` 跳过字典生成 | opencc 的构建规则要**运行** `opencc_dict.exe` 生成 `.ocd2`；ARM64 的 exe 在 x64 主机上跑不起来，报 `MSB8066`，退出码 216 |

`env.bat` 在原仓库里被 `.gitignore` 忽略（属于本地配置），首次构建时 `build.bat`
会从 `env.bat.template` 复制一份，所以本目录里的 `env.bat` 是**已经配好**的版本。

> **为什么补丁目录是扁平的**：`.gitignore` 里有 `deps/` 规则，而 Git 不允许重新包含
> 被排除目录下的文件（`!` 例外对目录排除无效）。所以 `deps/opencc/data/CMakeLists.txt`
> 以 `opencc-data-CMakeLists.txt` 存放，由脚本映射回原路径。

## 用法

```bat
:: 1) 子模块还没拉下来的话
git submodule update --init --recursive

:: 2) 应用补丁（先 --check 看看差异）
python arm64-patches\apply_librime_patches.py --check
python arm64-patches\apply_librime_patches.py
```

被覆盖的原文件会另存为 `*.bak-arm64patch`（已被 `.gitignore` 的 `*.bak-*` 忽略）。

## 注意事项

- `env.bat` 里含**本机绝对路径**，换机器后按实际位置改：
  - `BOOST_ROOT` 默认 `C:\Libraries\boost_1_84_0`
  - `CMAKE_GENERATOR_INSTANCE` 默认 `C:/BuildTools2022`（VS2022 BuildTools 安装目录）
- `build.bat` 是 **GBK 编码**，编辑时别用会转 UTF-8 的编辑器直接保存，否则中文 `echo` 会乱码。
- 子模块里还有大量历史遗留改动（与 ARM64 无关），本脚本只碰上面三个文件，不会 reset/clean 其他内容。

## 相关文档

- `ARM架构支持改造说明.md` — 第 1 轮：输入组件 ARM64/ARM32 支持
- `ARM64-WeaselServer原生化说明.md` — 第 2 轮起：WeaselServer 原生化，附录 B 是完整构建链实操记录，
  附录 C 是 `0xc000007b` 安装故障的根因与修复
