using Microsoft.EntityFrameworkCore;
using Zynko.Domain.Constants;
using Zynko.Domain.Entities;
using Zynko.Domain.Enums;
using Zynko.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Zynko.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.EnsureCreatedAsync();
            await ApplySchemaUpdatesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    private async Task ApplySchemaUpdatesAsync()
    {
        var conn = _context.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            // Add Name column to Games if it doesn't exist yet
            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Games') WHERE name='Name'";
            var exists = (long)(await checkCmd.ExecuteScalarAsync())! > 0;
            if (!exists)
            {
                using var alterCmd = conn.CreateCommand();
                alterCmd.CommandText = "ALTER TABLE Games ADD COLUMN Name TEXT NOT NULL DEFAULT ''";
                await alterCmd.ExecuteNonQueryAsync();
                _logger.LogInformation("Applied schema update: added Games.Name column.");
            }
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var administratorRole = new IdentityRole(Roles.Administrator);

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }

        // Default users
        var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost" };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name });
            }
        }

        // Seed cards
        if (!_context.Cards.Any())
        {
            const string pack = "Pato Escada Base";

            var blackCards = new List<(string Text, string TextEn)>
            {
                ("O segredo para o sucesso no Brasil é ___.", "The secret to success in Brazil is ___."),
                ("Na minha festa de aniversário eu pedi ___.", "At my birthday party I asked for ___."),
                ("O médico disse que eu precisava de ___ para me recuperar.", "The doctor said I needed ___ to recover."),
                ("O que faz o brasileiro feliz? ___.", "What makes a Brazilian happy? ___."),
                ("Minha vó morreu por causa de ___.", "My grandma died because of ___."),
                ("O governo deveria investir em ___ ao invés de educação.", "The government should invest in ___ instead of education."),
                ("O que você não quer encontrar no banheiro público? ___.", "What don't you want to find in a public bathroom? ___."),
                ("Meu terapeuta disse que meu problema é ___.", "My therapist said my problem is ___."),
                ("O que melhorou minha vida sexual? ___.", "What improved my sex life? ___."),
                ("Pesquisadores descobriram que ___ causa câncer.", "Researchers discovered that ___ causes cancer."),
                ("Minha resolução de ano novo é ___.", "My New Year's resolution is ___."),
                ("O que faltou no casamento dos meus pais? ___.", "What was missing from my parents' wedding? ___."),
                ("O que está escondido na mala do presidente? ___.", "What is hidden in the president's suitcase? ___."),
                ("Como você explica ___ para uma criança?", "How do you explain ___ to a child?"),
                ("O que salvou minha vida? ___.", "What saved my life? ___."),
                ("Meu vizinho foi preso por causa de ___.", "My neighbor was arrested because of ___."),
                ("O que eu encontrei embaixo do colchão do meu avô? ___.", "What did I find under my grandfather's mattress? ___."),
                ("A nova série da Netflix é sobre ___.", "The new Netflix series is about ___."),
                ("O que você nunca quer ouvir do seu chefe? ___.", "What do you never want to hear from your boss? ___."),
                ("Por que você não foi trabalhar? Por causa de ___.", "Why didn't you go to work? Because of ___."),
                ("___ é o novo petróleo.", "___ is the new oil."),
                ("O que está matando o planeta? ___.", "What is killing the planet? ___."),
                ("O que aprendi na faculdade? ___.", "What did I learn in college? ___."),
                ("Qual é o segredo de um casamento feliz? ___.", "What is the secret to a happy marriage? ___."),
                ("O que o Uber driver estava escutando? ___.", "What was the Uber driver listening to? ___."),
                ("Minha dieta consiste basicamente de ___.", "My diet consists mostly of ___."),
                ("O que você faria com R$1.000.000? ___.", "What would you do with R$1,000,000? ___."),
                ("Meu ex me deixou por causa de ___.", "My ex left me because of ___."),
                ("O que o político prometeu mas não cumpriu? ___.", "What did the politician promise but never delivered? ___."),
                ("Meu plano para aposentadoria é ___.", "My retirement plan is ___."),
                ("Por que fui banido do supermercado? ___.", "Why was I banned from the supermarket? ___."),
                ("O meu maior medo é ___.", "My biggest fear is ___."),
                ("O que aconteceu depois da festa junina? ___.", "What happened after the June festival? ___."),
                ("Qual é o pior presente de Natal? ___.", "What is the worst Christmas gift? ___."),
                ("O que está faltando no cardápio do McDonald's? ___.", "What's missing from McDonald's menu? ___."),
                ("O que o cantor sertanejo usa para se inspirar? ___.", "What does the country singer use for inspiration? ___."),
                ("O que a vovó guarda na bolsa? ___.", "What does grandma keep in her purse? ___."),
                ("Meu médico me receitou ___ três vezes ao dia.", "My doctor prescribed me ___ three times a day."),
                ("O que você encontrou no fogão da sua avó? ___.", "What did you find on your grandmother's stove? ___."),
                ("O Brasil vai ser campeão quando tiver ___.", "Brazil will be champion when it has ___."),
                ("Minha mãe me mandou no grupo da família: ___.", "My mom sent this in the family group chat: ___."),
                ("O que está incluído no pacote de lua de mel? ___.", "What is included in the honeymoon package? ___."),
                ("O novo app do governo serve para ___.", "The new government app is used for ___."),
                ("O que cai bem com uma farofa? ___.", "What goes well with farofa? ___."),
                ("O SUS deveria oferecer ___ de graça.", "The public health system should offer ___ for free."),
                ("Meu novo negócio vai lucrar vendendo ___.", "My new business will profit from selling ___."),
                ("No dia da minha formatura, faltou ___.", "On my graduation day, ___ was missing."),
                ("O que está no cardápio do bar do seu Zé? ___.", "What is on the menu at Seu Zé's bar? ___."),
                ("Meu personal trainer me proibiu de ___.", "My personal trainer forbade me from ___."),
                ("O que você não devia ter colocado no prato de comida do vizinho? ___.", "What shouldn't you have put in your neighbor's food? ___.")
            };

            var whiteCards = new List<(string Text, string TextEn)>
            {
                ("Churrasco às 9h da manhã", "Barbecue at 9am"),
                ("Tomar cerveja no café da manhã", "Drinking beer for breakfast"),
                ("Pedir delivery às 3h da manhã", "Ordering delivery at 3am"),
                ("Sair da balada no domingo de manhã", "Leaving the club on Sunday morning"),
                ("Boleto vencido", "Overdue bill"),
                ("Saldo negativo no final do mês", "Negative balance at the end of the month"),
                ("Pedir nota fiscal", "Asking for a receipt"),
                ("Trabalhador do mês que nunca chegou", "Employee of the month that never came"),
                ("A conta de luz em agosto", "The electricity bill in August"),
                ("Juros do cheque especial", "Overdraft interest"),
                ("Uma selfie com flash às 2h da manhã", "A selfie with flash at 2am"),
                ("O WhatsApp da família", "The family WhatsApp group"),
                ("Áudio de 30 minutos no WhatsApp", "A 30-minute WhatsApp voice message"),
                ("Spam do primo no grupo da família", "Cousin's spam in the family group"),
                ("Fake news do grupo do condomínio", "Fake news in the condo group chat"),
                ("Dentista sem hora marcada", "Dentist without an appointment"),
                ("Plano de saúde que nega tudo", "Health insurance that denies everything"),
                ("INSS que nunca vai pagar", "Social security that will never pay out"),
                ("Largar tudo e viver de aluguel", "Quit everything and live off rent"),
                ("Aplicativo de namoro para fazer amizade", "Dating app to make friends"),
                ("Casar com o primeiro que aparecer", "Marrying the first one who shows up"),
                ("Filhos não planejados", "Unplanned children"),
                ("Criança no restaurante às 23h", "A child in a restaurant at 11pm"),
                ("Bicho de estimação mais caro que filho", "A pet more expensive than a child"),
                ("Ração premium para cachorro misturado", "Premium food for a mixed-breed dog"),
                ("Viagem para Disney parcelada em 48x", "Disney trip in 48 installments"),
                ("Biquíni de fio dental no litoral", "Thong bikini on the beach"),
                ("Funk no último volume", "Funk music at full volume"),
                ("Arrocha no casamento", "Arrocha music at a wedding"),
                ("Sertanejo universitário", "College country music"),
                ("Pagodeiro de fundo de quintal", "Backyard pagode singer"),
                ("Forró com fumaça de cigarro", "Forró with cigarette smoke"),
                ("DJ que não lê a energia da pista", "A DJ who can't read the dancefloor"),
                ("Pastor de Tesla", "Tesla-driving pastor"),
                ("Dinheiro de dízimo", "Tithe money"),
                ("Bênção pelo Pix", "Blessing via Pix payment"),
                ("Oração antes do jogo do Brasil", "Prayer before Brazil's match"),
                ("Futebol como religião", "Football as religion"),
                ("Neymar se jogando", "Neymar diving"),
                ("Técnico de boteco", "Bar room coach"),
                ("Copa do Mundo no Qatar", "World Cup in Qatar"),
                ("Brasil x Argentina", "Brazil vs Argentina"),
                ("Cachorro-quente com purê", "Hot dog with mashed potato"),
                ("X-burgão com maionese verde", "X-burgão with green mayo"),
                ("Coxinha com molho de queijo", "Coxinha with cheese sauce"),
                ("Açaí com granola, banana e leite condensado", "Açaí with granola, banana and condensed milk"),
                ("Churrasco mal passado por ordem do dono", "Undercooked BBQ on the host's orders"),
                ("Pão de queijo às 6h da manhã", "Cheese bread at 6am"),
                ("Brigadeiro de colher", "Spoon brigadeiro"),
                ("Guaraná Antarctica gelado", "Ice cold Guaraná Antarctica"),
                ("Caipirinha com limão taiti", "Caipirinha with Persian lime"),
                ("Pinga de engenho", "Artisanal cachaça"),
                ("Cerveja quente em copo de plástico", "Warm beer in a plastic cup"),
                ("Red Bull com vodka", "Red Bull with vodka"),
                ("Fila do SUS às 5h da manhã", "Public health queue at 5am"),
                ("Pronto-socorro superlotado", "Overcrowded emergency room"),
                ("Remédio genérico que não funciona", "Generic medicine that doesn't work"),
                ("Médico que só diagnostica pelo Google", "Doctor who only diagnoses via Google"),
                ("Receita de homeopatia", "Homeopathy prescription"),
                ("Chá de camomila da vovó", "Grandma's chamomile tea"),
                ("Benzedeira da rua de cima", "The blessing lady down the street"),
                ("Simpatia para passar no concurso", "Ritual charm to pass the civil service exam"),
                ("Cursinho pré-vestibular de 3 anos", "3-year college prep course"),
                ("Faculdade privada para tirar foto", "Private university just to take graduation photos"),
                ("Estágio não remunerado", "Unpaid internship"),
                ("CLT que protege ninguém", "Labor law that protects no one"),
                ("Patrão que paga na sexta-feira", "Boss who pays on Friday"),
                ("Reunião que poderia ser e-mail", "Meeting that could've been an email"),
                ("E-mail que nunca foi respondido", "Email that was never answered"),
                ("Currículo sem experiência, mas com experiência", "Resume with no experience but experience required"),
                ("LinkedIn com foto de balada", "LinkedIn with a party photo"),
                ("Influenciador digital de meia-tarde", "Mid-afternoon digital influencer"),
                ("Promoção exclusiva que nunca acaba", "Exclusive sale that never ends"),
                ("Réveillon em Copacabana", "New Year's Eve in Copacabana"),
                ("Carnaval fora de época", "Carnival out of season"),
                ("São João sem chuva no Nordeste", "Festa Junina without rain in the Northeast"),
                ("Quadrilha que ninguém ensaiou", "Quadrilha that nobody rehearsed"),
                ("Parada Gay na Paulista", "Pride parade on Paulista Avenue"),
                ("Manifestação com carro de som", "Protest with a sound truck"),
                ("Greve geral que durou duas horas", "General strike that lasted two hours"),
                ("Ministro com cartão preto", "Minister with a black card"),
                ("Desvio de verbas para mansão", "Embezzlement for a mansion"),
                ("Propina no envelope pardo", "Bribe in a brown envelope"),
                ("Foro privilegiado", "Privileged jurisdiction"),
                ("Habeas corpus de última hora", "Last-minute habeas corpus"),
                ("Delação premiada falsa", "False plea bargain"),
                ("Mensagem apagada do celular", "Deleted phone messages"),
                ("Orçamento secreto", "Secret budget"),
                ("Emenda parlamentar para si mesmo", "Parliamentary amendment for themselves"),
                ("Privatização mal feita", "Poorly executed privatization"),
                ("Tarifa de luz que sobe todo mês", "Electricity tariff that rises every month"),
                ("Gasolina mais cara que o salário", "Gas more expensive than the salary"),
                ("Imposto que não volta em serviço", "Tax that never returns as service"),
                ("Carteira de motorista comprada", "Purchased driver's license"),
                ("Multa do radar que não estava lá", "Speed camera fine from a camera that wasn't there"),
                ("Blitz na sexta à noite", "Police checkpoint on Friday night"),
                ("Trânsito de São Paulo", "São Paulo traffic"),
                ("Mototaxi sem capacete", "Mototaxi without a helmet"),
                ("Buraco na BR-101", "Pothole on BR-101"),
                ("Enchente em outubro", "Flood in October"),
                ("Queimada da Amazônia", "Amazon burning"),
                ("Praia suja de óleo", "Oil-covered beach"),
                ("Barata de São Paulo que voa", "Flying São Paulo cockroach"),
                ("Jacaré no camping do Pantanal", "Caiman at the Pantanal campsite"),
                ("Pombos na estação de metrô", "Pigeons in the metro station"),
                ("Tatu que atravessou a rodovia", "Armadillo crossing the highway"),
                ("Macaco no parque de Brasília", "Monkey in Brasília's park"),
                ("Bolo de aniversário comprado no dia", "Birthday cake bought on the day"),
                ("Parabéns cantado fora do tom", "Off-key Happy Birthday song"),
                ("Presente embrulhado em sacola de mercado", "Gift wrapped in a grocery bag"),
                ("Vela que não apaga de primeira", "Candle that doesn't blow out on first try"),
                ("Festa surpresa que o aniversariante já sabia", "Surprise party the birthday person already knew about"),
                ("Sogra que mora do lado", "Mother-in-law who lives next door"),
                ("Cunhado desempregado", "Unemployed brother-in-law"),
                ("Primo rico que sumiu", "Rich cousin who disappeared"),
                ("Tio que fala de política no Natal", "Uncle who talks politics at Christmas"),
                ("Avó que coloca açúcar em tudo", "Grandma who puts sugar in everything"),
                ("Criança que não quer dormir", "Child who won't go to sleep"),
                ("Bebê que chora no avião", "Baby crying on the plane"),
                ("Vizinho que faz obra no domingo", "Neighbor doing construction on Sunday"),
                ("Cachorro que late às 3h", "Dog barking at 3am"),
                ("Gato que derruba tudo", "Cat that knocks everything over"),
                ("Papagaio que aprende palavrão", "Parrot that learns swear words"),
                ("Peixe de estimação sem nome", "Nameless pet fish"),
                ("Aquário que vira cemitério", "Aquarium that becomes a cemetery"),
                ("Planta que sobreviveu sem água", "Plant that survived without water"),
                ("Geladeira vazia antes do fim do mês", "Empty fridge before the end of the month"),
                ("Conta de streaming pra toda família", "Streaming account for the whole family"),
                ("Série que cancelaram no cliffhanger", "Series cancelled on a cliffhanger"),
                ("Spoiler no grupo do trabalho", "Spoiler in the work group chat"),
                ("Fone sem fio com bateria vazia", "Wireless earphone with dead battery"),
                ("Celular que esquenta na mão", "Phone that heats up in hand"),
                ("Atualização que travou o celular", "Update that froze the phone"),
                ("Aplicativo que pede acesso a tudo", "App that requests access to everything"),
                ("Senha que você não lembra", "Password you can't remember"),
                ("Captcha impossível de resolver", "Impossible CAPTCHA"),
                ("WiFi que cai na hora da videochamada", "WiFi that drops during a video call"),
                ("Entregador que liga pra você descer", "Delivery driver who calls you to come down"),
                ("Produto que chegou amassado", "Product that arrived dented"),
                ("Avaliação falsa de 5 estrelas", "Fake 5-star review"),
                ("Frete mais caro que o produto", "Shipping more expensive than the product"),
                ("Compra parcelada que já esqueceu", "Installment purchase already forgotten"),
                ("Cartão recusado na frente de todo mundo", "Card declined in front of everyone"),
                ("Pix enviado para a pessoa errada", "Pix sent to the wrong person"),
                ("Caixa eletrônico sem dinheiro", "ATM with no cash"),
                ("Fila do banco no horário de almoço", "Bank queue during lunch hour"),
            };

            var cards = new List<Card>();

            cards.AddRange(blackCards.Select(c => new Card
            {
                Text = c.Text,
                TextEn = c.TextEn,
                Type = CardType.Black,
                Pack = pack
            }));

            cards.AddRange(whiteCards.Select(c => new Card
            {
                Text = c.Text,
                TextEn = c.TextEn,
                Type = CardType.White,
                Pack = pack
            }));

            _context.Cards.AddRange(cards);
            await _context.SaveChangesAsync();
        }
    }
}
