/************************************************************************
    StatDlg.c
     -- Show dialog to display port status.

        History:   Date       Author         Comment
                   3/1/98     Casper         Wrote it.

*************************************************************************/

#include	<windows.h>
#include	<stdio.h>
#include	"PComm.h"
#include	"comm.h"
#include	"resource.h"

#define	MX_REFRESH	(WM_USER+100)

extern	HINSTANCE	GhInst;
extern	COMMDATA	GCommData;
extern	HWND		GhDlgStatus;

#define	IDT_TIMER	100
static void Refresh(HWND hDlg);

BOOL CALLBACK StatusDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam)
{
        switch(iMsg){
        case WM_INITDIALOG:
            Refresh(hDlg);
            SetTimer(hDlg,IDT_TIMER,100,NULL);
            return TRUE;

        case WM_TIMER:
            Refresh(hDlg);
            return TRUE;

        case WM_COMMAND:
            switch (LOWORD(wParam)){
            case IDCANCEL:
                KillTimer(hDlg,IDT_TIMER);
                DestroyWindow(hDlg);
                GhDlgStatus = NULL;
                return TRUE;
            }
            break;
        case MX_REFRESH:
            Refresh(hDlg);
            break;
        }

        return FALSE;
}


#define	DATABITSMSK	(BIT_5 | BIT_6 | BIT_7 | BIT_8)
#define STOPBITSMSK	(STOP_1 | STOP_2)
#define	PARITYMSK	(P_EVEN | P_ODD	| P_SPC	| P_MRK	| P_NONE)

static void Refresh(HWND hDlg)
{
        int     port;
        char    buf[80];
        long    baud;
        int     mode,databits,stopbits,parity;
        int     lstatus;
        int     txhold;
        int     d_status;
        int     flow;
        int     i;

        port = GCommData.Port;

        /* sio_data_status();This should be called first !! */
        d_status = sio_data_status(port);;
        if(d_status & 0x01){
            wsprintf(buf,"%s","parity error");
            SendDlgItemMessage(hDlg,IDC_ST_DATASTAT,
                    LB_ADDSTRING,0,(LPARAM)buf);
        }

        if(d_status & 0x02){
            wsprintf(buf,"%s","framing error");
            SendDlgItemMessage(hDlg,IDC_ST_DATASTAT,
                    LB_ADDSTRING,0,(LPARAM)buf);
        }

        if(d_status & 0x04){
            wsprintf(buf,"%s","overrun error");
            SendDlgItemMessage(hDlg,IDC_ST_DATASTAT,
                    LB_ADDSTRING,0,(LPARAM)buf);
        }

        if(d_status & 0x08){
            wsprintf(buf,"%s","overflow error");
            SendDlgItemMessage(hDlg,IDC_ST_DATASTAT,
                    LB_ADDSTRING,0,(LPARAM)buf);
        }

        wsprintf(buf,"COM%d",port);
        SetWindowText(GetDlgItem(hDlg,IDC_ST_PORT),buf);

        /* sio_getbaud() */
        baud = sio_getbaud(port);
        wsprintf(buf,"%ld",baud);
        SetWindowText(GetDlgItem(hDlg,IDC_ST_BAUD),buf);

	/* sio_getmode() */
        mode = sio_getmode(port);
        parity = mode & PARITYMSK;
        databits = mode & DATABITSMSK;
        stopbits = mode & STOPBITSMSK;
        for(i=0;i<5;i++)
            if(parity==GParityTable[i]){
                LoadString(GhInst,IDS_PARITYNONE+i,buf,sizeof(buf));
                break;
            }
        SetWindowText(GetDlgItem(hDlg,IDC_ST_PARITY),buf);

        for(i=0;i<4;i++)
            if(databits==GDataBitsTable[i]){
                LoadString(GhInst,IDS_DATABIT5+i,buf,sizeof(buf));
                break;
            }
        SetWindowText(GetDlgItem(hDlg,IDC_ST_DATABITS),buf);

        for(i=0;i<2;i++)
            if(stopbits==GStopBitsTable[i]){
                LoadString(GhInst,IDS_ONESTOPBIT+i,buf,sizeof(buf));
                break;
            }
        SetWindowText(GetDlgItem(hDlg,IDC_ST_STOPBITS),buf);

	/* sio_getflow() */
        flow = sio_getflow(port);
        if(flow==0){
            lstrcpy(buf,"NONE");
            SetWindowText(GetDlgItem(hDlg,IDC_ST_FLOW),buf);
        }else{
            lstrcpy(buf,"");
            if(flow&0x01)
                lstrcat(buf,"<CTS>");
            if(flow&0x02)
                lstrcat(buf,"<RTS>");
            if(flow&0x04)
                lstrcat(buf,"<Tx XON/XOFF>");
            if(flow&0x08)
                lstrcat(buf,"<Rx XON/XOFF>");
            SetWindowText(GetDlgItem(hDlg,IDC_ST_FLOW),buf);
        }

        /* sio_lstatus() */
        lstatus = sio_lstatus(port);
        wsprintf(buf,"%s",lstatus & S_CTS ? "CTS": "cts");
        SetWindowText(GetDlgItem(hDlg,IDC_ST_CTS),buf);
        wsprintf(buf,"%s",lstatus & S_DSR ? "DSR": "dsr");
        SetWindowText(GetDlgItem(hDlg,IDC_ST_DSR),buf);
        wsprintf(buf,"%s",lstatus & S_RI ? "RI": "ri");
        SetWindowText(GetDlgItem(hDlg,IDC_ST_RI),buf);
        wsprintf(buf,"%s",lstatus & S_CD ? "DCD": "dcd");
        SetWindowText(GetDlgItem(hDlg,IDC_ST_DCD),buf);

        /* sio_iqueue() */
        wsprintf(buf,"%ld",sio_iqueue(port));
        SetWindowText(GetDlgItem(hDlg,IDC_ST_IQUEUE),buf);

        /* sio_oqueue() */
        wsprintf(buf,"%ld",sio_oqueue(port));
        SetWindowText(GetDlgItem(hDlg,IDC_ST_OQUEUE),buf);


        /* sio_Tx_hold */
        SendDlgItemMessage(hDlg,IDC_ST_TXHOLD,
                    LB_RESETCONTENT,0,0);
        txhold = sio_Tx_hold(port);
        if(txhold & 0x01){
            wsprintf(buf,"%s","CTS is low");
            SendDlgItemMessage(hDlg,IDC_ST_TXHOLD,
                    LB_ADDSTRING,0,(LPARAM)buf);
        }
        if(txhold & 0x02){
            wsprintf(buf,"%s","XOFF char received");
            SendDlgItemMessage(hDlg,IDC_ST_TXHOLD,
                        LB_ADDSTRING,0,(LPARAM)buf);
        }
        if(txhold & 0x04){
            wsprintf(buf,"%s","by sio_disableTx()");
            SendDlgItemMessage(hDlg,IDC_ST_TXHOLD,
                        LB_ADDSTRING,0,(LPARAM)buf);
        }

}


