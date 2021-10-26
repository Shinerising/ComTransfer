VERSION 5.00
Begin VB.Form WStat 
   BorderStyle     =   1  'Fixed Single
   Caption         =   "Write Status"
   ClientHeight    =   2205
   ClientLeft      =   45
   ClientTop       =   330
   ClientWidth     =   3720
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   2205
   ScaleWidth      =   3720
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton WStatCancel 
      Caption         =   "Cancel"
      Height          =   375
      Left            =   2280
      TabIndex        =   0
      Top             =   1560
      Width           =   1095
   End
   Begin VB.Label Label1 
      Caption         =   "sio_write( ) > 0 Count : "
      Height          =   255
      Left            =   240
      TabIndex        =   6
      Top             =   240
      Width           =   1815
   End
   Begin VB.Label StatWCount 
      BorderStyle     =   1  'Fixed Single
      Height          =   255
      Left            =   2280
      TabIndex        =   5
      Top             =   240
      Width           =   1095
   End
   Begin VB.Label Label2 
      Caption         =   "Call sio_write( ) Count : "
      Height          =   255
      Left            =   240
      TabIndex        =   4
      Top             =   600
      Width           =   1815
   End
   Begin VB.Label StatWCallCount 
      BorderStyle     =   1  'Fixed Single
      Height          =   255
      Left            =   2280
      TabIndex        =   3
      Top             =   600
      Width           =   1095
   End
   Begin VB.Label Label3 
      Caption         =   "sio_write( ) Elapsed Time :"
      Height          =   255
      Left            =   240
      TabIndex        =   2
      Top             =   960
      Width           =   1935
   End
   Begin VB.Label StatWTime 
      BorderStyle     =   1  'Fixed Single
      Height          =   255
      Left            =   2280
      TabIndex        =   1
      Top             =   960
      Width           =   1095
   End
End
Attribute VB_Name = "WStat"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'***********************************************************
'    WStat.frm
'     -- Show dialog to diaplsy write timeout status.
'
'
'   History:   Date       Author         Comment
'              3/9/98     Casper         Wrote it.
'
'***********************************************************
Option Explicit

Private Sub Form_Load()
    Call RefreshDlg
End Sub

Public Sub RefreshDlg()
    StatWCallCount.Caption = GCallCount
    StatWCount.Caption = GCount
    StatWTime.Caption = GDifTime & "ms"
End Sub

Private Sub WStatCancel_Click()
    GhExit = True
    Call sio_AbortWrite(GCommData.Port)
End Sub
