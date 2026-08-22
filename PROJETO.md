# Visão do Projeto

> Documento de premissas iniciais. Base para todas as decisões futuras de produto, design e arquitetura. Atualizar conforme o projeto evoluir.

**Data de criação:** 2026-08-05
**Última atualização:** 2026-08-20
**Nome do app:** ainda não definido (placeholder: usar "GTA Connect" enquanto não houver nome oficial)

---

## 1. O que é o projeto

Uma **rede social dedicada a jogadores de GTA Online**, para PS5/PS4, com objetivo de conectar e permitir que jogadores se conheçam de verdade — não apenas um "buscador de grupo" para uma missão específica, mas um espaço social com perfil, feed e mensagens, focado inteiramente no universo GTA.

Pensado para funcionar hoje com **GTA V** e já preparado, na arquitetura, para incorporar **GTA VI** quando lançar, sem precisar refazer o produto.

## 2. Problema que resolve

Hoje, jogadores usam Discord, Reddit e grupos de WhatsApp para achar parceiros de GTA Online — ambientes genéricos, com ruído, sem estrutura voltada ao jogo, e sem um jeito confiável de saber se vale a pena jogar com alguém. O app propõe um espaço **dedicado, organizado e com identidade própria** para esse público, tirando o atrito de "achar gente boa pra jogar" de ferramentas que não foram feitas pra isso.

*(Diferencial ainda em aberto — ver seção 12, "Pontos em aberto".)*

## 3. Plataformas de jogo do público-alvo

- **PS5 / PS4** — foco exclusivo inicial. Não considerar PC/Xbox no MVP.

## 4. Plataforma do app (onde o app roda)

- **Web app responsivo**, funcionando bem tanto em desktop quanto em celular, independente do tamanho de tela. Não é um app nativo mobile (iOS/Android) — é web só mesmo, adaptável.

## 5. Idioma

- **Português (Brasil) e Inglês**, com seletor de idioma dentro do próprio app (troca em tempo real, sem reload de página, preferência salva entre sessões). pt-BR é o idioma padrão. Decisão original do MVP era "só português" — revista e ampliada porque o custo de suportar os dois desde já era baixo (ver seção 13).

## 6. Funcionalidades do MVP

1. **Perfil de jogador** — bio, plataforma, estilo de jogo, horas jogadas, jogos/modos favoritos dentro do GTA, fotos. **Status: concluído** (ver seção 13) — falta só a visualização do perfil de *outros* jogadores, que vem natural junto com a busca (item 2).
2. **Busca/filtro de jogadores** — encontrar gente por região, horário, tipo de atividade preferida (heist, RP, corrida, freemode, campanha/mundo aberto, etc). **Status: concluído** (ver seção 13).
3. **Chat / mensagens diretas** — conversar dentro do app antes de se conectar no jogo. **Status: concluído** (ver seção 13) — 1:1 em tempo real, sem chat em grupo.
4. **Feed / posts** — mural social com fotos, clipes e conquistas do jogo. **Status: concluído** (ver seção 13) — só fotos (sem vídeo/clipe, decisão consciente), feed público global com filtro opcional "só conexões" (reaproveita `Connection`, sem sistema de "seguir" separado). **Fecha o conjunto original de funcionalidades do MVP** — os quatro itens desta lista estão prontos.

## 7. Identidade e tom do produto

O fundador é um jogador de PS5 que valoriza **campanha, mundo aberto e ótima jogabilidade** — prefere experiências bem produzidas a conteúdo raso. Essa sensibilidade deve se refletir no produto como:

- **Curadoria de qualidade**: visual cuidado, não poluído, com cara "premium" — nada de interface genérica ou cheia de ruído visual.
- **Match por estilo de jogo**: os filtros de busca devem permitir conectar por afinidade de experiência (ex: alguém que curte explorar mundo aberto com calma vs. alguém focado em grind de heist), não só por disponibilidade de horário. O perfil já modela isso como tags estruturadas (`PlaystyleTag`), prontas para alimentar a busca (item 2 do MVP).
- **Identidade visual inspirada no GTA**: paleta e tipografia com sensação de GTA (urbano, neon sobre fundo escuro, tipografia bold), **sem copiar fontes/cores/logo oficiais da Rockstar** (risco de propriedade intelectual, decisão já tomada). **Status: concluído** — paleta "Vice City Sunset" (magenta/roxo/ciano sobre fundo quase-preto violeta, com glow radial sutil) + tipografia de destaque `Space Grotesk` (Google Fonts) nos títulos, mantendo o corpo de texto na fonte padrão do sistema. Ver seção 13.

## 8. Segurança e moderação

Importante desde o início, já que o app conecta estranhos.

- **Verificação de conta (e-mail)**: implementada com MailKit + Mailpit (dev local). Ao se cadastrar, o usuário recebe um link de confirmação; um banner persistente aparece enquanto o e-mail não for confirmado (acesso não bloqueado — UX deliberada). **Status: concluído** (ver seção 13).
- **Sistema de avaliação/reputação** — jogadores avaliam uns aos outros depois de jogar junto. **Status: concluído** (ver seção 13). Avaliar exige uma **conexão aceita** (pedido + aceite mútuo, tipo pedido de amizade) entre os dois perfis — forma de validar que realmente jogaram juntos, já que o app não tem como confirmar isso sozinho.
- **Denúncia e bloqueio** — usuários podem reportar comportamento tóxico e bloquear outros perfis. **Status: concluído** (ver seção 13). Bloqueio esconde os dois lados da busca e da lista de conversas, e impede mensagem nova; denúncia é registrada com motivo.
- **Painel de revisão de denúncias (moderador)** — tela `/moderacao` com as denúncias agrupadas por perfil, moderador pode marcar como revisada, apagar posts do perfil denunciado e banir/suspender a conta. **Status: concluído** (ver seção 13). Acesso via role "Moderator" do Identity, concedida automaticamente a e-mails configurados em `ModeratorEmails` (appsettings) — ver seção 11 pra detalhes técnicos.

## 9. Modelo de sustentação

**Freemium**: funcionalidades básicas gratuitas; recursos premium pagos no futuro (ex: destaque de perfil, filtros avançados de busca). Não é prioridade de curto prazo, mas o modelo já está definido para orientar decisões de arquitetura (ex: separar o que é "core" do que é "premium" desde o design). Nenhuma modelagem de cobrança/planos foi feita ainda.

## 10. GTA VI (futuro)

O app deve ser **construído com arquitetura pronta para multi-jogo** desde o início — perfil, filtros e demais estruturas de dados não devem assumir "GTA V" como fixo, para permitir adicionar GTA VI como uma segunda opção de jogo quando ele lançar, sem retrabalho estrutural. O enum `GameTitle` no backend já reflete isso (só `GtaV` hoje, mas criado para receber um segundo valor sem quebrar nada).

## 11. Equipe e stack técnica

- **Equipe**: ainda indefinida — pode ser projeto solo (com apoio de IA/Claude Code) ou ganhar colaboradores no futuro. Não travar decisões de produto nisso, mas manter o projeto simples o bastante para ser mantido por uma pessoa se necessário.
- **Frontend**: Angular (standalone components, signals), Tailwind CSS, ngx-translate para i18n.
- **Backend**: .NET 10, Clean Architecture (Domain/Application/Infrastructure/Api), ASP.NET Identity + JWT, EF Core.
- **Banco de dados**: SQL Server, rodando em container Docker localmente — decisão tomada (era "a definir"). Hospedagem/infra de produção ainda não definida.

## 12. Pontos em aberto (para revisitar)

- **Diferencial competitivo claro**: ainda não está definido exatamente o que torna esse app melhor do que Discord/Reddit/WhatsApp para esse público. Hipóteses levantadas: foco 100% em GTA (sem ruído de servidores genéricos) e/ou matching mais inteligente por compatibilidade real (horário, estilo, região). Vale validar com usuários reais antes de travar a proposta de valor.
- **Nome do app**: ainda não definido.
- **Infraestrutura de produção**: onde/como hospedar quando sair do ambiente de desenvolvimento local (hoje tudo roda em `localhost` + Docker). Inclui decisão futura de trocar o armazenamento de fotos (hoje disco local da API) por blob storage em nuvem.
- **E-mail em produção**: hoje o envio usa Mailpit (SMTP falso local). Para produção, a decisão é usar **Resend** (3.000 e-mails/mês grátis) — basta criar nova implementação de `IEmailService` sem tocar no restante da camada Application.

> Resolvido e removido desta lista: método de login (decidido como conta própria com email/senha — ver seção 13), idioma (decidido pt-BR + en — ver seção 5), verificação de conta (implementada — ver seção 8 e 13), indicador de presença/"online agora" (implementado — ver seção 13).

## 13. Status de implementação (atualizado a cada mudança relevante)

Esta seção existe para preservar contexto entre sessões de trabalho — o que já foi construído, decidido e testado, para não repetir decisões nem perder o fio da meada.

### Concluído

- **Scaffold do projeto**: monorepo (`/backend` + `/frontend`), Clean Architecture no backend, Angular standalone no frontend, banco SQL Server via Docker, README com passo a passo de setup.
- **Autenticação**: cadastro e login com email/senha (ASP.NET Identity), JWT, rota protegida (`/perfil`) com guard no frontend, interceptor que anexa o token automaticamente.
- **Internacionalização (pt-BR/en)**: seletor de idioma no app, sem reload; backend localiza mensagens de erro via `Accept-Language` (FluentValidation, Identity, exceções customizadas — tudo com `.resx` compartilhado).
- **Perfil de jogador completo**: visualizar/editar bio, estilo de jogo (`PlaystyleTag`, tags estruturadas multi-seleção), horas jogadas, modos favoritos, região (`Region`), disponibilidade (`AvailabilityTag`), upload de foto de avatar (armazenamento em disco local na API, servido via arquivos estáticos).
- **Busca/filtro de jogadores**: tela `/jogadores` com filtro por plataforma, região, estilo de jogo e disponibilidade (bitmask, tradução nativa pra SQL `&` verificada), paginação, exclusão do próprio usuário nos resultados; detalhe somente-leitura de outro jogador em `/jogadores/:id`; barra de navegação (Perfil/Buscar) visível quando autenticado.
- **Identidade visual "Vice City Sunset"**: paleta magenta/roxo/ciano sobre fundo quase-preto violeta (glow radial sutil), tipografia de destaque `Space Grotesk` nos títulos — aplicada via `@theme` do Tailwind v4 (sobrescreve os tons `neutral`/`amber` usados em todo o app, sem tocar em cada componente individualmente).
- **Chat / mensagens diretas**: 1:1 em tempo real via SignalR (`ChatHub` em `/hubs/chat`, autenticado por JWT via query string — WebSocket não permite header `Authorization`). Conversa nasce como efeito colateral do primeiro envio (find-or-create pelo par de participantes, sem endpoint de "criar conversa"); envio só pelo Hub, leitura (histórico + lista) por REST (`/api/conversations`). Status de leitura por conversa (não por mensagem), badge de não-lidas na navegação.
- **Bloqueio e denúncia**: `Block` (direcional no registro, bidirecional no efeito — checagem sempre nos dois sentidos) esconde o outro perfil da busca e da lista de conversas para os dois lados, e impede mensagem nova (`ChatService.SendMessageAsync` valida antes de criar/reaproveitar a conversa); bloquear/desbloquear são idempotentes. `Report` grava motivo (`ReportReason`) + detalhes opcionais. Tela `/bloqueados` pra gerenciar (desbloquear), botões "Bloquear"/"Denunciar" no perfil de outros jogadores.
- **Feed / posts**: mural público global (`/feed`) — texto opcional + foto opcional (nunca os dois vazios), curtida simples sem comentários, só o autor apaga o próprio post, posts de quem está bloqueado somem do feed dos dois lados (mesma consistência de busca/chat). Upload de foto reaproveita `IPhotoStorageService` (novo método `SavePostPhotoAsync`, salva em `uploads/posts/`). **Fecha os 4 itens do MVP original** (seção 6).
- **Conexões + avaliação/reputação**: `Connection` (pedido + aceite mútuo, máquina de estados `Pending → Accepted/Declined`; só o endereçado aceita, qualquer um dos dois recusa; recusado não trava um novo pedido entre o mesmo par) é pré-requisito pra `Rating` (estrelas de 1 a 5 + comentário opcional, upsert — reavaliar atualiza, não duplica). Tela `/conexoes` (pedidos recebidos/enviados/conexões aceitas); perfil de outro jogador (`/jogadores/:id`) ganha botão contextual (Conectar/Pedido enviado/Aceitar+Recusar) e, com conexão aceita, formulário de avaliação; média (`★ X.X (N)`) aparece nos cards de busca e no detalhe, calculada em lote (`GetAggregatesAsync`, uma query `GROUP BY`/`WHERE IN` por página, sem N+1).
- **Navegação responsiva**: bottom nav fixa com ícones no mobile (`<768px`), nav do topo em formato pill com ícones no desktop — mesmos 5 itens (Perfil/Buscar/Feed/Conexões/Mensagens) nos dois formatos. Cards de busca truncam badges de estilo/disponibilidade em 3 + "+N" pra não quebrar o layout com jogadores que marcam muitas tags.
- **Verificação de e-mail**: cadastro dispara e-mail de confirmação (MailKit + Mailpit em dev, Resend planejado pra produção); acesso não é bloqueado antes de confirmar — banner persistente com reenvio embutido. Token do Identity codificado em Base64Url pra ser seguro em URL.
- **Notificações in-app**: `Notification` (destinatário/ator/tipo/entidade relacionada) cobre 4 eventos — pedido de conexão recebido, mensagem recebida, avaliação recebida, curtida no post ("conexão aceita" foi deliberadamente deixado de fora). Entrega em tempo real reaproveitando a conexão SignalR do `ChatHub` (evento `ReceiveNotification`, sem hub/WebSocket novo) — `INotificationPusher` é a primeira interface da Application implementada na Api (não na Infrastructure), porque só a Api conhece o Hub. Sino único no header (mobile e desktop) **substituiu** os badges avulsos que existiam antes (mensagens não lidas, pedidos de conexão pendentes) — tela `/notificacoes` lista as recentes, clique marca como lida e navega pro destino, botão "marcar todas como lidas".
- **Painel de revisão de denúncias**: tela `/moderacao` (lista, agrupada por perfil denunciado, ordenada por pendentes) + `/moderacao/:profileId` (detalhe: denúncias individuais, posts recentes do perfil, ações). Moderador pode marcar denúncia como revisada, apagar qualquer post do perfil denunciado (`FeedService.DeletePostAsModeratorAsync`, sem checagem de autoria) e banir/desbanir a conta (reaproveita o lockout nativo do Identity — `LockoutEnd`/`LockoutEnabled`, sem coluna nova). Protegido por role "Moderator" de verdade (`[Authorize(Roles="Moderator")]`), concedida automaticamente no login/registro a e-mails listados em `ModeratorEmails` (appsettings) — primeiro uso de roles no projeto. Ícone de escudo no header só aparece pra quem é moderador.
- **Feed filtrado por conexões**: toggle "Todos"/"Conexões" na tela `/feed` — reaproveita `Connection` (status `Accepted`) em vez de criar um sistema de "seguir" separado. `IFeedRepository.GetFeedAsync` ganhou um parâmetro `onlyProfileIds` opcional (null = comportamento atual, lista = restringe aos autores informados), sem duplicar a query.
- **Indicador de presença ("online agora")**: `IPresenceTracker` (Application) + `InMemoryPresenceTracker` (Infrastructure, `Singleton` — não `Scoped`, precisa sobreviver entre requests) conta conexões SignalR ativas por usuário (`ConcurrentDictionary<Guid,int>`, suporta múltiplas abas/dispositivos). `ChatHub.OnConnectedAsync`/`OnDisconnectedAsync` marcam online/offline — reaproveita a conexão que já existe pro chat/notificações, sem hub novo. Sem push em tempo real de presença (fica só pra próxima consulta) — bolinha verde no avatar (busca) e "Online agora" (detalhe), `PlayerSummaryDto.IsOnline` calculado em memória (sem custo de banco). Estado é efêmero — reinicia zerado se a API reiniciar, e tudo bem, não é dado de negócio.
- **Testes automatizados**: 189 testes no backend (xUnit, Domain + Application), 1 no frontend — cobrindo entidades de domínio, validators, services com mocks; features de conexões/avaliação, notificações, feed filtrado, painel de moderação e presença também verificadas ponta a ponta (curl contra o banco real + Playwright com dois usuários no navegador, incluindo tempo real via SignalR sem reload, fluxo completo de banimento/desbanimento, e presença mudando de online pra offline ao fechar a conexão).

### Em andamento / próximo (nesta ordem, combinada com o usuário)

1. A definir — candidato: nome definitivo do app (único item restante da lista original de "próximos passos").

### Decisões técnicas já fechadas (não reabrir sem motivo novo)

- Login: conta própria (não Rockstar Social Club).
- Banco: SQL Server via Docker.
- Arquitetura backend: Clean Architecture, sem Repository genérico, sem MediatR/CQRS (reavaliar só se a Application crescer muito).
- Frontend: Tailwind CSS (não Angular Material), ngx-translate (não Angular `$localize`).
- Estilo de jogo: tags fixas estruturadas (`[Flags] enum`), não texto livre — pensado para alimentar a busca.
- Foto de perfil: disco local por enquanto, abstração (`IPhotoStorageService`) já isolada para trocar por blob storage depois sem refatorar Application/Domain.
- Paleta visual: "Vice City Sunset" (magenta `#FF3EC9` / roxo / ciano sobre fundo violeta quase-preto `#0A0612`) — escolhida entre 3 opções apresentadas ao usuário; tipografia de destaque `Space Grotesk`.
- E-mail em dev: **Mailpit** (container Docker, SMTP falso local em `localhost:1025`, UI web em `localhost:8025`) — zero configuração externa, nenhum e-mail real enviado, servidor sobe junto com o SQL Server via `docker compose up`.
- Biblioteca de envio de e-mail: **MailKit 4.17.0** (Infrastructure). Interface `IEmailService` na Application — permite trocar a implementação em produção (Resend, SendGrid) sem tocar em nada além do arquivo de implementação.
- Verificação de e-mail: acesso **não bloqueado** antes da confirmação — banner persistente no topo da tela até o e-mail ser confirmado, com botão de reenvio embutido. Token gerado pelo ASP.NET Identity com encoding Base64Url para URL-safety. `emailConfirmed` retornado em toda resposta de auth e persistido no `localStorage`; confirmação atualiza o sinal sem logout.
- Papel de moderador: role real do ASP.NET Identity ("Moderator"), não lista de e-mail solta — sincronizada automaticamente no login/registro a partir de `ModeratorEmails` (appsettings), sem tela de "conceder moderador" (projeto ainda é solo). JWT carrega a role como claim (`ClaimTypes.Role`) pra `[Authorize(Roles=...)]` funcionar — sem isso a checagem no backend falha mesmo com a role certa no banco (bug real encontrado e corrigido durante a implementação). Banimento reaproveita o lockout nativo do Identity, sem coluna nova; token já emitido continua válido até expirar (60min) — sem revogação de token na v1.

---

*Este documento cobre as premissas de produto. Decisões de arquitetura técnica detalhada, design de telas e roadmap de execução devem ser tratadas em documentos complementares, referenciando este como fonte da verdade sobre "o que é" e "para quem é" o projeto.*
