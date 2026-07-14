Imports System.IO
Imports System.Text.RegularExpressions


'Business Logic Layer
Public Class BLLControler
    Private eventViewer As EventLog

#Region "Metodos"
    Public Sub New(ByRef _eventViewer As EventLog)
        eventViewer = _eventViewer
    End Sub

    'Sub TransfereArquivosDoXml()
    'Requer : TelaServico    - Objeto da classe principal do serviço
    'Ação   : metodo que lê o dataset contendo os caminhos para transferencia e chama metodo para a transferencia
    Public Sub TransfereArquivosDoXml(ByRef telaServico As Service1)
        Try
            For countTb As Integer = 0 To dtsCaminhos.Tables.Count - 1
                For countRw As Integer = 0 To dtsCaminhos.Tables(countTb).Rows.Count - 1
                    'se nao estiver na ultima linha da tabela move arquivos, caso contrario apenas exclui do ultimo servidor
                    If countRw <> dtsCaminhos.Tables(countTb).Rows.Count - 1 Then
                        TransfereArquivos(dtsCaminhos.Tables(countTb).Rows(countRw).Item("DiretorioPrincipal"),
                        dtsCaminhos.Tables(countTb).Rows(countRw + 1).Item("DiretorioPrincipal"),
                        Configuration.ConfigurationManager.AppSettings.Item("SobreEscreverArquivos"),
                        dtsCaminhos.Tables(countTb).Rows(countRw).Item("DiasExcluir"),
                        telaServico,
                        dtsCaminhos.Tables(countTb).Rows(countRw).Item("MascaraArq"),
                        dtsCaminhos.Tables(countTb).Rows(0).Item("Etapa"),
                        dtsCaminhos.Tables(countTb).Rows(countRw).Item("DiretorioBackup"),
                        dtsCaminhos.Tables(countTb).Rows(countRw).Item("CompactaOrigemTipo"),
                        dtsCaminhos.Tables(countTb).Rows(countRw).Item("DescompactaDestino"),
                        dtsCaminhos.Tables(countTb).Rows(countRw).Item("TamanhoInicialArqBytes"),
                        dtsCaminhos.Tables(countTb).Rows(countRw).Item("TamanhoFinalArqBytes"))
                    Else
                        ExcluiArquivos(dtsCaminhos.Tables(countTb).Rows(countRw).Item("DiretorioBackup"),
                        dtsCaminhos.Tables(countTb).Rows(countRw).Item("DiretorioPrincipal"),
                        dtsCaminhos.Tables(countTb).Rows(countRw).Item("DiasExcluir"),
                        dtsCaminhos.Tables(countTb).Rows(countRw).Item("MascaraArq"),
                        telaServico,
                        dtsCaminhos.Tables(countTb).Rows(0).Item("Etapa"))
                    End If

                    If countRw = dtsCaminhos.Tables(countTb).Rows.Count - 1 Then
                        Dim oDaosybase As New DAOSybase(DbStringSybaseASE, eventViewer)

                        If Configuration.ConfigurationManager.AppSettings.Item("GeraLogSucessoEventView") Then
                            telaServico.GerarEventLog("Processos Realizados com sucesso da Etapa:" & vbNewLine & dtsCaminhos.Tables(countTb).Rows(0).Item("Etapa") & vbNewLine & msgTransferenciasExecSucesso, EventLogEntryType.Information)
                        End If
                        If Configuration.ConfigurationManager.AppSettings.Item("GeraLogSucessoBancoDados") Then
                            oDaosybase.GerarLogSistema(Now, "O", 0, 0, 0, 0, "<Etapa>" & dtsCaminhos.Tables(countTb).Rows(0).Item("Etapa") & "</Etapa><Observacao>" & msgTransferenciasExecSucesso & "</Observacao>")
                        End If

                        msgTransferenciasExecSucesso = ""

                        'telaServico.GerarEventLog("Fim Transferencia arquivos etapa:" & Module1.dtsCaminhos.Tables(countTb).Rows(0).Item("Etapa"), EventLogEntryType.Information)
                        'oDaosybase.GerarLogSistema(Now, "O", 0, 0, 0, 0, "<Etapa>" & Module1.dtsCaminhos.Tables(countTb).Rows(0).Item("Etapa") & "</Etapa><Observacao>Realizada com sucesso.</Observacao>")
                    End If
                Next
            Next

        Catch ex As Exception
            Dim oDaosybase As New DAOSybase(DbStringSybaseASE, eventViewer)
            telaServico.GerarEventLog("Ocorreu o seguinte Erro:" & ex.Message, EventLogEntryType.Error)
            oDaosybase.GerarLogSistema(Now, "E", 0, 0, 0, 0, "<Etapa>Transferencia de arquivos</Etapa><Observacao>" & ex.Message & "</Observacao>")
        End Try

    End Sub

    'Sub TransfereArquivos()
    'Requer : caminhoOrigem         - diretorio de origem
    '         caminhoDestino        - diretorio de destino
    '         sobrescreverArquivo   - flag sobreescrever arquivos
    '         DiasExclusao          - dias para exclusão de arquivos ja transferidos
    '         telaServico           - Objeto da classe principal do serviço
    '         mascara               - Mascara do arquivo no diretorio de origem
    '         Etapa                 - Descricao da etapa para os logs
    '         caminhoBackup         - Caminho para criacao de arquivo de backup
    '         CompactaArqOrigem     - Se informado os tipos (ZIP, RAR ou 7Z), compacta o arquivo no diretorio de origem da operacao.
    '         DescompactaArqDestino - Se informado "SIM", descompacta arquivo na pasta de destino nos formatos (ZIP, RAR, 7Z)
    'Ação   : metodo que trasfere arquivos de um diretorio para outro, e que conforme parametros exclui, move, compacta e descompacta arquivos
    'Compactar
    '7z a -t7z back.7z C:\*
    'Descompactar
    '7z e back.7z -oC:\pasta

    Private Function TransfereArquivos(ByVal caminhoOrigem As String,
        ByVal caminhoDestino As String,
        ByVal sobrescreverArquivo As Boolean,
        ByVal DiasExclusao As Integer,
        ByRef telaServico As Service1,
        ByVal mascara As String,
        ByVal Etapa As String,
        Optional ByVal caminhoBackup As String = "",
        Optional ByVal CompactaArqOrigem As String = "",
        Optional ByVal DescompactaArqDestino As String = "",
        Optional ByVal TamanhoInicialArq As Long = 0,
        Optional ByVal TamanhoFinalArq As Long = 0) As Boolean

        Try
            ' Variavel para rastrear no log os marcos de erro
            Dim tracer As Integer = 0

            'Abre conexao com banco para gravar os logs.
            Dim oDaoSybase As New DAOSybase(DbStringSybaseASE, eventViewer)
            ' Variavel para armazenar o arquivo em tratamento com o diretório.
            Dim ArqEmTratamentoComDir As String = ""
            ' Variavel para armazenar o arquivo em tratamento sem o diretório.
            Dim ArqEmTratamentoSemDir As String = ""

            'Tratamento de erro para transferencia do arquivo.
            Try

                'Busca todos arquivos do diretório de origem.
                Dim DiretorioFonte As New DirectoryInfo(caminhoOrigem)

                'Variavel para manipulacao de arquivo a arquivo, do diretório.
                Dim arquivos As FileInfo
                'Laco para tratar cada arquivo do diretório.
                For Each arquivos In DiretorioFonte.GetFiles()
                    ' Limpa Variaveis do arquivo a ser tratado
                    ArqEmTratamentoComDir = ""
                    ArqEmTratamentoSemDir = ""
                    'verifica se o arquivo que foi lido do diretorio realmente existe antes de iniciar o tratamento.
                    If File.Exists(Path.Combine(DiretorioFonte.FullName, arquivos.Name)) Then
                        'carrega o nome completo do arquivo que será tratado.
                        ArqEmTratamentoComDir = DiretorioFonte.FullName & "\" & arquivos.Name
                        'carrega o nome do arquivo sem o diretório.
                        ArqEmTratamentoSemDir = arquivos.Name
                        'verifica se arquivo esta dentro da mascara estabelecida
                        If ValidaMascara(mascara, arquivos.Name) Then
                            'verifica se o arquivo está em uso por outro processo.
                            If Not ArquivoAberto(ArqEmTratamentoComDir) Then
                                'verifica se o arquivo está dentro do tamanho solicitado.
                                If ValidaTamanho(TamanhoInicialArq, TamanhoFinalArq, arquivos.Length) Then
                                    'Inicio da manipulacao do arquivo. 

                                    '
                                    'Compactacao
                                    '
                                    'Verifica se o programa compactador foi informado e existe no servidor e 
                                    'se foi solicitada a compactação na origem para este arquivo no arquivo xml.
                                    tracer = 1
                                    ' verifica se foi solicitado a compactação no arquivo xml.
                                    If CompactaArqOrigem <> "" Then
                                        'valida se o setup e o sistema estao preparados para compactacao.
                                        If TrataCompactacao(CompactaArqOrigem, ArqEmTratamentoComDir) Then
                                            'se o arquivo de origem já estiver compactado desconsidera para não duplicar a compactacao
                                            If Not ArquivoCompactado(ArqEmTratamentoComDir) Then

                                                'atualiza o nome do arquivo sem diretório em tratamento, por ele com a extensão compactada.
                                                'o arquivo desta variavel será copiado para o destino e o backup
                                                ArqEmTratamentoSemDir = ArqEmTratamentoSemDir & "." & CompactaArqOrigem.ToUpper

                                                'compacta utilizando o 7zip no formato informado no arquivo de configuracao
                                                Dim proc As New Process()
                                                'variavel com a linha de comando DOS para compactação. Ex. c:\arquivos de programas\7-Zip\7z.exe a -t7z C:\temp\origem\arquivo.7z C:\temp\origem\arquivo.txt
                                                Dim parCompact As String = " a -t" & CompactaArqOrigem.ToLower & " """ & ArqEmTratamentoComDir & "." & CompactaArqOrigem.ToLower & """ """ & ArqEmTratamentoComDir & """"
                                                Dim TimeOutProc As Integer = Configuration.ConfigurationManager.AppSettings.Item("TimeoutCompactacao")
                                                'carrega argumentos para execução
                                                proc.StartInfo.Arguments = parCompact
                                                proc.StartInfo.FileName = Configuration.ConfigurationManager.AppSettings.Item("Arquivo7Zip")
                                                proc.StartInfo.UseShellExecute = False
                                                proc.StartInfo.CreateNoWindow = True
                                                proc.StartInfo.RedirectStandardOutput = False
                                                proc.Start()
                                                proc.WaitForExit(TimeOutProc)
                                                ' Aguarda a conclusão até o tempo de timeout. 
                                                Const SLEEP_AMOUNT As Integer = 100
                                                Dim elapsedTime As Integer = 0
                                                Do While Not proc.HasExited
                                                    elapsedTime += SLEEP_AMOUNT
                                                    If elapsedTime > TimeOutProc Then
                                                        'compactação com erro, segue copiando o arquivo original.
                                                        msgTransferenciasExecSucesso &= "Falha, tempo de timeout atingido ao compactar arquivo: " & ArqEmTratamentoComDir & vbNewLine
                                                        telaServico.GerarEventLog("Falha, tempo de timeout atingido ao compactar arquivo: " & ArqEmTratamentoComDir, EventLogEntryType.Warning)
                                                        oDaoSybase.GerarLogSistema(Now, "W", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Falha, tempo de timeout atingido ao compactar arquivo." & vbNewLine & ArqEmTratamentoComDir & "</Observacao>")
                                                        proc.Kill()
                                                        Exit Do
                                                    End If
                                                    Threading.Thread.Sleep(SLEEP_AMOUNT)
                                                Loop
                                                If proc.ExitCode <> 0 Then
                                                    'compactação com erro, segue copiando o arquivo original.
                                                    ArqEmTratamentoSemDir = arquivos.Name
                                                    msgTransferenciasExecSucesso &= "Falha " & proc.ExitCode.ToString & " ao compactar arquivo: " & ArqEmTratamentoComDir & " StandarError:" & proc.StandardError.ToString & " StandarOutput:" & proc.StandardOutput.ToString & vbNewLine
                                                    telaServico.GerarEventLog("Falha " & proc.ExitCode.ToString & " ao compactar arquivo: " & ArqEmTratamentoComDir & " StandarError:" & proc.StandardError.ToString & " StandarOutput:" & proc.StandardOutput.ToString, EventLogEntryType.Warning)
                                                    oDaoSybase.GerarLogSistema(Now, "W", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Falha " & proc.ExitCode.ToString & " ao compactar arquivo." & vbNewLine & ArqEmTratamentoComDir & " StandarError:" & proc.StandardError.ToString & " StandarOutput:" & proc.StandardOutput.ToString & "</Observacao>")
                                                Else
                                                    msgTransferenciasExecSucesso &= "Compactação realizada com sucesso. Arquivo: " & ArqEmTratamentoComDir & " Arquivo Compactado: " & ArqEmTratamentoSemDir & vbNewLine
                                                End If


                                                'Exemplo linha de compando para Compactar no DOS
                                                '7z.exe a -t7z back.7z C:\*
                                                'Exemplo linha de compando para Descompactar no DOS
                                                '7z.exe e back.7z -oC:\pasta
                                            End If
                                        Else
                                            'Se falhou a compactação, desconsidera e move o arquivo descompactado.
                                            'Desta forma o processo de compactação fica como sendo um opcional
                                            'O STA faz uma tentativa de compactar o arquivo.
                                            'Loga para investigação, a variavel ArqEmTratamentoSemDir já está carregada com o nome do arquivo sem o formato compactado.
                                            msgTransferenciasExecSucesso &= "Falha ao verificar compactação, arquivo: " & ArqEmTratamentoComDir & vbNewLine
                                            telaServico.GerarEventLog("Falha ao verificar compactação, arquivo: " & ArqEmTratamentoComDir, EventLogEntryType.Warning)
                                            oDaoSybase.GerarLogSistema(Now, "W", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Falha ao verificar compactação, arquivo." & vbNewLine & ArqEmTratamentoComDir & "</Observacao>")
                                        End If
                                    End If

                                    '
                                    'Copia arquivo em tratamento para o diretório de destino.
                                    '

                                    tracer = 2
                                    If CopiaArquivo(DiretorioFonte.FullName, ArqEmTratamentoSemDir, caminhoDestino, sobrescreverArquivo) Then
                                        msgTransferenciasExecSucesso &= "Copia realizada com sucesso.Origem: " & ArqEmTratamentoComDir & " Destino: " & caminhoDestino & "\" & arquivos.Name & vbNewLine
                                        'Trecho de código comentado para não sobregarregar o log no banco de dados.
                                        'arquivos.CopyTo(Path.Combine(caminhoDestino, ArqEmTratamentoSemDir), sobrescreverArquivo)
                                        'log de sucesso de copia (diretorio origem => diretorio destino)para banco de dados
                                        'If AppSettings("GeraLogSucessoBancoDados") Then
                                        '    oDaoSybase.GerarLogSistema(Now, "O", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Copia realizada com sucesso.Origem: " & ArqEmTratamentoComDir & vbNewLine & "Destino: " & caminhoDestino & "\" & arquivos.Name & "</Observacao>")
                                        'End If
                                        'log de sucesso de copia (diretorio origem => diretorio destino)para Event View
                                        'If AppSettings("GeraLogSucessoEventView") Then
                                        '    telaServico.GerarEventLog("Etapa: " & Etapa & vbNewLine & "Copia realizada com sucesso.Origem: " & ArqEmTratamentoComDir & vbNewLine & "Destino: " & caminhoDestino & "\" & arquivos.Name, EventLogEntryType.Information)
                                        'End If
                                    Else
                                        'Loga erro ao copiar arquivo.
                                        msgTransferenciasExecSucesso &= "Erro ao copiar arquivo.Origem: " & ArqEmTratamentoComDir & " Destino: " & caminhoDestino & "\" & arquivos.Name & vbNewLine
                                        telaServico.GerarEventLog("Falha ao copiar arquivo: " & ArqEmTratamentoComDir, EventLogEntryType.Warning)
                                        oDaoSybase.GerarLogSistema(Now, "W", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Arquivo em uso." & vbNewLine & ArqEmTratamentoComDir & "</Observacao>")
                                    End If

                                    '
                                    'Backup - Caso solicitado no arquivo de configuração, efetua backup do arquivo.
                                    '
                                    tracer = 3
                                    If Not caminhoBackup.Equals("") Then
                                        If CopiaArquivo(DiretorioFonte.FullName, ArqEmTratamentoSemDir, caminhoBackup, sobrescreverArquivo) Then
                                            msgTransferenciasExecSucesso &= "Copia backup realizada com sucesso. Origem: " & ArqEmTratamentoComDir & " Destino Backup: " & caminhoBackup & "\" & arquivos.Name & vbNewLine
                                            'Trecho de código comentado para não sobregarregar o log no banco de dados.
                                            'arquivos.CopyTo(Path.Combine(caminhoBackup, ArqEmTratamentoSemDir), sobrescreverArquivo)
                                            'log de sucesso de copia (diretorio origem => diretorio backup)para banco de dados
                                            'If AppSettings("GeraLogSucessoBancoDados") Then
                                            '    oDaoSybase.GerarLogSistema(Now, "O", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Copia realizada com sucesso.Origem: " & ArqEmTratamentoComDir & vbNewLine & "Destino Backup: " & caminhoBackup & "\" & arquivos.Name & "</Observacao>")
                                            'End If
                                            'log de sucesso de copia (diretorio origem => diretorio backup)para Event View
                                            'If AppSettings("GeraLogSucessoEventView") Then
                                            '    telaServico.GerarEventLog("Etapa: " & Etapa & vbNewLine & "Copia realizada com sucesso.Origem: " & ArqEmTratamentoComDir & vbNewLine & "Destino Backup: " & caminhoBackup & "\" & arquivos.Name, EventLogEntryType.Information)
                                            'End If
                                        Else
                                            msgTransferenciasExecSucesso &= "Erro ao copiar arquivo backup. Origem: " & ArqEmTratamentoComDir & " Destino Backup: " & caminhoBackup & "\" & arquivos.Name & vbNewLine
                                            telaServico.GerarEventLog("Falha ao copiar arquivo: " & ArqEmTratamentoComDir, EventLogEntryType.Warning)
                                            oDaoSybase.GerarLogSistema(Now, "W", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Arquivo em uso." & vbNewLine & ArqEmTratamentoComDir & "</Observacao>")
                                        End If
                                    Else
                                        'nada a fazer, não foi solicitado para criar backup
                                    End If

                                    '
                                    'Descompacta arquivo no destino
                                    '
                                    tracer = 4
                                    ' Se foi solicitado a descompactaçao destino no arquivo XML
                                    If DescompactaArqDestino <> "" Then
                                        Dim ArquivoCopiado As String = Path.Combine(caminhoDestino, ArqEmTratamentoSemDir)
                                        'Valida se o sistema e os parametros estão preparados para descompactação
                                        If TrataDesCompactacao(DescompactaArqDestino, ArquivoCopiado) Then
                                            'se o arquivo estiver compactado
                                            If ArquivoCompactado(ArquivoCopiado) Then
                                                'descompacta o arquivo no destino utilizando o 7zip no formato do arquivo copiado
                                                Dim proc As New Process()
                                                'variavel com a linha de comando DOS para descompactação. Ex. c:\arquivos de programas\7-Zip\7z.exe e back.7z -oC:\pasta
                                                Dim parCompact As String = " x """ & ArquivoCopiado & """ -o""" & caminhoDestino & """ -y"
                                                Dim TimeOutProc As Integer = Configuration.ConfigurationManager.AppSettings.Item("TimeoutCompactacao")
                                                'carrega argumentos para execução
                                                proc.StartInfo.Arguments = parCompact
                                                proc.StartInfo.FileName = Configuration.ConfigurationManager.AppSettings.Item("Arquivo7Zip")
                                                proc.StartInfo.UseShellExecute = False
                                                proc.StartInfo.CreateNoWindow = True
                                                proc.StartInfo.RedirectStandardOutput = False
                                                proc.Start()
                                                proc.WaitForExit(TimeOutProc)
                                                ' Aguarda a conclusão novamente até o tempo de timeout. 
                                                Const SLEEP_AMOUNT As Integer = 100
                                                Dim elapsedTime As Integer = 0
                                                Do While Not proc.HasExited
                                                    elapsedTime += SLEEP_AMOUNT
                                                    If elapsedTime > TimeOutProc Then
                                                        'compactação com erro, segue copiando o arquivo original.
                                                        msgTransferenciasExecSucesso &= "Falha, tempo de timeout atingido ao descompactar arquivo: " & ArquivoCopiado & vbNewLine
                                                        telaServico.GerarEventLog("Falha, tempo de timeout atingido ao descompactar arquivo: " & ArquivoCopiado, EventLogEntryType.Warning)
                                                        oDaoSybase.GerarLogSistema(Now, "W", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Falha, tempo de timeout atingido ao descompactar arquivo." & vbNewLine & ArquivoCopiado & "</Observacao>")
                                                        proc.Kill()
                                                        Exit Do
                                                    End If
                                                    Threading.Thread.Sleep(SLEEP_AMOUNT)
                                                Loop
                                                If proc.ExitCode <> 0 Then
                                                    'compactação com erro, segue copiando o arquivo original.
                                                    msgTransferenciasExecSucesso &= "Falha retorno " & proc.ExitCode.ToString & " ao descompactar arquivo: " & ArquivoCopiado & " StandarError:" & proc.StandardError.ToString & " StandarOutput:" & proc.StandardOutput.ToString & vbNewLine
                                                    telaServico.GerarEventLog("Falha retorno " & proc.ExitCode.ToString & " ao descompactar arquivo: " & ArquivoCopiado & " StandarError:" & proc.StandardError.ToString & " StandarOutput:" & proc.StandardOutput.ToString, EventLogEntryType.Warning)
                                                    oDaoSybase.GerarLogSistema(Now, "W", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Falha retorno " & proc.ExitCode.ToString & " ao descompactar arquivo." & vbNewLine & ArquivoCopiado & " StandarError:" & proc.StandardError.ToString & " StandarOutput:" & proc.StandardOutput.ToString & "</Observacao>")
                                                Else
                                                    'Apaga o arquivo compactado original.
                                                    If File.Exists(ArquivoCopiado) Then
                                                        File.Delete(ArquivoCopiado)
                                                    End If
                                                    msgTransferenciasExecSucesso &= "Descompactação realizada com sucesso. Arquivo: " & ArqEmTratamentoComDir & " Descompactado em: " & ArqEmTratamentoSemDir & vbNewLine
                                                End If
                                                'Exemplo linha de compando para Compactar no DOS
                                                '7z.exe a -t7z back.7z C:\*
                                                'Exemplo linha de compando para Descompactar no DOS
                                                '7z.exe x back.7z -oC:\pasta
                                            End If
                                        Else
                                            'Se falhou a verificação para descompactação, desconsidera o processo.
                                            'Desta forma o processo de descompactação fica como sendo um opcional
                                            'O STA faz uma tentativa de descompactar o arquivo.
                                            'Não ha nada a fazer aqui a variavel o arquivo já está copiado no destino.
                                            msgTransferenciasExecSucesso &= "Falha ao verificar descompactação, arquivo: " & ArquivoCopiado & vbNewLine
                                            telaServico.GerarEventLog("Falha ao verificar descompactação, arquivo: " & ArquivoCopiado, EventLogEntryType.Warning)
                                            oDaoSybase.GerarLogSistema(Now, "W", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Falha ao verificar descompactação, arquivo." & vbNewLine & ArquivoCopiado & "</Observacao>")
                                        End If
                                    End If



                                    '
                                    'Apaga arquivos tratados ( original e comprimido se existir) do diretório de origem.
                                    '
                                    tracer = 5

                                    'Se compactou apaga o arquivo compactado também
                                    If TrataCompactacao(CompactaArqOrigem, ArqEmTratamentoComDir) Then
                                        If File.Exists(ArqEmTratamentoComDir & "." & CompactaArqOrigem.ToUpper) Then
                                            File.Delete(ArqEmTratamentoComDir & "." & CompactaArqOrigem.ToUpper)
                                        End If
                                    End If
                                    'apaga arquivo original
                                    If File.Exists(ArqEmTratamentoComDir) Then
                                        File.Delete(ArqEmTratamentoComDir)
                                    End If
                                Else
                                    telaServico.GerarEventLog("Arquivo esta fora da faixa de tamanho, contate o analista para investigação. Arquivo não tratado " & vbNewLine & " Faixa solicitada no XML entre" & TamanhoInicialArq & " e " & TamanhoFinalArq & " arquivo " & ArqEmTratamentoComDir & " com tamanho encontrado:" & arquivos.Length, EventLogEntryType.Warning)
                                    oDaoSybase.GerarLogSistema(Now, "W", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Arquivo esta fora da faixa de tamanho, contate o analista para investigação. Arquivo não tratado. " & vbNewLine & " Faixa solicitada no XML entre" & TamanhoInicialArq & " e " & TamanhoFinalArq & " arquivo " & ArqEmTratamentoComDir & " com tamanho encontrado:" & arquivos.Length & "</Observacao>")
                                End If
                            Else
                                telaServico.GerarEventLog("Arquivo esta sendo usado por outro processo." & ArqEmTratamentoComDir, EventLogEntryType.Warning)
                                oDaoSybase.GerarLogSistema(Now, "W", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Arquivo esta em uso." & vbNewLine & ArqEmTratamentoComDir & "</Observacao>")
                            End If
                        Else
                            'Se o arquivo não está na mascara estipulada não deve ser tratado.
                        End If
                    Else
                        'Se o arquivo não existe, provavelmente foi excluído por outro processo e não há nada a fazer.
                    End If
                Next
            Catch ex As Exception
                telaServico.GerarEventLog("ocorreu erro ao tentar copiar arquivo da etapa:" & Etapa & " Arquivo:" & ArqEmTratamentoComDir & vbNewLine & ex.Message, EventLogEntryType.Error)
                Select Case tracer
                    Case 0 : oDaoSybase.GerarLogSistema(Now, "E", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Caminho de origem nao encontrado: " & vbNewLine & caminhoOrigem & vbNewLine & ex.Message & "</Observacao>")
                    Case 1 : oDaoSybase.GerarLogSistema(Now, "E", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Falha na Compactação do arquivo: " & vbNewLine & "Origem: " & vbNewLine & caminhoOrigem & vbNewLine & "Destino: " & vbNewLine & caminhoDestino & vbNewLine & ex.Message & "</Observacao>")
                    Case 2 : oDaoSybase.GerarLogSistema(Now, "E", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Caminho de destino nao encontrado: " & vbNewLine & "Origem: " & vbNewLine & caminhoOrigem & vbNewLine & "Destino: " & vbNewLine & caminhoDestino & vbNewLine & ex.Message & "</Observacao>")
                    Case 3 : oDaoSybase.GerarLogSistema(Now, "E", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Caminho de backup nao encontrado: " & vbNewLine & "Origem: " & vbNewLine & caminhoOrigem & vbNewLine & "Destino Backup: " & vbNewLine & caminhoBackup & vbNewLine & ex.Message & "</Observacao>")
                    Case 4 : oDaoSybase.GerarLogSistema(Now, "E", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Descompactar arquivo no destino: " & vbNewLine & ArqEmTratamentoComDir & vbNewLine & ex.Message & "</Observacao>")
                    Case 5 : oDaoSybase.GerarLogSistema(Now, "E", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Erro ao Excluir: " & vbNewLine & ArqEmTratamentoComDir & vbNewLine & ex.Message & "</Observacao>")
                End Select
            End Try

            'Tratamento de erro para excluir arquivos expirados do diretório de backup
            Try
                Dim arquivosBkp As FileInfo
                'verifica dias para excluir arquivos backup

                'se existir backup remover da pasta backup, senao remover da pasta copiada
                If Not caminhoBackup.Equals("") Then
                    Dim DiretorioBkp As New DirectoryInfo(caminhoBackup)
                    For Each arquivosBkp In DiretorioBkp.GetFiles()
                        If arquivosBkp.LastWriteTime < Now.AddDays(-DiasExclusao) Then
                            If ValidaMascara(mascara, arquivosBkp.Name) Then
                                ArqEmTratamentoComDir = DiretorioBkp.FullName & "\" & arquivosBkp.Name
                                arquivosBkp.Delete()

                                msgTransferenciasExecSucesso &= "Exclusao realizada com sucesso.Origem: " & ArqEmTratamentoComDir & vbNewLine

                                'log de sucesso de exclusao (diretorio origem)para banco de dados
                                'If AppSettings("GeraLogSucessoBancoDados") Then
                                '    oDaoSybase.GerarLogSistema(Now, "O", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Exclusao realizada com sucesso.Origem: " & ArqEmTratamentoComDir & "</Observacao>")
                                'End If
                                'log de sucesso de exclusao (diretorio origem )para Event View
                                'If AppSettings("GeraLogSucessoEventView") Then
                                '    telaServico.GerarEventLog("Etapa: " & Etapa & vbNewLine & "Exclusao realizada com sucesso.Origem: " & ArqEmTratamentoComDir, EventLogEntryType.Information)
                                'End If

                            End If
                        End If
                    Next
                Else
                    'Operação não necessária se não informou o diretório de backup não tem sentido excluir o diretorio de origem.
                    'O arquivo do diretorio de origem é movido na operação acima deixando o diretório vazio para arquivos na mascara.
                    'Arquivos fora da mascara no diretório de origem não fazem parte do escopo do programa.
                    'Dim DiretorioFonte As New System.IO.DirectoryInfo(caminhoOrigem)
                    'Dim arquivos As System.IO.FileInfo
                    'For Each arquivos In DiretorioFonte.GetFiles()
                    '    If arquivos.CreationTime < Now.AddDays(-DiasExclusao) Then
                    '        If validaMascara(mascara, arquivos.Name) Then
                    '            ArqEmTratamentoComDir = DiretorioFonte.FullName & "\" & arquivos.Name
                    '            arquivos.Delete()
                    '        End If
                    '    End If
                    'Next
                End If
            Catch ex As Exception
                telaServico.GerarEventLog("Ocorreu erro ao tentar excluir arquivo etapa:" & Etapa & " Arquivo:" & ArqEmTratamentoComDir & vbNewLine & ex.Message, EventLogEntryType.Error)
                oDaoSybase.GerarLogSistema(Now, "E", 0, 0, 0, 0, "<Etapa>" & Etapa & "</Etapa><Observacao>Ocorreu erro ao tentar excluir o seguinte arquivo:" & ArqEmTratamentoComDir & vbNewLine & ex.Message & "</Observacao>")
            End Try
        Catch ex As Exception
            telaServico.GerarEventLog("ocorreu erro ao conectar no banco de dados etapa:" & Etapa & " : " & vbNewLine & ex.Message, EventLogEntryType.Error)
        End Try

    End Function

    'function ArquivoAberto()
    'Requer : pathFile   - Caminho do arquivo
    'Ação   : retorna se arquivo esta aberto com outro processo, se sim retorna true, senao false.
    Public Function ArquivoAberto(ByVal pathFile As String) As Boolean
        Try
            Dim arquivo As New FileStream(pathFile, FileMode.Open)
            arquivo.Close()
            Return False
        Catch ex As Exception
            Return True
        End Try

    End Function

    'function CopiaArquivo()
    'Requer : FileNameFrom    - Arquivo a ser copiado com diretório
    '       : FileNameTo      - Nome do arquivo no diretório de destino
    '       : ToPathDest      - Caminho de destino
    '       : ReplaceFile     - Substituir arquivo
    'Ação   : retorna se arquivo esta aberto com outro processo, se sim retorna true, senao false.
    Public Function CopiaArquivo(ByVal PathWork As String, ByVal FileNameTo As String, ByVal ToPathDest As String, ByVal ReplaceFile As Boolean) As Boolean
        Try
            If File.Exists(Path.Combine(PathWork, FileNameTo)) Then
                'busca extensão do arquivo em tratamento.
                Dim arquivo As FileInfo = New FileInfo(Path.Combine(PathWork, FileNameTo))
                arquivo.CopyTo(Path.Combine(ToPathDest, FileNameTo), ReplaceFile)
                'copia realizada com sucesso
                Return True
            Else
                'arquivo para copiar nao existe
                Return False
            End If
        Catch ex As Exception
            'erro na copia do arquivo.
            Return False
        End Try

    End Function
    'function ArquivoCompactado()
    'Requer : ArqParaCompactar - Arquivo para validação do formato
    'Ação   : se for extensao conhecida de arquivo compactado, retorna falso para nao compactar novamente
    Public Function ArquivoCompactado(ByVal ArqCompact As String) As Boolean
        Try

            'busca extensão do arquivo em tratamento.
            Dim ArqComInf As FileInfo = New FileInfo(ArqCompact)
            Dim Extensao As String = ArqComInf.Extension
            'se for extensao conhecida de arquivo compactado, retorna falso para nao compactar novamente
            Select Case Extensao.ToUpper
                Case ".7Z"
                    'formato compactado
                    Return True
                Case ".RAR"
                    'formato compactado
                    Return True
                Case ".GZ"
                    'formato compactado
                    Return True
                Case ".BZ2"
                    'formato compactado
                    Return True
                Case ".XZ"
                    'formato compactado
                    Return True
                Case ".ZIP"
                    'formato compactado
                    Return True
                Case Else
                    'arquivo nao compactado
                    Return False
            End Select
        Catch ex As Exception
            'em caso de erro, desconsidera a compactacao.
            Return True
        End Try

    End Function
    'function TrataCompactacao()
    'Requer : TipoCompactacao   - Tipo de compactacao tratado pelo programa.
    'Ação   : retorna se o tipo informado é contemplado retorna verdadeiro, senão falso.
    Public Function TrataCompactacao(ByVal TipoCompactacao As String, ByVal ArqParaCompactar As String) As Boolean
        Dim prog7zip As String = ""
        Dim TimeOutProc As Integer = 0
        Try
            'valida se o diretório do programa 7zip é válido.
            prog7zip = Configuration.ConfigurationManager.AppSettings.Item("Arquivo7Zip")
            If prog7zip = "" Then
                'variavel com caminho do programa compactador não carregada
                Return False
            End If
            If Not File.Exists(prog7zip) Then
                'Arquivo do compactador não encontrado.
                Return False
            End If
            If Not File.Exists(ArqParaCompactar) Then
                'Arquivo para compactar não encontrado.
                Return False
            End If
            'Valida se foi informado o tempo de timeout de descompactação
            TimeOutProc = Configuration.ConfigurationManager.AppSettings.Item("TimeoutCompactacao")
            If TimeOutProc < 0 Then
                Return False
            End If
            'valida se a extensão informada é esperada pelo programa.
            'aqui são filtradas somente os tipos que o 7z.exe tem compatibilidade para compressão.
            Select Case (TipoCompactacao.ToUpper)
                Case "7Z"
                    'formato reconhecido
                    Return True
                Case "ZIP"
                    'formato reconhecido
                    Return True
                Case Else
                    'formato não reconhecido
                    Return False
            End Select

        Catch ex As Exception
            'em caso de erro, desconsidera a compactacao.
            Return False
        End Try

    End Function

    'function TrataDesCompactacao()
    'Requer : Descompactar   - Se informado SIM será efetuado o procedimento de descompactação.
    'Ação   : se o programa 7zip está configurado e foi solicitado para o arquivo retorna verdadeiro, senão falso.
    Public Function TrataDesCompactacao(ByVal Descompactar As String, ByVal ArqParaDescompactar As String) As Boolean
        Dim prog7zip As String = ""
        Dim TimeOutProc As Integer = 0
        Try
            'valida se o diretório do programa 7zip é válido.
            prog7zip = Configuration.ConfigurationManager.AppSettings.Item("Arquivo7Zip")
            If prog7zip = "" Then
                'variavel com caminho do programa compactador não carregada
                Return False
            End If
            If Not File.Exists(prog7zip) Then
                'Arquivo do compactador não encontrado.
                Return False
            End If
            If Not File.Exists(ArqParaDescompactar) Then
                'Arquivo para descompactar nao encontrado
                Return False
            End If
            'Valida se foi informado o tempo de timeout de descompactação
            TimeOutProc = Configuration.ConfigurationManager.AppSettings.Item("TimeoutCompactacao")
            If TimeOutProc < 0 Then
                Return False
            End If
            If Not File.Exists(ArqParaDescompactar) Then
                'O arquivo para descompactar nao existe.
                Return False
            End If
            'valida se a extensão informada é esperada pelo programa.
            If Descompactar.ToUpper = "SIM" Then
                'se foi informado tem que ser SIM.
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            'em caso de erro, desconsidera a compactacao.
            Return False
        End Try

    End Function

    'Sub ExcluirLog()
    'Requer :
    'Ação   : Exclui Logs do sistema
    Public Sub ExcluirLog()
        Try
            Dim odaosybase As New DAOSybase(DbStringSybaseASE, eventViewer)
            Dim numeroDias As Integer = Configuration.ConfigurationManager.AppSettings.Item("QtdDiasExcluirLog")
            If numeroDias <> 0 Then
                odaosybase.ExcluirLog(numeroDias)
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

#Region "validaMascaraSimples"
    'Function validaMascaraSimples()
    'Requer : mascara     - mascara para arquivos
    '         nomeArquivo - Nome do arquivo
    'Ação   : Valida Mascara de forma simples sendo '*' no inicio ou final do nome do arquivo ou na extensao do arquivo
    Public Function ValidaMascaraSimples(ByVal mascara As String, ByVal nomeArquivo As String) As Boolean
        Dim extensaoMsk As String
        Dim extensaoNmArq As String
        Dim separadorMsk() As String
        Dim separadorNmArq() As String
        Dim mascaraSemExts As String = ""
        Dim nomeArqSemExts As String = ""

        Try

            separadorMsk = mascara.Split(".")
            separadorNmArq = nomeArquivo.Split(".")

            extensaoMsk = separadorMsk(separadorMsk.Length - 1)
            extensaoNmArq = separadorNmArq(separadorNmArq.Length - 1)

            'verifica se copia todos os arquivos
            If mascara.Equals("*.*") Then
                Return True
            End If

            'verifica se extensoes sao iguais ou se a mascara aceita todas as extensoes
            If extensaoMsk.Equals(extensaoNmArq) Or extensaoMsk.Equals("*") Then
                For count As Integer = 0 To separadorMsk.Length - 2
                    mascaraSemExts = mascaraSemExts & separadorMsk(count)
                Next

                For count2 As Integer = 0 To separadorNmArq.Length - 2
                    nomeArqSemExts = nomeArqSemExts & separadorNmArq(count2)
                Next
                'verifica se nao possui '*'
                If Not mascaraSemExts.IndexOf("*") >= 0 Then
                    Return If(mascaraSemExts.Equals(nomeArqSemExts), True, False)
                Else
                    'se * estiver na ulitma posiçao o indexOf da mascara para o nome do arquivo deve ser 0
                    If mascaraSemExts.IndexOf("*") = mascaraSemExts.Length - 1 Then
                        'verifica se copia todos nomes de arquivos
                        If mascaraSemExts.Equals("*") Then
                            Return True
                        Else
                            Return If(nomeArqSemExts.IndexOf(mascaraSemExts.Substring(0, mascaraSemExts.Length - 2)) = 0, True, False)
                        End If
                    Else
                        Return If(nomeArqSemExts.IndexOf(mascaraSemExts.Substring(0, mascaraSemExts.Length - 2)) = nomeArqSemExts.IndexOf(nomeArqSemExts.Length - mascaraSemExts.Substring(0, mascaraSemExts.Length - 2).Length),
                            True,
                            False)

                    End If
                End If
            Else
                Return False
            End If
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Function
#End Region

    'Function validaMascara()
    'Requer : mascara     - mascara para arquivos
    '         nomeArquivo - Nome do arquivo
    'Ação   : Valida Mascara de arquivo no padrao do MSDOS 
    Public Function ValidaMascara(ByVal mascara As String, ByVal nomeArquivo As String) As Boolean
        Dim mask As New Regex("^" + mascara.Replace(".", "[.]").Replace("*", ".*").Replace("?", ".") & "$", RegexOptions.IgnoreCase)
        Return mask.IsMatch(nomeArquivo)
    End Function

    'Function validaTamanhoArq()
    'Requer : TamanhoArqInicial     - Faixa de Tamanho Inicial do Arquivo
    '         TamanhoArqFinal       - Faixa de Tamanho Final do Arquivo
    '         TamanhoArq            - Tamanho do arquivo em tratamento
    'Ação   : Valida se o arquivo em tratamento está na faixa solicitada no XML
    Public Function ValidaTamanho(ByVal TamanhoArqInicial As Long, ByVal TamanhoArqFinal As Long, ByVal TamanhoArq As Long) As Boolean
        Try
            'se nao inforou valor inicial e final
            If TamanhoArqInicial = 0 AndAlso TamanhoArqFinal = 0 Then
                'Indica que não foi solicitado no XML a validação do tamanho do arquivo.
                Return True
            Else
                'se informou somente o valor inicial
                If TamanhoArqInicial > 0 AndAlso TamanhoArqFinal = 0 Then
                    If TamanhoArq >= TamanhoArqInicial Then
                        'Tamanho do arquivo maior ou igual à faixa solicitada
                        Return True
                    Else
                        'Tamanho do arquivo menor do que a faixa de inicio, retorna erro.
                        Return False
                    End If
                Else
                    'se informou somente o valor final
                    If TamanhoArqInicial = 0 AndAlso TamanhoArqFinal > 0 Then
                        If TamanhoArq <= TamanhoArqFinal Then
                            'Tamanho do arquivo menor ou igual à faixa solicitada
                            Return True
                        Else
                            'Tamanho do arquivo maior do que a faixa de inicio, retorna erro.
                            Return False
                        End If
                    Else
                        'se inforou valor inicial e final
                        If TamanhoArqInicial > 0 AndAlso TamanhoArqFinal > 0 Then
                            If TamanhoArq >= TamanhoArqInicial AndAlso TamanhoArq <= TamanhoArqFinal Then
                                'Tamanho do arquivo na faixa solicitada
                                Return True
                            Else
                                'Tamanho do arquivo fora da faixa, retorna erro.
                                Return False
                            End If
                        Else
                            'situação não mapeada, retorna erro
                            Return False
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            'em caso de erro, retorna para falso tratamento externo.
            Return False
        End Try
    End Function

    'Function excluiArquivos()
    'Requer : caminhoBkp     - diretorio caminho backup
    '         caminhoOrigem  - diretorio caminho Origem
    '         DiasExclusao   - dias de duraçao que o arquivo pode ficar na pasta , caso seja maior exclui arquivo
    '         mascara        - mascara arquivos
    '         telaServico    - objeto tela principal
    'Ação   : Exclui arquivos conforme parametros 

    Public Function ExcluiArquivos(ByVal caminhoBkp As String,
    ByVal caminhoOrigem As String,
    ByVal DiasExclusao As Integer,
    ByVal mascara As String,
    ByRef telaServico As Service1,
    ByVal etapa As String) As Boolean

        Dim ArqEmTratamentoComDir As String = "Não mapeado"
        Dim oDaosybase As New DAOSybase(DbStringSybaseASE, eventViewer)

        Try
            'verifica dias para excluir arquivos tanto no path principal quanto no path backup se existir

            'se existir pasta backup exclui arquivos dela
            If Not caminhoBkp.Equals("") Or caminhoBkp <> String.Empty Then
                Dim arquivosBkp As FileInfo
                Dim DiretorioBkp As New DirectoryInfo(caminhoBkp)
                For Each arquivosBkp In DiretorioBkp.GetFiles()
                    If arquivosBkp.LastWriteTime < Now.AddDays(-DiasExclusao) Then
                        If ValidaMascara(mascara, arquivosBkp.Name) Then
                            ArqEmTratamentoComDir = DiretorioBkp.FullName & "\" & arquivosBkp.Name
                            arquivosBkp.Delete()

                            msgTransferenciasExecSucesso &= "Exclusao realizada com sucesso.Origem: " & ArqEmTratamentoComDir & vbNewLine

                            'log de sucesso de exclusao (diretorio origem)para banco de dados
                            'If AppSettings("GeraLogSucessoBancoDados") Then
                            '    oDaosybase.GerarLogSistema(Now, "O", 0, 0, 0, 0, "<Etapa>" & etapa & "</Etapa><Observacao>Exclusao realizada com sucesso.Origem: " & ArqEmTratamentoComDir & "</Observacao>")
                            'End If
                            'log de sucesso de exclusao (diretorio origem )para Event View
                            'If AppSettings("GeraLogSucessoEventView") Then
                            '    telaServico.GerarEventLog("Etapa: " & etapa & vbNewLine & "Exclusao realizada com sucesso.Origem: " & ArqEmTratamentoComDir, EventLogEntryType.Information)
                            'End If

                        End If
                    End If
                Next
            End If

            Dim DiretorioFonte As New DirectoryInfo(caminhoOrigem)
            Dim arquivos As FileInfo
            For Each arquivos In DiretorioFonte.GetFiles()
                If arquivos.CreationTime < Now.AddDays(-DiasExclusao) Then
                    If ValidaMascara(mascara, arquivos.Name) Then
                        ArqEmTratamentoComDir = DiretorioFonte.FullName & "\" & arquivos.Name
                        arquivos.Delete()

                        msgTransferenciasExecSucesso &= "Exclusao realizada com sucesso.Origem: " & ArqEmTratamentoComDir & vbNewLine

                        'log de sucesso de exclusao (diretorio origem)para banco de dados
                        'If AppSettings("GeraLogSucessoBancoDados") Then
                        '    oDaosybase.GerarLogSistema(Now, "O", 0, 0, 0, 0, "<Etapa>" & etapa & "</Etapa><Observacao>Exclusao realizada com sucesso.Origem: " & ArqEmTratamentoComDir & "</Observacao>")
                        'End If
                        'log de sucesso de exclusao (diretorio origem )para Event View
                        'If AppSettings("GeraLogSucessoEventView") Then
                        '    telaServico.GerarEventLog("Etapa: " & etapa & vbNewLine & "Exclusao realizada com sucesso.Origem: " & ArqEmTratamentoComDir, EventLogEntryType.Information)
                        'End If

                    End If
                End If
            Next
        Catch ex As Exception
            oDaosybase = New DAOSybase(DbStringSybaseASE, eventViewer)
            telaServico.GerarEventLog("ocorreu erro ao tentar excluir arquivo etapa:" & etapa & " Arquivo: " & ArqEmTratamentoComDir & vbNewLine & ex.Message, EventLogEntryType.Error)
            oDaosybase.GerarLogSistema(Now, "E", 0, 0, 0, 0, "<Etapa>" & etapa & "</Etapa><Observacao>ocorreu erro ao tentar excluir o seguinte arquivo:" & ArqEmTratamentoComDir & vbNewLine & ex.Message & "</Observacao>")
        End Try

    End Function

    'Public Sub EnviarEmail(ByVal Remetente As String, ByVal Destinatario As String, ByVal msg As String)

    'End Sub

#End Region

End Class
