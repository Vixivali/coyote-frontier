using Content.Shared.Emoting;
using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.InteractionVerbs.Events;
using Content.Shared.Item;
using Content.Shared.Popups;
using Robust.Shared.Serialization;

namespace Content.Shared.Ghost
{
    /// <summary>
    /// System for the <see cref="GhostComponent"/>.
    /// Prevents ghosts from interacting when <see cref="GhostComponent.CanGhostInteract"/> is false.
    /// </summary>
    public abstract class SharedGhostSystem : EntitySystem
    {
        [Dependency] protected readonly SharedPopupSystem Popup = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<GhostComponent, UseAttemptEvent>(OnAttempt);
            SubscribeLocalEvent<GhostComponent, InteractionAttemptEvent>(OnAttemptInteract);
            SubscribeLocalEvent<GhostComponent, EmoteAttemptEvent>(OnAttempt);
            SubscribeLocalEvent<GhostComponent, DropAttemptEvent>(OnAttempt);
            SubscribeLocalEvent<GhostComponent, PickupAttemptEvent>(OnAttempt);
            SubscribeLocalEvent<GhostComponent, InteractionVerbAttemptEvent>(OnAttempt);
        }

        private void OnAttemptInteract(Entity<GhostComponent> ent, ref InteractionAttemptEvent args)
        {
            if (!ent.Comp.CanGhostInteract)
                args.Cancelled = true;
        }

        private void OnAttempt(EntityUid uid, GhostComponent component, CancellableEntityEventArgs args)
        {
            if (!component.CanGhostInteract)
                args.Cancel();
        }

        /// <summary>
        /// Sets the ghost's time of death.
        /// </summary>
        public void SetTimeOfDeath(Entity<GhostComponent?> entity, TimeSpan value)
        {
            if (!Resolve(entity, ref entity.Comp))
                return;

            if (entity.Comp.TimeOfDeath == value)
                return;

            entity.Comp.TimeOfDeath = value;
            Dirty(entity);
        }

        [Obsolete("Use the Entity<GhostComponent?> overload")]
        public void SetTimeOfDeath(EntityUid uid, TimeSpan value, GhostComponent? component)
        {
            SetTimeOfDeath((uid, component), value);
        }

        /// <summary>
        /// Sets whether or not the ghost player is allowed to return to their original body.
        /// </summary>
        public void SetCanReturnToBody(Entity<GhostComponent?> entity, bool value)
        {
            if (!Resolve(entity, ref entity.Comp))
                return;

            if (entity.Comp.CanReturnToBody == value)
                return;

            entity.Comp.CanReturnToBody = value;
            Dirty(entity);
        }

        [Obsolete("Use the Entity<GhostComponent?> overload")]
        public void SetCanReturnToBody(EntityUid uid, bool value, GhostComponent? component = null)
        {
            SetCanReturnToBody((uid, component), value);
        }

        [Obsolete("Use the Entity<GhostComponent?> overload")]
        public void SetCanReturnToBody(GhostComponent component, bool value)
        {
            SetCanReturnToBody((component.Owner, component), value);
        }


        /// <summary>
        /// Sets whether the ghost is allowed to interact with other entities.
        /// </summary>
        public void SetCanGhostInteract(Entity<GhostComponent?> entity, bool value)
        {
            if (!Resolve(entity, ref entity.Comp))
                return;

            if (entity.Comp.CanGhostInteract == value)
                return;

            entity.Comp.CanGhostInteract = value;
            Dirty(entity);
        }

        // Frontier: uncryo status (mirroring CanReturnToBody)
        public void SetCanReturnFromCryo(EntityUid uid, bool value, GhostComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            component.CanReturnFromCryo = value;
        }

        public void SetCanReturnFromCryo(GhostComponent component, bool value)
        {
            component.CanReturnFromCryo = value;
        }
        // Frontier: uncryo status (mirroring CanReturnToBody)
    }

    /// <summary>
    /// A client to server request to get places a ghost can warp to.
    /// Response is sent via <see cref="GhostWarpsResponseEvent"/>
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostWarpsRequestEvent : EntityEventArgs
    {
    }

    /// <summary>
    /// An individual place a ghost can warp to.
    /// This is used as part of <see cref="GhostWarpsResponseEvent"/>
    /// </summary>
    [Serializable, NetSerializable]
    public struct GhostWarp
    {
        public GhostWarp(NetEntity entity,
            string displayName,
            bool isWarpPoint,
            GhostStatus isGhost,
            int dupeNumber = 0,
            string jobName = ""
            )
        {
            Entity = entity;
            DisplayName = displayName;
            IsWarpPoint = isWarpPoint;
            WarpKind = isGhost;
            DupeNumber = dupeNumber; // Frontier: warp point hiding
            JobName = jobName;
        }

        /// <summary>
        /// The entity representing the warp point.
        /// This is passed back to the server in <see cref="GhostWarpToTargetRequestEvent"/>
        /// </summary>
        public NetEntity Entity { get; }

        /// <summary>
        /// The display name to be surfaced in the ghost warps menu
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The Job name of the player
        /// </summary>
        public string JobName { get; set; } = string.Empty;

        /// <summary>
        /// Whether this warp represents a warp point or a player
        /// </summary>
        public bool IsWarpPoint { get;  }

        // Frontier: warp point hiding
        /// <summary>
        /// Whether this warp requires admin access to warp to
        /// </summary>
        public bool AdminOnly { get; }
        // End Frontier

        /// <summary>
        /// Its a g-ghost!
        /// </summary>
        public GhostStatus WarpKind { get;  }

        /// <summary>
        /// Used to disambiguate multiple warms with the same name.
        /// </summary>
        public int DupeNumber { get;  } = 0;
    }

    /// <summary>
    /// A server to client response for a <see cref="GhostWarpsRequestEvent"/>.
    /// Contains players, and locations a ghost can warp to
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostWarpsResponseEvent : EntityEventArgs
    {
        public GhostWarpsResponseEvent(List<GhostWarp> warps)
        {
            Warps = warps;
        }

        /// <summary>
        /// A list of warp points.
        /// </summary>
        public List<GhostWarp> Warps { get; }
    }

    /// <summary>
    /// Enum of stuff like are they alive, are they dead, a ghost, etc.
    /// </summary>
    public enum GhostStatus
    {
        Alive,
        Dead,
        Ghost,
        Unconscious,
        CryoSleep,
        OtherStuff,
    }

    /// <summary>
    ///  A client to server request for their ghost to be warped to an entity
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostWarpToTargetRequestEvent : EntityEventArgs
    {
        public NetEntity Target { get; }

        public GhostWarpToTargetRequestEvent(NetEntity target)
        {
            Target = target;
        }
    }

    /// <summary>
    /// A client to server request for their ghost to be warped to the most followed entity.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostnadoRequestEvent : EntityEventArgs;

    /// <summary>
    /// A client to server request for their ghost to return to body
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostReturnToBodyRequest : EntityEventArgs
    {
    }

    /// <summary>
    /// A server to client update with the available ghost role count
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostUpdateGhostRoleCountEvent : EntityEventArgs
    {
        public int AvailableGhostRoles { get; }

        public GhostUpdateGhostRoleCountEvent(int availableGhostRoleCount)
        {
            AvailableGhostRoles = availableGhostRoleCount;
        }
    }
}
