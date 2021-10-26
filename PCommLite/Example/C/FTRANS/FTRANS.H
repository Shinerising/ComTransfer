

#ifndef	_FTRANS_H
#define _FTRANS_H

#define	FTXMDM1KCRC	0
#define FTXMDMCHK	1
#define FTXMDMCRC	2
#define FTZMDM		3
#define FTYMDM		4
#define FTKERMIT        5
#define FTASCII		6

#define FT_XMIT		0
#define FT_RECV		1

#define WM_FTCHG	(WM_USER+100)
#define WM_FTEND	(WM_USER+101)
#define WM_STCLOSE	(WM_USER+102)

extern	int     GProtocol;
extern	int     GDirection;
extern	char    GxFname[];
extern	char    GrFname[];
extern	char    GrPath[];

void SetFtStatData(long iflen,long ixlen,char *ifname);
UINT FtProc( LPVOID pParam );


#endif