/************************************************************************
    Simple.c
     -- Main module for simple dumb terminal example program.

    Description:
      1.Select "setting..." menu item to set com port option.
      2.Select "Open" menu item to open com port.
        After selected "Open" from menu,you can type any character
        to send to com port.When any data received from com port,
        program will dump data to window.
      3.Select "Close" menu item to close com port.

    This program demo:
	How to open com port(sio_open),
	How to set commnunication parameter(sio_ioctl,sio_flowctrl),
	How to control line status(sio_DTR,sio_RTS),
	How to send data to com port(sio_write,sio_putch),
	How to read data(sio_read),
	How to close com port(sio_close),
	How to use background thread to read data.

    Use function:
        sio_open,       sio_close,      sio_ioctl,
        sio_flowctrl,   sio_DTR,        sio_RTS,
        sio_putch,      sio_write,
        sio_SetWriteTimeouts;

    History:   Date       Author         Comment
               3/1/98     Casper         Wrote it.

*************************************************************************/
#ifndef		STRICT
#define 	STRICT
#endif

#include	<windows.h>
#include	<windowsx.h>
#include	"PComm.h"
#include	"resource.h"
#include	"comm.h"
#include	"mxtool.h"

HINSTANCE       GhInst;
COMMDATA        GCommData;
BOOL            GbOpen;
char            GszAppName[] = "Simple Demo";

static	HANDLE	hExit;
static	HANDLE	hCommWatchThread;
static	HWND	hwndedit;
static	HWND	GhWnd;


LRESULT CALLBACK WndProc(HWND hwnd,UINT iMsg,WPARAM wParam,LPARAM lParam);
#ifdef _WIN64
INT_PTR	CALLBACK PortDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
INT_PTR	CALLBACK AboutDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
#else
BOOL	CALLBACK PortDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
BOOL	CALLBACK AboutDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
#endif
UINT	CommWatchProc( LPVOID pParam );
static	void	SwitchMenu(HWND	hwnd);
static	BOOL	OpenPort(void);
static	BOOL	ClosePort(void);
static	BOOL	PortSet(void);
static	void 	ShowStatus(void);

LRESULT	CALLBACK EditSubClassProc(HWND hwnd,UINT uMsg,WPARAM wParam,LPARAM lParam);
WNDPROC	_wpOrigWndProc;

int WINAPI WinMain(HINSTANCE hInstance,HINSTANCE hPrevInstance,
				   PSTR szCmdLine,int iCmdShow)
{

        WNDCLASSEX      wndclass;
        HWND            hwnd;
        MSG             msg;

        GhInst = hInstance;

        wndclass.cbSize		= sizeof(WNDCLASSEX);
        wndclass.style 		= CS_HREDRAW | CS_VREDRAW;
        wndclass.lpfnWndProc	= WndProc;
        wndclass.cbClsExtra	= 0;
        wndclass.cbWndExtra	= 0;
        wndclass.hInstance 	= hInstance;
        wndclass.hIcon	   	= LoadIcon(NULL,IDI_APPLICATION);
        wndclass.hCursor   	= LoadCursor(NULL,IDC_ARROW);
        wndclass.hbrBackground	= (HBRUSH)(COLOR_WINDOW + 1);
        wndclass.lpszMenuName	= MAKEINTRESOURCE(IDM_SIMPLE);
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
                if(DialogBox(GhInst,MAKEINTRESOURCE(IDD_OPEN),hwnd,PortDlgProc)==IDCANCEL)
                    return 0;
                if(GbOpen)
                    if (!PortSet())
                        GCommData = bakdata;
                return 0;
                }
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

            hwndedit = CreateWindowEx (WS_EX_CLIENTEDGE | WS_EX_WINDOWEDGE,
                       "edit",NULL,
                       WS_CHILD | WS_VISIBLE | ES_LEFT | WS_VSCROLL |
                       ES_MULTILINE |  ES_AUTOVSCROLL,
                       0,0,0,0,
                       hwnd,(HMENU)4,GhInst,NULL) ;
            _wpOrigWndProc = SubclassWindow(hwndedit,EditSubClassProc);

            SwitchMenu(hwnd);
            return 0;

        case WM_SIZE:
            MoveWindow (hwndedit,0,0,LOWORD(lParam),HIWORD(lParam),TRUE) ;
            return 0;
        case WM_CLOSE:
            if(GbOpen)
                SendMessage(hwnd,WM_COMMAND,IDM_PORT_CLOSE,0);
            break;
        case WM_SETFOCUS:
            SetFocus(hwndedit);
            return 0 ;
        case WM_DESTROY:
            PostQuitMessage(0);
            return 0;
        }
        return DefWindowProc(hwnd,iMsg,wParam,lParam);
}

LRESULT	CALLBACK EditSubClassProc(HWND hwnd,UINT uMsg,WPARAM wParam,LPARAM lParam)
{
        char ch;

        switch(uMsg){
        case WM_CHAR:
            if(GbOpen){
                ch = (TCHAR) wParam;
                sio_write(GCommData.Port,&ch,1);
                /* Or use sio_putch() :
                sio_putch(GCommData.Port,ch);*/
            }
            return 0;
        }

        return CallWindowProc(_wpOrigWndProc,hwnd,uMsg,wParam,lParam);
}

static void SwitchMenu(HWND hwnd)
{
        HMENU	hMenu;

        hMenu = GetMenu(hwnd) ;

        if(GbOpen){
            EnableMenuItem( hMenu, IDM_PORT_OPEN,
            	MF_GRAYED | MF_DISABLED | MF_BYCOMMAND ) ;
            EnableMenuItem( hMenu, IDM_PORT_CLOSE, MF_ENABLED | MF_BYCOMMAND ) ;
        }else{
            EnableMenuItem( hMenu, IDM_PORT_OPEN,MF_ENABLED | MF_BYCOMMAND);
            EnableMenuItem( hMenu, IDM_PORT_CLOSE,
            	MF_GRAYED | MF_DISABLED | MF_BYCOMMAND ) ;
        }

        DrawMenuBar(hwnd);
}


BOOL OpenPort(void)
{
        DWORD   dwThreadID;
        int     ret;


        if((ret=sio_open(GCommData.Port))!=SIO_OK){
            MxShowError("sio_open",ret);
            return FALSE;
        }

        if(!PortSet()){
            sio_close(GCommData.Port);
            return FALSE;
        }

        if((hExit = CreateEvent(NULL,TRUE,FALSE,NULL))==NULL)
            return FALSE;

        hCommWatchThread = CreateThread( (LPSECURITY_ATTRIBUTES) NULL,
                         0,
                         (LPTHREAD_START_ROUTINE) CommWatchProc,
                         (LPVOID) NULL,
                         0, &dwThreadID );
        if(hCommWatchThread==NULL){
            CloseHandle(hExit);
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
        DWORD   tout;

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

        tout = 1000 / sio_getbaud(port);  /* ms/byte */
        if (tout < 1)
            tout = 1;
        tout = tout * 1 * 3;             /* 1 byte; '*3' is for delay */
        if(tout<100)
            tout = 100;
        sio_SetWriteTimeouts(port, tout);

        ShowStatus();
        return TRUE;
}


BOOL ClosePort(void)
{

        SetEvent(hExit);

        if(WaitForSingleObject(hCommWatchThread,1000)==WAIT_TIMEOUT)
            TerminateThread(hCommWatchThread,0);

        CloseHandle(hExit);
        CloseHandle(hCommWatchThread);
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


UINT CommWatchProc( LPVOID pParam )
{
        int	len,lend;
        char	buf[512];
        DWORD	dRes;

        while(1){
            dRes = WaitForSingleObject(hExit,10);
            switch(dRes){
            case WAIT_OBJECT_0:/* got hExit event,exit thread. */
                return 0;
            default:
                break;
            }

            len = sio_read(GCommData.Port,buf,512);
            if(len>0){
                /*
                  When got any data,dump buffer to Edit window.

                  NOTE:
                       If any Null character in buffer,
                       characters after null can't be dumped
                       to Edit window.
                */
                lend = Edit_GetTextLength(hwndedit);

                if(lend>25000){
                    /* Edit Control buffer size limit */
                    Edit_SetSel(hwndedit,0,-1);
                    Edit_ReplaceSel(hwndedit,"");
                    lend = 0;
                }

                Edit_SetSel(hwndedit,lend,lend);
                buf[len] ='\x0';
                Edit_ReplaceSel(hwndedit,buf);
            }
        }
}
