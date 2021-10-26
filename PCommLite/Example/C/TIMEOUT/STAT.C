/************************************************************************
    Stat.c
     -- Show dialog to display read & write timeout status.

    History:   Date       Author         Comment
               3/1/98     Casper         Wrote it.

*************************************************************************/

#include	<windows.h>
#include	"PComm.h"
#include	"resource.h"
#include	"comm.h"

extern	DWORD	GCount;
extern	DWORD	GCallCount;
extern	DWORD	GDifTime;

#define	IDT_TIMER	100

BOOL CALLBACK WStatDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam)
{

        char	buf[15];

        switch(iMsg){
        case WM_INITDIALOG:
            SetTimer(hDlg,IDT_TIMER,300,NULL);
            return TRUE;
        case WM_TIMER:
            wsprintf(buf,"%ld",GCount);
            SetWindowText(GetDlgItem(hDlg,IDC_WRITE_COUNT),buf);
            wsprintf(buf,"%ldms",GDifTime);
            SetWindowText(GetDlgItem(hDlg,IDC_WRITE_TIME),buf);
            wsprintf(buf,"%ld",GCallCount);
            SetWindowText(GetDlgItem(hDlg,IDC_WRITE_CALLCNT),buf);
            return TRUE;
        case WM_COMMAND:
            switch (LOWORD(wParam)){
            case IDCANCEL:
                KillTimer(hDlg,IDT_TIMER);
                EndDialog(hDlg,LOWORD(wParam));
                return TRUE;
            }
            break;
        }
        return FALSE;
}


BOOL CALLBACK RStatDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam)
{

        char	buf[15];

        switch(iMsg){
        case WM_INITDIALOG:
            SetTimer(hDlg,IDT_TIMER,300,NULL);
            return TRUE;
        case WM_TIMER:
            wsprintf(buf,"%ld",GCount);
            SetWindowText(GetDlgItem(hDlg,IDC_READ_COUNT),buf);
            wsprintf(buf,"%ldms",GDifTime);
            SetWindowText(GetDlgItem(hDlg,IDC_READ_TIME),buf);
            wsprintf(buf,"%ld",GCallCount);
            SetWindowText(GetDlgItem(hDlg,IDC_READ_CALLCNT),buf);
            return TRUE;
        case WM_COMMAND:
            switch (LOWORD(wParam)){
            case IDCANCEL:
                KillTimer(hDlg,IDT_TIMER);
                EndDialog(hDlg,LOWORD(wParam));
                return TRUE;
            }
            break;
        }
        return FALSE;
}
