#include "AI/Public/Behaviors/DodgeBehavior.h"
#include "AI/Public/Components/Senses.h"
#include "AI/Public/Components/MoveEnactor.h"
#include "AI/Public/Components/PhysicsView.h"
#include "GameObject/Public/Core.h"
#include "Navigation/Public/Core.h"
#include "projectile.h"

namespace AI
{
    using namespace beehive;
    using namespace ECS;
    using namespace GameObj;
    using namespace Nav;

    // TODO: Parameterize
    const float DodgeStartTime = 5.0f;
    const float DodgeSafeRadius = 10.0f;

    static bool DodgeTargetValid(const std::optional<DodgeTarget>& target)
    {
        return target && !target->Projectile.expired();
    }

    static float GetTimeToImpact(const P3D& avoidLocation, const PhysicsView& projectilePhysics)
    {
        float timeToImpact = avoidLocation.Distance3D(projectilePhysics.Location) / projectilePhysics.Velocity.Length();
        return timeToImpact;
    }

    static void AbortDodge(DodgeBehaviorState& state, BehaviorTreeContext& context)
    {
        state.DodgeTarget = std::nullopt;
        context.ResetResumeIndex();
    }

    static std::optional<DodgeTarget> ChooseDodgeTarget(Entity* entity, Senses& senses, const PhysicsView& objectPhysics)
    {
        Object* aiObject = entity->GetComponent<ObjectRef>().GetObj();

        std::sort(senses.KnownEnemyProjectiles.begin(), senses.KnownEnemyProjectiles.end(), [&objectPhysics](const auto& e1, const auto& e2)
        {
            const PhysicsView& entity1Physics = e1->GetComponent<PhysicsView>();
            const PhysicsView& entity2Physics = e2->GetComponent<PhysicsView>();

            return entity1Physics.Location.DistanceSquared3D(objectPhysics.Location) < entity2Physics.Location.DistanceSquared3D(objectPhysics.Location);
        });

        for (const auto& projectile : senses.KnownEnemyProjectiles)
        {
            Object* projectileObject = projectile->GetComponent<ObjectRef>().GetObj();
            const PhysicsView& projectilePhysics = projectile->GetComponent<PhysicsView>();
            ObjSpecProjectile* spec = ObjSpecProjectile::Cast(projectileObject);

            if (!spec->ground_collision)
            {
                TRACE_BEHAVIOR("Projectile is not expected to collide with ground");
                continue;
            }

            const PROJ_Def* def = PROJ_DefGet(spec->projectile_index);

            float speed = projectileObject->velocity.Length();
            float timeToImpact = GetTimeToImpact(spec->ground_collision_position, projectilePhysics);
            if (timeToImpact > DodgeStartTime)
            {
                TRACE_BEHAVIOR("Projectile time to impact of {0} is above threshold", timeToImpact);
                continue;
            }

            P3D aiProjectedLocation = aiObject->location + (aiObject->velocity * timeToImpact);
            if (aiProjectedLocation.DistanceSquared3D(spec->ground_collision_position) < Squared(def->fardist))
            {
                return DodgeTarget{ projectile, def->fardist, spec->ground_collision_position };
            }
        }

        return std::nullopt;
    }

    bool DodgeBehavior::ShouldStartDodge(Entity* entity, Object* object, BehaviorTreeContext& context)
    {
        if (!entity->HasComponent<Senses, PhysicsView>())
            return false;

        if (!World->navMesh)
            return false;

        auto& state = entity->GetComponent<DodgeBehaviorState>();
        if (!DodgeTargetValid(state.DodgeTarget))
        {
            // Make sure invalid targets have been cleared out
            state.DodgeTarget = std::nullopt;
        }
        else
        {
            // Already have dodge in progress
            return false;
        }

        auto [senses, physicsView] = entity->GetComponent<Senses, PhysicsView>();

        state.DodgeTarget = ChooseDodgeTarget(entity, senses, physicsView);
        if (!state.DodgeTarget)
        {
            // No nearby threats
            return false;
        }

        float angleToCenter = dirfcalcp3d(&state.DodgeTarget->AvoidLocation, &physicsView.Location);

        float sa, ca;
        calc_sincosd(dirfadjust(angleToCenter), &sa, &ca);
        P3D wantLoc = state.DodgeTarget->AvoidLocation;
        wantLoc.x += (state.DodgeTarget->ImpactRadius + DodgeSafeRadius) * ca;
        wantLoc.y += (state.DodgeTarget->ImpactRadius + DodgeSafeRadius) * sa;

        MoveGoalParams params;
        params.Speed = MoveGoalSpeed::Fast;
        std::shared_ptr<Path> path = PathUtil::GetPath(entity, World->navMesh.get(), physicsView.Location, wantLoc, &params);
        if (!path)
        {
            TRACE_BEHAVIOR("Couldn't path away from projectile");
            return false;
        }

        TRACE_BEHAVIOR("Dodging to location {0} {1} {2}, est. impact time {3}", 
            wantLoc.x, 
            wantLoc.y, 
            wantLoc.z, 
            GetTimeToImpact(state.DodgeTarget->AvoidLocation, state.DodgeTarget->Projectile.lock()->GetComponent<PhysicsView>()));

        PathUtil::SetPath(entity, path);

        context.SetChainingEnabled(false);
        return true;
    }

    Status DodgeBehavior::MoveToDodgeLocation(Entity* entity, Object* object, BehaviorTreeContext& context)
    {
        auto& state = entity->GetComponent<DodgeBehaviorState>();
        const auto& moveEnactor = entity->GetComponent<MoveEnactor>();

        if (!DodgeTargetValid(state.DodgeTarget)
            || !moveEnactor.Path)
        {
            TRACE_BEHAVIOR("Dodge target no longer valid; canceling");
            AbortDodge(state, context);
            return Status::SUCCESS;
        }

        const auto& aiPhysics = entity->GetComponent<PhysicsView>();

        float distanceFromImpactSq = state.DodgeTarget->AvoidLocation.DistanceSquared3D(aiPhysics.Location);
        if (distanceFromImpactSq > Squared(state.DodgeTarget->ImpactRadius + DodgeSafeRadius))
        {
            // Got far enough away somehow, we can reset
            TRACE_BEHAVIOR("AI is {0} units away from impact location with still active dodge behavior; canceling", sqrt(distanceFromImpactSq));
            AbortDodge(state, context);
            return Status::SUCCESS;
        }

        return Status::RUNNING;
    }

    InnerBehaviorTree DodgeBehavior::Build(ECS::Entity* entity)
    {
        assert(entity);

        DodgeBehaviorState& state = entity->AddComponent<DodgeBehaviorState>();

        return BehaviorTreeBuilder()
            .sequence()
            .leaf(CreateLeaf(entity, ShouldStartDodge))
            .leaf(CreateLeaf(entity, MoveToDodgeLocation))
            .end()
            .build();
    }
}