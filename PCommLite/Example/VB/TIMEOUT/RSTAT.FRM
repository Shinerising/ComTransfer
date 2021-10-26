VERSION 5.00
Begin VB.Form RStat 
   BorderStyle     =   1  'Fixed Single
   Caption         =   "Read Status"
   ClientHeight    =   2568
   ClientLeft      =   48
   ClientTop       =   336
   ClientWidth     =   3792
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   2568
   ScaleWidth      =   3792
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton RStatCancel 
      Caption         =   "Cancel"
      Height          =   375
      Left            =   2280
      TabIndex        =   6
      Top             =   2040
      Width           =   1095
   End
   Begin VB.Label Label4 
      Caption         =   "Press Cancel, Read process will beak until next timeout."
      Height          =   492
      Left            =   240
      TabIndex        =   7
      Top             =   1440
      Width           =   3132
   End
   Begin VB.Label StatRTime 
      BorderStyle     =   1  'Fixed Single
      Height          =   255
      Left            =   2280
      TabIndex        =   5
      Top             =   960
      Width           =   1095
   End
   Begin VB.Label Label3 
      Caption         =   "sio_read( ) Elapsed Time :"
      Height          =   255
      Left            =   240
      TabIndex        =   4
      Top             =   960
      Width           =   1935
   End
   Begin VB.Label StatRCallCount 
      BorderStyle     =   1  'Fixed Single
      Height          =   255
      Left            =   2280
      TabIndex        =   3
      Top             =   600
      Width           =   1095
   End
   Begin VB.Label Label2 
      Caption         =   "Call sio_read( ) Count : "
      Height          =   255
      Left            =   240
      TabIndex        =   2
      Top             =   600
      Width           =   1815
   End
   Begin VB.Label StatRCount 
      BorderStyle     =   1  'Fixed Single
      Height          =   255
      Left            =   2280
      TabIndex        =   1
      Top             =   240
      Width           =   1095
   End
   Begin VB.Label Label1 
      Caption         =   "sio_read( ) > 0 Count : "
      Height          =   255
      Left            =   240
      TabIndex        =   0
      Top             =   240
      Width           =   1815
   End
End
Attribute VB_Name = "RStat"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'*********************************************************************
'    RStat.frm
'     -- Show dialog to diaplsy read timeout status.
'
'
'    History:   Date       Author         Comment
'               3/9/98     Casper         Wrote it.
'
'**********************************************************************)

Option Explicit
Private Sub Form_Load()
    Call RefreshDlg
End Sub

Public Sub RefreshDlg()
    StatRCallCount.Caption = GCallCount
    StatRCount.Caption = GCount
    StatRTime.Caption = GDifTime & "ms"
End Sub

Private Sub RStatCancel_Click()
    GhExit = True
    Call sio_AbortRead(GCommData.Port)
End Sub


