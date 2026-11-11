#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""把 ARM64 构建所需的 librime 子模块补丁应用回 librime/。

用法（在仓库任意位置执行均可）：
    python arm64-patches/apply_librime_patches.py            # 应用补丁
    python arm64-patches/apply_librime_patches.py --check    # 只检查，不写文件

为什么要这个：
    librime 是 git 子模块，remote 指向上游 rime/librime，本仓库无法推送它的
    改动。但这三个文件必须修改后才能在 VS2022 / BuildTools 环境下构建出
    ARM64 版本（详见同目录 README.md）。因此把改好的版本固化在这里，
    重新 clone 或重置子模块后用本脚本一键还原。

被覆盖的原文件会备份为 <文件名>.bak-arm64patch（.gitignore 已忽略 *.bak-*）。
"""

import filecmp
import os
import shutil
import sys

try:  # 让中文输出在 UTF-8 终端不乱码；GBK 终端会退化为替换字符，但不影响流程
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
except Exception:
    pass

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)                      # 仓库根目录
LIBRIME = os.path.join(ROOT, 'librime')
SRC = os.path.join(HERE, 'librime')

# (补丁目录里的文件名, librime 子模块内的目标相对路径)
# 注意：补丁目录刻意保持扁平——.gitignore 里有 `deps/` 规则，Git 无法重新
#       包含被排除目录下的文件，所以 CMakeLists.txt 不能按原目录结构存放。
FILES = [
    ('env.bat',                   'env.bat'),
    ('build.bat',                 'build.bat'),
    ('opencc-data-CMakeLists.txt', os.path.join('deps', 'opencc', 'data', 'CMakeLists.txt')),
]


def main():
    check_only = '--check' in sys.argv

    if not os.path.isdir(LIBRIME):
        print('ERROR: 找不到 librime/')
        print('       先执行: git submodule update --init --recursive')
        return 2

    applied = skipped = failed = 0
    for name, rel in FILES:
        src = os.path.join(SRC, name)
        dst = os.path.join(LIBRIME, rel)
        if not os.path.exists(src):
            print('[MISS ] %-30s -> %s  （补丁目录里没有此文件）' % (name, rel))
            failed += 1
            continue
        if os.path.exists(dst) and filecmp.cmp(src, dst, shallow=False):
            print('[SAME ] %-30s -> %s' % (name, rel))
            skipped += 1
            continue
        if check_only:
            print('[DIFF ] %-30s -> %s  （需要应用）' % (name, rel))
            applied += 1
            continue
        os.makedirs(os.path.dirname(dst), exist_ok=True)
        if os.path.exists(dst):
            shutil.copy2(dst, dst + '.bak-arm64patch')
        shutil.copy2(src, dst)
        print('[APPLY] %-30s -> %s' % (name, rel))
        applied += 1

    print()
    if check_only:
        print('检查完成：%d 个待应用，%d 个已一致，%d 个缺失' % (applied, skipped, failed))
    else:
        print('完成：%d 个已应用，%d 个已一致，%d 个缺失' % (applied, skipped, failed))

    if failed:
        return 1
    if not check_only:
        print()
        print('提示：env.bat 里的 BOOST_ROOT / CMAKE_GENERATOR_INSTANCE 是本机路径，')
        print('      换机器后请按实际安装位置修改（默认 Boost 在 C:\\Libraries\\boost_1_84_0，')
        print('      VS2022 BuildTools 在 C:\\BuildTools2022）。')
    return 0


if __name__ == '__main__':
    sys.exit(main())
