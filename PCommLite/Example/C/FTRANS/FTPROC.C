/************************************************************************
    FtProc.c
     -- File transfer thread & callback function

    This module demo :
        How to use file transfer function(sio_FtxxxTx,sio_FtxxxRx);
        How to use callback function to display file transfer status;
        How to process file transfer function return code;
        How to abort file transfer process in callback function;


    History:   Date       Author         Comment
               3/1/98     Casper         Wrote it.
               12/14/98   Casper         Modify message.Update.
*************************************************************************/


#include	<windows.h>
#include	"PComm.h"
#include	"resource.h"
#include	"ftrans.h"
#include	"comm.h"


extern  HWND            GhWnd,GStatWnd;
extern	HINSTANCE       GhInst;
extern	HANDLE          GftStop;
extern	COMMDATA        GCommData;


int CALLBACK xCallback(long xmitlen, int buflen, char *buf, long flen)
{
        /* when user push cancel button,GftStop will be set.
           return < 0 will terminate file transfer */
        if(WaitForSingleObject(GftStop,0)==WAIT_OBJECT_0)
            return -1;

        SetFtStatData(flen,xmitlen,GxFname);
        SendMessage(GStatWnd,WM_FTCHG,0,0);

        return 0;
}


int CALLBACK rCallback(long recvlen, int buflen, char *buf, long flen)
{

        char tmp[_MAX_PATH];

        /* when user push cancel button,GftStop will be set.
           return < 0 will terminate file transfer */
        if(WaitForSingleObject(GftStop,0)==WAIT_OBJECT_0)
            return -1;

        lstrcpy(tmp,GrPath);
        lstrcat(tmp,"\\");
        lstrcat(tmp,GrFname);

        SetFtStatData(flen,recvlen,tmp);

        SendMessage(GStatWnd,WM_FTCHG,0,0);

        return 0;
}


void	ProcessRet(int port,int ret,int protocol,BOOL recv)
{
        char	msg[50];
        LPSTR	lpMsgBuf;

        if(ret!= SIOFT_WIN32FAIL){
            switch(ret){
            case SIOFT_BADPORT:
                LoadString(GhInst,IDS_EBADPORT,msg,sizeof(msg));
                break;
            case SIOFT_TIMEOUT:
                if(recv)
                    LoadString(GhInst,IDS_RTIMEOUT,msg,sizeof(msg));
                else
                    LoadString(GhInst,IDS_TTIMEOUT,msg,sizeof(msg));
                break;
            case SIOFT_FUNC:
                if((protocol==FTASCII) && (recv))
                    /* When downloading ASCII file,user must press "Cancel"
                       button to stop ASCII receive */
                    LoadString(GhInst,IDS_RECV_OK,msg,sizeof(msg));
                else
                    LoadString(GhInst,IDS_ABORT,msg,sizeof(msg));
                break;
            case SIOFT_FOPEN	:
                LoadString(GhInst,IDS_FOPEN,msg,sizeof(msg));
                break;
            case SIOFT_CANABORT	:
                LoadString(GhInst,IDS_CANABORT,msg,sizeof(msg));
                break;
            case SIOFT_BOARDNOTSUPPORT :
                LoadString(GhInst,IDS_BOARDNOTSUPPORT,msg,sizeof(msg));
                break;
            case SIOFT_PROTOCOL	:
            case SIOFT_SKIP	:
	    default:
                LoadString(GhInst,IDS_FTERR,msg,sizeof(msg));
                break;
            }
            MessageBox(GhWnd,msg,"File Transfer",MB_OK | MB_ICONINFORMATION);
            return;
        }else{
            FormatMessage(
                    FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM,
                    NULL,
                    GetLastError(),
                    MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
                    (LPTSTR) &lpMsgBuf,
                    0,
                    NULL
            );
            /* Display the string. */
            MessageBox(GhWnd,lpMsgBuf,"File Transfer", MB_OK|MB_ICONINFORMATION );
            /* Free the buffer. */
            LocalFree( lpMsgBuf );
        }
}




UINT FtProc( LPVOID pParam )
{
        int  ret,port;
        char *ntmp[1];

        port = GCommData.Port;

        ret = 0;
        if(GDirection==FT_XMIT){
            switch(GProtocol){
            case FTXMDM1KCRC	:
                ret = sio_FtXmodem1KCRCTx(port,GxFname,xCallback,27);
                break;
            case FTXMDMCHK	:
                ret = sio_FtXmodemCheckSumTx(port,GxFname,xCallback,27);
                break;
            case FTXMDMCRC:
                ret = sio_FtXmodemCRCTx(port,GxFname,xCallback,27);
                break;
            case FTZMDM	:
                ret = sio_FtZmodemTx(port,GxFname,xCallback,27);
                break;
            case FTYMDM	:
                ret = sio_FtYmodemTx(port,GxFname,xCallback,27);
                break;
            case FTKERMIT:
                ret = sio_FtKermitTx(port,GxFname,xCallback,27);
                break;
            case FTASCII:
                ret = sio_FtASCIITx(port,GxFname,xCallback,27);
                break;
            }
	}else if(GDirection==FT_RECV){
            switch(GProtocol){
            case FTXMDM1KCRC	:
                ret = sio_FtXmodem1KCRCRx(port,GrFname,rCallback,27);
                break;
            case FTXMDMCHK	:
                ret = sio_FtXmodemCheckSumRx(port,GrFname,rCallback,27);
                break;
            case FTXMDMCRC:
                ret = sio_FtXmodemCRCRx(port,GrFname,rCallback,27);
                break;
            case FTZMDM	:
                ntmp[0] = GrFname;
                ret = sio_FtZmodemRx(port,&(ntmp[0]),1,rCallback,27);
                break;
            case FTYMDM	:
                ntmp[0] = GrFname;
                ret = sio_FtYmodemRx(port,&(ntmp[0]),1,rCallback,27);
                break;
            case FTKERMIT:
                ntmp[0] = GrFname;
                ret = sio_FtKermitRx(port,&(ntmp[0]),1,rCallback,27);
                break;
            case FTASCII:
                ret = sio_FtASCIIRx(port,GxFname,rCallback,27,3);
                break;
            }
        }

        PostMessage(GhWnd,WM_STCLOSE,0,0);

        if(ret<0)
            ProcessRet(port,ret,GProtocol,GDirection);
        else{
            char msg[20];
            if(GDirection==FT_XMIT)
                LoadString(GhInst,IDS_TRAN_OK,msg,20);
            else
                LoadString(GhInst,IDS_RECV_OK,msg,20);
            MessageBox(GhWnd,msg,"File Transfer",
                    MB_OK | MB_ICONINFORMATION);

	}
        PostMessage(GhWnd,WM_FTEND,0,0);
	return TRUE;
}