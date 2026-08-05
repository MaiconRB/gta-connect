# Visão do Projeto

> Documento de premissas iniciais. Base para todas as decisões futuras de produto, design e arquitetura. Atualizar conforme o projeto evoluir.

**Data de criação:** 2026-08-05
**Nome do app:** ainda não definido (placeholder: usar "GTA Connect" enquanto não houver nome oficial)

---

## 1. O que é o projeto

Uma **rede social dedicada a jogadores de GTA Online**, para PS5/PS4, com objetivo de conectar e permitir que jogadores se conheçam de verdade — não apenas um "buscador de grupo" para uma missão específica, mas um espaço social com perfil, feed e mensagens, focado inteiramente no universo GTA.

Pensado para funcionar hoje com **GTA V** e já preparado, na arquitetura, para incorporar **GTA VI** quando lançar, sem precisar refazer o produto.

## 2. Problema que resolve

Hoje, jogadores usam Discord, Reddit e grupos de WhatsApp para achar parceiros de GTA Online — ambientes genéricos, com ruído, sem estrutura voltada ao jogo, e sem um jeito confiável de saber se vale a pena jogar com alguém. O app propõe um espaço **dedicado, organizado e com identidade própria** para esse público, tirando o atrito de "achar gente boa pra jogar" de ferramentas que não foram feitas pra isso.

*(Diferencial ainda em aberto — ver seção 8, "Pontos em aberto".)*

## 3. Plataformas de jogo do público-alvo

- **PS5 / PS4** — foco exclusivo inicial. Não considerar PC/Xbox no MVP.

## 4. Plataforma do app (onde o app roda)

- **Web app responsivo**, funcionando bem tanto em desktop quanto em celular, independente do tamanho de tela. Não é um app nativo mobile (iOS/Android) — é web só mesmo, adaptável.

## 5. Idioma

- **Português (Brasil)** como idioma principal e único no MVP. Internacionalização não é prioridade agora.

## 6. Funcionalidades do MVP

1. **Perfil de jogador** — bio, plataforma, estilo de jogo, horas jogadas, jogos/modos favoritos dentro do GTA, fotos.
2. **Busca/filtro de jogadores** — encontrar gente por região, horário, tipo de atividade preferida (heist, RP, corrida, freemode, campanha/mundo aberto, etc).
3. **Chat / mensagens diretas** — conversar dentro do app antes de se conectar no jogo.
4. **Feed / posts** — mural social com fotos, clipes e conquistas do jogo.

## 7. Identidade e tom do produto

O fundador é um jogador de PS5 que valoriza **campanha, mundo aberto e ótima jogabilidade** — prefere experiências bem produzidas a conteúdo raso. Essa sensibilidade deve se refletir no produto como:

- **Curadoria de qualidade**: visual cuidado, não poluído, com cara "premium" — nada de interface genérica ou cheia de ruído visual.
- **Match por estilo de jogo**: os filtros de busca devem permitir conectar por afinidade de experiência (ex: alguém que curte explorar mundo aberto com calma vs. alguém focado em grind de heist), não só por disponibilidade de horário.

## 8. Segurança e moderação

Importante desde o início, já que o app conecta estranhos:

- **Verificação de conta** (email, telefone, ou outro método a definir).
- **Sistema de avaliação/reputação** — jogadores avaliam uns aos outros depois de jogar junto.
- **Denúncia e bloqueio** — usuários podem reportar comportamento tóxico e bloquear outros perfis.

## 9. Modelo de sustentação

**Freemium**: funcionalidades básicas gratuitas; recursos premium pagos no futuro (ex: destaque de perfil, filtros avançados de busca). Não é prioridade de curto prazo, mas o modelo já está definido para orientar decisões de arquitetura (ex: separar o que é "core" do que é "premium" desde o design).

## 10. GTA VI (futuro)

O app deve ser **construído com arquitetura pronta para multi-jogo** desde o início — perfil, filtros e demais estruturas de dados não devem assumir "GTA V" como fixo, para permitir adicionar GTA VI como uma segunda opção de jogo quando ele lançar, sem retrabalho estrutural.

## 11. Equipe e stack técnica

- **Equipe**: ainda indefinida — pode ser projeto solo (com apoio de IA/Claude Code) ou ganhar colaboradores no futuro. Não travar decisões de produto nisso, mas manter o projeto simples o bastante para ser mantido por uma pessoa se necessário.
- **Frontend**: Angular.
- **Backend**: .NET / C#.
- **Banco de dados**: a definir (ex: SQL Server ou PostgreSQL) — decisão técnica detalhada fica para uma etapa posterior de planejamento de arquitetura.

## 12. Pontos em aberto (para revisitar)

- **Diferencial competitivo claro**: ainda não está definido exatamente o que torna esse app melhor do que Discord/Reddit/WhatsApp para esse público. Hipóteses levantadas: foco 100% em GTA (sem ruído de servidores genéricos) e/ou matching mais inteligente por compatibilidade real (horário, estilo, região). Vale validar com usuários reais antes de travar a proposta de valor.
- **Método de login/autenticação**: em aberto entre login via Rockstar Social Club (mais integrado, mas depende de viabilidade técnica/API) ou conta própria simples (email/senha ou redes sociais). Decidir isso impacta diretamente o design do perfil (dados vindos automaticamente do jogo vs. preenchidos manualmente).
- **Nome do app**: ainda não definido.
- **Detalhes de banco de dados e infraestrutura**: a aprofundar em documento técnico separado.

---

*Este documento cobre as premissas de produto. Decisões de arquitetura técnica detalhada, design de telas e roadmap de execução devem ser tratadas em documentos complementares, referenciando este como fonte da verdade sobre "o que é" e "para quem é" o projeto.*
