#pragma once

#include "resource.h"
#include <resource.h>
#include <WeaselIPC.h>
#include <WeaselUI.h>
#include <RimeWithWeasel.h>
#include <WeaselUtility.h>
#include <filesystem>
#include <functional>
#include <memory>
#include <winsparkle.h>

#include "WeaselTrayIcon.h"

namespace fs = std::filesystem;

class WeaselServerApp {
 public:
  static bool execute(const fs::path& cmd, const std::wstring& args) {
    return (uintptr_t)ShellExecuteW(NULL, NULL, cmd.c_str(), args.c_str(), NULL,
                                    SW_SHOWNORMAL) > 32;
  }

  static bool explore(const fs::path& path) {
    std::wstring quoted_path(L"\"" + path.wstring() + L"\"");
    return (uintptr_t)ShellExecuteW(NULL, L"explore", quoted_path.c_str(), NULL,
                                    NULL, SW_SHOWNORMAL) > 32;
  }

  static bool open(const fs::path& path) {
    return (uintptr_t)ShellExecuteW(NULL, L"open", path.c_str(), NULL, NULL,
                                    SW_SHOWNORMAL) > 32;
  }

  static bool check_update() {
    // when checked manually, show testing versions too
    std::string feed_url = GetCustomResource("ManualUpdateFeedURL", "APPCAST");
    std::wstring channel{};
    auto ret = RegGetStringValue(HKEY_CURRENT_USER, L"Software\\Rime\\Weasel",
                                 L"UpdateChannel", channel);
    if (!ret && channel == L"testing") {
      feed_url = GetCustomResource("TestingManualUpdateFeedURL", "APPCAST");
    }
    if (!feed_url.empty()) {
      win_sparkle_set_appcast_url(feed_url.c_str());
    }
    win_sparkle_check_update_with_ui();
    return true;
  }

  static fs::path install_dir() {
    WCHAR exe_path[MAX_PATH] = {0};
    GetModuleFileNameW(GetModuleHandle(NULL), exe_path, _countof(exe_path));
    fs::path dir = fs::path(exe_path).remove_filename();
    // ARM64 build: the native server lives in <root>\ARM64\ whereas the helper
    // tools (WeaselDeployer.exe / update.exe / numkeyboard.exe / t9keyboard.exe)
    // stay in <root>.  When <dir> has no WeaselDeployer.exe but the parent does,
    // the parent is the real installation root (tray menu must use it).
    std::error_code ec;
    if (!fs::exists(dir / L"WeaselDeployer.exe", ec)) {
      fs::path parent = dir.parent_path();
      if (!parent.empty() && fs::exists(parent / L"WeaselDeployer.exe", ec))
        return parent;
    }
    return dir;
  }

 public:
  WeaselServerApp();
  ~WeaselServerApp();
  int Run();

 protected:
  void SetupMenuHandlers();

  weasel::Server m_server;
  weasel::UI m_ui;
  WeaselTrayIcon tray_icon;
  std::unique_ptr<RimeWithWeaselHandler> m_handler;
};
