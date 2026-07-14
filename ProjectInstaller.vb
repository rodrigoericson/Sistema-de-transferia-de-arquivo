Imports System.ComponentModel
Imports System.Configuration.Install

<RunInstaller(True)> Public Class ProjectInstaller
    Inherits Installer

#Region " Component Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Component Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Installer overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Component Designer
    Private components As IContainer

    'NOTE: The following procedure is required by the Component Designer
    'It can be modified using the Component Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents ServiceProcessInstaller1 As ServiceProcess.ServiceProcessInstaller
    Friend WithEvents ServiceInstaller1 As ServiceProcess.ServiceInstaller
    <DebuggerStepThrough()> Private Sub InitializeComponent()
        ServiceProcessInstaller1 = New ServiceProcess.ServiceProcessInstaller
        ServiceInstaller1 = New ServiceProcess.ServiceInstaller
        '
        'ServiceProcessInstaller1
        '
        ServiceProcessInstaller1.Account = ServiceProcess.ServiceAccount.LocalSystem
        ServiceProcessInstaller1.Password = Nothing
        ServiceProcessInstaller1.Username = Nothing
        '
        'ServiceInstaller1
        '
        ServiceInstaller1.DisplayName = "Sistema STA - Trasferencia de arquivos"
        ServiceInstaller1.ServiceName = "Sistema STA - Trasferencia de arquivos"
        '
        'ProjectInstaller
        '
        Installers.AddRange(New Installer() {ServiceProcessInstaller1, ServiceInstaller1})

    End Sub

#End Region

End Class
