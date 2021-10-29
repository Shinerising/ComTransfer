# 自动化串口文件传输工具说明文档

![](https://img.shields.io/badge/Windows-0078D6?style=for-the-badge&logo=windows)
![](https://img.shields.io/badge/Visual%20Studio-5C2D91?style=for-the-badge&logo=visualstudio)
![](https://img.shields.io/badge/.NET%20Framework-512BD4?style=for-the-badge&logo=dotnet)
![](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp)
![](https://img.shields.io/badge/XAML-0C54C2?style=for-the-badge&logo=xaml)

一款支持计算机之间通过串口执行文件发送、文件拉取、自动化文件备份等操作的轻量级软件。

<img width="426" src="Images/image00.png">

## 软件功能

- 采用.NET Framework 4.0开发，支持Windows XP及之后所有的Windows系统，同时支持32位和64位系统；
- 采用广泛使用的串口通信扩展库（Pcomm Lite）实现串口操作和文件发送功能，工作稳定可靠；
- 采用ZModem文件传输协议，可支持使用最高921600波特率进行传输作业，平均数据传输速度超过100KiB/s；
- 软件支持本地文件选择发送、文件拉取、传输进度显示、串口状态监视、日志记录、程序设置快速修改功能；
- 软件实现了访问远端计算机文件目录树的功能，可手动选取远端计算机中的文件并请求拉取；
- 软件提供计划任务功能，可定时发送固定文件夹中的某类文件至另一台计算机；
- 轻量化开发，用户界面简单易于使用，软件启动速度和执行效率较好。

## 界面说明

软件界面从上至下依次为：

- 串口信息：显示串口参数、启动状态、流状态指示等信息，最右侧为串口启动、关闭切换按钮；
- 程序设置：显示当前的文件存储目录设置信息，最右侧为程序设置按钮；
- 拉取文件：执行远端文件拉取工作，文本框中为远端计算机中文件位置，右侧分别为远端文件选择按钮、文件拉取指令按钮；
- 发送文件：执行文件发送工作，文本框中为本地计算机文件位置，右侧分别为本地文件选择按钮、文件发送指令按钮；
- 计划任务：显示和管理当前已使用的自动化文件传输作业，右侧按钮为计划任务管理窗口启动按钮；
- 工作日志：显示程序的文件操作、指令发送以及其他历史工作内容，右侧按钮为日志清除按钮；
- 传输记录：显示所有成功的文件传输记录，右侧按钮为记录清除按钮；
- 传输进度：显示文件接收与发送进度条和剩余时间信息，默认状态下隐藏。

## 操作步骤

以下介绍常见工作流程的具体软件操作步骤：

### 程序设置

<img width="387" src="Images/image01.png">

### 打开串口通信

### 发送文件

### 拉取文件

<img width="309" src="Images/image02.png">

### 使用计划任务

<img width="343" src="Images/image03.png">

## 注意事项

- 文件传输过程为半双工，一方计算机在接受文件时无法同时发送文件，因此不宜同时发送多个文件，以避免数据通信过程长期被占用；
- 为实现较高的文件传输速度，建议使用能够支持较高波特率的通信设备（如NPort、USB虚拟串口等）；
- 受限于串口通信速度，请勿传输体积较大的文件。

## 开发说明

本项目使用了Moxa提供的PComm Lite串口通信软件开发工具中所包含的扩展库文件，相关信息可参见 https://www.moxa.com.tw/product/download_pcommlite_info.htm 。
