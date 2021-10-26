VERSION 5.00
Begin VB.Form Simple 
   Caption         =   "Simple Demo"
   ClientHeight    =   4245
   ClientLeft      =   165
   ClientTop       =   735
   ClientWidth     =   5775
   LinkTopic       =   "Form1"
   ScaleHeight     =   4245
   ScaleWidth      =   5775
   StartUpPosition =   3  
   Begin VB.Timer Timer1 
      Interval        =   100
      Left            =   720
      Top             =   1560
   End
   Begin VB.TextBox Term 
      Height          =   4215
      Left            =   0
      MultiLine       =   -1  'True
      ScrollBars      =   3  
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
      Begin VB.Menu cmSendAscii 
         Caption         =   "Send ASCII"
      End
      Begin VB.Menu test2 
         Caption         =   "-"
      End
      Begin VB.Menu cmExit 
         Caption         =   "Exit"
      End
   End
   Begin VB.Menu cmSetting 
      Caption         =   "Setting..."
   End
   Begin VB.Menu Help 
      Caption         =   "Help"
      Begin VB.Menu cmAbout 
         Caption         =   "About"
      End
   End
End
Attribute VB_Name = "Simple"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'****************************************************************
'    Simple.bas
'     -- Main window for simple dumb terminal example program.
'
'    Description:
'      1.Select "setting..." menu item to set com port option.
'      2.Select "Open" menu item to open com port.
'        After selected "Open" from menu,you can type any character
'        to send to com port.When any data received from com port,
'        program will dump data to window.
'      3.Select "Close" menu item to close com port.
'
'    This program demo:
'        How to open com port(sio_open);
'        How to set commnunication parameter(sio_ioctl,sio_flowctrl);
'        How to control line status(sio_DTR,sio_RTS);
'        How to send data to com port(sio_write,sio_putch);
'        How to read data(sio_read);
'        How to close com port(sio_close);
'        How to use timer to read data.
'
'    Use function:
'        sio_open,       sio_close,   sio_ioctl,
'        sio_flowctrl,   sio_DTR,     sio_RTS,
'        sio_read,       sio_putch,   sio_write.
'        sio_SetWriteTimeouts;
'
'    History:   Date       Author         Comment
'               6/1/98     Casper         Wrote it.
'               11/24/99   Casper         Modified, use timer
'
'****************************************************************


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
    GszAppName = "Simple Demo"
    Set GhForm = Simple
    
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

Private Sub cmSendAscii_Click()
Dim buf(0 To 255) As Byte
Dim i As Integer
Dim tout, baktout As Long

    For i = 0 To 255
        buf(i) = i
    Next i

    Call sio_GetWriteTimeouts(GCommData.Port, baktout)
    
    tout = 1000 / sio_getbaud(GCommData.Port)  'ms /byte'
    If tout < 1 Then
        tout = 1
    End If
    tout = tout * 256 * 2             ' 256 byte;*2 for delay
    Call sio_SetWriteTimeouts(GCommData.Port, tout)

    Call sio_write(GCommData.Port, buf(0), 256)

    Call sio_SetWriteTimeouts(GCommData.Port, baktout)
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

Private Sub cmAbout_Click()
    About.AboutTxt1.Caption = "PComm Simple Example"
    About.Show
End Sub

Private Sub cmExit_Click()
    If GbOpen Then
        Call ClosePort
    End If
    Unload Simple
    End
End Sub

Sub SwitchMenu()
    cmOpen.Enabled = Not GbOpen
    cmClose.Enabled = GbOpen
    cmSendAscii.Enabled = GbOpen
End Sub

Private Sub Term_KeyPress(KeyAscii As Integer)
    If GbOpen Then
       ' Send the keystroke to the port.
       Call sio_putch(GCommData.Port, KeyAscii)
       KeyAscii = 0
    End If
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
Private Function ClosePort()
    sio_close (GCommData.Port)
    GbOpen = False
    Call SwitchMenu
    Call ShowStatus
End Function

Private Function PortSet() As Boolean
Dim Port As Long
Dim mode As Long
Dim Hw  As Long, Sw As Long
Dim ret As Long
Dim tout As Long

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
    
    tout = 1000 / sio_getbaud(GCommData.Port)  'ms /byte'
    If tout < 1 Then
        tout = 1
    End If
    tout = tout * 1 * 3             ' 1 byte; '*3' is for delay
    If tout < 100 Then
        tout = 100
    End If
    Call sio_SetWriteTimeouts(GCommData.Port, tout)

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

Private Sub Timer1_Timer()
Dim rlen As Long
Dim buf(0 To 511) As Byte
   
    rlen = sio_read(GCommData.Port, buf(0), 512)
    If rlen > 0 Then
        Call GhForm.ShowData(GhForm.Term, buf, rlen)
    Else
        If rlen = 0 Then
            Exit Sub
        End If
    End If
End Sub
