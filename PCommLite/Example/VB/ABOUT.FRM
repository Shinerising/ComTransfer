VERSION 5.00
Begin VB.Form About 
   BorderStyle     =   1  'Fixed Single
   Caption         =   "About"
   ClientHeight    =   1356
   ClientLeft      =   48
   ClientTop       =   336
   ClientWidth     =   4524
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   1356
   ScaleWidth      =   4524
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton AboutOk 
      Caption         =   "OK"
      Default         =   -1  'True
      Height          =   375
      Left            =   3360
      TabIndex        =   0
      Top             =   240
      Width           =   855
   End
   Begin VB.Label AboutTxt1 
      Height          =   255
      Left            =   240
      TabIndex        =   1
      Top             =   240
      Width           =   2775
   End
End
Attribute VB_Name = "About"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'********************************************************
'    About.frm
'     -- About dialog for example program
'
'    History:   Date       Author         Comment
'               3/10/98    Casper         Wrote it.
'
'********************************************************

Option Explicit
Private Sub AboutOk_Click()
    Unload About
End Sub
