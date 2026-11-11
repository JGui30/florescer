# Dicionário de dados

Estrutura completa do banco Florescer: cada tabela, cada coluna, o que
significa e como se relaciona com as demais.

Extraído diretamente do banco em execução. Complementa o
[MODELAGEM-BANCO.md](florescer-api/MODELAGEM-BANCO.md), que explica **por que**
cada decisão foi tomada — aqui está **o que** existe.

**15 tabelas**, mais a `__EFMigrationsHistory`, controlada pelo Entity Framework
para saber quais migrations já foram aplicadas.

---

## Como ler

- `NOT NULL` — preenchimento obrigatório
- `IDENTITY` — numeração automática do banco
- `nvarchar(120)` — texto de até 120 caracteres, com suporte a acentos
- `datetime2` — data e hora
- `decimal(10,2)` — número exato com 2 casas, usado em dinheiro
- `bit` — verdadeiro ou falso
- `int` com legenda — código numérico; a tabela de valores vem logo abaixo

---

# Bloco 1 — Acesso

## Usuarios

Clientes e administradores na mesma tabela, diferenciados pelo `Perfil`.

| Coluna | Tipo | Nulo | Significado |
|---|---|---|---|
| `Id` | int IDENTITY | não | Chave primária |
| `Nome` | nvarchar(120) | não | Nome completo |
| `Email` | nvarchar(160) | não | **Único.** Identifica o login |
| `Telefone` | nvarchar(20) | sim | Usado na recuperação por SMS/WhatsApp |
| `SenhaHash` | nvarchar(300) | não | Formato `salt.hash` em Base64. A senha real nunca é gravada |
| `Perfil` | int | não | 1 = Cliente · 2 = Administrador |
| `Ativo` | bit | não | Permite desativar sem excluir |
| `CriadoEm` | datetime2 | não | Data do cadastro |

**Índices:** PK em `Id`, **único** em `Email`.

## CodigosRecuperacao

Código de 6 dígitos para quem esqueceu a senha.

| Coluna | Tipo | Nulo | Significado |
|---|---|---|---|
| `Id` | int IDENTITY | não | Chave primária |
| `UsuarioId` | int | não | → `Usuarios.Id` (cascata) |
| `Codigo` | nvarchar(6) | não | Os seis dígitos sorteados |
| `Canal` | int | não | 1 = Email · 2 = SMS · 3 = WhatsApp |
| `ExpiraEm` | datetime2 | não | Validade de 15 minutos |
| `UsadoEm` | datetime2 | sim | Preenchido no uso; impede reutilização |
| `Tentativas` | int | não | Invalida o código após 5 erros |
| `CriadoEm` | datetime2 | não | Momento da geração |

**Índices:** PK em `Id`, índice em (`UsuarioId`, `Codigo`).

---

# Bloco 2 — Catálogo

## Categorias

Unhas, Cabelo, Spa e Estética.

| Coluna | Tipo | Nulo | Significado |
|---|---|---|---|
| `Id` | int IDENTITY | não | Chave primária |
| `Nome` | nvarchar(60) | não | Nome exibido |
| `Slug` | nvarchar(30) | não | **Único.** Identificador do front (`unhas`, `cabelo`) |
| `Cor` | nvarchar(9) | não | Hexadecimal usado nos gráficos |
| `ImagemUrl` | nvarchar(300) | sim | Foto exibida à cliente na escolha do serviço. Nula = usa o círculo colorido |
| `Ordem` | int | não | Sequência de exibição |
| `Ativa` | bit | não | Liga/desliga a categoria |

## Procedimentos

| Coluna | Tipo | Nulo | Significado |
|---|---|---|---|
| `Id` | int IDENTITY | não | Chave primária |
| `CategoriaId` | int | não | → `Categorias.Id` (restrito) |
| `Nome` | nvarchar(120) | não | Nome do serviço |
| `Descricao` | nvarchar(400) | sim | Texto de apoio na vitrine |
| `DuracaoMinutos` | int | não | **Base do cálculo de disponibilidade** |
| `Preco` | decimal(10,2) | não | Preço de tabela atual |
| `Ativo` | bit | não | Exclusão lógica |

---

# Bloco 3 — Equipe e agenda

## Profissionais

| Coluna | Tipo | Nulo | Significado |
|---|---|---|---|
| `Id` | int IDENTITY | não | Chave primária |
| `Nome` | nvarchar(120) | não | Nome |
| `Especialidade` | nvarchar(60) | não | Manicure, Cabeleireira, Esteticista |
| `Email` | nvarchar(160) | sim | Contato |
| `Telefone` | nvarchar(20) | sim | Contato |
| `FotoUrl` | nvarchar(300) | sim | Foto na vitrine |
| `Ativa` | bit | não | Se recebe novos agendamentos |
| `CriadoEm` | datetime2 | não | Data do cadastro |

## ProfissionaisProcedimentos

Tabela associativa que resolve o N:N. É ela que garante a regra
*"cada profissional realiza apenas os procedimentos cadastrados para ela"*.

| Coluna | Tipo | Nulo | Significado |
|---|---|---|---|
| `ProfissionalId` | int | não | → `Profissionais.Id` (cascata) |
| `ProcedimentoId` | int | não | → `Procedimentos.Id` (cascata) |

**Chave primária composta** pelas duas colunas: o par não pode se repetir.

## HorariosTrabalho

Jornada semanal. Uma linha por dia trabalhado — **dia sem linha significa folga**.

| Coluna | Tipo | Nulo | Significado |
|---|---|---|---|
| `Id` | int IDENTITY | não | Chave primária |
| `ProfissionalId` | int | não | → `Profissionais.Id` (cascata) |
| `DiaSemana` | int | não | 0 = domingo · 1 = segunda · … · 6 = sábado |
| `HoraInicio` | time | não | Início do expediente |
| `HoraFim` | time | não | Fim do expediente |

**Índices:** PK em `Id`, índice em (`ProfissionalId`, `DiaSemana`) — consultado
a cada cálculo de disponibilidade.

## Bloqueios

Indisponibilidades que se sobrepõem à jornada normal.

| Coluna | Tipo | Nulo | Significado |
|---|---|---|---|
| `Id` | int IDENTITY | não | Chave primária |
| `ProfissionalId` | int | **sim** | → `Profissionais.Id`. **Nulo = vale para todo o estabelecimento** |
| `Inicio` | datetime2 | não | Começo do bloqueio |
| `Fim` | datetime2 | não | Fim do bloqueio |
| `Tipo` | int | não | 1 = Folga · 2 = Férias · 3 = Feriado · 4 = Bloqueio pontual |
| `Motivo` | nvarchar(200) | sim | Observação |
| `CriadoEm` | datetime2 | não | Registro |

---

# Bloco 4 — Operação

## Agendamentos

Entidade central do sistema.

| Coluna | Tipo | Nulo | Significado |
|---|---|---|---|
| `Id` | int IDENTITY | não | Chave primária |
| `ClienteId` | int | não | → `Usuarios.Id` (restrito) |
| `ProfissionalId` | int | não | → `Profissionais.Id` (restrito) |
| `ProcedimentoId` | int | não | → `Procedimentos.Id` (restrito) |
| `Inicio` | datetime2 | não | Início do atendimento |
| `Fim` | datetime2 | não | Fim, calculado pela duração. Guardado para simplificar a detecção de conflito |
| `Status` | int | não | 1 = Agendado · 2 = Confirmado · 3 = Concluído · 4 = Cancelado |
| `ValorPrevisto` | decimal(10,2) | não | **Cópia do preço na hora da reserva.** Preserva o histórico se a tabela mudar |
| `PrazoConfirmacao` | datetime2 | não | Limite para o cliente confirmar (regra 24h/2h) |
| `ConfirmadoEm` | datetime2 | sim | Quando o cliente confirmou |
| `ConcluidoEm` | datetime2 | sim | Quando o admin deu baixa |
| `CanceladoEm` | datetime2 | sim | Quando foi cancelado |
| `MotivoCancelamento` | int | sim | 1 = Cliente · 2 = Administrador · 3 = Falta de confirmação · 4 = Não compareceu |
| `Observacoes` | nvarchar(300) | sim | Anotações |
| `CriadoEm` | datetime2 | não | Data da reserva |

**Índices:** PK em `Id`; (`ProfissionalId`, `Inicio`) — a consulta mais
frequente do sistema; `Inicio`; `Status`; `ClienteId`; `ProcedimentoId`.

### Ciclo de vida

```
                      confirma
   [Agendado] ─────────────────────▶ [Confirmado]
       │                                   │
       │ prazo vence                       │ admin dá baixa
       ▼                                   ▼
  [Cancelado]                        [Concluído] ──▶ gera LancamentoFinanceiro
  motivo = 3
```

## LancamentosFinanceiros

Receita apurada. **Único ponto do sistema em que uma receita nasce.**

| Coluna | Tipo | Nulo | Significado |
|---|---|---|---|
| `Id` | int IDENTITY | não | Chave primária |
| `Tipo` | int | não | 1 = Receita · 2 = Despesa (previsto, ainda não usado) |
| `AgendamentoId` | int | sim | **Único.** → `Agendamentos.Id` (cascata). Relação 1:1 |
| `Valor` | decimal(10,2) | não | Valor efetivamente cobrado |
| `Data` | datetime2 | não | **Data de competência**: o dia do atendimento |
| `Descricao` | nvarchar(200) | não | Ex.: "Manicure — Renata Alves" |
| `RegistradoPorUsuarioId` | int | sim | → `Usuarios.Id` (restrito). Quem deu a baixa |
| `CriadoEm` | datetime2 | não | Momento do lançamento |

**Índices:** PK em `Id`, **único** em `AgendamentoId` (garante o 1:1),
índice em `Data`.

## Notificacoes

Toda mensagem gerada pelo sistema, inclusive as simuladas.

| Coluna | Tipo | Nulo | Significado |
|---|---|---|---|
| `Id` | int IDENTITY | não | Chave primária |
| `UsuarioId` | int | não | → `Usuarios.Id` (cascata). Destinatário |
| `AgendamentoId` | int | sim | → `Agendamentos.Id` (cascata) |
| `Canal` | int | não | 1 = Email · 2 = SMS · 3 = WhatsApp |
| `Tipo` | int | não | Ver tabela abaixo |
| `Destino` | nvarchar(160) | não | E-mail ou telefone |
| `Assunto` | nvarchar(160) | não | Assunto da mensagem |
| `Mensagem` | nvarchar(1200) | não | Texto completo |
| `Status` | int | não | 1 = Pendente · 2 = Enviada · 3 = Falhou · 4 = Simulada |
| `Erro` | nvarchar(400) | sim | Detalhe da falha |
| `EnviadaEm` | datetime2 | sim | Momento do envio |
| `CriadaEm` | datetime2 | não | Momento da geração |

### Valores de `Tipo`

| Valor | Significado |
|---|---|
| 1 | Agendamento criado |
| 2 | Lembrete de confirmação |
| 3 | Agendamento confirmado |
| 4 | Lembrete de atendimento (véspera) |
| 5 | Cancelado por falta de confirmação |
| 6 | Agendamento cancelado |
| 7 | Agendamento remarcado |
| 8 | Código de recuperação |

**Índices:** PK em `Id`, índice em (`AgendamentoId`, `Tipo`) — evita enviar o
mesmo lembrete duas vezes; índice em `UsuarioId`.

---

# Bloco 5 — Divulgação

## SecoesLanding

| Coluna | Tipo | Nulo | Significado |
|---|---|---|---|
| `Id` | int IDENTITY | não | Chave primária |
| `Chave` | nvarchar(40) | não | **Único.** `hero`, `diferenciais`, `servicos`, `equipe`, `galeria`, `depoimentos`, `faq`, `cta` |
| `Titulo` | nvarchar(160) | não | Título da seção |
| `Subtitulo` | nvarchar(300) | sim | Texto de apoio |
| `Texto` | nvarchar(2000) | sim | Corpo, quando houver |
| `TextoBotao` | nvarchar(60) | sim | Rótulo do botão |
| `ImagemUrl` | nvarchar(300) | sim | Imagem de destaque |
| `Ordem` | int | não | Posição na página |
| `Ativa` | bit | não | Exibe ou esconde |

## MidiasLanding

| Coluna | Tipo | Nulo | Significado |
|---|---|---|---|
| `Id` | int IDENTITY | não | Chave primária |
| `SecaoLandingId` | int | sim | → `SecoesLanding.Id` (`SET NULL`) |
| `Url` | nvarchar(300) | não | Caminho em `wwwroot/uploads` |
| `Legenda` | nvarchar(160) | sim | Texto da imagem |
| `Ordem` | int | não | Posição na galeria |
| `Ativa` | bit | não | Exibe ou esconde |
| `CriadaEm` | datetime2 | não | Data do envio |

## Depoimentos

| Coluna | Tipo | Nulo | Significado |
|---|---|---|---|
| `Id` | int IDENTITY | não | Chave primária |
| `NomeCliente` | nvarchar(120) | não | Quem deu o depoimento |
| `Texto` | nvarchar(500) | não | Conteúdo |
| `Nota` | int | não | De 1 a 5 estrelas |
| `Servico` | nvarchar(120) | sim | Procedimento realizado |
| `Ordem` | int | não | Posição |
| `Ativo` | bit | não | Publicado ou não |

## Faqs

| Coluna | Tipo | Nulo | Significado |
|---|---|---|---|
| `Id` | int IDENTITY | não | Chave primária |
| `Pergunta` | nvarchar(200) | não | Pergunta |
| `Resposta` | nvarchar(1000) | não | Resposta |
| `Ordem` | int | não | Posição |
| `Ativa` | bit | não | Publicada ou não |

## ConfiguracaoEstabelecimento

Tabela de **registro único** com os dados institucionais.

| Coluna | Tipo | Nulo | Significado |
|---|---|---|---|
| `Id` | int IDENTITY | não | Chave primária |
| `Nome` | nvarchar(120) | não | Nome do estabelecimento |
| `Slogan` | nvarchar(200) | sim | Frase de apoio |
| `Telefone` | nvarchar(20) | sim | Telefone fixo |
| `WhatsApp` | nvarchar(20) | sim | Número com DDI, para o link `wa.me` |
| `Email` | nvarchar(160) | sim | Contato |
| `Endereco` | nvarchar(300) | sim | Endereço completo |
| `HorarioFuncionamento` | nvarchar(200) | sim | Texto livre |
| `InstagramUrl` | nvarchar(300) | sim | Rede social |
| `FacebookUrl` | nvarchar(300) | sim | Rede social |
| `MapaUrl` | nvarchar(600) | sim | Mapa incorporado |

---

# Relacionamentos

| Origem | Destino | Ao excluir o pai |
|---|---|---|
| `Agendamentos.ClienteId` | `Usuarios.Id` | **Restrito** |
| `Agendamentos.ProfissionalId` | `Profissionais.Id` | **Restrito** |
| `Agendamentos.ProcedimentoId` | `Procedimentos.Id` | **Restrito** |
| `Procedimentos.CategoriaId` | `Categorias.Id` | **Restrito** |
| `LancamentosFinanceiros.RegistradoPorUsuarioId` | `Usuarios.Id` | **Restrito** |
| `LancamentosFinanceiros.AgendamentoId` | `Agendamentos.Id` | Cascata |
| `Bloqueios.ProfissionalId` | `Profissionais.Id` | Cascata |
| `HorariosTrabalho.ProfissionalId` | `Profissionais.Id` | Cascata |
| `CodigosRecuperacao.UsuarioId` | `Usuarios.Id` | Cascata |
| `Notificacoes.UsuarioId` | `Usuarios.Id` | Cascata |
| `Notificacoes.AgendamentoId` | `Agendamentos.Id` | Cascata |
| `ProfissionaisProcedimentos.ProfissionalId` | `Profissionais.Id` | Cascata |
| `ProfissionaisProcedimentos.ProcedimentoId` | `Procedimentos.Id` | Cascata |
| `MidiasLanding.SecaoLandingId` | `SecoesLanding.Id` | Anula o campo |

**Critério:** *Cascata* quando o filho não faz sentido sozinho (um horário de
trabalho sem profissional). *Restrito* quando apagar destruiria histórico — e é
por isso que o sistema desativa em vez de excluir usuários, profissionais e
procedimentos.

## Restrições de unicidade

| Tabela | Coluna | Garante |
|---|---|---|
| `Usuarios` | `Email` | Um cadastro por e-mail |
| `Categorias` | `Slug` | Identificador estável para o front |
| `SecoesLanding` | `Chave` | Uma seção por posição da página |
| `LancamentosFinanceiros` | `AgendamentoId` | Um único lançamento por atendimento |
| `ProfissionaisProcedimentos` | (`ProfissionalId`, `ProcedimentoId`) | Vínculo não duplicado |

---

# Consultas úteis

Agenda de um dia:

```sql
SELECT a.Inicio, u.Nome AS Cliente, p.Nome AS Profissional,
       pr.Nome AS Procedimento, a.ValorPrevisto,
       CASE a.Status WHEN 1 THEN 'Agendado' WHEN 2 THEN 'Confirmado'
                     WHEN 3 THEN 'Concluído' ELSE 'Cancelado' END AS Status
FROM Agendamentos a
JOIN Usuarios u       ON u.Id  = a.ClienteId
JOIN Profissionais p  ON p.Id  = a.ProfissionalId
JOIN Procedimentos pr ON pr.Id = a.ProcedimentoId
WHERE CAST(a.Inicio AS DATE) = CAST(GETDATE() AS DATE)
ORDER BY a.Inicio;
```

Faturamento por mês:

```sql
SELECT YEAR(Data) AS Ano, MONTH(Data) AS Mes,
       COUNT(*) AS Atendimentos, SUM(Valor) AS Receita,
       AVG(Valor) AS TicketMedio
FROM LancamentosFinanceiros
WHERE Tipo = 1
GROUP BY YEAR(Data), MONTH(Data)
ORDER BY Ano, Mes;
```

Perda por falta de confirmação:

```sql
SELECT COUNT(*) AS Cancelados, SUM(ValorPrevisto) AS ValorPerdido
FROM Agendamentos
WHERE Status = 4 AND MotivoCancelamento = 3;
```

Procedimentos que cada profissional realiza:

```sql
SELECT p.Nome AS Profissional, COUNT(*) AS Habilitados
FROM ProfissionaisProcedimentos pp
JOIN Profissionais p ON p.Id = pp.ProfissionalId
GROUP BY p.Nome
ORDER BY p.Nome;
```
