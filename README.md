# GTA Connect

> Rede social dedicada a jogadores de **GTA Online** (PS5/PS4) — feita pra resolver a dor real de quem joga: achar parceiros **confiáveis**, não só disponíveis.

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![Angular 22](https://img.shields.io/badge/Angular-22-DD0031?logo=angular&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-real--time-informational)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-v4-06B6D4?logo=tailwindcss&logoColor=white)

Projeto solo, construído do zero: backend em .NET com Clean Architecture, frontend Angular com signals, tempo real via SignalR, e um sistema de reputação pensado pra resolver um problema real do universo GTA Online — randoms que abandonam heist no meio, jogadores despreparados, sem nenhum jeito de saber com quem vale a pena jogar antes de tentar.

## Por que esse projeto existe

GTA Online tem uma comunidade gigante em Discord/Reddit pra achar parceiros de jogo, mas nenhuma delas resolve o problema de verdade: **não existe reputação portátil ligada a sessões reais**. Você entra num grupo, não sabe se a pessoa vai completar o heist ou sumir no meio, e da próxima vez começa do zero — sem histórico, sem sinal de confiança.

O GTA Connect ataca isso diretamente: conexões (tipo pedido de amizade) só existem entre quem aceitou jogar junto, sessões de jogo são registradas, e cada avaliação carrega sinais estruturados de confiabilidade (terminou a sessão? sabia jogar? foi tóxico?) — não só uma nota solta. Esses sinais alimentam um **score de compatibilidade calculado no banco**, não em memória, que ordena a busca por quem realmente vale a pena conectar.

## Destaques técnicos

- **Score de compatibilidade 100% em SQL.** A busca de jogadores não ordena por "mais recente" — ordena por um score (confiabilidade + afinidade de estilo de jogo + disponibilidade + região) computado via subquery correlacionada e `CASE WHEN` por bit de tag, verificado no log de SQL gerado pelo EF Core pra garantir zero client-side evaluation.
- **Uma conexão SignalR, três propósitos.** O mesmo hub autenticado por JWT entrega chat 1:1, notificações in-app e indicador de presença ("online agora") em tempo real — sem abrir um hub novo pra cada feature.
- **Reputação por sessão, não por impressão geral.** Avaliar alguém exige ter registrado uma sessão de jogo real contra uma conexão aceita; reavaliar a mesma sessão faz upsert (não duplica), permitindo medir "completou 12 de 12 sessões" em vez de uma nota única sobrescrita a cada vez.
- **Clean Architecture de verdade.** `Domain` sem dependência nenhuma de infraestrutura, entidades ricas com factory methods e invariantes protegidas (não anemic model), `Application` orquestrando via services (sem MediatR/CQRS — decisão consciente pro tamanho do projeto), EF Core isolado na `Infrastructure`.
- **i18n ponta a ponta.** Não é só o frontend — mensagens de erro do backend (validação, Identity, exceções de domínio) são localizadas via `Accept-Language` com recurso compartilhado, então um erro de validação chega traduzido pro cliente sem tradução manual em cada endpoint.
- **Moderação com roles reais.** Painel de revisão de denúncias protegido por role de verdade do ASP.NET Identity (não uma lista de e-mail solta), com banimento reaproveitando o lockout nativo do Identity.
- **~200 testes automatizados** no backend (xUnit, Domain + Application), cobrindo entidades, validators e services.

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET 10 · Clean Architecture (`Domain` / `Application` / `Infrastructure` / `Api`) · ASP.NET Core Identity + JWT · EF Core |
| Frontend | Angular 22 (standalone components, signals) · Tailwind CSS v4 · ngx-translate · Angular CDK |
| Tempo real | SignalR (chat, notificações, presença) |
| Banco | SQL Server 2022 (Docker) |
| E-mail (dev) | MailKit + Mailpit |

## Funcionalidades

- **Perfil de jogador** — estilo de jogo, disponibilidade, região, horas jogadas, foto, tudo como tags estruturadas (não texto livre) pra alimentar busca e matching.
- **Busca por compatibilidade** — ordenada por score calculado no banco, com o "porquê" do match visível ("🎯 N em comum").
- **Chat 1:1 em tempo real** e **feed social** com fotos, curtidas e filtro por conexões.
- **Conexões + reputação por sessão** — pedido/aceite mútuo, registro de sessão jogada, avaliação com sinais de confiabilidade.
- **Notificações in-app** em tempo real e **indicador de presença** ("online agora").
- **Segurança**: verificação de e-mail, bloqueio/denúncia, painel de moderação com banimento.
- **Onboarding guiado** e **barra de completude de perfil**, com pesos alinhados ao que realmente importa pro matching.
- **pt-BR / en** com troca de idioma em tempo real, sem reload.

Visão completa de produto (contexto, decisões e histórico de evolução) em [PROJETO.md](PROJETO.md).

## Rodando localmente

Pré-requisitos: .NET SDK 10+, Node.js 24+/npm, Docker Desktop, `dotnet-ef` (`dotnet tool install --global dotnet-ef`).

### 1. Banco de dados

```bash
cp .env.example .env   # ajuste a senha se quiser
docker compose up -d   # SQL Server 2022 + Mailpit (SMTP fake em localhost:8025)
```

### 2. Backend

```bash
cd backend/src/GtaConnect.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=GtaConnect;User Id=sa;Password=<definida-no-seu-env>;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:SecretKey" "<gere-uma-chave-aleatoria-de-32-mais-caracteres>"
```

Aplicar as migrations (primeira vez ou após novas migrations):

```bash
cd backend
dotnet ef database update --project src/GtaConnect.Infrastructure --startup-project src/GtaConnect.Api
```

Rodar a API:

```bash
cd backend/src/GtaConnect.Api
dotnet run --urls "http://localhost:5080"
```

Swagger UI: http://localhost:5080/swagger — Health check: http://localhost:5080/health

Outras chaves de configuração (`Email:*`, `FrontendUrl`, `ModeratorEmails`) ficam em `appsettings.Development.json`; para produção, todas precisam vir de configuração/segredo — ver `appsettings.json` pra lista completa.

### 3. Frontend

```bash
cd frontend
npm install
npx ng serve
```

App em http://localhost:4200 (chamadas a `/api/*` são redirecionadas para a API via `proxy.conf.json`).

### 4. Dados de teste (dev, opcional)

Com a API rodando em `Development`, popula o banco com ~24 perfis de teste (tags de estilo/disponibilidade, região, bio e horas jogadas sorteadas — dá pra exercitar busca, score de compatibilidade e barra de completude sem criar dado manual):

```bash
curl -X POST http://localhost:5080/api/dev/seed
```

Idempotente (rodar de novo só completa o que falta, não duplica). Só funciona em `Development` — fora disso a rota devolve 404. Os e-mails seguem o padrão `seed.*@gtaconnect.local`, senha `Seed123!@#`, fácil de reconhecer/limpar do banco.

## Testes

```bash
# Backend
cd backend && dotnet test

# Frontend
cd frontend && npx ng test --watch=false
```

## Status do projeto

MVP completo e funcional em ambiente local (perfil, busca, chat, feed, conexões, reputação, moderação — ver seção 6 do [PROJETO.md](PROJETO.md)). Em evolução ativa: design system/acessibilidade e "dar vida ao app" (seed, onboarding, completude) já entregues; produção/infraestrutura (Docker, CI/CD, deploy) e modelo de ML próprio pra reputação são os próximos passos — detalhes em [PROJETO.md](PROJETO.md), seção 13.

---

**GTA Connect** é um projeto pessoal de portfólio. O código está público para fins de estudo e avaliação técnica — todos os direitos reservados. Uso, cópia, modificação ou redistribuição sem autorização expressa do autor não são permitidos. © 2026 Maicon Rodrigues.
