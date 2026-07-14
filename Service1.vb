Imports System.ServiceProcess
Public Class Service1
    Inherits ServiceBase

#Region " Component Designer generated code "

    Public Sub New()
        MyBase.New()

        ' This call is required by the Component Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call

        'Inicializa as "Strings" de conexão
        Dim DesCpto As New Decrypt.ClsRecCripto With {
            .CryptText = Configuration.ConfigurationManager.AppSettings.Item("DbStringSybaseASE")
        }
        DbStringSybaseASE = DesCpto.DeCryptedText()

        'DesCpto.CryptText = AppSettings("SDContaConnectionString")
        'Module1.SDContaConnectionString = DesCpto.DeCryptedText()

        'DesCpto.CryptText = AppSettings("SDMRTConnectionString")
        'Module1.SDMRTConnectionString = DesCpto.DeCryptedText()

        'DesCpto.CryptText = AppSettings("NativeSqlServerConnectionString")
        'Module1.NativeSqlServerConnectionString = DesCpto.DeCryptedText()

        'Retorna o valor da String de conexão do Recupera
        'Incluido por Eduardo S. Alves em 30/12/2010
        'DesCpto.CryptText = AppSettings("NativeOracleConnectionRecuperaString")
        'Module1.strOracleConnectionRecuperaString = DesCpto.DeCryptedText()

    End Sub

    'UserService overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    ' The main entry point for the process
    <STAThread()>
    Public Shared Sub Main()
        Dim ServicesToRun() As ServiceBase

        ' More than one NT Service may run within the same process. To add
        ' another service to this process, change the following line to
        ' create a second service object. For example,
        '
        '   ServicesToRun = New System.ServiceProcess.ServiceBase () {New Service1, New MySecondUserService}
        '
        ServicesToRun = New ServiceBase() {New Service1}

        Run(ServicesToRun)
    End Sub

    'Required by the Component Designer
    Private components As ComponentModel.IContainer

    ' NOTE: The following procedure is required by the Component Designer
    ' It can be modified using the Component Designer.  
    ' Do not modify it using the code editor.
    Friend WithEvents Timer1 As Timers.Timer

    <DebuggerStepThrough()> Private Sub InitializeComponent()
        Timer1 = New Timers.Timer
        CType(Timer1, ComponentModel.ISupportInitialize).BeginInit()
        '
        'Timer1
        '
        Timer1.Enabled = True
        Timer1.Interval = 2000
        '
        'Service1
        '
        AutoLog = False
        ServiceName = "Sistema STA - Trasferencia de arquivos"
        CType(Timer1, ComponentModel.ISupportInitialize).EndInit()

    End Sub

#End Region

#Region "Eventos Serviço"
    'Sub OnStart()
    'Requer :   
    'Ação   : Evento disparado quando inicializar o Serviço

    Protected Overrides Sub OnStart(ByVal args() As String)
        Timer1.Enabled = True
    End Sub

    'Sub OnStop()
    'Requer :   
    'Ação   : Evento disparado quando Finalizar o Serviço
    Protected Overrides Sub OnStop()
        Timer1.Enabled = False
    End Sub

    'Sub Timer1_Elapsed()
    'Requer :   
    'Ação   : Evento do timer
    Private Sub Timer1_Elapsed(ByVal sender As Object, ByVal e As Timers.ElapsedEventArgs) Handles Timer1.Elapsed

        Timer1.Enabled = False

        ' Procedimento para debugar o serviço em execução sem a mensagem de instalação necessária.

        '1o crie um laço infinito no programa e inclua um break point na linha. exemplo abaixo:
        'While True
        '    Dim test As String = "Debugando"
        'End While

        '2o compile e copie o executável gerado com o laço infinito para outra pasta, exemplo c:\temp\Tribanco.TransferenciaDeArquivos.exe
        '3o no cmd vá ao diretorio do framework C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\ para instalar 
        ' faça o comando abaixo para instalar
        ' installutil -i "c:\temp\Tribanco.TransferenciaDeArquivos.exe"
        ' caso necessário desinstalar o comando é 
        ' installutil -u "c:\temp\Tribanco.TransferenciaDeArquivos.exe"
        '4o no services.msc do windows vá ao serviço que foi criado e inicie ele. "start"
        '5o aqui no visual studio, vá no menu debug e depois em Attch Process
        '6o busque nas opçoes o serviço que foi iniciado no services.msc e anexe no debug, pelo botão attach
        '7o no proximo ciclo do processo, ele vai parar na linha com break do passo 1o
        '8o arraste a linha de execução em amarelo para a primeira linha do programa e continue com o debug sem interferencia.


        ' Inicializa variavél com a etapa do erro.
        Dim tipoErro As String = "Inicializacao da operacao."

        '1o tratamento de excessao verifica descriptografia e conexao com banco de dados.
        Try

            Dim DesCpto As New Decrypt.ClsRecCripto With {
                .CryptText = Configuration.ConfigurationManager.AppSettings.Item("DbStringSybaseASE")
            }
            DbStringSybaseASE = DesCpto.DeCryptedText()
            Dim oDaoSybase As DAOSybase = New DAOSybase(DbStringSybaseASE, EventLog)

            '2o tratamento de excessao com a conexao aberta compacta e movimenta os arquivos.
            Try
                'Carrega eventos da camada de negocio.
                Dim bll As New BLLControler(EventLog)
                'Abre o arquivo xml de configurações do sistema.
                If Not ErroXmlPaths() Then
                    oDaoSybase.BuscarDataInternaloProcesso(Me)
                    If PeriodoSistemaMin <> 0 And horaIniSistema.Trim.Length = 8 And horaFimSistema.Trim.Length = 8 Then
                        Timer1.Interval = PeriodoSistemaMin * 60000
                        'verifica se o horário da execução está dentro da configuração do sistema de segurança.
                        If DentroPeriodo() Then
                            'tranfereArquivos
                            EventLog.WriteEntry("Inicio da operacao", EventLogEntryType.Information)
                            oDaoSybase.GerarLogSistema(Now, "O", 0, 0, 0, 0, "<Etapa>Inicio da operacao</Etapa><Observacao></Observacao>")
                            tipoErro = "Transferencia de arquivos"
                            bll.TransfereArquivosDoXml(Me)

                            'exclusao de log
                            tipoErro = "Exclusão de log de processos"
                            bll.ExcluirLog()
                            EventLog.WriteEntry("Termino da operacao", EventLogEntryType.Information)
                            oDaoSybase.GerarLogSistema(Now, "O", 0, 0, 0, 0, "<Etapa>Termino da operacao</Etapa><Observacao></Observacao>")
                        Else
                            EventLog.WriteEntry("O processo de transferencia de arquivos não foi executada pois não esta dentre o periodo de hora inicial e final.", EventLogEntryType.Information)
                            'Comentado a instrução para não sobrecarregar o tamanho do log em banco de dados.
                            'oDaoSybase.GerarLogSistema(Now, "W", 0, 0, 0, 0, "<Etapa>Transferencia não executada</Etapa><Observacao>O processo de transferencia de arquivos não foi executada pois não esta dentre o periodo de hora inicial e final.</Observacao>")
                        End If
                    Else
                        EventLog.WriteEntry("Data inicial/final ou periodo não estão informados corretamente.", EventLogEntryType.Warning)
                        oDaoSybase.GerarLogSistema(Now, "E", 0, 0, 0, 0, "<Etapa>Inicio da operacao</Etapa><Observacao>Parametros incorretos - verifique o Sistema de Seguranca (STA).</Observacao>")
                    End If
                Else
                    EventLog.WriteEntry("Falha ao abrir arquivo XML de configuração", EventLogEntryType.Error)
                    oDaoSybase.GerarLogSistema(Now, "E", 0, 0, 0, 0, "<Etapa>Falha ao abrir arquivo XML de configuração</Etapa><Observacao>Falha ao abrir arquivo XML de configuração - verifique o arquivo paths.xml do (STA).</Observacao>")
                End If

                Timer1.Enabled = True
            Catch ex As Exception
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error)
                oDaoSybase.GerarLogSistema(Now, "E", 0, 0, 0, 0, "<Etapa>" & tipoErro & "</Etapa><Observacao>" & ex.Message & "</Observacao>")
                Timer1.Enabled = True
            End Try
        Catch ex As Exception
            EventLog.WriteEntry("Erro em " & tipoErro & " . " & ex.Message, EventLogEntryType.Information)
            Timer1.Enabled = True
        End Try

        Timer1.Enabled = True
    End Sub

#End Region

#Region "Funções"

    'Sub GerarEventLog()
    'Requer :   
    'Ação   : Gera eventos no visualizador de eventos do windows
    Public Sub GerarEventLog(ByVal msg As String, ByVal tipoEvent As EventLogEntryType)
        Try
            EventLog.WriteEntry(msg, tipoEvent)
        Catch ex As Exception
            EventLog.WriteEntry(ex.Message, EventLogEntryType.Error)
        End Try
    End Sub

    'Function DentroPeriodo()boolean
    'Requer :   
    'Ação   : verifica se o serviço esta executando entre o intervalo de tempo inicial e final estipulado.
    Public Function DentroPeriodo() As Boolean
        Try
            Return If("1900/01/01 " & horaIniSistema <= CDate("1900/01/01 " & CStr(Now.Hour).PadLeft(2, "0") & ":" & CStr(Now.Minute).PadLeft(2, "0") & ":" & CStr(Now.Second).PadLeft(2, "0")) _
                            And "1900/01/01 " & horaFimSistema >= CDate("1900/01/01 " & CStr(Now.Hour).PadLeft(2, "0") & ":" & CStr(Now.Minute).PadLeft(2, "0") & ":" & CStr(Now.Second).PadLeft(2, "0")),
                True,
                False)
        Catch ex As Exception
            Return False
        End Try
    End Function

    'Function ErroXmlPaths()boolean
    'Requer :   
    'Ação   : Verifica erro na leitura do xml, caso haja erro retorna true, caso contrario false
    Private Function ErroXmlPaths() As Boolean
        Try
            Dim odts As New DataSet
            Dim leitura As New IO.StreamReader(Configuration.ConfigurationManager.AppSettings.Item("ArquivoPathsXml"))
            Dim oObeLetCsn As IO.StringReader = New IO.StringReader(leitura.ReadToEnd)

            odts.ReadXml(oObeLetCsn)
            dtsCaminhos = odts

            For countTb As Integer = 0 To dtsCaminhos.Tables.Count - 1
                For countRw As Integer = 0 To dtsCaminhos.Tables(countTb).Rows.Count - 2
                    If dtsCaminhos.Tables(countTb).Rows(countRw).Item("DiretorioPrincipal") Is DBNull.Value Then
                        GerarEventLog("Erro na leitura do xml, tag <DiretorioPrincipal> é obrigatoria.", EventLogEntryType.Error)
                        Dim oDaosybase As New DAOSybase(DbStringSybaseASE, EventLog)
                        oDaosybase.GerarLogSistema(Now, "W", 0, 0, 0, 0, "<Etapa>Validação XML</Etapa><Observacao>Erro na leitura do xml, tag <DiretorioPrincipal> é obrigatoria.</Observacao>")
                        Return True
                    ElseIf CStr(dtsCaminhos.Tables(countTb).Rows(countRw).Item("DiretorioPrincipal")) = "" Then
                        GerarEventLog("Erro na leitura do xml, tag <DiretorioPrincipal> é obrigatoria.", EventLogEntryType.Error)
                        Dim oDaosybase As New DAOSybase(DbStringSybaseASE, EventLog)
                        oDaosybase.GerarLogSistema(Now, "W", 0, 0, 0, 0, "<Etapa>Validação XML</Etapa><Observacao>Erro na leitura do xml, tag <DiretorioPrincipal> é obrigatoria.</Observacao>")
                        Return True
                    End If
                    If dtsCaminhos.Tables(countTb).Rows(countRw).Item("DiasExcluir") Is DBNull.Value Then
                        GerarEventLog("Erro na leitura do xml, tag <DiasExcluir> é obrigatoria.", EventLogEntryType.Error)
                        Dim oDaosybase As New DAOSybase(DbStringSybaseASE, EventLog)
                        oDaosybase.GerarLogSistema(Now, "W", 0, 0, 0, 0, "<Etapa>Validação XML</Etapa><Observacao>Erro na leitura do xml, tag <DiasExcluir> é obrigatoria.</Observacao>")
                        Return True
                    ElseIf CStr(dtsCaminhos.Tables(countTb).Rows(countRw).Item("DiasExcluir")) = "" Then
                        GerarEventLog("Erro na leitura do xml, tag <DiasExcluir> é obrigatoria.", EventLogEntryType.Error)
                        Dim oDaosybase As New DAOSybase(DbStringSybaseASE, EventLog)
                        oDaosybase.GerarLogSistema(Now, "W", 0, 0, 0, 0, "<Etapa>Validação XML</Etapa><Observacao>Erro na leitura do xml, tag <DiasExcluir> é obrigatoria.</Observacao>")
                        Return True
                    End If
                    'If Module1.dtsCaminhos.Tables(countTb).Rows(countRw).Item("Etapa") Is DBNull.Value Then
                    '    GerarEventLog("Erro na leitura do xml, tag <Etapa> é obrigatoria.", EventLogEntryType.Error)
                    '    Dim oDaosybase As New DAOSybase(Module1.DbStringSybaseASE)
                    '    oDaosybase.GerarLogSistema(Now, "W", 0, 0, 0, 0, "<Etapa>Validação XML</Etapa><Observacao>Erro na leitura do xml, tag <Etapa> é obrigatoria.</Observacao>")
                    '    Return True
                    'ElseIf CStr(Module1.dtsCaminhos.Tables(countTb).Rows(countRw).Item("Etapa")) = "" Then
                    '    GerarEventLog("Erro na leitura do xml, tag <Etapa> é obrigatoria.", EventLogEntryType.Error)
                    '    Dim oDaosybase As New DAOSybase(Module1.DbStringSybaseASE)
                    '    oDaosybase.GerarLogSistema(Now, "W", 0, 0, 0, 0, "<Etapa>Validação XML</Etapa><Observacao>Erro na leitura do xml, tag <Etapa> é obrigatoria.</Observacao>")
                    '    Return True
                    'End If
                Next
            Next

            Return False
        Catch ex As Exception
            GerarEventLog("Erro na leitura do xml.Erro:" & ex.Message, EventLogEntryType.Error)
            Dim oDaosybase As New DAOSybase(DbStringSybaseASE, EventLog)
            oDaosybase.GerarLogSistema(Now, "E", 0, 0, 0, 0, "<Etapa>Leitura Xml</Etapa><Observacao>Erro na leitura do xml.Erro:" & ex.Message & "</Observacao>")
            Return True
        End Try

    End Function

#End Region

End Class
