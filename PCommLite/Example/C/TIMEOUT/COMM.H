/*
	Comm.h

	Include file for port setting.
*/

#ifndef _COMM_H
#define _COMM_H

typedef struct tagCOMMDATA{
	int     Port;
	int     BaudRate,Parity,ByteSize,StopBits;
        int	ibaudrate,iparity,ibytesize,istopbits;
	BOOL    Hw;		/* RTS/CTS hardware flow control */
	BOOL	Sw;		/* XON/XOFF software flow control */
	BOOL    Dtr,Rts;
}COMMDATA,*LPCOMMDATA;

extern int GBaudTable[] ;
extern int GParityTable[] ;
extern int GStopBitsTable[] ;
extern int GDataBitsTable[];

#define MAXCOM  256

#endif
