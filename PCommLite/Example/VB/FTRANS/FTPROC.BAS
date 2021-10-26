Attribute VB_Name = "FtProc"
'*********************************************************************
'    FtProc.bas
'     -- File transfer procedure for file transfer example program.
'
'    History:   Date       Author         Comment
'               3/9/98     Casper         Wrote it.
'               12/14/98   Casper         Modify message,Update.
'
'**********************************************************************)
Option Explicit

Public Const FT_XMIT = 0
Public Const FT_RECV = 1

Public Const FTXMDM1KCRC = 0
Public Const FTXMDMCHK = 1
Public Const FTXMDMCRC = 2
Public Const FTZMDM = 3
Public Const FTYMDM = 4
Public Const FTKERMIT = 5
Public Const FTASCII = 6

Public Const MAX_PATH = 260     'Win32 defined

Public GstrProtocol(7) As String
Public GProtocol As Integer
Public GDirection As Integer
Public GftCancel As Boolean

Public GxFname As String * MAX_PATH
Public GrFname As String * MAX_PATH
Public GrPath As String * MAX_PATH

Dim G_buftab(0) As Long     ' This is received file name buffer for
                            ' ZModem & Ymodem & Kermit.
                            
Public Const GMEM_FIXED = &H0
Declare Function GlobalAlloc Lib "kernel32" (ByVal wFlags As Long, ByVal dwBytes As Long) As Long
Declare Function GlobalLock Lib "kernel32" (ByVal hMem As Long) As Long
Declare Function GlobalFree Lib "kernel32" (ByVal hMem As Long) As Long
Declare Function lstrcpy Lib "kernel32" Alias "lstrcpyA" (ByVal lpString1 As Long, ByVal lpString2 As String) As Long
Declare Function lstrcpyn Lib "kernel32" Alias "lstrcpynA" (ByVal lpString1 As String, ByVal lpString2 As Long, ByVal iMaxLength As Long) As Long
Declare Function lstrlen Lib "kernel32" Alias "lstrlenA" (ByVal lpString As Long) As Long

Public Function xCallBack(ByVal xmitlen As Long, ByVal buflen As Long, ByVal buf As Long, ByVal flen As Long) As Long
    On Error Resume Next
    
    'If you want to parsing buf's content,
    'you should do like below :
    
    'Dim tmp As String
    'tmp = String(buflen, 0)          'make a empty string
    'Call lstrcpyn(tmp, buf, buflen)  'buf is a pointer
    
    
    If GftCancel Then
        'If user set GftCancel to true to terminate file transfer,
        'you can return < 0 to stop file transfer
        xCallBack = -1
        Exit Function
    End If
    
    DoEvents
    Call FtStat.RefreshDlg(xmitlen, flen, GxFname)
    xCallBack = 0
End Function

Public Function rCallBack(ByVal recvlen As Long, ByVal buflen As Long, ByVal buf As Long, ByVal flen As Long) As Long
    On Error Resume Next
    
    'If you want to parsing buf's content,
    'you should do like below :
    
    'Dim tmp As String
    'tmp = String(buflen, 0)          'make a empty string
    'Call lstrcpyn(tmp, buf, buflen)  'buf is a pointer
    
    If GftCancel Then
        'If user set GftCancel to true to terminate file transfer
        'you can return < 0 to stop file transfer
        rCallBack = -1
        Exit Function
    End If
    
    'If using ZModem or YModem or Kermit protocol to download file,
    'sio_Ftxxx_Rx() will put the received file name to file name
    'buffer which you pass to sio_Ftxxx_Rx().
    If (GProtocol = FTZMDM) Or (GProtocol = FTYMDM) Or (GProtocol = FTKERMIT) Then
        Call lstrcpyn(GrFname, G_buftab(0), lstrlen(G_buftab(0)) + 1)
    End If
    
    DoEvents
    Call FtStat.RefreshDlg(recvlen, flen, GrFname)
    rCallBack = 0
End Function

Public Sub FtTableInit()
    GstrProtocol(0) = "XModem-1KCRC"
    GstrProtocol(1) = "XModem-CheckSum"
    GstrProtocol(2) = "XModem-CRC"
    GstrProtocol(3) = "ZModem"
    GstrProtocol(4) = "YModem"
    GstrProtocol(5) = "Kermit"
    GstrProtocol(6) = "ASCII"
End Sub

Public Sub FtFunc()
Dim ret As Long
Dim Port As Long

    Port = GCommData.Port
    
    If (GDirection = FT_XMIT) Then
        Select Case GProtocol
        Case FTXMDM1KCRC:
            ret = sio_FtXmodem1KCRCTx(Port, GxFname, AddressOf xCallBack, 27)
        Case FTXMDMCHK:
            ret = sio_FtXmodemCheckSumTx(Port, GxFname, AddressOf xCallBack, 27)
        Case FTXMDMCRC:
            ret = sio_FtXmodemCRCTx(Port, GxFname, AddressOf xCallBack, 27)
        Case FTZMDM:
            ret = sio_FtZmodemTx(Port, GxFname, AddressOf xCallBack, 27)
        Case FTYMDM:
            ret = sio_FtYmodemTx(Port, GxFname, AddressOf xCallBack, 27)
        Case FTKERMIT:
            ret = sio_FtKermitTx(Port, GxFname, AddressOf xCallBack, 27)
        Case FTASCII:
            ret = sio_FtASCIITx(Port, GxFname, AddressOf xCallBack, 27)
        End Select
    Else
        Select Case GProtocol
        Case FTXMDM1KCRC:
            ret = sio_FtXmodem1KCRCRx(Port, GrFname, AddressOf rCallBack, 27)
        Case FTXMDMCHK:
            ret = sio_FtXmodemCheckSumRx(Port, GrFname, AddressOf rCallBack, 27)
        Case FTXMDMCRC:
            ret = sio_FtXmodemCRCRx(Port, GrFname, AddressOf rCallBack, 27)
        Case FTZMDM:
            'To get received file name,
            'you can do like below :
            G_buftab(0) = GlobalLock(GlobalAlloc(GMEM_FIXED, MAX_PATH + 1))
            Call lstrcpy(G_buftab(0), "")
            ret = sio_FtZmodemRx(GCommData.Port, G_buftab(0), 1, AddressOf rCallBack, 27)
            GlobalFree (G_buftab(0))
            
            'If you want to download multi file,
            'you can do like below(ex: download 2 files):
            '
            'G_buftab(0) = GlobalLock(GlobalAlloc(GMEM_FIXED, MAX_PATH))
            'Call lstrcpy(G_buftab(0), "")
            'G_buftab(1) = GlobalLock(GlobalAlloc(GMEM_FIXED, MAX_PATH))
            'Call lstrcpy(G_buftab(1), "")
            'ret = sio_FtZmodemRx(GCommData.Port, G_buftab(0), 2, AddressOf rCallBack, 27)
            'GlobalFree (G_buftab(0))
            'GlobalFree (G_buftab(1))
            
        Case FTYMDM:
            'To get received file name,
            'you can do like below :
            G_buftab(0) = GlobalLock(GlobalAlloc(GMEM_FIXED, MAX_PATH) + 1)
            Call lstrcpy(G_buftab(0), "")
            ret = sio_FtYmodemRx(GCommData.Port, G_buftab(0), 1, AddressOf rCallBack, 27)
            GlobalFree (G_buftab(0))
            
            'see above FTZMDM for more infomation.
            
        Case FTKERMIT:
            'To get received file name,
            'you can do like below :
            G_buftab(0) = GlobalLock(GlobalAlloc(GMEM_FIXED, MAX_PATH + 1))
            Call lstrcpy(G_buftab(0), "")
            ret = sio_FtKermitRx(GCommData.Port, G_buftab(0), 1, AddressOf rCallBack, 27)
            GlobalFree (G_buftab(0))
            
            'see above FTZMDM for more infomation.
            
        Case FTASCII:
            ret = sio_FtASCIIRx(Port, GrFname, AddressOf rCallBack, 27, 3)
        End Select
    End If
    Unload FtStat
    If ret < 0 Then
        Call ProcessRet(Port, ret, GProtocol, GDirection)
    Else
        If (GDirection = FT_XMIT) Then
            MsgBox "File Transmit OK"
        Else
            MsgBox "Flie Receive OK"
        End If
    End If
End Sub

Public Sub ProcessRet(Port As Long, ret As Long, protocol As Integer, direction As Integer)
Dim buf As String
    If (ret <> SIOFT_WIN32FAIL) Then
        Select Case ret
        Case SIOFT_BADPORT:
            buf = "Port is not opened in advance"
        Case SIOFT_TIMEOUT:
            If (direction = FT_RECV) Then
                buf = "Receive timeout"
            Else
                buf = "Transmit Timeout"
            End If
        Case SIOFT_FUNC:
            If ((protocol = FTASCII) And direction = FT_RECV) Then
                ' When downloading ASCII file,user must press "Cancel"
                ' button to stop ASCII receive
                buf = "Receive File Ok"
            Else
                buf = "User abort"
            End If
        Case SIOFT_FOPEN:
            buf = "Can't open file"
        Case SIOFT_CANABORT:
            buf = "Remote side abort"
        Case SIOFT_BOARDNOTSUPPORT:
            buf = "Board does not support this function"
        Case SIOFT_PROTOCOL, SIOFT_SKIP:
            buf = "File transfer error"
        Case Else
            buf = "File transfer error"
        End Select
        MsgBox buf, vbOKOnly, "File Transfer"
    Else
        Call ShowSysErr("File Transfer", GetLastError())
    End If

End Sub

