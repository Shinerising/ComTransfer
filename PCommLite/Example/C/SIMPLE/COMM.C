/************************************************************************
    Comm.c
      -- Show dialog for user to set commnication parameter.


    History:    Date          Author              Comment
                3/1/98        Casper              Wrote it.

*************************************************************************/

#include	<windows.h>
#include	"PComm.h"
#include	"resource.h"
#include	"comm.h"

#pragma comment(lib, "pcomm.lib")

extern	HINSTANCE	GhInst;
extern	COMMDATA	GCommData;
extern	BOOL		GbOpen;

static	void	InitOpenDlg(HWND hDlg);

#define	BAUDCOUNT	20
#define	DATABITCOUNT	4
#define	PARITYCOUNT	5
#define	STOPBITCOUNT	2

int GBaudTable[BAUDCOUNT] = {
	B50,B75,B110,B134,B150,B300,B600,B1200,B1800,B2400,
	B4800,B7200,B9600,B19200,B38400,B57600,
	B115200,B230400,B460800,B921600
};

int GDataBitsTable[DATABITCOUNT] = {
	BIT_5,BIT_6,BIT_7,BIT_8
};

int GParityTable[PARITYCOUNT] = {
	P_NONE, P_EVEN, P_ODD, P_MRK, P_SPC
};

int GStopBitsTable[STOPBITCOUNT] ={
	STOP_1,STOP_2
};


BOOL CALLBACK PortDlgProc(HWND hDlg,UINT iMsg,WPARAM wParam,LPARAM lParam)
{
        static BOOL	fhw;

        switch(iMsg){
        case WM_INITDIALOG:
            InitOpenDlg(hDlg);
            if(SendDlgItemMessage(hDlg,IDC_HW,BM_GETSTATE,0,0L)==BST_CHECKED)
                fhw = TRUE;
            else
                fhw = FALSE;
                return TRUE;

        case WM_COMMAND:
            if(HIWORD(wParam)==BN_CLICKED){
                if(LOWORD(wParam) == IDC_HW){
                    EnableWindow(GetDlgItem(hDlg,IDC_RTS),fhw);
                    fhw = !fhw;
                    return TRUE;
                }
            }
            switch (LOWORD(wParam)){
            case IDOK:
                GCommData.Port	    = (int)SendDlgItemMessage(hDlg,IDC_PORT,CB_GETCURSEL,0,0L)+1;

                GCommData.ibaudrate = (int)SendDlgItemMessage(hDlg,IDC_BAUD,CB_GETCURSEL,0,0L);
                GCommData.iparity   = (int)SendDlgItemMessage(hDlg,IDC_PARITY,CB_GETCURSEL,0,0L);
                GCommData.ibytesize = (int)SendDlgItemMessage(hDlg,IDC_DATABITS,CB_GETCURSEL,0,0L);
                GCommData.istopbits = (int)SendDlgItemMessage(hDlg,IDC_STOPBITS,CB_GETCURSEL,0,0L);
                GCommData.BaudRate = GBaudTable[GCommData.ibaudrate];
                GCommData.Parity = GParityTable[GCommData.iparity];
                GCommData.ByteSize = GDataBitsTable[GCommData.ibytesize];
                GCommData.StopBits = GStopBitsTable[GCommData.istopbits];

                GCommData.Hw =
                  (SendDlgItemMessage(hDlg,IDC_HW,BM_GETSTATE,0,0L)==BST_CHECKED);

                GCommData.Sw =
                  (SendDlgItemMessage(hDlg,IDC_SW,BM_GETSTATE,0,0L)==BST_CHECKED);

                GCommData.Dtr =
                  (SendDlgItemMessage(hDlg,IDC_DTR,BM_GETSTATE,0,0L)==BST_CHECKED);

                GCommData.Rts =
                  (SendDlgItemMessage(hDlg,IDC_RTS,BM_GETSTATE,0,0L)==BST_CHECKED);
                /* Fall through */
            case IDCANCEL:
                EndDialog(hDlg,LOWORD(wParam));
                return TRUE;
            }
            break;
        }
        return FALSE;
}


static void FillComboBox(HINSTANCE hInstance,HWND hCtrlWnd,
			int idstr,int tablelen,int pos)
{
        char	buf[ 20 ] ;
        int	idx;

        for (idx=0; idx<tablelen; idx++){
            /* load the string from the string resources and
               add it to the combo box */
            LoadString(hInstance,idstr+idx,buf,sizeof(buf)) ;
            SendMessage(hCtrlWnd,CB_ADDSTRING,0,(LPARAM)buf);
        }
        SendMessage(hCtrlWnd,CB_SETCURSEL,(WPARAM)pos,0L) ;
}



static void InitOpenDlg(HWND hDlg)
{
        char	buf[10];
        int	com;
        int	set;

        /* fill port combo box and make initial selection */
        for (com=1; com<=MAXCOM; com++){
            wsprintf(buf,"%s%d","COM",com) ;
            SendDlgItemMessage(hDlg,IDC_PORT,CB_ADDSTRING,0,(LPARAM)buf);
        }
        SendDlgItemMessage(hDlg,IDC_PORT,CB_SETCURSEL,
                (WPARAM)(GCommData.Port-1),0L);
        if(GbOpen)
            EnableWindow(GetDlgItem(hDlg,IDC_PORT),FALSE);

        /* fill baudrate combo box and make initial selection */
        FillComboBox(GhInst,GetDlgItem(hDlg,IDC_BAUD),
                IDS_BAUD50,BAUDCOUNT,GCommData.ibaudrate);

        /* fill data bits combo box and make initial selection */
        FillComboBox(GhInst,GetDlgItem(hDlg,IDC_DATABITS),
                IDS_DATABIT5,DATABITCOUNT,GCommData.ibytesize);

        /* fill parity combo box and make initial selection */
        FillComboBox(GhInst,GetDlgItem(hDlg,IDC_PARITY),
                IDS_PARITYNONE,PARITYCOUNT,GCommData.iparity);

        /* fill stop bits combo box and make initial selection */
        FillComboBox(GhInst,GetDlgItem(hDlg,IDC_STOPBITS),
                IDS_ONESTOPBIT,STOPBITCOUNT,GCommData.istopbits);

        set = GCommData.Hw ? BST_CHECKED : BST_UNCHECKED;
        SendDlgItemMessage(hDlg,IDC_HW,BM_SETCHECK,(WPARAM)set,0L);

        set = GCommData.Sw ? BST_CHECKED : BST_UNCHECKED;
        SendDlgItemMessage(hDlg,IDC_SW,BM_SETCHECK,(WPARAM)set,0L);

        set = GCommData.Dtr ? BST_CHECKED : BST_UNCHECKED;
        SendDlgItemMessage(hDlg,IDC_DTR,BM_SETCHECK,(WPARAM)set,0L);

        set = GCommData.Rts ? BST_CHECKED : BST_UNCHECKED;
        SendDlgItemMessage(hDlg,IDC_RTS,BM_SETCHECK,(WPARAM)set,0L);

        /* disable RTS setting when RTS/CTS flow control */
        EnableWindow(GetDlgItem(hDlg,IDC_RTS),!(GCommData.Hw));
}
