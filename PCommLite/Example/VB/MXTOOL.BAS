Attribute VB_Name = "Mxtool"
'************************************************************
'    Mxtool.bas
'     -- Process PComm Lib function return value
'
'
'    History:   Date       Author         Comment
'               3/10/98     Casper         Wrote it.
'               12/08/98    Casper         Modify message.
'
'************************************************************
Option Explicit

Public Const FORMAT_MESSAGE_FROM_SYSTEM = &H1000
Public Const LANG_NEUTRAL = &H0
Public Const SUBLANG_DEFAULT = &H1

Declare Function GetLastError Lib "kernel32" () As Long
Declare Function FormatMessage Lib "kernel32" Alias "FormatMessageA" _
    (ByVal dwFlags As Long, lpSource As Any, ByVal dwMessageId As Long, _
    ByVal dwLanguageId As Long, ByVal lpBuffer As String, _
    ByVal nSize As Long, Arguments As Long) As Long

Public Sub ShowSysErr(title As String, syserr As Long)
Dim lpMsgBuf As String * 80
Dim lang As Integer
    
    'lang = {MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT) }
    lang = SUBLANG_DEFAULT * 2 ^ 10 + LANG_NEUTRAL
    Call FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, _
        0, syserr, lang, lpMsgBuf, 80, 0)
    Call MsgBox(lpMsgBuf, vbOKOnly Or vbExclamation, title)
End Sub

Public Sub MxShowError(title As String, errcode As Long, syserr As Long)
Dim buf As String
    
    If errcode <> SIO_WIN32FAIL Then
        Select Case errcode
        Case SIO_BADPORT
            buf = "Port number is invalid or port is not opened in advance"
        Case SIO_OUTCONTROL:
            buf = "The board does not support this function"
        Case SIO_NODATA:
            buf = "No data to read"
        Case SIO_OPENFAIL:
            buf = "No such port or port is occupied by other program"
        Case SIO_RTS_BY_HW:
            buf = "RTS can't be set because H/W Flowctrl"
        Case SIO_BADPARM:
            buf = "Bad parameter"
        Case SIO_BOARDNOTSUPPORT:
            buf = "The board does not support this function"
        Case SIO_ABORT_WRITE:
            buf = "Write has blocked, and user abort write"
        Case SIO_WRITETIMEOUT:
            buf = "Write timeout has happened"
        Case Else
            buf = "Unknown Error:" & errcode
        End Select
        Call MsgBox(buf, vbOKOnly Or vbExclamation, title)
    Else
        Call ShowSysErr(title, syserr)
    End If
End Sub

