VERSION 5.00
Begin VB.Form FSet 
   BorderStyle     =   1  'Fixed Single
   Caption         =   "File Transfer"
   ClientHeight    =   3690
   ClientLeft      =   45
   ClientTop       =   330
   ClientWidth     =   4380
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   3690
   ScaleWidth      =   4380
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton SetDlgCancel 
      Cancel          =   -1  'True
      Caption         =   "Cancel"
      Height          =   375
      Left            =   2880
      TabIndex        =   3
      Top             =   2760
      Width           =   1215
   End
   Begin VB.CommandButton SetDlgOk 
      Caption         =   "OK"
      Height          =   375
      Left            =   2880
      TabIndex        =   2
      Top             =   1920
      Width           =   1215
   End
   Begin VB.Frame Frame2 
      Caption         =   "Direction"
      Height          =   1215
      Left            =   2640
      TabIndex        =   1
      Top             =   240
      Width           =   1455
      Begin VB.OptionButton obDirection 
         Caption         =   "Recieve"
         Height          =   255
         Index           =   1
         Left            =   120
         TabIndex        =   11
         Top             =   720
         Width           =   1095
      End
      Begin VB.OptionButton obDirection 
         Caption         =   "Transmit"
         Height          =   255
         Index           =   0
         Left            =   120
         TabIndex        =   10
         Top             =   360
         Width           =   1095
      End
   End
   Begin VB.Frame Frame1 
      Caption         =   "Protocol"
      Height          =   3015
      Left            =   240
      TabIndex        =   0
      Top             =   240
      Width           =   2175
      Begin VB.OptionButton obProtocol 
         Caption         =   "Kermit"
         Height          =   255
         Index           =   5
         Left            =   240
         TabIndex        =   12
         Top             =   2160
         Width           =   1815
      End
      Begin VB.OptionButton obProtocol 
         Caption         =   "ASCII"
         Height          =   255
         Index           =   6
         Left            =   240
         TabIndex        =   9
         Top             =   2520
         Width           =   1815
      End
      Begin VB.OptionButton obProtocol 
         Caption         =   "YModem"
         Height          =   255
         Index           =   4
         Left            =   240
         TabIndex        =   8
         Top             =   1800
         Width           =   1695
      End
      Begin VB.OptionButton obProtocol 
         Caption         =   "ZModem"
         Height          =   255
         Index           =   3
         Left            =   240
         TabIndex        =   7
         Top             =   1440
         Width           =   1695
      End
      Begin VB.OptionButton obProtocol 
         Caption         =   "XModem-CRC"
         Height          =   255
         Index           =   2
         Left            =   240
         TabIndex        =   6
         Top             =   1080
         Width           =   1695
      End
      Begin VB.OptionButton obProtocol 
         Caption         =   "XModem-CheckSum"
         Height          =   255
         Index           =   1
         Left            =   240
         TabIndex        =   5
         Top             =   720
         Width           =   1815
      End
      Begin VB.OptionButton obProtocol 
         Caption         =   "XModem-1KCRC"
         Height          =   255
         Index           =   0
         Left            =   240
         TabIndex        =   4
         Top             =   360
         Width           =   1695
      End
   End
End
Attribute VB_Name = "FSet"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'**************************************************************
'    FSet.frm
'     -- Show dialog for user to specify the file transfer
'        protocol & upload or download.
'
'
'     History:   Date       Author         Comment
'                3/9/98     Casper         Wrote it.
'
'**************************************************************

Option Explicit
Public GFsetOk As Boolean

Dim LProtocolIdx As Integer
Dim LDirectionIdx As Integer

Private Sub Form_Load()
    obProtocol(GProtocol).Value = True
    obDirection(GDirection).Value = True
    GFsetOk = False
End Sub

Private Sub obDirection_Click(Index As Integer)
    LDirectionIdx = Index
End Sub

Private Sub obProtocol_Click(Index As Integer)
    LProtocolIdx = Index
End Sub

Private Sub Option1_Click()

End Sub

Private Sub SetDlgCancel_Click()
    GhForm.GFsetCancel = True
    Unload FSet
End Sub

Private Sub SetDlgOk_Click()
    GProtocol = LProtocolIdx
    GDirection = LDirectionIdx
    Unload FSet
End Sub
