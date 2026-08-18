# Visão do Projeto

> Documento de premissas iniciais. Base para todas as decisões futuras de produto, design e arquitetura. Atualizar conforme o projeto evoluir.

**Data de criação:** 2026-08-05
**Última atualização:** 2026-08-18
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
4. **Feed / posts** — mural social com fotos, clipes e conquistas do jogo. **Status: não iniciado.**

## 7. Identidade e tom do produto

O fundador é um jogador de PS5 que valoriza **campanha, mundo aberto e ótima jogabilidade** — prefere experiências bem produzidas a conteúdo raso. Essa sensibilidade deve se refletir no produto como:

- **Curadoria de qualidade**: visual cuidado, não poluído, com cara "premium" — nada de interface genérica ou cheia de ruído visual.
- **Match por estilo de jogo**: os filtros de busca devem permitir conectar por afinidade de experiência (ex: alguém que curte explorar mundo aberto com calma vs. alguém focado em grind de heist), não só por disponibilidade de horário. O perfil já modela isso como tags estruturadas (`PlaystyleTag`), prontas para alimentar a busca (item 2 do MVP).
- **Identidade visual inspirada no GTA**: paleta e tipografia com sensação de GTA (urbano, neon sobre fundo escuro, tipografia bold), **sem copiar fontes/cores/logo oficiais da Rockstar** (risco de propriedade intelectual, decisão já tomada). **Status: concluído** — paleta "Vice City Sunset" (magenta/roxo/ciano sobre fundo quase-preto violeta, com glow radial sutil) + tipografia de destaque `Space Grotesk` (Google Fonts) nos títulos, mantendo o corpo de texto na fonte padrão do sistema. Ver seção 13.

## 8. Segurança e moderação

Importante desde o início, já que o app conecta estranhos. **Status: nada disso foi implementado ainda** — e ficou mais urgente agora que o chat (item 3 do MVP) já está no ar: hoje qualquer jogador encontrado na busca pode mandar mensagem pra qualquer outro, sem nenhuma checagem de bloqueio. Trade-off consciente (foi a opção que não foi priorizada quando o chat foi escolhido), não esquecimento — mas é a lacuna mais concreta em aberto agora.

- **Verificação de conta** (email, telefone, ou outro método a definir).
- **Sistema de avaliação/reputação** — jogadores avaliam uns aos outros depois de jogar junto.
- **Denúncia e bloqueio** — usuários podem reportar comportamento tóxico e bloquear outros perfis.

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
- **Verificação de conta / moderação**: método ainda não escolhido (ver seção 8).

> Resolvido e removido desta lista: método de login (decidido como conta própria com email/senha — ver seção 13), idioma (decidido pt-BR + en — ver seção 5).

## 13. Status de implementação (atualizado a cada mudança relevante)

Esta seção existe para preservar contexto entre sessões de trabalho — o que já foi construído, decidido e testado, para não repetir decisões nem perder o fio da meada.

### Concluído

- **Scaffold do projeto**: monorepo (`/backend` + `/frontend`), Clean Architecture no backend, Angular standalone no frontend, banco SQL Server via Docker, README com passo a passo de setup.
- **Autenticação**: cadastro e login com email/senha (ASP.NET Identity), JWT, rota protegida (`/perfil`) com guard no frontend, interceptor que anexa o token automaticamente.
- **Internacionalização (pt-BR/en)**: seletor de idioma no app, sem reload; backend localiza mensagens de erro via `Accept-Language` (FluentValidation, Identity, exceções customizadas — tudo com `.resx` compartilhado).
- **Perfil de jogador completo**: visualizar/editar bio, estilo de jogo (`PlaystyleTag`, tags estruturadas multi-seleção), horas jogadas, modos favoritos, região (`Region`), disponibilidade (`AvailabilityTag`), upload de foto de avatar (armazenamento em disco local na API, servido via arquivos estáticos).
- **Busca/filtro de jogadores**: tela `/jogadores` com filtro por plataforma, região, estilo de jogo e disponibilidade (bitmask, tradução nativa pra SQL `&` verificada), paginação, exclusão do próprio usuário nos resultados; detalhe somente-leitura de outro jogador em `/jogadores/:id`; barra de navegação (Perfil/Buscar) visível quando autenticado.
- **Identidade visual "Vice City Sunset"**: paleta magenta/roxo/ciano sobre fundo quase-preto violeta (glow radial sutil), tipografia de destaque `Space Grotesk` nos títulos — aplicada via `@theme` do Tailwind v4 (sobrescreve os tons `neutral`/`amber` usados em todo o app, sem tocar em cada componente individualmente).
- **Chat / mensagens diretas**: 1:1 em tempo real via SignalR (`ChatHub` em `/hubs/chat`, autenticado por JWT via query string — WebSocket não permite header `Authorization`). Conversa nasce como efeito colateral do primeiro envio (find-or-create pelo par de participantes, sem endpoint de "criar conversa"); envio só pelo Hub, leitura (histórico + lista) por REST (`/api/conversations`). Status de leitura por conversa (não por mensagem), badge de não-lidas na navegação. **Sem checagem de bloqueio** — qualquer jogador encontrado na busca pode mandar mensagem pra qualquer outro (ver seção 8).
- **Testes automatizados**: 87 testes no backend (xUnit, Domain + Application), 1 no frontend — cobrindo entidades de domínio, validators, services com mocks.

### Em andamento / próximo (nesta ordem, combinada com o usuário)

1. A definir — candidatos: feed/posts, segurança/moderação (denúncia/bloqueio/reputação — ganhou urgência com o chat no ar), nome definitivo do app.

### Decisões técnicas já fechadas (não reabrir sem motivo novo)

- Login: conta própria (não Rockstar Social Club).
- Banco: SQL Server via Docker.
- Arquitetura backend: Clean Architecture, sem Repository genérico, sem MediatR/CQRS (reavaliar só se a Application crescer muito).
- Frontend: Tailwind CSS (não Angular Material), ngx-translate (não Angular `$localize`).
- Estilo de jogo: tags fixas estruturadas (`[Flags] enum`), não texto livre — pensado para alimentar a busca.
- Foto de perfil: disco local por enquanto, abstração (`IPhotoStorageService`) já isolada para trocar por blob storage depois sem refatorar Application/Domain.
- Paleta visual: "Vice City Sunset" (magenta `#FF3EC9` / roxo / ciano sobre fundo violeta quase-preto `#0A0612`) — escolhida entre 3 opções apresentadas ao usuário; tipografia de destaque `Space Grotesk`.

---

*Este documento cobre as premissas de produto. Decisões de arquitetura técnica detalhada, design de telas e roadmap de execução devem ser tratadas em documentos complementares, referenciando este como fonte da verdade sobre "o que é" e "para quem é" o projeto.*
