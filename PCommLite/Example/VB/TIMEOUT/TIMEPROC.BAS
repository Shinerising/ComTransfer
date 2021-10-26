Attribute VB_Name = "TimeProc"
'*********************************************************************
'    TOThread.pas
'     -- Read & write procedure for timeout demo example program.
'
'    History:   Date       Author         Comment
'               3/11/98    Casper         Wrote it.
'               4/14/00    Casper         Update.
'**********************************************************************)


Option Explicit

Public Const IDM_WRITE_POLL = 0
Public Const IDM_WRITE_BLOCK = 1
Public Const IDM_WRITE_TIMEOUT = 2
Public Const IDM_READ_POLL = 3
Public Const IDM_READ_BLOCK = 4
Public Const IDM_READ_BLOCK_T = 5
Public Const IDM_READ_BLOCK_I = 6
Public Const IDM_READ_BLOCK_TI = 7

Public Const BUFCNT = 1024
Public Const BUFLEN As Long = 10 * BUFCNT

Declare Function GetTickCount Lib "kernel32" () As Long

Public GDifTime As Long
Public GCount As Long
Public GCallCount As Long
Public GhExit As Boolean

Dim LBuf(0 To BUFLEN) As Byte


Public Sub WriteProc()
Dim i As Integer
Dim t1 As Long
    
    For i = 0 To BUFLEN
        LBuf(i) = Asc("T")
    Next

    Do
        DoEvents
        
        t1 = GetTickCount()
        If (sio_write(GCommData.Port, LBuf(0), BUFLEN) > 0) Then
            GCount = GCount + 1
        End If
        GCallCount = GCallCount + 1
        GDifTime = GetTickCount() - t1
        Call WStat.RefreshDlg
    Loop Until GhExit
End Sub



Public Sub ReadProc()
Dim t1 As Long

    Do
        DoEvents
        If GhExit Then
            Exit Sub
        End If
            
        t1 = GetTickCount()
        If (sio_read(GCommData.Port, LBuf(0), BUFLEN) > 0) Then
            GCount = GCount + 1
        End If
        GCallCount = GCallCount + 1
        GDifTime = GetTickCount() - t1
        Call RStat.RefreshDlg
    Loop Until False
End Sub


