/************************************************************************
    About.c
      -- Show "About" dialog



    History:  Date          Author      Comment
              3/1/98        Casper      Wrote it.

*************************************************************************/
#include <windows.h>
#include "resource.h"

extern char     GszAppName[];

BOOL CALLBACK AboutDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam)
{
        char    buf[80];
        switch(iMsg){
        case WM_INITDIALOG:
            wsprintf(buf,"PComm %s Example",GszAppName);
            SetWindowText(GetDlgItem(hDlg,IDC_ABOUTSTR1),buf);
            wsprintf(buf,"",GszAppName);
            lstrcpy(buf,"Copyright (c) 1998");
            SetWindowText(GetDlgItem(hDlg,IDC_ABOUTSTR2),buf);

            return TRUE;

        case WM_COMMAND:
            switch (LOWORD(wParam)){
            case IDOK:
            case IDCANCEL:
                EndDialog(hDlg,LOWORD(wParam));
                return TRUE;
            }
        }
        return FALSE;
}
