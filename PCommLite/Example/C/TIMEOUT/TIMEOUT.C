/************************************************************************
    TimeOut.c
     -- Main module for read & write timeout setting example program.

    Description:
      1.Select "setting..." menu item to set com port option.
      2.Select "Open" menu item to open com port.
        After selected "Open" from menu, you can select
        "Timeout Demo"  to test timeout status.
        Polling Write:
		sio_write() will return immediately. The data will be 
 		copied to driver,but do not really wrtie to com port. This 
		operation will maybe let sio_write() return 0, means that
		not enough buffer to write. Be careful this polling operation 
		will 'eat' a large amount of system resource, including 
		memory, processor time.

	Block Write:
		sio_write() will block until the all data has be write to 
		com port (But maybe some data is still in com port output 
		buffer when sio_write() return). You can check the sio_write() 
		elapsed time. This operation is suitable to using another 
		thread to write data. Then you can call sio_AbortWrite() 
		to abort write operation in main thread when you want to 
		stop writing.

	Block Write (Timeout):
		This operation is the same as "Block Write". The difference 
		is that, sio_write() will block until the tiemout is arrived 
		or the all data has be write to com port. You can decrease 
		the timeout value to check the difference.

	Polling Read:
		sio_read() will return immediately. sio_read() just checks
		input buffer, gets all buffer data (maybe less than or equal
		to that sio_read() want to read), then returns. If no data in 
		buffer,	sio_read() return 0. Be careful this polling operation 
		will 'eat' a large amount of system resource, including memory, 
		processor time.
	
	Block Read:
		sio_read() will block until the input buffer data length is
		equal to that sio_read() want to read. This operation is 
		suitable to using another thread to read data. Then you can 
		call sio_AbortRead() to abort read operation in main thread 
		when you want to stop reading.

	Block Read (Total Timeout):
		This operation is the saem as "Block Read". The difference
		is that, sio_read() will block until timeout is arrived or
		the input buffer data length is equal to that sio_read()
		want to read.You can decrease the timeout value to check 
		the difference.
		In this example, you can connect to terminal. Then you can
		try 2 cases from terminal:
			send 10240 bytes, 
			or wait the timeout is arrived.
		Check the sio_read() elapsed time.

	Block Read (Interval Timeout):
		sio_read() will wait the first byte arrived, then begin
		interval timeout checking.sio_read will block until the
		interval timeout is arrived or the input buffer data length
		is equal to that sio_read() want to read.
		In this example, you can connect to terminal. Then you can
		try 2 cases from terminal:
			send 1 or 2 byte, 
			send 10240 bytes, 
		Check the sio_read() elapsed time.

	Block Read ( Total+Interval Timeout ):
		sio_read() will block until the timeout is arrived or the 
		input buffer data length is equdal to that sio_read() want 
		to read.
		In this example, you can connect to terminal. Then you can
		try 3 cases from terminal : 
			send 1 or 2 byte, 
			send 10240 bytes, 
			send > 10240 bytes
		Check the sio_read() elapsed time.
		                            
      3.Select "Close" menu item to close com port.

    This program demo:
        How to set write timeout(sio_SetWriteTimeouts());
        How to set read timeout(sio_SetReadTimeouts());
        How to abort write process(sio_AbortWrite());
        How to abort read process(sio_AbortRead());

    Use function:
        sio_open,       sio_close,   sio_ioctl,
        sio_flowctrl,   sio_DTR,     sio_RTS,
        sio_read,       sio_write,

        sio_SetWriteTimesout,        sio_SetReadTimeouts,
        sio_AbortWrite,              sio_AbortRead;


    History:   Date       Author         Comment
               3/1/98     Casper         Wrote it.

*************************************************************************/

#include	<windows.h>
#include	<windowsx.h>
#include	"PComm.h"
#include	"mxtool.h"
#include	"resource.h"
#include	"comm.h"

#pragma comment(lib, "pcomm.lib")

#define	BUFLEN			(10*1024)

HINSTANCE	GhInst;
COMMDATA	GCommData;
BOOL		GbOpen;
HANDLE		GhExit;

DWORD		GDifTime;
DWORD		GCount;
DWORD		GCallCount;
char	GszAppName[] = "TimeOut Setting Demo";

static	char	_GBuf[BUFLEN];

static	HANDLE	hWriteThread;
static	HANDLE	hReadThread;

static	HWND	GhWnd;

LRESULT CALLBACK WndProc(HWND hwnd,UINT iMsg,WPARAM wParam,LPARAM lParam);

#ifdef _WIN64
INT_PTR	CALLBACK PortDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
INT_PTR	CALLBACK AboutDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
INT_PTR	CALLBACK WStatDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
INT_PTR	CALLBACK RStatDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
#else
BOOL	CALLBACK PortDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
BOOL	CALLBACK AboutDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
BOOL	CALLBACK WStatDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
BOOL	CALLBACK RStatDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
#endif

static	void	SwitchMenu(HWND	hwnd);
static	BOOL	OpenPort(void);
static	BOOL	ClosePort(void);
static	BOOL	PortSet(void);
static	void 	ShowStatus(void);

UINT	WriteProc( LPVOID pParam );
UINT	ReadProc( LPVOID pParam );
void	DemoWriteTimeout(int port,UINT testitem);
void	DemoReadTimeout(int port,UINT testitem);

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
        wndclass.lpszMenuName	= MAKEINTRESOURCE(IDM_TIMEOUT);
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
            case IDM_WRITE_POLL:
            case IDM_WRITE_BLOCK:
            case IDM_WRITE_TIMEOUT:
                DemoWriteTimeout(GCommData.Port,LOWORD(wParam));
                return 0;
            case IDM_READ_POLL:
            case IDM_READ_BLOCK:
            case IDM_READ_BLOCK_T:
            case IDM_READ_BLOCK_I:
            case IDM_READ_BLOCK_TI:
                DemoReadTimeout(GCommData.Port,LOWORD(wParam));
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

            SwitchMenu(hwnd);
            return 0;
        case WM_CLOSE:
            if(GbOpen)
                SendMessage(hwnd,WM_COMMAND,IDM_PORT_CLOSE,0);
            break;
        case WM_DESTROY:
            PostQuitMessage(0);
            return 0;
        }
        return DefWindowProc(hwnd,iMsg,wParam,lParam);
}


static void SwitchMenu(HWND hwnd)
{
        HMENU	hMenu;
        int     i;

        hMenu = GetMenu(hwnd) ;

        if(GbOpen){
            EnableMenuItem( hMenu, IDM_PORT_OPEN,
            	MF_GRAYED | MF_DISABLED | MF_BYCOMMAND ) ;
            EnableMenuItem( hMenu, IDM_PORT_CLOSE, MF_ENABLED | MF_BYCOMMAND ) ;
            for(i=0;i<8;i++)
                EnableMenuItem(hMenu,IDM_WRITE_POLL+i,MF_ENABLED | MF_BYCOMMAND) ;
        }else{
            EnableMenuItem( hMenu, IDM_PORT_OPEN,MF_ENABLED | MF_BYCOMMAND);
            EnableMenuItem( hMenu, IDM_PORT_CLOSE,
            	MF_GRAYED | MF_DISABLED | MF_BYCOMMAND ) ;
            for(i=0;i<8;i++)
                EnableMenuItem( hMenu, IDM_WRITE_POLL+i,
            	    MF_GRAYED | MF_DISABLED | MF_BYCOMMAND ) ;
        }

        DrawMenuBar(hwnd);
}


BOOL OpenPort(void)
{
        int     ret;


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


void DemoWriteTimeout(int port,UINT testitem)
{
        DWORD		dwThreadID;
        DWORD		baud;
        DWORD		to;

        switch(testitem){
        case IDM_WRITE_POLL:
            sio_SetWriteTimeouts(port,MAXDWORD);
            break;
        case IDM_WRITE_BLOCK:
            sio_SetWriteTimeouts(port,0);
            break;
        case IDM_WRITE_TIMEOUT:
            baud = sio_getbaud(port);
            /* timeout(ms)  = Buflen *(1000/ (baud/10));
                            = Buflen *  (ms /byte) */
            to = (BUFLEN / (baud/10) * 1000) * 3;
            sio_SetWriteTimeouts(port,to);
            break;
        }


        if((GhExit = CreateEvent(NULL,TRUE,FALSE,NULL))==NULL)
            return ;

        hWriteThread = CreateThread( (LPSECURITY_ATTRIBUTES) NULL,
                     0,
                     (LPTHREAD_START_ROUTINE) WriteProc,
                     (LPVOID) GhWnd,
                     0, &dwThreadID );

        if(hWriteThread==NULL){
            CloseHandle(GhExit);
            sio_close(GCommData.Port);
            return ;
        }

        DialogBox(GhInst,MAKEINTRESOURCE(IDD_WRITE_STAT),GhWnd,WStatDlgProc);
        SetEvent(GhExit);
        sio_AbortWrite(port);
        if(WaitForSingleObject(hWriteThread,3000)==WAIT_TIMEOUT)
            TerminateThread(hWriteThread,0);
        CloseHandle(GhExit);
        CloseHandle(hReadThread);
}

UINT WriteProc( LPVOID pParam )
{
        int     i;
        DWORD   t1;
        for(i=0;i<BUFLEN;i++)
            _GBuf[i] = '0'+(char)(i%10);

        GCount  = 0;
        GCallCount = 0;
        GDifTime = 0;
        Sleep(1000);
        while(1){
            if(WaitForSingleObject(GhExit,0)==WAIT_OBJECT_0)//hExit
                break;

            t1 = GetCurrentTime();
            if(sio_write(GCommData.Port,_GBuf,BUFLEN)>0)
                GCount ++;
            GCallCount++;
            GDifTime = GetCurrentTime()-t1;
        }

        return TRUE;
}

void DemoReadTimeout(int port,UINT testitem)
{
        DWORD		dwThreadID;
        DWORD		baud;
        DWORD		to;

        baud = sio_getbaud(port);
        /* timeout(ms)  = Buflen *(1000/ (baud/10));
                        = Buflen *  (ms /byte) */
        to = (BUFLEN / (baud/10) * 1000) * 3;

        switch(testitem){
        case IDM_READ_POLL:
            sio_SetReadTimeouts(port,MAXDWORD,0);
            break;
        case IDM_READ_BLOCK:
            sio_SetReadTimeouts(port,0,0);
            break;
        case IDM_READ_BLOCK_T:
            sio_SetReadTimeouts(port,to,0);
            break;
        case IDM_READ_BLOCK_I:
            sio_SetReadTimeouts(port,0,1000);
            break;
        case IDM_READ_BLOCK_TI:
            sio_SetReadTimeouts(port,to,1000);
            break;
        }

        if((GhExit = CreateEvent(NULL,TRUE,FALSE,NULL))==NULL)
            return ;

        hReadThread = CreateThread( (LPSECURITY_ATTRIBUTES) NULL,
                    0,
                    (LPTHREAD_START_ROUTINE) ReadProc,
                    (LPVOID) GhWnd,
                    0, &dwThreadID );

        if(hReadThread==NULL){
            CloseHandle(GhExit);
            sio_close(port);
            return ;
        }

        DialogBox(GhInst,MAKEINTRESOURCE(IDD_READ_STAT),GhWnd,RStatDlgProc);

        SetEvent(GhExit);
        sio_AbortRead(port);
        if(WaitForSingleObject(hReadThread,3000)==WAIT_TIMEOUT)
            TerminateThread(hReadThread,0);
        CloseHandle(GhExit);
        CloseHandle(hReadThread);
}

UINT ReadProc( LPVOID pParam )
{
        DWORD   t1;
        DWORD   len;

        GCount  = 0;
        GCallCount  = 0;
        GDifTime = 0;
        Sleep(1000);
        while(1){
            if(WaitForSingleObject(GhExit,0)==WAIT_OBJECT_0)//hExit
                break;

            t1 = GetCurrentTime();
            len = sio_read(GCommData.Port,_GBuf,BUFLEN-1);
            GCallCount++;
            GDifTime = GetCurrentTime()-t1;
            if(len>0)
                GCount ++;
        }

        return TRUE;
}
