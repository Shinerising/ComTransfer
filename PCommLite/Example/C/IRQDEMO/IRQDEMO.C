/************************************************************************
    IrqDemo.c
     -- Main module for Irq function demo example program.

    Description:
      1.Select "setting..." menu item to set com port option.
      2.Select "Open" menu item to open com port.
        After selected "Open" from menu,you can type any key to
        test sio_Tx_empty_irq(). You can also connect to another
        terminal to test Irq function:
          a.Sending some data to gerenate 'Rx event'(sio_cnt_irq);
          b.Changing modem line(DTR/RTS) to generate
            'modem line changed event'(sio_modem_irq);
          c.Sending break signal to generate 'break event'
            (sio_break_irq);
          d.Typing 'A' will generate 'RxFlag event'(sio_term_irq)
            and 'Rx event'(sio_cnt_irq);
        This program will got evnet and show Irq count to screen.
      3.Select "Close" menu item to close com port.

    This program demo:
        How to use Irq function(sio_xxx_irq);
        How to disable Irq function;
        How to use Irq callback funtion.

    Use function:
        sio_open,       sio_close,      sio_ioctl,
        sio_flowctrl,   sio_DTR,        sio_RTS,
        sio_putch,      sio_write,      sio_read,
        sio_SetWriteTimeouts,

        sio_term_irq,   sio_cnt_irq,    sio_break_irq,
        sio_modem_irq,  sio_Tx_empty_irq.

    History:   Date       Author         Comment
               3/1/98     Casper         Wrote it.
              12/07/98    Casper         Modified. Read process in cntirq.
*************************************************************************/
#ifndef STRICT
#define STRICT
#endif

#include        <windows.h>
#include        <windowsx.h>
#include        "PComm.h"
#include        "mxtool.h"
#include        "resource.h"
#include        "comm.h"

#pragma comment(lib, "pcomm.lib")

#define         IDT_DUMMY    101

HINSTANCE       GhInst;
COMMDATA        GCommData;
BOOL            GbOpen;
char            GszAppName[] = "Irq Demo";

int     GIdx;
int     GTermIrqCnt;
int     GCntIrqCnt;
int     GModemIrqCnt;
int     GBreakIrqCnt;
int     GTxEmptyIrqCnt;


LRESULT CALLBACK WndProc(HWND hwnd,UINT iMsg,WPARAM wParam,LPARAM lParam);
#ifdef _WIN64
INT_PTR    CALLBACK PortDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
INT_PTR    CALLBACK AboutDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
#else
BOOL    CALLBACK PortDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
BOOL    CALLBACK AboutDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam);
#endif


VOID    CALLBACK TermIrq(int port);
VOID    CALLBACK CntIrq(int port);
VOID    CALLBACK ModemIrq(int port);
VOID    CALLBACK BreakIrq(int port);
VOID    CALLBACK TxEmptyIrq(int port);


static  HWND    hwndedit;
static  HWND    GhWnd;

static  void    SwitchMenu(HWND hwnd);
static  BOOL    OpenPort(void);
static  BOOL    ClosePort(void);
static  BOOL    PortSet(void);
static  void    ShowStatus(void);
static  void    ClearIrq(int port);
static  BOOL    InitIrq(int port,char termcode);
static  void    ShowCnt(HWND hwndedit,int cnt,char *title);

LRESULT CALLBACK EditSubClassProc(HWND hwnd,UINT uMsg,WPARAM wParam,LPARAM lParam);
WNDPROC _wpOrigWndProc;


int WINAPI WinMain(HINSTANCE hInstance,HINSTANCE hPrevInstance,
                                   PSTR szCmdLine,int iCmdShow)
{

        WNDCLASSEX      wndclass;
        HWND            hwnd;
        MSG             msg;

        GhInst = hInstance;

        wndclass.cbSize         = sizeof(WNDCLASSEX);
        wndclass.style          = 0;
        wndclass.lpfnWndProc    = WndProc;
        wndclass.cbClsExtra     = 0;
        wndclass.cbWndExtra     = 0;
        wndclass.hInstance      = hInstance;
        wndclass.hIcon          = LoadIcon(NULL,IDI_APPLICATION);
        wndclass.hCursor        = LoadCursor(NULL,IDC_ARROW);
        wndclass.hbrBackground  = (HBRUSH)(COLOR_WINDOW + 1);
        wndclass.lpszMenuName   = MAKEINTRESOURCE(IDM_IRQDEMO);
        wndclass.lpszClassName  = GszAppName;
        wndclass.hIconSm        = LoadIcon(NULL,IDI_APPLICATION);

        RegisterClassEx(&wndclass);
        hwnd = CreateWindow(GszAppName,
             GszAppName,
             WS_OVERLAPPEDWINDOW ,
             CW_USEDEFAULT,     CW_USEDEFAULT,
             CW_USEDEFAULT,     CW_USEDEFAULT,
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
        case WM_TIMER:{
                char buf[128];
                int  len,cnt=0;
                do{
                    len = sio_read(GCommData.Port,buf,128);
                }while(len!=0 && (++cnt<10));
                return 0;
            }
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

LRESULT CALLBACK EditSubClassProc(HWND hwnd,UINT uMsg,WPARAM wParam,LPARAM lParam)
{
        char ch;

        switch(uMsg){
        case WM_CHAR:
            if(GbOpen){
                ch = (TCHAR) wParam;
                sio_write(GCommData.Port,&ch,1);
                /* Or use sio_putch() :
                sio_putch(GCommData.Port,ch);
                */
            }
            return 0;
        }

        return CallWindowProc(_wpOrigWndProc,hwnd,uMsg,wParam,lParam);
}

static void SwitchMenu(HWND hwnd)
{
        HMENU   hMenu;

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
        int             ret;


        if((ret=sio_open(GCommData.Port))!=SIO_OK){
            MxShowError("sio_open",ret);
            return FALSE;
        }

        if(!PortSet()){
            sio_close(GCommData.Port);
            return FALSE;
        }

        if(!InitIrq(GCommData.Port,'A')){
            ClearIrq(GCommData.Port);
            sio_close(GCommData.Port);
            return FALSE;
        }

        GbOpen = TRUE;
        ShowStatus();
        SwitchMenu(GhWnd);

        SetTimer(GhWnd,IDT_DUMMY,100,NULL);
        return TRUE;
}

BOOL PortSet(void)
{
        int     port = GCommData.Port;
        int     mode = GCommData.Parity | GCommData.ByteSize | GCommData.StopBits;
        int     hw = GCommData.Hw ? 3 : 0;      /* bit0 and bit1 */
        int     sw = GCommData.Sw ? 12 : 0;     /* bit2 and bit3 */
        int     ret ;
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

        ClearIrq(GCommData.Port);
        sio_close(GCommData.Port);
        GbOpen = FALSE;

        SwitchMenu(GhWnd);
        ShowStatus();
        KillTimer(GhWnd,IDT_DUMMY);

        return TRUE;
}



static void ShowCnt(HWND hwndedit,int cnt,char *title)
{
        char buf[50];
        int  lend;

        lend = Edit_GetTextLength(hwndedit);

        if(lend>25000){
            /* Edit Control buffer size limit */
            Edit_SetSel(hwndedit,0,-1);
            Edit_ReplaceSel(hwndedit,"");
            lend = 0;
        }

        GIdx++;
        Edit_SetSel(hwndedit,lend,lend);
        wsprintf(buf,"(idx=%d)%s,count=%d\r\n",GIdx,title,cnt);
        Edit_ReplaceSel(hwndedit,buf);
}


VOID CALLBACK TermIrq(int port)
{
        GTermIrqCnt++;
        ShowCnt(hwndedit,GTermIrqCnt,"sio_term_irq()");
}

VOID CALLBACK CntIrq(int port)
{
        GCntIrqCnt++;
        ShowCnt(hwndedit,GCntIrqCnt,"sio_cnt_irq()");
}

VOID CALLBACK ModemIrq(int port)
{
        GModemIrqCnt++;
        ShowCnt(hwndedit,GModemIrqCnt,"sio_modem_irq()");
}

VOID CALLBACK BreakIrq(int port)
{
        GBreakIrqCnt++;
        ShowCnt(hwndedit,GBreakIrqCnt,"sio_break_irq()");
}

VOID CALLBACK TxEmptyIrq(int port)
{
        GTxEmptyIrqCnt++;
        ShowCnt(hwndedit,GTxEmptyIrqCnt,"sio_Tx_empty_irq()");
}


static BOOL InitIrq(int port,char termcode)
{
        int     ret;
        if((ret=sio_term_irq(port,TermIrq,termcode))!=SIO_OK){
            MxShowError("sio_term_irq",ret);
            return FALSE;
        }

        if((ret=sio_cnt_irq(port,CntIrq,1))!=SIO_OK){
            MxShowError("sio_cnt_irq",ret);
            return FALSE;
        }

        if((ret=sio_modem_irq(port,ModemIrq))!=SIO_OK){
            MxShowError("sio_modem_irq",ret);
            return FALSE;
        }

        if((ret=sio_break_irq(port,BreakIrq))!=SIO_OK){
            MxShowError("sio_break_irq",ret);
            return FALSE;
        }

        if((ret=sio_Tx_empty_irq(port,TxEmptyIrq))!=SIO_OK){
            MxShowError("sio_Tx_empty_irq",ret);
            return FALSE;
        }

        GIdx=0;
        GTermIrqCnt=0;
        GCntIrqCnt=0;
        GModemIrqCnt=0;
        GBreakIrqCnt=0;
        GTxEmptyIrqCnt=0;

        return TRUE;
}



static void     ClearIrq(int port)
{
        int     ret;
        if((ret=sio_term_irq(port,NULL,0))!=SIO_OK)
            MxShowError("sio_term_irq",ret);

        if((ret=sio_cnt_irq(port,NULL,0))!=SIO_OK)
            MxShowError("sio_cnt_irq",ret);

        if((ret=sio_modem_irq(port,NULL))!=SIO_OK)
            MxShowError("sio_modem_irq",ret);

        if((ret=sio_break_irq(port,NULL))!=SIO_OK)
            MxShowError("sio_break_irq",ret);

        if((ret=sio_Tx_empty_irq(port,NULL))!=SIO_OK)
            MxShowError("sio_Tx_empty_irq",ret);
}


static void ShowStatus(void)
{
        char    szMessage[70];
        char    szbuf[20];

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



