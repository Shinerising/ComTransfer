VERSION 5.00
Begin VB.Form Timeout 
   Caption         =   "Timeout Setting Demo"
   ClientHeight    =   4236
   ClientLeft      =   132
   ClientTop       =   588
   ClientWidth     =   5772
   LinkTopic       =   "Form1"
   ScaleHeight     =   4236
   ScaleWidth      =   5772
   StartUpPosition =   3  'Windows Default
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
   Begin VB.Menu cmSetting 
      Caption         =   "Setting..."
   End
   Begin VB.Menu cmTimeout 
      Caption         =   "Timeout Demo"
      Begin VB.Menu cmWDemo 
         Caption         =   "Polling Write"
         Index           =   0
      End
      Begin VB.Menu cmWDemo 
         Caption         =   "Block Write"
         Index           =   1
      End
      Begin VB.Menu cmWDemo 
         Caption         =   "Block Write(Timeout)"
         Index           =   2
      End
      Begin VB.Menu test3 
         Caption         =   "-"
      End
      Begin VB.Menu cmRDemo 
         Caption         =   "Polling Read"
         Index           =   3
      End
      Begin VB.Menu cmRDemo 
         Caption         =   "Block Read"
         Index           =   4
      End
      Begin VB.Menu cmRDemo 
         Caption         =   "Block Read(Total Timeout)"
         Index           =   5
      End
      Begin VB.Menu cmRDemo 
         Caption         =   "Block Read(Interval Timeout)"
         Index           =   6
      End
      Begin VB.Menu cmRDemo 
         Caption         =   "Block Read(Total+Interval Timeout)"
         Index           =   7
      End
   End
   Begin VB.Menu Help 
      Caption         =   "Help"
      Begin VB.Menu cmAbout 
         Caption         =   "About"
      End
   End
End
Attribute VB_Name = "Timeout"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'*************************************************************
'    TmOutM.frm
'      -- Main window for read & write timeout setting
'         example program.
'
'    Description:
'      1.Select "setting..." menu item to set com port option.
'      2.Select "Open" menu item to open com port.
'        After selected "Open" from menu,you can select
'        "Timeout Demo"  to test timeout status.
'        Polling Write:
'               sio_write() will return immediately. The data will be
'               copied to driver,but do not really wrtie to com port. This
'               operation will maybe let sio_write() return 0, means that
'               not enough buffer to write. Be careful this polling
'               operation will eat a large amount of system resource,
'               including memory, processor time.
'
'       Block Write:
'               sio_write() will block until the all data has be write to
'               com port (But maybe some data is still in com port output
'               buffer when sio_write() return). You can check the
'               sio_write() elapsed time. This operation is suitable to
'               using another thread to write data. Then you can call
'               sio_AbortWrite() to abort write operation in main thread
'               when you want to stop writing.
'
'       Block Write (Timeout):
'               This operation is the same as "Block Write". The difference
'               is that, sio_write() will block until the tiemout is arrived
'               or the all data has be write to com port. You can decrease
'               the timeout value to check the difference.
'
'       Polling Read:
'               sio_read() will return immediately. sio_read() just checks
'               input buffer, gets all buffer data (maybe less than or equal
'               to that sio_read() want to read), then returns. If no data
'               in buffer, sio_read() return 0. Be careful this polling
'               operation will eat a large amount of system resource,
'               including memory, processor time.
'
'       Block Read:
'               sio_read() will block until the input buffer data length is
'               equal to that sio_read() want to read. This operation is
'               suitable to using another thread to read data. Then you can
'               call sio_AbortRead() to abort read operation in main thread
'               when you want to stop reading.
'
'       Block Read (Total Timeout):
'               This operation is the saem as "Block Read". The difference
'               is that, sio_read() will block until timeout is arrived or
'               the input buffer data length is equal to that sio_read()
'               want to read.You can decrease the timeout value to check
'               the difference.
'               In this example, you can connect to terminal. Then you can
'               try 2 cases from terminal:
'                       send 10240 bytes,
'                       or wait the timeout is arrived.
'               Check the sio_read() elapsed time.
'
'       Block Read (Interval Timeout):
'               sio_read() will wait the first byte arrived, then begin
'               interval timeout checking.sio_read will block until the
'               interval timeout is arrived or the input buffer data length
'               is equal to that sio_read() want to read.
'               In this example, you can connect to terminal. Then you can
'               try 2 cases from terminal:
'                       send 1 or 2 byte,
'                       send 10240 bytes,
'               Check the sio_read() elapsed time.
'
'       Block Read ( Total+Interval Timeout ):
'               sio_read() will block until the timeout is arrived or the
'               input buffer data length is equdal to that sio_read() want
'               to read.
'               In this example, you can connect to terminal. Then you can
'               try 3 cases from terminal :
'                       send 1 or 2 byte,
'                       send 10240 bytes,
'                       send > 10240 bytes
'               Check the sio_read() elapsed time.
'
'      3.Select "Close" menu item to close com port.
'
'
'    This program demo:
'       How to set write timeout(sio_SetWriteTimeouts());
'       How to set read timeout(sio_SetReadTimeouts());
'       How to abort write process(sio_AbortWrite());
'       How to abort read process(sio_AbortRead());
'
'    Use function:
'        sio_open,       sio_close,   sio_ioctl,
'        sio_flowctrl,   sio_DTR,     sio_RTS,
'        sio_read,       sio_write,
'
'        sio_SetWriteTimesout,        sio_SetReadTimeouts,
'        sio_AbortWrite,              sio_AbortRead;
'
'
'
'   History:   Date       Author         Comment
'              6/1/98     Casper         Wrote it.
'
'*************************************************************

Option Explicit
Public GbOpen As Boolean
Public GszAppName As String

Dim Ldx As Long
Dim Ldy As Long
Const MAXDWORD = &HFFFFFFFF

Private Sub cmClear_Click()
    Term.Text = ""
End Sub


Private Sub cmRDemo_Click(Index As Integer)
    Call DemoReadTimeout(GCommData.Port, Index)
End Sub

Private Sub cmWDemo_Click(Index As Integer)
    Call DemoWriteTimeout(GCommData.Port, Index)
End Sub

Private Sub Form_Load()
Dim i As Integer
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
    GszAppName = "Timeout Setting Demo"
    Set GhForm = Timeout
    
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
    About.AboutTxt1.Caption = "PComm Timeout Function Example"
    About.Show
End Sub

Private Sub cmExit_Click()
    If GbOpen Then
        Call ClosePort
    End If
    Unload Timeout
    End
End Sub

Sub SwitchMenu()
    cmOpen.Enabled = Not GbOpen
    cmClose.Enabled = GbOpen
    cmTimeout.Enabled = GbOpen
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


Private Sub DemoWriteTimeout(Port As Long, testitem As Integer)
Dim baud As Long
Dim tout As Long
    
    Select Case testitem
    Case IDM_WRITE_POLL:
        Call sio_SetWriteTimeouts(Port, MAXDWORD)
    Case IDM_WRITE_BLOCK:
        Call sio_SetWriteTimeouts(Port, 0)
    Case IDM_WRITE_TIMEOUT:
        baud = sio_getbaud(Port)
        ' timeout(ms)  = Buflen *(1000/ (baud/10));
        '              = Buflen *  (ms /byte)
        tout = (BUFLEN * 1000 / (baud / 10)) * 3  '*3 is for delay
        Call sio_SetWriteTimeouts(Port, tout)
    End Select
    
    GhExit = False
    GCount = 0
    GCallCount = 0
    GDifTime = 0
    WStat.Show vbModeless
    Call WriteProc
    'must flush oqueue,or prog will be block in sio_close
    Call sio_flush(GCommData.Port, 2)
    Unload WStat
End Sub


Private Sub DemoReadTimeout(Port As Long, testitem As Integer)
Dim baud As Long
Dim tout As Long
Dim msg As String

    baud = sio_getbaud(Port)
    ' timeout(ms)  = Buflen *(1000/ (baud/10));
    '              = Buflen *  (ms /byte)
    tout = (BUFLEN * 1000 / (baud / 10)) * 3    '*3 is for delay
    Select Case testitem
    Case IDM_READ_POLL:
        Call sio_SetReadTimeouts(Port, MAXDWORD, 0)
    Case IDM_READ_BLOCK:
        Call sio_SetReadTimeouts(Port, 0, 0)
        msg = "Program will block until got " + Str(BUFLEN) + " byte."
    Case IDM_READ_BLOCK_T:
        Call sio_SetReadTimeouts(Port, tout, 0)
        msg = "timeout is " + Str(tout) + " ms."
    Case IDM_READ_BLOCK_I:
        Call sio_SetReadTimeouts(Port, 0, 1000)
        msg = "Program will block until got fisrt byte."
    Case IDM_READ_BLOCK_TI:
        Call sio_SetReadTimeouts(Port, tout, 1000)
        msg = "Timeout is " + Str(tout) + " ms."
    End Select
    
    msg = msg + "Press OK to continue."
    If MsgBox(msg, vbOKCancel) = vbCancel Then
        Exit Sub
    End If
    
    GhExit = False
    GCount = 0
    GCallCount = 0
    GDifTime = 0
    RStat.Show vbModeless
    Call ReadProc
    Unload RStat
End Sub


