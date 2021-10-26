Attribute VB_Name = "Global"
'*********************************************************
'    ExGlobal.bas
'     -- Global variable & Com port record defined for
'        example program.
'
'
'    History:   Date       Author         Comment
'               3/9/98     Casper         Wrote it.
'               4/14/00    Casper         Use timer
'
'*********************************************************

Option Explicit
Public Type COMMDATA
    Port As Long
    
    BaudRate As Integer
    Parity As Integer
    ByteSize As Integer
    StopBits As Integer
    
    ibaudrate As Integer
    iparity As Integer
    ibytesize As Integer
    istopbits As Integer
    
    Hw As Integer
    Sw As Integer
    Dtr As Integer
    Rts As Integer
End Type

Public GCommData As COMMDATA
Public GhForm As Form
'Public GhExit As Boolean

Global GBaudTable(0 To 19) As Integer
Global GParityTable(0 To 4) As Integer
Global GByteSizeTable(0 To 3) As Integer
Global GStopBitsTable(0 To 1) As Integer

Global GstrBaudTable(0 To 19) As String
Global GstrParityTable(0 To 4) As String
Global GstrByteSizeTable(0 To 3) As String
Global GstrStopBitsTable(0 To 1) As String

Public Sub InitTable()
    GstrBaudTable(0) = "50"
    GstrBaudTable(1) = "75"
    GstrBaudTable(2) = "110"
    GstrBaudTable(3) = "134"
    GstrBaudTable(4) = "150"
    GstrBaudTable(5) = "300"
    GstrBaudTable(6) = "600"
    GstrBaudTable(7) = "1200"
    GstrBaudTable(8) = "1800"
    GstrBaudTable(9) = "2400"
    GstrBaudTable(10) = "4800"
    GstrBaudTable(11) = "7200"
    GstrBaudTable(12) = "9600"
    GstrBaudTable(13) = "19200"
    GstrBaudTable(14) = "38400"
    GstrBaudTable(15) = "57600"
    GstrBaudTable(16) = "115200"
    GstrBaudTable(17) = "230400"
    GstrBaudTable(18) = "460800"
    GstrBaudTable(19) = "921600"
    
    GstrParityTable(0) = "None"
    GstrParityTable(1) = "Even"
    GstrParityTable(2) = "Odd"
    GstrParityTable(3) = "Mark"
    GstrParityTable(4) = "Space"

    GstrByteSizeTable(0) = "5"
    GstrByteSizeTable(1) = "6"
    GstrByteSizeTable(2) = "7"
    GstrByteSizeTable(3) = "8"

    GstrStopBitsTable(0) = "1"
    GstrStopBitsTable(1) = "2"
    
    GBaudTable(0) = B50
    GBaudTable(1) = B75
    GBaudTable(2) = B110
    GBaudTable(3) = B134
    GBaudTable(4) = B150
    GBaudTable(5) = B300
    GBaudTable(6) = B600
    GBaudTable(7) = B1200
    GBaudTable(8) = B1800
    GBaudTable(9) = B2400
    GBaudTable(10) = B4800
    GBaudTable(11) = B7200
    GBaudTable(12) = B9600
    GBaudTable(13) = B19200
    GBaudTable(14) = B38400
    GBaudTable(15) = B57600
    GBaudTable(16) = B115200
    GBaudTable(17) = B230400
    GBaudTable(18) = B460800
    GBaudTable(19) = B921600
    
    GParityTable(0) = P_NONE
    GParityTable(1) = P_EVEN
    GParityTable(2) = P_ODD
    GParityTable(3) = P_MRK
    GParityTable(4) = P_SPC

    GByteSizeTable(0) = BIT_5
    GByteSizeTable(1) = BIT_6
    GByteSizeTable(2) = BIT_7
    GByteSizeTable(3) = BIT_8

    GStopBitsTable(0) = STOP_1
    GStopBitsTable(1) = STOP_2
End Sub



