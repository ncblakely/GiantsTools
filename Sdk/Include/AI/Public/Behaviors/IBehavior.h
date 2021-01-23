#pragma once

#include "ECS/Public/Entity.h"

#if 0
#define TRACE_BEHAVIOR(...) DEBUG_INFOC(__VA_ARGS__)
#else
#define TRACE_BEHAVIOR(...) do { } while(0)
#endif

namespace AI
{
    using InnerBehaviorTree = beehive::Tree<struct BehaviorTreeContext>;

    struct BehaviorTreeContext
    {
        BehaviorTreeContext(std::shared_ptr<ECS::Entity> entity, beehive::Tree<BehaviorTreeContext>* tree)
            : entity(entity),
            innerTree(tree),
            innerTreeState(tree->make_state())
        {
            assert(entity);
        }

        ECS::Entity* GetEntity() const
        {
            return entity.get();
        }

        void ResetResumeIndex()
        {
            innerTreeState = innerTree->make_state();
        }

        bool GetChainingEnabled() const { return chainingEnabled; }

        void SetChainingEnabled(bool chainingEnabled)
        {
            this->chainingEnabled = chainingEnabled;
        }

    private:
        beehive::TreeState& GetBeehiveTreeState()
        {
            return innerTreeState;
        }

        std::shared_ptr<ECS::Entity> entity;
        beehive::Tree<BehaviorTreeContext>* innerTree;
        beehive::TreeState innerTreeState;
        bool chainingEnabled = true; // True if we allow behavior trees further down in the processing order to execute

        friend class BehaviorTree;
    };

    using BehaviorTreeBuilder = beehive::Builder<BehaviorTreeContext>;

    class BehaviorTree
    {
    public:
        BehaviorTree(
            const char* name, 
            std::shared_ptr<ECS::Entity> entity, 
            std::shared_ptr<InnerBehaviorTree> innerTree)
            : m_name(name),
            m_innerTree(innerTree),
            m_treeContext(entity, m_innerTree.get())
        {
        }

        bool Process()
        {
            beehive::TreeState& beehiveTreeState = m_treeContext.GetBeehiveTreeState();

            m_innerTree->process(beehiveTreeState, m_treeContext);

            // Check to see if we should allow any trees further down the priority order to execute:
            bool continueExecution = m_treeContext.GetChainingEnabled();
            m_treeContext.SetChainingEnabled(true); // Always re-enable for next frame

            return continueExecution;
        }

        const char* GetName()
        {
            return m_name;
        }

    private:
        std::shared_ptr<InnerBehaviorTree> m_innerTree;
        BehaviorTreeContext m_treeContext;
        const char* m_name;
    };

    struct IBehavior
    {
        virtual InnerBehaviorTree Build() = 0;
    };
}