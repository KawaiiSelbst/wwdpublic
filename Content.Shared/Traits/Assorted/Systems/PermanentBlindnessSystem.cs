﻿using Content.Shared.Examine;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Traits.Assorted.Components;
using Robust.Shared.Network;

namespace Content.Shared.Traits.Assorted.Systems;

/// <summary>
/// This handles permanent blindness, both the examine and the actual effect.
/// </summary>
public sealed class PermanentBlindnessSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly BlindableSystem _blinding = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PermanentBlindnessComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PermanentBlindnessComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PermanentBlindnessComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<PermanentBlindnessComponent, EyeDamageChangedEvent>(OnEyeDamageChanged); // WD ADD
    }

    private void OnExamined(Entity<Components.PermanentBlindnessComponent> blindness, ref ExaminedEvent args)
    {
        if (args.IsInDetailsRange && !_net.IsClient && blindness.Comp.Blindness == 0)
        {
            args.PushMarkup(Loc.GetString("trait-examined-Blindness", ("target", Identity.Entity(blindness, EntityManager))));
        }
    }

    private void OnShutdown(Entity<Components.PermanentBlindnessComponent> blindness, ref ComponentShutdown args)
    {
        _blinding.UpdateIsBlind(blindness.Owner);
    }

    private void OnMapInit(Entity<Components.PermanentBlindnessComponent> blindness, ref MapInitEvent args)
    {
        if (!_entityManager.TryGetComponent<BlindableComponent>(blindness, out var blindable))
            return;

        if (blindness.Comp.Blindness != 0)
            _blinding.SetMinDamage(new Entity<BlindableComponent?>(blindness.Owner, blindable), blindness.Comp.Blindness);
        else
        {
            var maxMagnitudeInt = (int) BlurryVisionComponent.MaxMagnitude;
            _blinding.SetMinDamage(new Entity<BlindableComponent?>(blindness.Owner, blindable), maxMagnitudeInt);
        }
    }
    

// WD EDIT START
    /// <summary>
    /// Handles eye healing attempts to ensure blindness from the trait cannot be cured
    /// </summary>
    private void OnEyeDamageChanged(Entity<Components.PermanentBlindnessComponent> blindness, ref EyeDamageChangedEvent args)
    {
        if (!_entityManager.TryGetComponent<BlindableComponent>(blindness, out var blindable))
            return;

        if (blindness.Comp.Blindness != 0)
        {
            // Set minimum eye damage to the trait value
            _blinding.SetMinDamage(new Entity<BlindableComponent?>(blindness.Owner, blindable), blindness.Comp.Blindness);
        }
        else
        {
            // Set maximum possible eye damage
            var maxMagnitudeInt = (int) BlurryVisionComponent.MaxMagnitude;
            _blinding.SetMinDamage(new Entity<BlindableComponent?>(blindness.Owner, blindable), maxMagnitudeInt);
        }
        
        // Update blindness state after parameter changes
        _blinding.UpdateIsBlind(blindness.Owner);
    }
// WD EDIT END
}
