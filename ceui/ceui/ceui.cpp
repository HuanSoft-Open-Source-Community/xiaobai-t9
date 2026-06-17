#include <windows.h>
#include <imm.h>
#include <stdio.h>
#include <tchar.h>
#pragma comment(lib, "imm32.lib")

int main() {
  int nBuff = GetKeyboardLayoutList(0, NULL);
  HKL* lpList = (HKL*)malloc(nBuff * sizeof(HKL));
  GetKeyboardLayoutList(nBuff, lpList);

  printf("共找到 %d 个输入法布局:\n\n", nBuff);

  for (int i = 0; i < nBuff; i++) {
    HKL hkl = lpList[i];
    printf("[%d] HKL = 0x%08X\n", i + 1, (DWORD)hkl);

    // 获取 IME 文件名
    TCHAR imeFile[260] = {0};
    if (ImmGetIMEFileName(hkl, imeFile, 260) > 0) {
      printf("    IME 文件: %ls\n", imeFile);
    }

    // 获取描述名称
    TCHAR desc[512] = {0};
    if (ImmGetDescription(hkl, desc, 512) > 0) {
      printf("    描述名称: %ls\n", desc);
    }
    printf("\n");
  }

  free(lpList);
  printf("按回车键退出...\n");
  getchar();
  return 0;
}