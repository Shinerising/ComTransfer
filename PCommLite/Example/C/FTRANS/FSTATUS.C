/************************************************************************
    FStatus.c
     -- show dialog to diaplsy file transfer status


    History:   Date       Author         Comment
               3/1/98     Casper         Wrote it.

*************************************************************************/

#include	<windows.h>
#include	"PComm.h"
#include	"resource.h"
#include	"ftrans.h"
#include	"comm.h"

extern HINSTANCE	GhInst;
extern HANDLE		GftStop;
extern COMMDATA		GCommData;

static long             Flen;
static long             Xlen;
static char             Fname[_MAX_PATH];


static void UpdateDlg(HWND hDlg);

BOOL CALLBACK FtStatProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam)
{
        char	buf[15];

        switch(iMsg){
        case WM_INITDIALOG:
            wsprintf(buf,"COM%d",GCommData.Port);
            SendDlgItemMessage(hDlg,IDC_X_PORT,
                    WM_SETTEXT,(WPARAM)0,(LPARAM)buf);

            LoadString(GhInst,IDS_XMDM1KCRC+GProtocol,buf,15);

            SendDlgItemMessage(hDlg,IDC_X_PROTO,
                    WM_SETTEXT,(WPARAM)0,(LPARAM)buf);
            LoadString(GhInst,IDS_XMIT+GDirection,buf,15);
            SendMessage(hDlg,WM_SETTEXT,0,(LPARAM)buf);
            if(GProtocol==FT_XMIT)
                SendDlgItemMessage(hDlg,IDC_LEN_TEXT,
                        WM_SETTEXT,(WPARAM)0,(LPARAM)"X'mit Length :");
            else
                SendDlgItemMessage(hDlg,IDC_LEN_TEXT,
                        WM_SETTEXT,(WPARAM)0,(LPARAM)"Receive Length :");

            UpdateDlg(hDlg);
            return TRUE;

            case WM_COMMAND:
                switch (LOWORD(wParam)){
                case IDOK:
                case IDCANCEL:
                    /* user select cancel,set event to notify
                       callback function to return -1. */
                    SetEvent(GftStop);
                    sio_AbortRead(GCommData.Port);
                    sio_AbortWrite(GCommData.Port);
                    return TRUE;
                }
                break;
            case WM_FTCHG:
                UpdateDlg(hDlg);
                return TRUE;
	}
        return FALSE;
}


static void UpdateDlg(HWND hDlg)
{
        char	buf[15];

        wsprintf(buf,"%ld",Flen);
        SendDlgItemMessage(hDlg,IDC_X_FLEN,
                WM_SETTEXT,(WPARAM)0,(LPARAM)buf);

        wsprintf(buf,"%ld",Xlen);
        SendDlgItemMessage(hDlg,IDC_X_LEN,
                WM_SETTEXT,(WPARAM)0,(LPARAM)buf);

        SendDlgItemMessage(hDlg,IDC_X_FNAME,
                WM_SETTEXT,(WPARAM)0,(LPARAM)Fname);
}


void	SetFtStatData(long iflen,long ixlen,char *ifname)
{
        Flen = iflen;
        Xlen = ixlen;
        lstrcpy(Fname,ifname);
}
