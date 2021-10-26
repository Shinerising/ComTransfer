/************************************************************************
    FtSet.c
     -- Show dialog for user to set file transfer protocol.


        History:   Date       Author         Comment
                   3/1/98     Casper         Wrote it.

*************************************************************************/


#include	<windows.h>
#include	"resource.h"
#include	"ftrans.h"

int GProtocol = FTXMDM1KCRC;
int GDirection = FT_XMIT;

static	void	InitFtDlg(HWND hDlg);

BOOL CALLBACK FtDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam)
{
        switch(iMsg){
        case WM_INITDIALOG:
            InitFtDlg(hDlg);
            return TRUE;

        case WM_COMMAND:
            if(HIWORD(wParam)==BN_CLICKED){
                switch(LOWORD(wParam)){
                case IDC_FT_XMDM1KCRC:
                    GProtocol = FTXMDM1KCRC;
                    break;
                case IDC_FT_XMDMCHK:
                    GProtocol = FTXMDMCHK;
                    break;
                case IDC_FT_XMDMCRC:
                    GProtocol = FTXMDMCRC;
                    break;
                case IDC_FT_ZMDM:
                    GProtocol = FTZMDM;
                    break;
                case IDC_FT_YMDM:
                    GProtocol = FTYMDM;
                    break;
                case IDC_FT_KERMIT:
                    GProtocol = FTKERMIT;
                    break;
                case IDC_FT_ASCII:
                    GProtocol = FTASCII;
                    break;
                case IDC_FT_XMIT:
                    GDirection = FT_XMIT;
                    break;
                case IDC_FT_RECV:
                    GDirection = FT_RECV;
                    break;
                }
            }
            switch (LOWORD(wParam)){
            case IDOK:
            case IDCANCEL:
                EndDialog(hDlg,LOWORD(wParam));
                return TRUE;
            }
        }
        return FALSE;
}


static void InitFtDlg(HWND hDlg)
{
        CheckRadioButton(hDlg,IDC_FT_XMDM1KCRC,IDC_FT_ASCII,
            IDC_FT_XMDM1KCRC+GProtocol);

        CheckRadioButton(hDlg,IDC_FT_XMIT,IDC_FT_RECV,
            IDC_FT_XMIT+GDirection);
}

