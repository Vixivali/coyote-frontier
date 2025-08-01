using Content.Shared.Access;
using Content.Shared._NF.Shipyard;
using Content.Shared._NF.Shipyard.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._NF.Shipyard.Components;

[RegisterComponent]
public sealed partial class ShipyardVoucherComponent : Component
{
    /// <summary>
    ///  Number of redeemable ships that this voucher can still be used for. Decremented on purchase.
    /// </summary>
    [DataField]
    public uint RedemptionsLeft = 1;

    /// <summary>
    ///  If true, card will be destroyed when no redemptions are left. Checked at time of sale.
    /// </summary>
    [DataField]
    public bool DestroyOnEmpty = false;

    /// <summary>
    ///  Access tags and groups for shipyard access.
    /// </summary>
    [DataField]
    public IReadOnlyCollection<ProtoId<AccessLevelPrototype>> Access { get; private set; } = Array.Empty<ProtoId<AccessLevelPrototype>>();

    [DataField]
    public IReadOnlyCollection<ProtoId<AccessGroupPrototype>> AccessGroups { get; private set; } = Array.Empty<ProtoId<AccessGroupPrototype>>();

    [DataField]
    public List<VesselClass> Classes = new();

    [DataField]
    public List<string> Vessels = new();

    [DataField]
    public bool UseAnywhere = false;

    /// <summary>
    ///  The type of console where this voucher can be used.
    ///  Should not be ShipyardConsoleUiKey.Custom.  Note: currently cannot be used for mothership consoles.
    /// </summary>
    [DataField(required: true)]
    public ShipyardConsoleUiKey ConsoleType;
}
