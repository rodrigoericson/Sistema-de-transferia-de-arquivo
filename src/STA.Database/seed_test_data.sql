-- Seed: dados de teste para validação local do Worker
-- Pré-requisito: sistema 'STA' (cn_sistema=1) já existe no banco
-- Executar uma vez: docker exec postgres-lab psql -U postgres -d sta -f /path/to/seed_test_data.sql

-- Etapas
INSERT INTO sta.tbl_etapa_transferencia (cn_sistema, nm_etapa, fl_ativo, nr_ordem_execucao, dt_criacao) VALUES
(1, 'Assessoria GRB - Envio Carga', true, 1, NOW()),
(1, 'Assessoria CRC - Envio Carga', true, 2, NOW()),
(1, 'Cobrança - Envio', true, 3, NOW()),
(1, 'Cobrança - Retorno', true, 4, NOW()),
(1, 'Teste Local - Transferência Simples', true, 5, NOW());

-- Rotas (1 por etapa)
INSERT INTO sta.tbl_rota_transferencia (cn_etapa, nr_ordem, ds_diretorio_origem, ds_diretorio_backup, ds_mascara_arquivo, ds_compacta_origem_tipo, nr_dias_excluir, nr_tamanho_inicial_bytes, nr_tamanho_final_bytes, fl_excluir_origem, fl_ativo) VALUES
(1, 1, 'F:/Git/STA/test-data/assessoria-grb/origem', 'F:/Git/STA/test-data/assessoria-grb/backup', '*.REM', NULL, 30, 0, 0, true, true),
(2, 1, 'F:/Git/STA/test-data/assessoria-crc/origem', 'F:/Git/STA/test-data/assessoria-crc/backup', '*.TXT', NULL, 30, 0, 0, true, true),
(3, 1, 'F:/Git/STA/test-data/cobranca-envio/origem', 'F:/Git/STA/test-data/cobranca-envio/backup', '*.DAT', NULL, 30, 0, 0, true, true),
(4, 1, 'F:/Git/STA/test-data/cobranca-retorno/origem', 'F:/Git/STA/test-data/cobranca-retorno/backup', '*.DAT', NULL, 30, 0, 0, true, true),
(5, 1, 'F:/Git/STA/test-data/origem', 'F:/Git/STA/test-data/backup', '*.txt', NULL, 30, 0, 0, true, true);

-- Destinos (fan-out para assessoria)
INSERT INTO sta.tbl_rota_destino (cn_rota, nr_ordem, ds_diretorio_destino, ds_descompacta_destino, fl_ativo) VALUES
-- GRB: fan-out para 3 destinos
(1, 1, 'F:/Git/STA/test-data/assessoria-grb/destino1', NULL, true),
(1, 2, 'F:/Git/STA/test-data/assessoria-grb/destino2', NULL, true),
(1, 3, 'F:/Git/STA/test-data/assessoria-grb/destino3', NULL, true),
-- CRC: fan-out para 2 destinos
(2, 1, 'F:/Git/STA/test-data/assessoria-crc/destino1', NULL, true),
(2, 2, 'F:/Git/STA/test-data/assessoria-crc/destino2', NULL, true),
-- Cobrança Envio: 1 destino
(3, 1, 'F:/Git/STA/test-data/cobranca-envio/destino', NULL, true),
-- Cobrança Retorno: 1 destino com descompactação
(4, 1, 'F:/Git/STA/test-data/cobranca-retorno/destino', 'SIM', true),
-- Teste local: 1 destino
(5, 1, 'F:/Git/STA/test-data/destino', NULL, true);
