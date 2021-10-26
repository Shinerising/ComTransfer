VERSION 5.00
Begin VB.Form FtStat 
   BorderStyle     =   1  'Fixed Single
   Caption         =   "File Transfer"
   ClientHeight    =   2940
   ClientLeft      =   45
   ClientTop       =   330
   ClientWidth     =   5460
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   2940
   ScaleWidth      =   5460
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton StatDlgCancel 
      Caption         =   "Cancel"
      Height          =   375
      Left            =   3720
      TabIndex        =   11
      Top             =   2040
      Width           =   1215
   End
   Begin VB.Frame Frame1 
      Height          =   2535
      Left            =   120
      TabIndex        =   0
      Top             =   120
      Width           =   5175
      Begin VB.Label clFName 
         BorderStyle     =   1  'Fixed Single
         Height          =   255
         Left            =   1200
         TabIndex        =   10
         Top             =   1440
         Width           =   3615
      End
      Begin VB.Label Label5 
         Caption         =   "File Name"
         Height          =   255
         Left            =   240
         TabIndex        =   9
         Top             =   1440
         Width           =   855
      End
      Begin VB.Label clxLen 
         BorderStyle     =   1  'Fixed Single
         Height          =   255
         Left            =   3240
         TabIndex        =   8
         Top             =   960
         Width           =   1575
      End
      Begin VB.Label label4 
         Caption         =   "Length"
         Height          =   255
         Left            =   2520
         TabIndex        =   7
         Top             =   960
         Width           =   615
      End
      Begin VB.Label clProtocol 
         BorderStyle     =   1  'Fixed Single
         Height          =   255
         Left            =   3240
         TabIndex        =   6
         Top             =   360
         Width           =   1575
      End
      Begin VB.Label Label3 
         Caption         =   "Protocol"
         Height          =   255
         Left            =   2520
         TabIndex        =   5
         Top             =   360
         Width           =   735
      End
      Begin VB.Label clFSize 
         BorderStyle     =   1  'Fixed Single
         Height          =   255
         Left            =   1200
         TabIndex        =   4
         Top             =   960
         Width           =   975
      End
      Begin VB.Label Label2 
         Caption         =   "File Size"
         Height          =   255
         Left            =   240
         TabIndex        =   3
         Top             =   960
         Width           =   735
      End
      Begin VB.Label clPort 
         BorderStyle     =   1  'Fixed Single
         Height          =   255
         Left            =   1200
         TabIndex        =   2
         Top             =   360
         Width           =   975
      End
      Begin VB.Label Label1 
         Caption         =   "Port"
         Height          =   255
         Left            =   240
         TabIndex        =   1
         Top             =   360
         Width           =   615
      End
   End
End
Attribute VB_Name = "FtStat"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'**********************************************************
'    FStat.frm
'     -- Show dialog to diaplsy file transfer status.
'
'
'     History:   Date       Author         Comment
'                3/9/98     Casper         Wrote it.
'
'**********************************************************

Option Explicit

Private Sub Form_Load()
    GftCancel = False
End Sub

Private Sub StatDlgCancel_Click()
    GftCancel = True
End Sub

Public Sub RefreshDlg(xlen As Long, flen As Long, fname As String)
    FtStat.clFSize.Caption = flen
    FtStat.clPort.Caption = GCommData.Port
    FtStat.clProtocol.Caption = GstrProtocol(GProtocol)
    FtStat.clFName.Caption = fname
    FtStat.clxLen.Caption = xlen
End Sub
