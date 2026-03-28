# Zynko

Jogo de cartas adulto estilo Cards Against Humanity. Feito em português-brasileiro, com campo de tradução para inglês. Stack: Clean Architecture + ASP.NET Core 10 + React 19 + Tailwind CSS.

## Comandos rápidos

```bash
# Backend
cd src/Web && dotnet watch run

# Frontend (em outro terminal)
cd src/Web/ClientApp && npm start

# Abrir browser
powershell -Command "Start-Process 'http://localhost:5174'"

# Tirar screenshot da tela (útil para ver o estado do app)
# Criar C:\Users\lucas\AppData\Local\Temp\screenshot.ps1 e executar:
powershell -File "C:\Users\lucas\AppData\Local\Temp\screenshot.ps1"

# Build completo
dotnet build
cd src/Web/ClientApp && npm run build

# Migrations
cd src/Web
dotnet ef migrations add <Nome> --project ../Infrastructure -- --environment Development
dotnet ef database update --project ../Infrastructure -- --environment Development
```

## Iniciando o ambiente de dev (quando o usuário pedir para rodar/iniciar)

1. Matar processos antigos: `powershell -Command "Get-Process -Name dotnet -ErrorAction SilentlyContinue | Stop-Process -Force"`
2. Backend em background: `cd src/Web && dotnet watch run`
3. Frontend em background: `cd src/Web/ClientApp && npm start`
4. Abrir browser: `powershell -Command "Start-Process 'http://localhost:5174'"`
5. Aguardar logs: backend sobe em `http://localhost:5256`, frontend em `http://localhost:5174` (ou 5175 se ocupada)

**Portas:** backend=5256, frontend=5173 (ou próxima livre)
**Problema comum:** se porta 5256 em uso, matar com `powershell -Command "Get-NetTCPConnection -LocalPort 5256 | ... | Stop-Process -Force"`

## Screenshots

Para ver o estado do browser programaticamente, usar o script ps1:
```powershell
# C:\Users\lucas\AppData\Local\Temp\screenshot.ps1
Start-Sleep 3  # tempo para alternar para o browser
Add-Type -AssemblyName System.Windows.Forms,System.Drawing
$s = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$b = New-Object System.Drawing.Bitmap($s.Width, $s.Height)
$g = [System.Drawing.Graphics]::FromImage($b)
$g.CopyFromScreen($s.Location, [System.Drawing.Point]::Empty, $s.Size)
$b.Save('C:\Users\lucas\AppData\Local\Temp\screenshot2.png')
$g.Dispose(); $b.Dispose()
```
Executar com `powershell -File "C:\Users\lucas\AppData\Local\Temp\screenshot.ps1"` e ler com Read tool.

## Arquitetura

```
src/
  Domain/          → Entidades, enums, eventos de domínio
  Application/     → CQRS: Commands, Queries, DTOs, validações (MediatR)
  Infrastructure/  → EF Core (SQLite), Identity, seed de dados
  Web/             → ASP.NET Core Minimal API + React SPA (Vite)
  Shared/          → Constantes compartilhadas (nomes de serviços Aspire)
tests/
  Application.UnitTests/
  Application.FunctionalTests/
  Domain.UnitTests/
  Infrastructure.IntegrationTests/
  Web.AcceptanceTests/         → Playwright + Reqnroll (BDD)
```

## Entidades do domínio

| Entidade | Descrição |
|---|---|
| `Card` | Carta do baralho (preta ou branca), com texto em pt-BR e inglês |
| `Game` | Partida identificada por um código curto (ex: `PATO42`) |
| `Player` | Jogador dentro de uma partida |
| `Round` | Rodada com carta preta sorteada |
| `Submission` | Carta branca submetida por um jogador em uma rodada |

## Enums

- `CardType`: `Black = 0`, `White = 1`
- `GameStatus`: `WaitingForPlayers = 0`, `InProgress = 1`, `Finished = 2`
- `RoundStatus`: `Submitting = 0`, `Judging = 1`, `Finished = 2`

## Endpoints da API

| Método | Rota | Ação |
|---|---|---|
| POST | `/api/games` | Criar partida |
| GET | `/api/games/{code}` | Estado da partida |
| GET | `/api/games/{id}/hand/{playerId}` | Mão do jogador |
| POST | `/api/games/{id}/rounds/start` | Iniciar rodada |
| POST | `/api/games/{id}/rounds/{roundId}/submit` | Submeter carta |
| POST | `/api/games/{id}/rounds/{roundId}/winner` | Escolher vencedor |

## Frontend (React + Tailwind)

```
ClientApp/src/
  pages/
    SetupPage.jsx       → Criar partida (nomes, limite de pontos)
    GamePage.jsx        → Jogo (orquestra fases da rodada)
    ResultsPage.jsx     → Fim de jogo
  components/
    BlackCard.jsx       → Carta preta (bg-black text-white)
    WhiteCard.jsx       → Carta branca (bg-white, clicável)
    PlayerHand.jsx      → Mão do jogador
    SubmissionPile.jsx  → Cartas anônimas para o juiz
    Scoreboard.jsx      → Placar
    PassScreen.jsx      → "Passe o celular para [Nome]"
    WinnerBanner.jsx    → Animação de vitória da rodada
    NavMenu.jsx         → Só o nome do jogo + ThemeToggle
```

## Fluxo de jogo (local/mesmo celular)

1. Setup: inserir nomes (2–12), escolher limite de pontos
2. Por rodada:
   - Exibir carta preta
   - Para cada jogador (exceto juiz): PassScreen → mão → escolher carta
   - Juiz: PassScreen → ver cartas anônimas → escolher melhor
   - WinnerBanner → próxima rodada
3. ResultsPage quando alguém atinge o limite

## Cartas

Seed em `Infrastructure/Data/ApplicationDbContextInitialiser.cs`. Pack padrão: `"Zynko Base"`. ~50 cartas pretas + ~150 cartas brancas em pt-BR, com campo `TextEn` para inglês.

Para adicionar expansões: inserir novas cartas com `Pack = "Nome da Expansão"`.

## Tecnologias

| Camada | Stack |
|---|---|
| Backend | ASP.NET Core 10, EF Core 10 (SQLite), MediatR, FluentValidation, AutoMapper |
| API Docs | Scalar (`/scalar`) |
| Auth | ASP.NET Core Identity (não obrigatório para jogar) |
| Frontend | React 19, Vite 8, React Router v7, Tailwind CSS v4 |
| Testes | NUnit, Playwright, Reqnroll |
| Observabilidade | OpenTelemetry via Aspire |

## Futuro: Multiplayer Online

- Identificação por `Game.Code` já permite entrada via código de sala
- Estado completo no banco → SignalR só precisa notificar clientes
- Adicionar `GameHub.cs` em `Web/Hubs/` com grupos por `Game.Code`
