VERSION 5.00
Begin VB.Form PortStat 
   Caption         =   "Port Status Demo"
   ClientHeight    =   4245
   ClientLeft      =   165
   ClientTop       =   735
   ClientWidth     =   5775
   LinkTopic       =   "Form1"
   ScaleHeight     =   4245
   ScaleWidth      =   5775
   StartUpPosition =   3  'Windows Default
   Begin VB.Timer Timer1 
      Interval        =   100
      Left            =   1560
      Top             =   2160
   End
   Begin VB.TextBox Term 
      Height          =   4215
      Left            =   0
      MultiLine       =   -1  'True
      ScrollBars      =   3  'Both
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
      Begin VB.Menu cmClear 
         Caption         =   "Clear Screen"
      End
      Begin VB.Menu test2 
         Caption         =   "-"
      End
      Begin VB.Menu cmExit 
         Caption         =   "Exit"
      End
   End
   Begin VB.Menu cmConfig 
      Caption         =   "Config"
      Begin VB.Menu cmSetting 
         Caption         =   "Setting"
      End
      Begin VB.Menu cmStatus 
         Caption         =   "Status"
      End
   End
   Begin VB.Menu Help 
      Caption         =   "Help"
      Begin VB.Menu cmAbout 
         Caption         =   "About"
      End
   End
End
Attribute VB_Name = "PortStat"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'**************************************************************
'    PortStat.frm
'     -- Main window for port status example program.
'
'    Description:
'      1.Select "setting..." menu item to set com port option.
'      2.Select "Open" menu item to open com port.
'        After selected "Open" from menu,program will pop up
'        one dialog to display the status of the opened com port.
'      3.Select "Close" menu item to close com port.
'
'    This program demo:
'        How to get the status of the com port.
'
'    NOTE:To correctly use sio_data_status(),you must
'         call sio_data_status() before calling sio_iqueue(),
'         sio_oqueue(),sio_Tx_hold().
'
'    Use function:
'        sio_open,       sio_close,   sio_ioctl,
'        sio_flowctrl,   sio_DTR,     sio_RTS,
'        sio_read,       sio_putch,   sio_write.
'        sio_SetWriteTimeouts,
'
'        sio_lstatus(),               sio_iqueue(),
'        sio_oqueue(),                sio_Tx_hold(),
'        sio_getbaud(),               sio_getmode(),
'        sio_getflow(),               sio_data_status();
'
'
'    History:   Date       Author         Comment
'               3/9/98     Casper         Wrote it.
'               4/14/00    Casper         Use timer.
'
'**************************************************************

Option Explicit
Public GbOpen As Boolean
Public GszAppName As String

Dim Ldx As Long
Dim Ldy As Long

Private Sub cmClear_Click()
    Term.Text = ""
End Sub

Private Sub Form_Load()
    With GCommData
        .port = 1
        .BaudRate = B38400
        .parity = P_NONE
        .bytesize = BIT_8
        .stopbits = STOP_1
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
    GszAppName = "Simple Demo"
    Set GhForm = PortStat
    
    Call InitTable
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
Dim bakdata  As COMMDATA
    bakdata = GCommData
    Config.Show vbModal
    If (GbOpen) Then
        If PortSet = False Then
            GCommData = bakdata
        End If
    End If
    Call ShowStatus
End Sub

Private Sub cmStatus_Click()
    StatDlg.Show vbModeless
End Sub

Private Sub cmAbout_Click()
    About.AboutTxt1.Caption = "PComm Port Sataus Example"
    About.Show
End Sub

Private Sub cmExit_Click()
    If GbOpen Then
        Call ClosePort
    End If
    Unload PortStat
    End
End Sub

Sub SwitchMenu()
    cmOpen.Enabled = Not GbOpen
    cmClose.Enabled = GbOpen
    cmStatus.Enabled = GbOpen
End Sub

Private Sub Term_KeyPress(KeyAscii As Integer)
    If GbOpen Then
       ' Send the keystroke to the port.
       Call sio_putch(GCommData.port, KeyAscii)
       KeyAscii = 0
    End If
End Sub

Private Function OpenPort() As Boolean
Dim ret As Long
Dim syserr As Long
    
    OpenPort = False
    
    ret = sio_open(GCommData.port)
    If ret <> SIO_OK Then
        Call MxShowError("sio_open", ret, GetLastError())
        sio_close (GCommData.port)
        Exit Function
    End If
    
    If PortSet() = False Then
        sio_close (GCommData.port)
        Exit Function
    End If
    
    OpenPort = True
    GbOpen = True
    StatDlg.Show vbModeless
    Call SwitchMenu
    Call ShowStatus
End Function
Private Function ClosePort()
    sio_close (GCommData.port)
    GbOpen = False
    Unload StatDlg
    Call SwitchMenu
    Call ShowStatus
End Function

Private Function PortSet() As Boolean
Dim port As Long
Dim mode As Long
Dim Hw  As Long, Sw As Long
Dim ret As Long

    port = GCommData.port
    mode = GCommData.parity Or GCommData.bytesize Or GCommData.stopbits
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
    
    ret = sio_ioctl(port, GCommData.BaudRate, mode)
    If ret <> SIO_OK Then
        Call MxShowError("sio_ioctl", ret, GetLastError())
        Exit Function
    End If

    ret = sio_flowctrl(port, Hw Or Sw)
    If ret <> SIO_OK Then
        Call MxShowError("sio_flowctrl", ret, GetLastError())
        Exit Function
    End If

    ret = sio_DTR(port, GCommData.Dtr)
    If ret <> SIO_OK Then
        Call MxShowError("sio_DTR", ret, GetLastError())
        Exit Function
    End If

    If GCommData.Hw = 0 Then
        ret = sio_RTS(port, GCommData.Rts)
        If ret <> SIO_OK Then
            Call MxShowError("sio_RTS", ret, GetLastError())
            Exit Function
        End If
    End If
    
    Call ShowStatus
    PortSet = True
End Function

Public Sub ShowData(ByVal Term As Control, ByRef Dta() As Byte, ByVal rlen As Long)
Dim termlen
Dim i As Integer

    On Error Resume Next

    ' Make sure the existing text doesn't get too large.
    For i = 0 To rlen - 1
        termlen = LenB(Term.Text)
        If termlen >= 16384 Then
            Term.Text = Mid$(Term.Text, 4097)
            termlen = LenB(Term.Text)
        End If

        ' Point to the end of Term's data.
        Term.SelStart = termlen
        Term.SelLength = 0
        Term.SelText = "char ascii = " + Str(Dta(i)) + Chr(13) + Chr(10)
    Next i
   
End Sub


Public Sub ShowStatus()
Dim szMessage As String

    szMessage = GszAppName
    
    If GbOpen Then
        szMessage = szMessage & " -- COM" & GCommData.port & ","
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

Private Sub Timer1_Timer()
Dim rlen As Long
Dim buf(0 To 511) As Byte
   
    rlen = sio_read(GCommData.port, buf(0), 512)
    If rlen > 0 Then
        Call GhForm.ShowData(GhForm.Term, buf, rlen)
    Else
        If rlen = 0 Then
            Exit Sub
        End If
    End If
End Sub
