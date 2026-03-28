using Zynko.Domain.Common;
using Zynko.Domain.Enums;

namespace Zynko.Domain.Entities;

public class Card : BaseEntity
{
    public string Text { get; set; } = string.Empty;

    public string TextEn { get; set; } = string.Empty;

    public CardType Type { get; set; }

    public string Pack { get; set; } = string.Empty;
}
