/************************************************************************
    FTrans.c
     -- Main module for file transfer example program.

    Description:
      1.Select "setting..." menu item to set com port option.
      2.Select "Open" menu item to open com port.
        After selected "Open" from menu,you can select
        "File Transfer" form menu to test file transfer function.
        Program will pop up a dialog to display status.
      3.When transferring file,you can push "Cancel" button to
        abort.
      4.Select "Close" menu item to close com port.

    This program demo:
        How to use file transfer function(sio_FtxxxTx,sio_FtxxxRx);
        How to use callback function to display file transfer status;
        How to process file transfer function return code;
        How to abort file transfer process in callback function;


    Use function:
        sio_open,       sio_close,         sio_ioctl,
        sio_flowctrl,   sio_DTR,           sio_RTS,
        sio_FtASCIITx,                     sio_FtASCIIRx,
        sio_FtXmodemCheckSumTx,            sio_FtXmodemCheckSumRx,
        sio_FtXmodemCRCTx,                 sio_FtXmodemCRCRx,
        sio_FtXmodem1KCRCTx,               sio_FtXmodem1KCRCRx,
        sio_FtYmodemTx,                    sio_FtYmodemRx,
        sio_FtZmodemTx,                    sio_FtZmodemRx,
        sio_FtKermitTx,                    sio_FtKermitRx.

    History:   Date       Author         Comment
               3/1/98     Casper         Wrote it.

*************************************************************************/

#include <windows.h>
#include <commdlg.h>
#include <windowsx.h>
#include <dlgs.h>
#include "PComm.h"
#include "mxtool.h"
#include "resource.h"
#include "comm.h"
#include "FTrans.h"

#pragma comment(lib, "pcomm.lib")

HINSTANCE       GhInst;
COMMDATA        GCommData;
BOOL            GbOpen;

char            GszAppName[] = "File Transfer";

char		GxFname[_MAX_PATH];
char		GrFname[_MAX_PATH];
char		GrPath[_MAX_PATH];

HWND	GhWnd;
HWND	GStatWnd;
HANDLE	GftStop;

LRESULT CALLBACK WndProc(HWND hwnd,UINT iMsg,WPARAM wParam,LPARAM lParam);
#ifdef _WIN64
INT_PTR	CALLBACK PortDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
INT_PTR	CALLBACK FtDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
INT_PTR	CALLBACK FtStatProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
INT_PTR	CALLBACK AboutDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
#else
BOOL	CALLBACK PortDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
BOOL	CALLBACK FtDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
BOOL	CALLBACK FtStatProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
BOOL	CALLBACK AboutDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
#endif

UINT	FtProc( LPVOID pParam );
static BOOL	XmitFile(HWND hwnd);
static BOOL	RecvFile(HWND hwnd);

static	void	SwitchMenu(HWND	hwnd);
static	BOOL	OpenPort(void);
static	BOOL	ClosePort(void);
static	BOOL	PortSet(void);
static	void 	ShowStatus(void);
static	HANDLE  hExit;
static  HANDLE  hFtThread;
static  BOOL    b_busy;


int WINAPI WinMain(HINSTANCE hInstance,HINSTANCE hPrevInstance,
				   PSTR szCmdLine,int iCmdShow)
{

        WNDCLASSEX      wndclass;
        HWND            hwnd;
        MSG             msg;

        GhInst = hInstance;

        wndclass.cbSize		= sizeof(WNDCLASSEX);
        wndclass.style 		= CS_HREDRAW | CS_VREDRAW;
        wndclass.lpfnWndProc= WndProc;
        wndclass.cbClsExtra	= 0;
        wndclass.cbWndExtra	= 0;
        wndclass.hInstance 	= hInstance;
        wndclass.hIcon	   	= LoadIcon(NULL,IDI_APPLICATION);
        wndclass.hCursor   	= LoadCursor(NULL,IDC_ARROW);
        wndclass.hbrBackground	= (HBRUSH)(COLOR_WINDOW + 1);
        wndclass.lpszMenuName	= MAKEINTRESOURCE(IDM_FTRANS);
        wndclass.lpszClassName	= GszAppName;
        wndclass.hIconSm   	= LoadIcon(NULL,IDI_APPLICATION);

        RegisterClassEx(&wndclass);

        hwnd = CreateWindow(GszAppName,
             GszAppName,
             WS_OVERLAPPEDWINDOW ,
             CW_USEDEFAULT,	CW_USEDEFAULT,
             CW_USEDEFAULT,	CW_USEDEFAULT,
             NULL,NULL,
             hInstance,
             NULL);

        GhWnd = hwnd;
        ShowWindow(hwnd,iCmdShow);
        UpdateWindow(hwnd);

        while(GetMessage(&msg,NULL,0,0)){
            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }

        return (int)msg.wParam;
}


LRESULT CALLBACK WndProc(HWND hwnd,UINT iMsg,WPARAM wParam,LPARAM lParam)
{
        switch(iMsg){
        case WM_COMMAND:
            switch(LOWORD(wParam)){
            case IDM_PORT_OPEN:
                OpenPort();
                return 0;
            case IDM_PORT_CLOSE:
                ClosePort();
                return 0;
            case IDM_PORT_SETTING:{
                COMMDATA bakdata = GCommData;
                if(b_busy)
                    return 0;
                if(DialogBox(GhInst,MAKEINTRESOURCE(IDD_OPEN),hwnd,PortDlgProc)==IDCANCEL)
                    return 0;
                if(GbOpen)
                    if (!PortSet())
                        GCommData = bakdata;
                return 0;
                }
            case IDM_FILE_TRANS:
                if(b_busy)
                    return 0;
                if(DialogBox(GhInst,MAKEINTRESOURCE(IDD_FILETRANS),hwnd,FtDlgProc)==IDCANCEL)
                    return 0;
                if(GDirection==FT_XMIT)
                    XmitFile(hwnd);
                else if(GDirection==FT_RECV)
                    RecvFile(hwnd);
                return 0;
            case IDM_HELP_ABOUT:
                DialogBox(GhInst,MAKEINTRESOURCE(IDD_ABOUT),hwnd,AboutDlgProc);
                return 0;
            case IDM_PORT_EXIT:
                SendMessage(hwnd,WM_CLOSE,0,0L);
                return 0;
            }
            break;
        case WM_CREATE:
            GCommData.Port = 1;
            GCommData.ibaudrate  = 14;
            GCommData.iparity = 0;
            GCommData.ibytesize = 3;
            GCommData.istopbits = 0;
            GCommData.BaudRate  = B38400;
            GCommData.Parity = P_NONE;
            GCommData.ByteSize = BIT_8;
            GCommData.StopBits = STOP_1;
            GCommData.Hw = FALSE;
            GCommData.Sw = FALSE;
            GCommData.Dtr = TRUE;
            GCommData.Rts = TRUE;
            GbOpen = FALSE;
            b_busy = FALSE;

            SwitchMenu(hwnd);
            return 0;
        case WM_CLOSE:
            if(GbOpen)
                SendMessage(hwnd,WM_COMMAND,IDM_PORT_CLOSE,0);
            break;
        case WM_DESTROY:
            PostQuitMessage(0);
            return 0;
        case WM_STCLOSE:
            DestroyWindow(GStatWnd);
            return 0;	 
        case WM_FTEND:
            WaitForSingleObject(hFtThread,INFINITE);
            CloseHandle(GftStop);
            CloseHandle(hFtThread);
            b_busy = FALSE;
            return 0;
        }

        return DefWindowProc(hwnd,iMsg,wParam,lParam);
}


static void SwitchMenu(HWND hwnd)
{
        HMENU	hMenu;

        hMenu = GetMenu(hwnd) ;

        if(GbOpen){
            EnableMenuItem( hMenu, IDM_PORT_OPEN,
            	MF_GRAYED | MF_DISABLED | MF_BYCOMMAND ) ;
            EnableMenuItem( hMenu, IDM_PORT_CLOSE, MF_ENABLED | MF_BYCOMMAND ) ;
            EnableMenuItem( hMenu, IDM_FILE_TRANS, MF_ENABLED | MF_BYCOMMAND ) ;
        }else{
            EnableMenuItem( hMenu, IDM_PORT_OPEN,MF_ENABLED | MF_BYCOMMAND);
            EnableMenuItem( hMenu, IDM_PORT_CLOSE,
            	MF_GRAYED | MF_DISABLED | MF_BYCOMMAND ) ;
            EnableMenuItem( hMenu, IDM_FILE_TRANS,
            	MF_GRAYED | MF_DISABLED | MF_BYCOMMAND ) ;
        }

        DrawMenuBar(hwnd);
}

BOOL OpenPort(void)
{
        int		ret;


        if((ret=sio_open(GCommData.Port))!=SIO_OK){
            MxShowError("sio_open",ret);
            return FALSE;
        }

        if(!PortSet()){
            sio_close(GCommData.Port);
            return FALSE;
        }

        GbOpen = TRUE;
        ShowStatus();
        SwitchMenu(GhWnd);

        return TRUE;
}


BOOL PortSet(void)
{
        int	port = GCommData.Port;
        int	mode = GCommData.Parity | GCommData.ByteSize | GCommData.StopBits;
        int	hw = GCommData.Hw ? 3 : 0;	/* bit0 and bit1 */
        int	sw = GCommData.Sw ? 12 : 0;     /* bit2 and bit3 */
        int	ret ;

        if((ret=sio_ioctl(port,GCommData.BaudRate,mode))!=SIO_OK){
            MxShowError("sio_ioctl",ret);
            return FALSE;
        }

        if((ret=sio_flowctrl(port,hw | sw))!=SIO_OK){
            MxShowError("sio_flowctrl",ret);
            return FALSE;
        }

        if((ret=sio_DTR(port,(GCommData.Dtr ? 1:0)))!=SIO_OK){
            MxShowError("sio_DTR",ret);
            return FALSE;
        }

        if(!GCommData.Hw){
            if((ret=sio_RTS(port,(GCommData.Rts ? 1:0)))!=SIO_OK){
                MxShowError("sio_RTS",ret);
                return FALSE;
            }
        }

        ShowStatus();
        return TRUE;
}


BOOL ClosePort(void)
{

        sio_close(GCommData.Port);
        GbOpen = FALSE;

        SwitchMenu(GhWnd);
        ShowStatus();

        return TRUE;
}



static void ShowStatus(void)
{
        char	szMessage[70];
        char	szbuf[20];

        lstrcpy(szMessage,GszAppName);

        if(GbOpen){
            wsprintf(szbuf," -- COM%d,",GCommData.Port);
            lstrcat(szMessage,szbuf);

            LoadString(GhInst,IDS_BAUD50+GCommData.ibaudrate,
                szbuf,20);
            lstrcat(szMessage,szbuf);
            lstrcat(szMessage,",");

            LoadString(GhInst,IDS_PARITYNONE+GCommData.iparity,
                szbuf,20);
            lstrcat(szMessage,szbuf);
            lstrcat(szMessage,",");

            LoadString(GhInst,IDS_DATABIT5+GCommData.ibytesize,
                szbuf,20);
            lstrcat(szMessage,szbuf);
            lstrcat(szMessage,",");

            LoadString(GhInst,IDS_ONESTOPBIT+GCommData.istopbits,
                szbuf,20);
            lstrcat(szMessage,szbuf);

            if(GCommData.Hw)
                lstrcat(szMessage,",RTS/CTS");

            if(GCommData.Sw)
                lstrcat(szMessage,",XON/XOFF");
        }

        SetWindowText(GhWnd,szMessage);
}



static BOOL XmitFile(HWND hwnd)
{
        OPENFILENAME ofn;
        char	szFile[_MAX_PATH];
        char	szFileTitle[_MAX_PATH];
        char	szFilter[] = "All file(*.*)|*.*|";
        char	chReplace;
        int	i;
        DWORD	dwThreadID;

        chReplace = szFilter[strlen(szFilter)-1];
        for(i=0;szFilter[i]!='\0';i++){
            if(szFilter[i]==chReplace)
                szFilter[i] = '\0';
        }

        lstrcpy(szFile,"*.*");
        lstrcpy(szFileTitle,"*.*");

        ZeroMemory(&ofn,sizeof(OPENFILENAME));
        ofn.lStructSize = sizeof(OPENFILENAME);
        ofn.hwndOwner = hwnd;
        ofn.lpstrFilter = szFilter;
        ofn.nFilterIndex = 1;
        ofn.lpstrFile = szFile;
        ofn.nMaxFile = sizeof(szFile);
        ofn.lpstrFileTitle = szFileTitle;
        ofn.nMaxFileTitle = sizeof(szFileTitle);
        ofn.lpstrInitialDir = NULL;
        ofn.lpstrTitle = "Select File to Transmit";
        ofn.Flags = OFN_FILEMUSTEXIST | OFN_HIDEREADONLY;

        if(!GetOpenFileName(&ofn))
            return FALSE;

        lstrcpy(GxFname,ofn.lpstrFile);


        GftStop = CreateEvent(NULL,TRUE,FALSE,NULL);

        GStatWnd = CreateDialog(GhInst,MAKEINTRESOURCE(IDD_STATUS),
                   hwnd,FtStatProc);

        hFtThread = CreateThread(
                  (LPSECURITY_ATTRIBUTES) NULL,
                  0,
                  (LPTHREAD_START_ROUTINE) FtProc,
                  (LPVOID) hwnd,
                  0, &dwThreadID );
        b_busy = TRUE;

        return TRUE;
}



#ifdef _WIN64
UINT_PTR APIENTRY OfHookProc(HWND hdlg,UINT uiMsg,WPARAM wParam,LPARAM lParam)
#else
UINT APIENTRY OfHookProc(HWND hdlg,UINT uiMsg,WPARAM wParam,LPARAM lParam)
#endif
{
        static BOOL m_bDlgJustCameUp;
        switch(uiMsg){
        case WM_INITDIALOG:
            ShowWindow(GetDlgItem(hdlg,stc2),SW_HIDE);
            ShowWindow(GetDlgItem(hdlg,stc3),SW_HIDE);
            ShowWindow(GetDlgItem(hdlg,edt1),SW_HIDE);
            ShowWindow(GetDlgItem(hdlg,lst1),SW_HIDE);
            ShowWindow(GetDlgItem(hdlg,cmb1),SW_HIDE);
            SetDlgItemText(hdlg,edt1,"Junk");
            SetFocus(GetDlgItem(hdlg,lst2));
            m_bDlgJustCameUp=TRUE;
            return TRUE;
        case WM_PAINT:
            if (m_bDlgJustCameUp){
                m_bDlgJustCameUp=FALSE;
                SendDlgItemMessage(hdlg,lst2,LB_SETCURSEL,0,0L);
            }
            break;
        }
        return FALSE;
}



static BOOL RecvFile(HWND hwnd)
{
        OPENFILENAME ofn;
        char	szFile[_MAX_PATH];
        char	szFileTitle[_MAX_PATH];
        char	szFilter[] = "All file(*.*)|*.*|";
        char	chReplace;
        int     i;
        WORD	wFileOffset;
        DWORD	dwThreadID;

        chReplace = szFilter[strlen(szFilter)-1];
        for(i=0;szFilter[i]!='\0';i++){
            if(szFilter[i]==chReplace)
                szFilter[i] = '\0';
        }

        lstrcpy(szFile,"*.*");
        lstrcpy(szFileTitle,"*.*");

        ZeroMemory(&ofn,sizeof(OPENFILENAME));
        ofn.lStructSize = sizeof(OPENFILENAME);
        ofn.hwndOwner = hwnd;
        ofn.lpstrFilter = szFilter;
        ofn.nFilterIndex = 1;
        ofn.lpstrFile = szFile;
        ofn.nMaxFile = sizeof(szFile);
        ofn.lpstrFileTitle = szFileTitle;
        ofn.nMaxFileTitle = sizeof(szFileTitle);
        ofn.lpstrInitialDir = NULL;

        if((GProtocol!=FTYMDM) && (GProtocol!=FTZMDM) && (GProtocol!=FTKERMIT)){
            ofn.Flags = OFN_OVERWRITEPROMPT | OFN_HIDEREADONLY;
            ofn.lpstrTitle = "Select File to Receive";
            if(!GetSaveFileName(&ofn))
                return FALSE;
            GetCurrentDirectory(_MAX_PATH-1,GrPath);
            lstrcpy(GrFname,ofn.lpstrFile);
        }else{
            /* open dialog for user to select directory. */
            /* ZModem, YModem, Kermit protocol will transfer
               download file name in package */
            ofn.Flags = OFN_OVERWRITEPROMPT | OFN_HIDEREADONLY
                  | OFN_ENABLETEMPLATE | OFN_ENABLEHOOK ;
            ofn.hInstance = GhInst;
            ofn.lpTemplateName = MAKEINTRESOURCE(FILEOPENORD);
            ofn.Flags         &= ~OFN_EXPLORER;
            ofn.lpstrTitle = "Select Folder";
            ofn.lpfnHook = OfHookProc; /* change dialog style */
            if(!GetSaveFileName(&ofn))
                return FALSE;

            wFileOffset = ofn.nFileOffset;  //for convenience
            ofn.lpstrFile[wFileOffset-1]=0;
            SetCurrentDirectory(ofn.lpstrFile);
            lstrcpy(GrPath,ofn.lpstrFile);
            lstrcpy(GrFname,"");
        }


        GftStop = CreateEvent(NULL,TRUE,FALSE,NULL);

        GStatWnd = CreateDialog(GhInst,MAKEINTRESOURCE(IDD_STATUS),
                   hwnd,FtStatProc);

        hFtThread = CreateThread(
                  (LPSECURITY_ATTRIBUTES) NULL,
                  0,
                  (LPTHREAD_START_ROUTINE) FtProc,
                  (LPVOID) hwnd,
                  0, &dwThreadID );

        b_busy = TRUE;

        return TRUE;
}



