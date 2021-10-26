VERSION 5.00
Begin VB.Form Config 
   BorderStyle     =   1  'Fixed Single
   Caption         =   "Com Option"
   ClientHeight    =   4170
   ClientLeft      =   30
   ClientTop       =   255
   ClientWidth     =   4860
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   4170
   ScaleWidth      =   4860
   StartUpPosition =   3  'Windows Default
   Begin VB.Frame Frame3 
      Caption         =   "Output Status"
      Height          =   972
      Left            =   3120
      TabIndex        =   18
      Top             =   2760
      Width           =   1572
      Begin VB.CheckBox chRts 
         Caption         =   "RTS"
         Height          =   252
         Left            =   240
         TabIndex        =   8
         Top             =   600
         Width           =   1092
      End
      Begin VB.CheckBox chDtr 
         Caption         =   "DTR"
         Height          =   252
         Left            =   240
         TabIndex        =   7
         Top             =   240
         Width           =   1092
      End
   End
   Begin VB.Frame Frame2 
      Caption         =   "Flow Control"
      Height          =   972
      Left            =   240
      TabIndex        =   17
      Top             =   2760
      Width           =   2772
      Begin VB.CheckBox chSw 
         Caption         =   "XON/XOFF"
         Height          =   252
         Left            =   240
         TabIndex        =   6
         Top             =   600
         Width           =   1212
      End
      Begin VB.CheckBox chHw 
         Caption         =   "RTS/CTS"
         Height          =   252
         Left            =   240
         TabIndex        =   5
         Top             =   240
         Width           =   1812
      End
   End
   Begin VB.CommandButton Cancel 
      Caption         =   "Cancel"
      Height          =   372
      Left            =   3360
      TabIndex        =   10
      Top             =   1320
      Width           =   972
   End
   Begin VB.CommandButton OK 
      Caption         =   "OK"
      Height          =   372
      Left            =   3360
      TabIndex        =   9
      Top             =   480
      Width           =   972
   End
   Begin VB.Frame Frame1 
      Caption         =   "Com Option"
      Height          =   2292
      Left            =   240
      TabIndex        =   11
      Top             =   240
      Width           =   2772
      Begin VB.ComboBox cbPort 
         Height          =   288
         ItemData        =   "Config.frx":0000
         Left            =   1320
         List            =   "Config.frx":0002
         Style           =   2  'Dropdown List
         TabIndex        =   0
         Top             =   360
         Width           =   1212
      End
      Begin VB.ComboBox cbBaudRate 
         Height          =   288
         ItemData        =   "Config.frx":0004
         Left            =   1320
         List            =   "Config.frx":0044
         Style           =   2  'Dropdown List
         TabIndex        =   1
         Top             =   720
         Width           =   1212
      End
      Begin VB.ComboBox cbParity 
         Height          =   288
         ItemData        =   "Config.frx":00C2
         Left            =   1320
         List            =   "Config.frx":00D5
         Style           =   2  'Dropdown List
         TabIndex        =   2
         Top             =   1080
         Width           =   1212
      End
      Begin VB.ComboBox cbByteSize 
         Height          =   288
         ItemData        =   "Config.frx":00F7
         Left            =   1320
         List            =   "Config.frx":0107
         Style           =   2  'Dropdown List
         TabIndex        =   3
         Top             =   1440
         Width           =   1212
      End
      Begin VB.ComboBox cbStopBits 
         Height          =   288
         ItemData        =   "Config.frx":0117
         Left            =   1320
         List            =   "Config.frx":0121
         Style           =   2  'Dropdown List
         TabIndex        =   4
         Top             =   1800
         Width           =   1212
      End
      Begin VB.Label Label1 
         Caption         =   "Port :"
         Height          =   252
         Left            =   240
         TabIndex        =   16
         Top             =   360
         Width           =   972
      End
      Begin VB.Label Label2 
         Caption         =   "Baud Rate :"
         Height          =   252
         Left            =   240
         TabIndex        =   15
         Top             =   720
         Width           =   972
      End
      Begin VB.Label Label3 
         Caption         =   "Parity : "
         Height          =   252
         Left            =   240
         TabIndex        =   14
         Top             =   1080
         Width           =   972
      End
      Begin VB.Label Label4 
         Caption         =   "Data Bits :"
         Height          =   252
         Left            =   240
         TabIndex        =   13
         Top             =   1440
         Width           =   972
      End
      Begin VB.Label Label5 
         Caption         =   "Stop Bits :"
         Height          =   252
         Left            =   240
         TabIndex        =   12
         Top             =   1800
         Width           =   1092
      End
   End
End
Attribute VB_Name = "Config"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'************************************************************
'    Config.frm
'     -- Config dialog for com port commnucation parameters
'
'
'   History:   Date       Author         Comment
'              3/9/98     Casper         Wrote it.
'
'************************************************************


Option Explicit
Dim Gfhw As Boolean

Private Sub Cancel_Click()
    Unload Config
End Sub
Private Sub Form_Load()
Dim i As Long

    'initial Port combo box
    For i = 1 To 256
        cbPort.AddItem ("COM" & i)
    Next i
    With GCommData
        cbPort.ListIndex = .Port - 1
        cbBaudRate.ListIndex = .ibaudrate
        cbParity.ListIndex = .iparity
        cbByteSize.ListIndex = .ibytesize
        cbStopBits.ListIndex = .istopbits
        chHw.Value = .Hw
        chSw.Value = .Sw
        chRts.Value = .Rts
        chDtr.Value = .Dtr
        If (.Hw = 1) Then
            Gfhw = True
        Else
            Gfhw = False
        End If
        chRts.Enabled = Not Gfhw
    End With
    cbPort.Enabled = Not GhForm.GbOpen
End Sub

Private Sub chHw_Click()
    chRts.Enabled = Gfhw
    Gfhw = Not Gfhw
End Sub

Private Sub OK_Click()
    With GCommData
        .Port = cbPort.ListIndex + 1
        .ibaudrate = cbBaudRate.ListIndex
        .iparity = cbParity.ListIndex
        .ibytesize = cbByteSize.ListIndex
        .istopbits = cbStopBits.ListIndex
        .BaudRate = GBaudTable(.ibaudrate)
        .ByteSize = GByteSizeTable(.ibytesize)
        .Parity = GParityTable(.iparity)
        .StopBits = GStopBitsTable(.istopbits)
        .Hw = chHw.Value
        .Sw = chSw.Value
        .Rts = chRts.Value
        .Dtr = chDtr.Value
    End With
    Unload Config
End Sub
