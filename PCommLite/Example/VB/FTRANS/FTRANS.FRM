VERSION 5.00
Object = "{F9043C88-F6F2-101A-A3C9-08002B2F49FB}#1.1#0"; "COMDLG32.OCX"
Begin VB.Form FTrans 
   Caption         =   "File Transfer Demo"
   ClientHeight    =   4230
   ClientLeft      =   165
   ClientTop       =   735
   ClientWidth     =   5775
   LinkTopic       =   "Form1"
   ScaleHeight     =   4230
   ScaleWidth      =   5775
   StartUpPosition =   3  'Windows Default
   Begin MSComDlg.CommonDialog ComDlg 
      Left            =   2760
      Top             =   480
      _ExtentX        =   847
      _ExtentY        =   847
      _Version        =   327680
      CancelError     =   -1  'True
      DialogTitle     =   "Select File to Transmit"
   End
   Begin VB.TextBox Term 
      Height          =   4215
      Left            =   0
      MultiLine       =   -1  'True
      TabIndex        =   0
      Top             =   0
      Width           =   5775
   End
   Begin VB.Menu cmPort 
      Caption         =   "Port"
      Begin VB.Menu cmOpen 
         Caption         =   "Open"
      End
      Begin VB.Menu cmClose 
         Caption         =   "Close"
      End
      Begin VB.Menu stat 
         Caption         =   "-"
      End
      Begin VB.Menu cmExit 
         Caption         =   "Exit"
      End
   End
   Begin VB.Menu cmSetting 
      Caption         =   "Setting..."
   End
   Begin VB.Menu cmFtrans 
      Caption         =   "File Transfer..."
   End
   Begin VB.Menu Help 
      Caption         =   "Help"
      Begin VB.Menu cmAbout 
         Caption         =   "About"
      End
   End
End
Attribute VB_Name = "FTrans"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'*********************************************************************
'    FTrans.frm
'     -- Main Window for file transfer example program.
'
'    Description:
'      1.Select "setting..." menu item to set com port option.
'      2.Select "Open" menu item to open com port.
'        After selected "Open" from menu,user can select
'        "File Transfer" form menu to test file transfer function.
'        Program will pop up a dialog to display status.
'      3.When transferring file,you can push "Cancel" button to
'        abort.
'      4.Select "Close" menu item to close com port.
'
'    This program demo:
'        How to use file transfer function(sio_FtxxxTx,sio_FtxxxRx);
'        How to use callback function to display file transfer status;
'        How to process file transfer function return code;
'        How to abort file transfer process in callback function;
'
'    Use function:
'        sio_open,       sio_close,         sio_ioctl,
'        sio_flowctrl,   sio_DTR,           sio_RTS,
'        sio_FtASCIITx,                     sio_FtASCIIRx,
'        sio_FtXmodemCheckSumTx,            sio_FtXmodemCheckSumRx,
'        sio_FtXmodemCRCTx,                 sio_FtXmodemCRCRx,
'        sio_FtXmodem1KCRCTx,               sio_FtXmodem1KCRCRx,
'        sio_FtYmodemTx,                    sio_FtYmodemRx,
'        sio_FtZmodemTx,                    sio_FtZmodemRx,
'        sio_FtKermitTx,                    sio_FtKermitRx.
'
'    History:   Date       Author         Comment
'               6/1/98     Casper         Wrote it.
'
'*******************************************************************


Option Explicit

Public GbOpen As Boolean
Public GszAppName As String

Public GDirDlgCancel As Boolean
Public GFsetCancel As Boolean


Dim Ldx As Long
Dim Ldy As Long


Private Sub Form_Load()
    With GCommData
        .Port = 1
        .BaudRate = B38400
        .Parity = P_NONE
        .ByteSize = BIT_8
        .StopBits = STOP_1
        .ibaudrate = 14
        .iparity = 0
        .ibytesize = 3
        .istopbits = 0
        .Hw = 0
        .Sw = 0
        .Dtr = 1
        .Rts = 1
    End With
    
    Ldx = Width - Term.Width
    Ldy = Height - Term.Height
    GbOpen = False
    GszAppName = "File Transfer Demo"
    Set GhForm = FTrans
    GDirection = 0
    GProtocol = 0
    
    Call InitTable
    Call FtTableInit
    Call SwitchMenu
End Sub

Private Sub Form_Resize()
    If WindowState <> vbMinimized Then
        Term.Width = Width - Ldx
        Term.Height = Height - Ldy
    End If
End Sub

Private Sub Form_Unload(Cancel As Integer)
    If GbOpen Then
        Call ClosePort
    End If
End Sub

Private Sub cmOpen_Click()
    Call OpenPort
End Sub

Private Sub cmClose_Click()
    Call ClosePort
End Sub

Private Sub cmSetting_Click()
Dim bakdata As COMMDATA
    
    bakdata = GCommData
    Config.Show vbModal
    If (GbOpen) Then
        If (PortSet() = False) Then
            GCommData = bakdata
            Exit Sub
        End If
    End If
    Call ShowStatus
End Sub

Private Sub cmFtrans_Click()
    GFsetCancel = False
    FSet.Show vbModal
    If GFsetCancel = True Then
        Exit Sub
    End If
    
    If GDirection = FT_XMIT Then
        Call XmitFile
    Else
        Call RecvFile
    End If
End Sub

Private Sub cmAbout_Click()
    About.AboutTxt1.Caption = "PComm File Transfer Demo Example"
    About.Show
End Sub

Private Sub cmExit_Click()
    If GbOpen Then
        Call ClosePort
    End If
    Unload FTrans
    End
End Sub

Sub SwitchMenu()
    cmOpen.Enabled = Not GbOpen
    cmClose.Enabled = GbOpen
    cmFtrans.Enabled = GbOpen
End Sub

Private Function OpenPort() As Boolean
Dim ret As Long
Dim syserr As Long
    
    OpenPort = False
    
    ret = sio_open(GCommData.Port)
    If ret <> SIO_OK Then
        Call MxShowError("sio_open", ret, GetLastError())
        sio_close (GCommData.Port)
        Exit Function
    End If
    
    If PortSet() = False Then
        sio_close (GCommData.Port)
        Exit Function
    End If
    
    OpenPort = True
    GbOpen = True
    Call SwitchMenu
    Call ShowStatus
End Function
Private Sub ClosePort()
    sio_close (GCommData.Port)
    GbOpen = False
    Call SwitchMenu
    Call ShowStatus
End Sub

Private Function PortSet() As Boolean
Dim Port As Long
Dim mode As Long
Dim Hw  As Long, Sw As Long
Dim ret As Long

    Port = GCommData.Port
    mode = GCommData.Parity Or GCommData.ByteSize Or GCommData.StopBits
    If GCommData.Hw = 1 Then
        Hw = 3      'bit0 and bit1
    Else
        Hw = 0
    End If
    If GCommData.Sw = 1 Then
        Sw = 12     'bit2 and bit3
    Else
        Sw = 0
    End If
    
    PortSet = False
    
    ret = sio_ioctl(Port, GCommData.BaudRate, mode)
    If ret <> SIO_OK Then
        Call MxShowError("sio_ioctl", ret, GetLastError())
        Exit Function
    End If

    ret = sio_flowctrl(Port, Hw Or Sw)
    If ret <> SIO_OK Then
        Call MxShowError("sio_flowctrl", ret, GetLastError())
        Exit Function
    End If

    ret = sio_DTR(Port, GCommData.Dtr)
    If ret <> SIO_OK Then
        Call MxShowError("sio_DTR", ret, GetLastError())
        Exit Function
    End If

    If GCommData.Hw = 0 Then
        ret = sio_RTS(Port, GCommData.Rts)
        If ret <> SIO_OK Then
            Call MxShowError("sio_RTS", ret, GetLastError())
            Exit Function
        End If
    End If
    
    Call ShowStatus
    PortSet = True
End Function

Public Sub ShowStatus()
Dim szMessage As String

    szMessage = GszAppName
    
    If GbOpen Then
        szMessage = szMessage & " -- COM" & GCommData.Port & ","
        szMessage = szMessage & _
                GstrBaudTable(GCommData.ibaudrate) & ","
        szMessage = szMessage & _
                GstrParityTable(GCommData.iparity) & ","
        szMessage = szMessage & _
                GstrByteSizeTable(GCommData.ibytesize) & ","
        szMessage = szMessage & _
                GstrStopBitsTable(GCommData.istopbits)
        If GCommData.Hw = 1 Then
            szMessage = szMessage & ",RTS/CTS"
        End If

        If GCommData.Sw = 1 Then
           szMessage = szMessage & ",XON/XOFF"
        End If
    End If
    Caption = szMessage
End Sub


Private Sub XmitFile()
Dim tmp As String
    On Error GoTo XmitCancel
   
    ComDlg.Flags = cdlOFNFileMustExist Or _
            cdlOFNHideReadOnly Or cdlOFNLongNames
    ComDlg.ShowOpen
    tmp = ComDlg.filename
    GxFname = Left$(tmp, Len(tmp)) + Chr(0)
    FtStat.Show vbModeless
    Call FtFunc
   
    Exit Sub
XmitCancel:
    
End Sub

Private Sub RecvFile()
Dim tmp As String
    On Error GoTo RecvCancel
    If (GProtocol = FTZMDM) Or (GProtocol = FTYMDM) Or (GProtocol = FTKERMIT) Then
        GDirDlgCancel = False
        DirDlg.Show vbModal
        If GDirDlgCancel Then
            Exit Sub
        End If
        ChDir GrPath
        GrFname = ""
    Else
        ComDlg.Flags = cdlOFNOverwritePrompt Or _
            cdlOFNHideReadOnly Or cdlOFNLongNames
        ComDlg.ShowSave
        tmp = ComDlg.filename
        GrFname = Left$(tmp, Len(tmp)) + Chr(0)
    End If
    
    FtStat.Show vbModeless
    Call FtFunc
    Exit Sub
RecvCancel:
    
End Sub


