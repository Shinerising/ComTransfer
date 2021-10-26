VERSION 5.00
Begin VB.Form DirDlg 
   BorderStyle     =   1  'Fixed Single
   Caption         =   "Select Directory"
   ClientHeight    =   3870
   ClientLeft      =   45
   ClientTop       =   330
   ClientWidth     =   3795
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   3870
   ScaleWidth      =   3795
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton DirCancel 
      Caption         =   "Cancel"
      Height          =   375
      Left            =   2280
      TabIndex        =   5
      Top             =   1080
      Width           =   975
   End
   Begin VB.CommandButton DirOk 
      Caption         =   "OK"
      Height          =   375
      Left            =   2280
      TabIndex        =   4
      Top             =   360
      Width           =   975
   End
   Begin VB.DriveListBox DrvList 
      Height          =   315
      Left            =   240
      TabIndex        =   1
      Top             =   3000
      Width           =   1695
   End
   Begin VB.DirListBox Pathlist 
      Height          =   1440
      Left            =   240
      TabIndex        =   0
      Top             =   1200
      Width           =   1695
   End
   Begin VB.Label DirResult 
      Height          =   735
      Left            =   240
      TabIndex        =   3
      Top             =   360
      Width           =   1815
   End
   Begin VB.Label Label1 
      Caption         =   "Directories"
      Height          =   255
      Left            =   240
      TabIndex        =   2
      Top             =   120
      Width           =   1695
   End
End
Attribute VB_Name = "DirDlg"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'*******************************************************
'    Dir.frm
'     -- Show dialog for user to select directory.
'        (File transfer example)
'
'
'    History:   Date       Author         Comment
'               3/9/98     Casper         Wrote it.
'
'*******************************************************


Option Explicit

Private Sub DirCancel_Click()
    GhForm.GDirDlgCancel = True
    Unload DirDlg
End Sub

Private Sub DirOk_Click()
    Dim tmp As String
    GhForm.GDirDlgCancel = False
    tmp = Pathlist.Path
    GrPath = Left(tmp, Len(tmp)) + Chr(0)
    Unload DirDlg
End Sub

Private Sub DrvList_Change()
    Pathlist.Path = DrvList.Drive
End Sub

Private Sub Form_Load()
    DirResult.Caption = Pathlist.Path
End Sub

Private Sub Pathlist_Change()
    DirResult.Caption = Pathlist.Path
End Sub
