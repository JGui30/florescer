# Florescer

Sistema de agendamento para centro de beleza e estética.
Trabalho de Conclusão de Curso — Sistemas de Informação.

---

## O que é

Aplicação web responsiva com dois ambientes de uso:

- **Cliente** — escolhe procedimento, profissional e horário, confirma o
  agendamento e acompanha seu histórico.
- **Administrativo** — gerencia agenda, equipe, catálogo e controle
  financeiro.

## Estrutura

```
florescer/
├── florescer-api/     back-end em C# (.NET 8 + Entity Framework Core)
└── florescer-web/     front-end em React (Vite)
```

## Tecnologias

| Camada | Tecnologia |
|---|---|
| Front-end | React 18 + Vite |
| Back-end | ASP.NET Core 8 (Web API) |
| Acesso a dados | Entity Framework Core 8 |
| Banco | SQL Server Express / Azure SQL |
| Autenticação | JWT + PBKDF2 |
| Testes | xUnit |

---

## Pré-requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18 ou superior](https://nodejs.org/)
- SQL Server Express, ou qualquer instância local do SQL Server

---

## Como executar

São dois processos, em terminais separados. **A API precisa estar no ar antes
do front.**

### 1. Back-end

```bash
dotnet run --project florescer-api/src/Florescer.API
```

Sobe em `http://localhost:5080`, com a documentação da API em `/swagger`.

Na primeira execução o banco é criado, as migrations são aplicadas e os dados
iniciais são carregados automaticamente. Não é preciso rodar nenhum script.

A conexão padrão é `localhost\SQLEXPRESS` com autenticação integrada do
Windows. Para apontar para outra instância, altere `ConnectionStrings:Local`
em `florescer-api/src/Florescer.API/appsettings.json`.

### 2. Front-end

```bash
npm install --prefix florescer-web
```

```bash
npm run dev --prefix florescer-web
```

| Tela | Endereço |
|---|---|
| Aplicativo da cliente | `http://localhost:5173/` |
| Painel administrativo | `http://localhost:5173/admin` |

### Acesso

| Perfil | Usuário | Senha |
|---|---|---|
| Administrador | `admin@florescer.com.br` | `Florescer@2025` |
| Cliente de exemplo | `renata.cliente@email.com` | `Cliente@123` |

Novas contas de cliente podem ser criadas pela própria tela de cadastro.

---

## Testes

```bash
dotnet test florescer-api
```

---

## Configuração

| Arquivo | Quando é usado |
|---|---|
| `appsettings.json` | Base, comum aos dois ambientes |
| `appsettings.Development.json` | `dotnet run` |
| `appsettings.Production.json` | Servidor de hospedagem |
| `.env.development` | `npm run dev` |
| `.env.production` | `npm run build` |

A chave `BancoAtivo` escolhe entre o banco local e o da nuvem.

**Nenhum segredo é versionado.** Em produção, a senha do banco, a chave JWT e
as credenciais de e-mail chegam por variáveis de ambiente definidas no
servidor, que sobrescrevem os campos vazios de `appsettings.Production.json`.

O envio de e-mail é opcional: com os campos de SMTP vazios, as mensagens são
apenas registradas no banco e ficam visíveis no painel administrativo, sem
sair pela internet.

---

## Documentação técnica

| Documento | Conteúdo |
|---|---|
| [DICIONARIO-DADOS.md](DICIONARIO-DADOS.md) | Tabelas e colunas, com significado e consultas de exemplo |
| [florescer-api/MODELAGEM-BANCO.md](florescer-api/MODELAGEM-BANCO.md) | Modelo de dados e justificativa de cada relacionamento |

## Documentação da API

A especificação OpenAPI gerada pelo projeto é publicada junto com o
front-end, em `florescer-web/public/api-docs/`. Com a aplicação no ar, a
documentação fica em `/api-docs`: 35 rotas agrupadas por assunto, com
parâmetros, formatos de resposta e códigos de erro.

Em desenvolvimento, o Swagger interativo continua disponível em
`http://localhost:5080/swagger` enquanto a API estiver rodando.
