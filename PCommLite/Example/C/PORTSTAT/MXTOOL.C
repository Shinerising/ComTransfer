/*************************************************************
    Mxtool.c
     -- Process PComm Lib function return value


    History:   Date       Author         Comment
               3/1/98     Casper         Wrote it.
               12/08/98   Casper         Modify message.

************************************************************/

#include	<windows.h>
#include	"PComm.h"


void ShowSysErr(LPCTSTR title)
{
	LPSTR	lpMsgBuf;
	DWORD	syserr;

	syserr = GetLastError();
	FormatMessage( 
		FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM,
		NULL,
		syserr,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), /* Default language */
		(LPTSTR) &lpMsgBuf,
		0,
		NULL
	);
	// Display the string.
	MessageBox(NULL,lpMsgBuf,title,MB_OK|MB_ICONEXCLAMATION);
	// Free the buffer.
	LocalFree(lpMsgBuf);
}


void MxShowError(LPCTSTR title,int errcode)
{
	char	buf[60];

	if(errcode != SIO_WIN32FAIL){
	    switch(errcode){
	    case SIO_BADPORT:
		lstrcpy(buf,"Port number is invalid or port is not opened in advance");
		break;
	    case SIO_OUTCONTROL:
		lstrcpy(buf,"The board does not support this function");
		break;
	    case SIO_NODATA:
		lstrcpy(buf,"No data to read");
		break;
            case SIO_OPENFAIL:
		lstrcpy(buf,"No such port or port is occupied by other program");
		break;
	    case SIO_RTS_BY_HW:
		lstrcpy(buf,"RTS can't be set because RTS/CTS Flowctrl");
		break;
	    case SIO_BADPARM:
		lstrcpy(buf,"Bad parameter");
		break;
	    case SIO_BOARDNOTSUPPORT:
		lstrcpy(buf,"The board does not support this function");
		break;
	    case SIO_ABORTWRITE:
		lstrcpy(buf,"Write has blocked, and user abort write");
		break;
	    case SIO_WRITETIMEOUT:
		lstrcpy(buf,"Write timeout has happened");
		break;
	    default:
		wsprintf(buf,"Unknown Error:%d",errcode);
		break;
	    }
	    MessageBox(NULL,buf,title,MB_OK|MB_ICONEXCLAMATION);
	}else
	    ShowSysErr(title);
}

