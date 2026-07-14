Imports System.Text
Imports Sybase.Data.AseClient

'Data Access Object Sybase
Public Class DAOSybase
    Private ReadOnly strConexaoSybase As String
    Private eventViewer As EventLog

#Region "Metodos"

    'Sub New()
    'Requer : conexaoSybase - String de conexao com o sybase  
    'Ação   : Metodo Construtor da classe que possui como parametro a string de conexao do sybase
    Public Sub New(ByVal conexaoSybase As String, ByRef _eventViewer As EventLog)
        strConexaoSybase = conexaoSybase
        eventViewer = _eventViewer
    End Sub


    'Sub BuscarDataInternaloProcesso()
    'Requer : telaServico   - Objeto da Classe principal do serviço
    'Ação   : busca e seta variaveis Data inicial / final / periodo de execuçao do serviço
    Public Sub BuscarDataInternaloProcesso(ByRef telaServico As Service1)

        Dim Query As New StringBuilder
        Dim cn As AseConnection = New AseConnection(DbStringSybaseASE)
        Dim tb As New DataTable
        Try
            cn.Open()
            Query.Append("SELECT * FROM DB_SEG_PROD..TBL_PARAMETRO_SISTEMA A ,  DB_SEG_PROD..TBL_SISTEMA B WHERE ")
            Query.Append(" A.CN_SISTEMA = B.CN_SISTEMA AND ")
            Query.Append(" B.CD_ALIAS_SISTEMA = '" & Configuration.ConfigurationManager.AppSettings.Item("NomeSistema") & "' and  cn_parametro_sistema in(" & Configuration.ConfigurationManager.AppSettings.Item("CodParametroHoraIniSistema") & "," & Configuration.ConfigurationManager.AppSettings.Item("CodParametroHoraFimSistema") & "," & Configuration.ConfigurationManager.AppSettings.Item("CodPeriodoExecSistema") & ") order by cn_parametro_sistema")

            Dim daSybase As AseDataAdapter = New AseDataAdapter(Query.ToString, cn)
            daSybase.SelectCommand.CommandTimeout = 120 '2 Minutos para TimeOut
            daSybase.Fill(tb)

            If tb.Rows.Count = 3 Then
                horaIniSistema = tb.Rows(0).Item("cd_parametro_sistema")
                horaFimSistema = tb.Rows(1).Item("cd_parametro_sistema")
                PeriodoSistemaMin = CInt(tb.Rows(2).Item("cd_parametro_sistema"))
            Else
                horaIniSistema = ""
                horaFimSistema = ""
                PeriodoSistemaMin = 0
            End If
        Catch ex As Exception
            horaIniSistema = ""
            horaFimSistema = ""
            PeriodoSistemaMin = 0
            telaServico.GerarEventLog("Ocorreu Erro ao tentar buscar a hora inicia/final e periodo do processo." & ex.Message, EventLogEntryType.Error)
            Dim oDaosybase As New DAOSybase(DbStringSybaseASE, eventViewer)
            oDaosybase.GerarLogSistema(Now, "E", 0, 0, 0, 0, "<Etapa>Consulta Hora Ini, Hora Fim ,periodo</Etapa><Observacao>Ocorreu Erro ao tentar buscar a hora inicia/final e periodo do processo." & vbNewLine & ex.Message & "</Observacao>")
        Finally
            If cn.State = ConnectionState.Open Then cn.Close()
        End Try

    End Sub

    'Sub GerarLogSistema
    'Requer : cd_alias_sistema             - Nome do Sistema
    '         cn_processo                  - numero do processo
    '         dt_inicio                    - data inicio
    '         id_status_processo           - Status do processo
    '         qt_registros_processados     - quantidade registros processados
    '         vl_registros_processados     - valor registros processados
    '         qt_registros_erro            - quantidade registros erros
    '         vl_registros_erro            - valor registro erros
    '         xml_obs_processo             - xml com observaçao do processo
    'Ação   : Metodo que gera log do sistema

    Public Sub GerarLogSistema(ByVal dt_inicio As Date,
    ByVal id_status_processo As String,
    ByVal qt_registros_processados As Long,
    ByVal vl_registros_processados As Long,
    ByVal qt_registros_erro As Long,
    ByVal vl_registros_erro As Long,
    ByVal xml_obs_processo As String)

        Dim oAseCon As AseConnection = Nothing

        Try
            oAseCon = New AseConnection(strConexaoSybase)
            oAseCon.Open()

            Dim oAseCmd As AseCommand
            oAseCmd = New AseCommand("DB_SEG_PROD..sp_inclui_log_processo", oAseCon) With {
                .CommandType = CommandType.StoredProcedure,
                .CommandTimeout = 120 'Dois minutos para TimeOut
                }

            Dim oAseParam As AseParameter
            oAseParam = oAseCmd.Parameters.Add(New AseParameter("@cd_alias_sistema", Configuration.ConfigurationManager.AppSettings.Item("NomeSistema")))
            oAseParam = oAseCmd.Parameters.Add(New AseParameter("@cn_processo", CInt(Configuration.ConfigurationManager.AppSettings.Item("Cn_Processo"))))
            oAseParam = oAseCmd.Parameters.Add(New AseParameter("@dt_inicio", dt_inicio))
            oAseParam = oAseCmd.Parameters.Add(New AseParameter("@id_status_processo", id_status_processo))
            oAseParam = oAseCmd.Parameters.Add(New AseParameter("@qt_registros_processados", qt_registros_processados))
            oAseParam = oAseCmd.Parameters.Add(New AseParameter("@vl_registros_processados", vl_registros_processados))
            oAseParam = oAseCmd.Parameters.Add(New AseParameter("@qt_registros_erro", qt_registros_erro))
            oAseParam = oAseCmd.Parameters.Add(New AseParameter("@vl_registros_erro", vl_registros_erro))
            oAseParam = oAseCmd.Parameters.Add(New AseParameter("@xml_obs_processo", xml_obs_processo))
            oAseParam = oAseCmd.Parameters.Add(New AseParameter("@cn_log_processo", AseDbType.Integer))
            oAseParam.Direction = ParameterDirection.Output

            oAseCmd.ExecuteNonQuery()

            'Dim Cd_Lote As Integer
            'Cd_Lote = CInt(oAseCmd.Parameters("@cn_log_processo").Value)
        Catch ex As Exception
            'Throw ex
            If eventViewer IsNot Nothing Then
                eventViewer.WriteEntry(ex.Message, EventLogEntryType.Error)
            End If
        Finally
            If Not IsNothing(oAseCon) AndAlso oAseCon.State = ConnectionState.Open Then
                oAseCon.Close()
            End If
        End Try
    End Sub

    'function ExcluirLog()
    'Requer : QtddiasExcluir   - quantidade de dias para excluir logs
    'Ação   : Exclui logs com base no parametro de dias informado onde data do log seja menor que (data atual - QtddiasExcluir)
    Public Sub ExcluirLog(ByVal QtddiasExcluir As Integer)
        Dim cn As AseConnection = New AseConnection(strConexaoSybase)

        Try
            cn.Open()
            Dim Query As New StringBuilder
            Dim dateExcluir As Date = Now.AddDays(-QtddiasExcluir)

            Query.Append(" delete db_seg_prod..tbl_log_processo ")
            Query.Append(" where cn_sistema = " & ObterNumeroSistema(Configuration.ConfigurationManager.AppSettings.Item("NomeSistema")))
            Query.Append(" and cn_processo = " & Configuration.ConfigurationManager.AppSettings.Item("Cn_Processo") & " and ")
            Query.Append(" dt_fim_processo < '" & Format(dateExcluir, "yyyy/MM/dd HH:mm:ss") & "'")


            Dim cmd As AseCommand = New AseCommand(Query.ToString, cn) With {
                .CommandTimeout = 120 '2 Minutos para TimeOut
                }
            cmd.ExecuteNonQuery()
        Catch ex As Exception
            Throw ex
        Finally
            cn.Close()
        End Try

    End Sub
    'function obterNumeroSistema()
    'Requer : sistema   - Alias sistema
    'Ação   : Retorna numero do sistema com base no alias informado
    Public Function ObterNumeroSistema(ByVal sistema As String) As Integer

        Dim Query As New StringBuilder
        Dim cn As AseConnection = New AseConnection(DbStringSybaseASE)
        Dim tb As New DataTable
        Try

            cn.Open()
            Query.Append("select cn_sistema  from db_seg_prod..tbl_sistema where cd_alias_sistema = '" & Configuration.ConfigurationManager.AppSettings.Item("NomeSistema") & "'")

            Dim daSybase As AseDataAdapter = New AseDataAdapter(Query.ToString, cn)
            daSybase.SelectCommand.CommandTimeout = 120 '2 Minutos para TimeOut

            daSybase.Fill(tb)

            Return tb.Rows(0).Item(0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function


#End Region

End Class
