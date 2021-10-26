VERSION 5.00
Begin VB.Form StatDlg 
   Caption         =   "Port Status"
   ClientHeight    =   5745
   ClientLeft      =   60
   ClientTop       =   345
   ClientWidth     =   5205
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   5745
   ScaleWidth      =   5205
   StartUpPosition =   3  'Windows Default
   Begin VB.Timer Timer1 
      Interval        =   300
      Left            =   3360
      Top             =   120
   End
   Begin VB.CommandButton StatDlgCancel 
      Cancel          =   -1  'True
      Caption         =   "Cancel"
      Default         =   -1  'True
      Height          =   375
      Left            =   3720
      TabIndex        =   27
      Top             =   5280
      Width           =   1215
   End
   Begin VB.Frame Frame6 
      Caption         =   " Tx Hold "
      Height          =   1215
      Left            =   2880
      TabIndex        =   24
      Top             =   3960
      Width           =   2055
      Begin VB.ListBox clbTxhold 
         Height          =   840
         Left            =   120
         TabIndex        =   26
         Top             =   240
         Width           =   1815
      End
   End
   Begin VB.Frame Frame5 
      Caption         =   " Data Status "
      Height          =   1215
      Left            =   240
      TabIndex        =   23
      Top             =   3960
      Width           =   2535
      Begin VB.ListBox clbDStatus 
         Height          =   840
         Left            =   120
         TabIndex        =   25
         Top             =   240
         Width           =   2295
      End
   End
   Begin VB.Frame Frame4 
      Caption         =   " Queue "
      Height          =   1215
      Left            =   2880
      TabIndex        =   18
      Top             =   2640
      Width           =   2055
      Begin VB.Label clOutqueue 
         BorderStyle     =   1  'Fixed Single
         Height          =   255
         Left            =   1080
         TabIndex        =   22
         Top             =   720
         Width           =   855
      End
      Begin VB.Label clInqueue 
         BorderStyle     =   1  'Fixed Single
         Height          =   255
         Left            =   1080
         TabIndex        =   21
         Top             =   360
         Width           =   855
      End
      Begin VB.Label Label7 
         Caption         =   "Out Queue"
         Height          =   255
         Left            =   120
         TabIndex        =   20
         Top             =   720
         Width           =   975
      End
      Begin VB.Label Label6 
         Caption         =   "In Queue"
         Height          =   255
         Left            =   120
         TabIndex        =   19
         Top             =   360
         Width           =   855
      End
   End
   Begin VB.Frame Frame3 
      Caption         =   " Line Status "
      Height          =   1215
      Left            =   240
      TabIndex        =   13
      Top             =   2640
      Width           =   2535
      Begin VB.Label clDcd 
         Alignment       =   2  'Center
         BorderStyle     =   1  'Fixed Single
         Height          =   255
         Left            =   1440
         TabIndex        =   17
         Top             =   720
         Width           =   975
      End
      Begin VB.Label clRi 
         Alignment       =   2  'Center
         BorderStyle     =   1  'Fixed Single
         Height          =   255
         Left            =   1440
         TabIndex        =   16
         Top             =   360
         Width           =   975
      End
      Begin VB.Label clDsr 
         Alignment       =   2  'Center
         BorderStyle     =   1  'Fixed Single
         Height          =   255
         Left            =   120
         TabIndex        =   15
         Top             =   720
         Width           =   975
      End
      Begin VB.Label clCts 
         Alignment       =   2  'Center
         BorderStyle     =   1  'Fixed Single
         Height          =   255
         Left            =   120
         TabIndex        =   14
         Top             =   360
         Width           =   975
      End
   End
   Begin VB.Frame Frame2 
      Caption         =   " Flow Control "
      Height          =   735
      Left            =   240
      TabIndex        =   11
      Top             =   1800
      Width           =   4695
      Begin VB.Label clFlowCtrl 
         BorderStyle     =   1  'Fixed Single
         Height          =   375
         Left            =   120
         TabIndex        =   12
         Top             =   240
         Width           =   4455
      End
   End
   Begin VB.Frame Frame1 
      Caption         =   " Communcation "
      Height          =   1215
      Left            =   240
      TabIndex        =   2
      Top             =   480
      Width           =   4695
      Begin VB.Label clStopBits 
         BorderStyle     =   1  'Fixed Single
         Height          =   255
         Left            =   3600
         TabIndex        =   10
         Top             =   720
         Width           =   855
      End
      Begin VB.Label Label5 
         Caption         =   "Stop Bits :"
         Height          =   255
         Left            =   2640
         TabIndex        =   9
         Top             =   720
         Width           =   735
      End
      Begin VB.Label clDataBits 
         BorderStyle     =   1  'Fixed Single
         Height          =   255
         Left            =   3600
         TabIndex        =   8
         Top             =   360
         Width           =   855
      End
      Begin VB.Label Label4 
         Caption         =   "Data Bits :"
         Height          =   255
         Left            =   2640
         TabIndex        =   7
         Top             =   360
         Width           =   855
      End
      Begin VB.Label clParity 
         BorderStyle     =   1  'Fixed Single
         Height          =   255
         Left            =   1200
         TabIndex        =   6
         Top             =   720
         Width           =   1095
      End
      Begin VB.Label clBaud 
         BorderStyle     =   1  'Fixed Single
         Height          =   255
         Left            =   1200
         TabIndex        =   5
         Top             =   360
         Width           =   1095
      End
      Begin VB.Label Label3 
         Caption         =   "Parity :"
         Height          =   255
         Left            =   240
         TabIndex        =   4
         Top             =   720
         Width           =   855
      End
      Begin VB.Label Label2 
         Caption         =   "Baud Rate :"
         Height          =   255
         Left            =   240
         TabIndex        =   3
         Top             =   360
         Width           =   975
      End
   End
   Begin VB.Label clPort 
      BorderStyle     =   1  'Fixed Single
      Height          =   255
      Left            =   1560
      TabIndex        =   1
      Top             =   120
      Width           =   975
   End
   Begin VB.Label Label1 
      Caption         =   "Port"
      Height          =   255
      Left            =   480
      TabIndex        =   0
      Top             =   120
      Width           =   735
   End
End
Attribute VB_Name = "StatDlg"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'*********************************************************************
'    StatDlg.frm
'     -- Show dialog to display port status.
'
'    History:   Date       Author         Comment
'               3/9/98     Casper         Wrote it.
'
'***********************************************************************)


Option Explicit
Const DATABITSMSK = BIT_5 Or BIT_6 Or BIT_7 Or BIT_8
Const STOPBITSMSK = STOP_1 Or STOP_2
Const PARITYMSK = P_EVEN Or P_ODD Or P_SPC Or P_MRK Or P_NONE

Private Sub Form_Load()
    Call RefreshStat
End Sub

Private Sub StatDlgCancel_Click()
    Unload StatDlg
End Sub

Private Sub Timer1_Timer()
    Call RefreshStat
End Sub

Private Sub RefreshStat()
    Dim port As Long
    Dim buf As String
    Dim baud As Long
    Dim mode As Integer, bytesize As Integer, stopbits As Integer, parity As Integer
    Dim lstatus As Long
    Dim txhold As Long
    Dim d_status As Long
    Dim flow As Long
    Dim i As Integer
    
    port = GCommData.port
    'sio_data_status();This should be called first !!
    d_status = sio_data_status(port)
    If (d_status And &H1) > 0 Then
        clbDStatus.AddItem ("parity error")
    End If
    If (d_status And &H2) > 0 Then
        clbDStatus.AddItem ("framing error")
    End If
    If (d_status And &H4) > 0 Then
        clbDStatus.AddItem ("overrun error")
    End If
    If (d_status And &H8) > 0 Then
        clbDStatus.AddItem ("overflow error")
    End If
   
    clPort.Caption = "COM" & port
    
    'sio_getbaud()
    StatDlg.clBaud = sio_getbaud(port)
    
    ' sio_getmode()
    mode = sio_getmode(port)
    parity = mode And PARITYMSK
    bytesize = mode And DATABITSMSK
    stopbits = mode And STOPBITSMSK
    For i = 0 To 4
        If parity = GParityTable(i) Then
            clParity = GstrParityTable(i)
            Exit For
        End If
    Next i
    For i = 0 To 3
        If bytesize = GByteSizeTable(i) Then
            clDataBits = GstrByteSizeTable(i)
            Exit For
        End If
    Next i
    For i = 0 To 1
        If stopbits = GStopBitsTable(i) Then
            clStopBits = GstrStopBitsTable(i)
            Exit For
        End If
    Next i
    ' sio_getflow()
    flow = sio_getflow(port)
    If (flow = 0) Then
        clFlowCtrl.Caption = "None"
    Else
        buf = ""
        If (flow And &H1) > 0 Then
            buf = buf + "<RTS>"
        End If
        If (flow And &H2) > 0 Then
            buf = buf + "<CTS>"
        End If
        If (flow And &H4) > 0 Then
            buf = buf + "<Tx XON/XOFF>"
        End If
        If (flow And &H8) > 0 Then
            buf = buf + "<Rx XON/XOFF>"
        End If
        clFlowCtrl.Caption = buf
    End If
    
    'sio_lstatus()
    lstatus = sio_lstatus(port)
    If (lstatus And S_CTS) > 0 Then
        clCts.Caption = "CTS"
    Else
        clCts.Caption = "cts"
    End If
    If (lstatus And S_DSR) > 0 Then
        clDsr.Caption = "DSR"
    Else
        clDsr.Caption = "dsr"
    End If
    If (lstatus And S_RI) > 0 Then
        clRi.Caption = "RI"
    Else
        clRi.Caption = "ri"
    End If
    If (lstatus And S_CD) > 0 Then
        clDcd.Caption = "DCD"
    Else
        clDcd.Caption = "dcd"
    End If
    
    ' sio_iqueue()
    clInqueue.Caption = sio_iqueue(port)
    
    ' sio_iqueue()
    clOutqueue.Caption = sio_oqueue(port)
    
    ' sio_Tx_hold
    clbTxhold.Clear
    txhold = sio_Tx_hold(port)
    If (txhold And &H1) Then
        clbTxhold.AddItem ("CTS is low")
    End If
    If (txhold And &H2) Then
        clbTxhold.AddItem ("XOFF char received")
    End If
    
    If (txhold And &H4) Then
        clbTxhold.AddItem ("by sio_disableTx()")
    End If
End Sub
